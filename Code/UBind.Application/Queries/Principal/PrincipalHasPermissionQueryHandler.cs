// <copyright file="PrincipalHasPermissionQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.Principal
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    public class PrincipalHasPermissionQueryHandler : IQueryHandler<PrincipalHasPermissionQuery, bool>
    {
        private readonly IUserSessionService userSessionService;

        public PrincipalHasPermissionQueryHandler(
            IUserSessionService userSessionService)
        {
            this.userSessionService = userSessionService;
        }

        public async Task<bool> Handle(PrincipalHasPermissionQuery query, CancellationToken cancellationToken)
        {
            Guid? tenantId = query.Principal.GetTenantIdOrNull();
            if (tenantId == null)
            {
                return false;
            }

            var userSessionModel = await this.userSessionService.Get(query.Principal);
            if (userSessionModel == null)
            {
                return false;
            }

            return userSessionModel.Permissions.Contains(query.Permission);
        }
    }
}
