// <copyright file="GetPrincipalAuthenticationDataQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.Principal
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.ExtensionMethods;
    using UBind.Domain.Authentication;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    public class GetPrincipalAuthenticationDataQueryHandler
        : IQueryHandler<GetPrincipalAuthenticationDataQuery, IUserAuthenticationData>
    {
        private readonly IUserSessionService userSessionService;

        public GetPrincipalAuthenticationDataQueryHandler(
            IUserSessionService userSessionService)
        {
            this.userSessionService = userSessionService;
        }

        public async Task<IUserAuthenticationData?> Handle(GetPrincipalAuthenticationDataQuery query, CancellationToken cancellationToken)
        {
            Guid? tenantId = query.Principal.GetTenantIdOrNull();
            if (tenantId == null)
            {
                throw new ErrorException(Domain.Errors.General.NotAuthenticated());
            }

            var userSessionModel = await this.userSessionService.Get(query.Principal);
            if (userSessionModel == null)
            {
                throw new ErrorException(Domain.Errors.General.NotAuthenticated());
            }

            Guid? userId = query.Principal.GetId();
            if (!userId.HasValue)
            {
                throw new ErrorException(Domain.Errors.General.NotAuthenticated());
            }

            return new UserAuthenticationData(
                query.Principal.GetTenantId(),
                query.Principal.GetOrganisationId(),
                query.Principal.GetUserType(),
                userId.Value,
                query.Principal.GetCustomerId(),
                userSessionModel.Permissions);
        }
    }
}
