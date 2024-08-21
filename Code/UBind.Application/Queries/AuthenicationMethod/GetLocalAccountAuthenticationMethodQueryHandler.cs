// <copyright file="GetLocalAccountAuthenticationMethodQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.AuthenicationMethod
{
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Organisation;

    public class GetLocalAccountAuthenticationMethodQueryHandler
        : IQueryHandler<GetLocalAccountAuthenticationMethodQuery, LocalAccountAuthenticationMethodReadModel>
    {
        private readonly IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository;

        public GetLocalAccountAuthenticationMethodQueryHandler(
            IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository)
        {
            this.authenticationMethodReadModelRepository = authenticationMethodReadModelRepository;
        }

        public Task<LocalAccountAuthenticationMethodReadModel> Handle(
            GetLocalAccountAuthenticationMethodQuery request,
            CancellationToken cancellationToken)
        {
            var record = this.authenticationMethodReadModelRepository.GetLocalAccountRecordForOrganisation(
                request.TenantId, request.OrganisationId);

            if (record == null)
            {
                record = new LocalAccountAuthenticationMethodReadModel
                {
                    Id = default, // this is not a real record, so we can't give it a real ID.
                    TenantId = request.TenantId,
                    OrganisationId = request.OrganisationId,
                    AllowCustomerSelfRegistration = false,
                    TypeName = AuthenticationMethodType.LocalAccount.Humanize(),
                    Name = "Local Account",
                    IncludeSignInButtonOnPortalLoginPage = true,
                    Disabled = false,
                };
            }

            return Task.FromResult(record);
        }
    }
}
