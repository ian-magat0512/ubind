// <copyright file="AllRefundsByPayerQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Accounting
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Accounting;

    /// <summary>
    /// Query handler for getting all refund for accounting purposes.
    /// </summary>
    public class AllRefundsByPayerQueryHandler : IQueryHandler<AllRefundsByPayerQuery, IEnumerable<RefundReadModel>>
    {
        private readonly IFinancialTransactionReadModelRepository<RefundReadModel> readModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AllRefundsByPayerQueryHandler"/> class.
        /// </summary>
        /// <param name="readModelRepository">The readmodel repository.</param>
        public AllRefundsByPayerQueryHandler(
            IFinancialTransactionReadModelRepository<RefundReadModel> readModelRepository)
        {
            this.readModelRepository = readModelRepository;
        }

        /// <summary>
        /// Handle the query for refunds by given payer.
        /// </summary>
        /// <param name="query">The query for getting all Refundsby payer.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The refunds collection.</returns>
        public Task<IEnumerable<RefundReadModel>> Handle(AllRefundsByPayerQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(this.readModelRepository.GetAllForPayer(query.PayerId, query.PayerType).ToList().AsEnumerable());
        }
    }
}
