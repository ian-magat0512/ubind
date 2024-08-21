// <copyright file="GetPolicyTransactionByIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Policy.Transaction
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;

    public class GetPolicyTransactionByIdQueryHandler : IQueryHandler<GetPolicyTransactionByIdQuery, PolicyTransaction>
    {
        private readonly IPolicyTransactionReadModelRepository policyTransactionReadModelRepository;

        public GetPolicyTransactionByIdQueryHandler(
            IPolicyTransactionReadModelRepository policyTransactionReadModelRepository)
        {
            this.policyTransactionReadModelRepository = policyTransactionReadModelRepository;
        }

        public Task<PolicyTransaction> Handle(GetPolicyTransactionByIdQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var policyTransaction = this.policyTransactionReadModelRepository.GetById(query.TenantId, query.PolicyTransactionId);
            return Task.FromResult(policyTransaction);
        }
    }
}
