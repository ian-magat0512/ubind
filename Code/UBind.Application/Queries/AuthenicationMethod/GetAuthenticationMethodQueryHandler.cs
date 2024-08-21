// <copyright file="GetAuthenticationMethodQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.AuthenicationMethod
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Organisation;

    public class GetAuthenticationMethodQueryHandler
        : IQueryHandler<GetAuthenticationMethodQuery, IAuthenticationMethodReadModelSummary?>
    {

        private readonly IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository;

        public GetAuthenticationMethodQueryHandler(
            IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository)
        {
            this.authenticationMethodReadModelRepository = authenticationMethodReadModelRepository;
        }

        public Task<IAuthenticationMethodReadModelSummary> Handle(
            GetAuthenticationMethodQuery query, CancellationToken cancellationToken)
        {
            IAuthenticationMethodReadModelSummary? result
                = this.authenticationMethodReadModelRepository.Get(query.TenantId, query.AuthenticationMethodId);
            return Task.FromResult(result);
        }
    }
}
