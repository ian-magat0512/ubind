// <copyright file="GetAuthenticationMethodSummariesQueryHandler.cs" company="uBind">
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

    public class GetAuthenticationMethodSummariesQueryHandler
        : IQueryHandler<GetAuthenticationMethodSummariesQuery, IList<IAuthenticationMethodReadModelSummary>>
    {
        private readonly IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository;

        public GetAuthenticationMethodSummariesQueryHandler(
            IAuthenticationMethodReadModelRepository authenticationMethodReadModelRepository)
        {
            this.authenticationMethodReadModelRepository = authenticationMethodReadModelRepository;
        }

        public Task<IList<IAuthenticationMethodReadModelSummary>> Handle(
            GetAuthenticationMethodSummariesQuery query, CancellationToken cancellationToken)
        {
            var result = this.authenticationMethodReadModelRepository.GetSummaries(query.TenantId, query.Filters);
            return Task.FromResult(result);
        }
    }
}
