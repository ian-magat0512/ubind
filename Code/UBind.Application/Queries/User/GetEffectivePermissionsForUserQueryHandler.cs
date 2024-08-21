// <copyright file="GetEffectivePermissionsForUserQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Gets the effective permissions of a user.
    /// The effective permissions are the aggregate permissions from all of the assigned roles of the user,
    /// minus any permissions which may not be enabled for that user's organisation.
    /// </summary>
    public class GetEffectivePermissionsForUserQueryHandler : IQueryHandler<GetEffectivePermissionsForUserQuery, List<Permission>>
    {
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly IUserService userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetEffectivePermissionsForUserQueryHandler"/> class.
        /// </summary>
        /// <param name="tenantRepository">The tenant repository.</param>
        /// <param name="organisationReadModelRepository">The organisation read model repo.</param>
        public GetEffectivePermissionsForUserQueryHandler(
            IUserReadModelRepository userReadModelRepository,
            ICachingResolver cachingResolver,
            IUserService userService)
        {
            this.userReadModelRepository = userReadModelRepository;
            this.cachingResolver = cachingResolver;
            this.userService = userService;
        }

        /// <inheritdoc/>
        public async Task<List<Permission>> Handle(GetEffectivePermissionsForUserQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var user = this.userReadModelRepository.GetUser(request.TenantId, request.UserId);
            EntityHelper.ThrowIfNotFound(user, request.UserId);
            var organisation = await this.cachingResolver.GetOrganisationOrThrow(request.TenantId, user.OrganisationId);
            return await this.userService.GetEffectivePermissions(user, organisation);
        }
    }
}
