// <copyright file="GetPolicyByIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Policy
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;

    public class GetPolicyByIdQueryHandler : IQueryHandler<GetPolicyByIdQuery, PolicyReadModel>
    {
        private readonly IPolicyReadModelRepository policyReadModelRepository;

        public GetPolicyByIdQueryHandler(
            IPolicyReadModelRepository policyReadModelRepository)
        {
            this.policyReadModelRepository = policyReadModelRepository;
        }

        public Task<PolicyReadModel> Handle(GetPolicyByIdQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var policy = this.policyReadModelRepository.GetById(query.TenantId, query.PolicyId);
            return Task.FromResult(policy);
        }
    }
}
