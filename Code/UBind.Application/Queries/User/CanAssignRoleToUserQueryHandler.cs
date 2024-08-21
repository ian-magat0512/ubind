// <copyright file="CanAssignRoleToUserQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.Authorisation;
    using UBind.Application.Queries.Role;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Checks if a role can be assigned to a user.
    /// </summary>
    public class CanAssignRoleToUserQueryHandler : IQueryHandler<CanAssignRoleToUserQuery, bool>
    {
        private readonly IAuthorisationService authorisationService;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CanAssignRoleToUserQueryHandler"/> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        public CanAssignRoleToUserQueryHandler(
            IAuthorisationService authorisationService,
            ICqrsMediator mediator)
        {
            this.authorisationService = authorisationService;
            this.mediator = mediator;
        }

        /// <inheritdoc/>
        public async Task<bool> Handle(CanAssignRoleToUserQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RoleReadModelFilters filters = new RoleReadModelFilters
            {
                TenantId = request.User.TenantId,
                OrganisationIds = new Guid[] { request.User.OrganisationId },
            };

            await this.authorisationService.ApplyModifyUserRestrictionsToFilters(
                request.PerformingUserTenantId, request.PerformingUserId, filters);

            var assignableRoles = await this.mediator.Send(
                new GetAssignableRolesMatchingFiltersQuery(filters), cancellationToken);
            var userType = (RoleType)Enum.Parse(typeof(RoleType), request.User.UserType);
            bool included = assignableRoles.Any(r => r.Id == request.Role.Id && r.Type == userType);
            return await Task.FromResult(included);
        }
    }
}
