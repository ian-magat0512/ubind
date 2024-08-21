// <copyright file="DeleteOrganisationAndAssociatedUsersCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Organisation
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using IUserService = Application.User.IUserService;

    /// <summary>
    /// Handler for marking users as deleted by organisation command.
    /// </summary>
    public class DeleteOrganisationAndAssociatedUsersCommandHandler : ICommandHandler<DeleteOrganisationAndAssociatedUsersCommand, IOrganisationReadModelSummary>
    {
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IUserService userService;
        private readonly IOrganisationService organisationService;
        private readonly ICachingResolver cachingResolver;

        public DeleteOrganisationAndAssociatedUsersCommandHandler(
            IUserReadModelRepository userReadModelRepository,
            IUserService userService,
            IOrganisationService organisationService,
            ICachingResolver cachingResolver)
        {
            this.userReadModelRepository = userReadModelRepository;
            this.userService = userService;
            this.organisationService = organisationService;
            this.cachingResolver = cachingResolver;
        }

        public async Task<IOrganisationReadModelSummary> Handle(DeleteOrganisationAndAssociatedUsersCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var users = this.userReadModelRepository.GetAllUsersByOrganisation(request.TenantId, request.OrganisationId).ToList();
            foreach (var user in users)
            {
                await this.userService.Delete(request.TenantId, user.Id);
            }
            var organisationSummary = await this.organisationService.MarkAsDeleted(request.TenantId, request.OrganisationId);
            this.cachingResolver.RemoveCachedOrganisations(
                request.TenantId, request.OrganisationId, new List<string> { organisationSummary.Alias });
            return organisationSummary;
        }
    }
}
