// <copyright file="SinglePaymentQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Accounting
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Accounting;

    /// <summary>
    /// Query handler for getting all payment for accounting purposes.
    /// </summary>
    public class SinglePaymentQueryHandler : IQueryHandler<SinglePaymentQuery, PaymentReadModel>
    {
        private readonly IFinancialTransactionReadModelRepository<PaymentReadModel> readModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SinglePaymentQueryHandler"/> class.
        /// </summary>
        /// <param name="readModelRepository">The readmodel repository.</param>
        public SinglePaymentQueryHandler(IFinancialTransactionReadModelRepository<PaymentReadModel> readModelRepository)
        {
            this.readModelRepository = readModelRepository;
        }

        /// <summary>
        /// Handles getting a financial transaction by id.
        /// </summary>
        /// <param name="query">The query for getting single financial transaction.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The financial transaction.</returns>
        public Task<PaymentReadModel> Handle(SinglePaymentQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(this.readModelRepository.GetById(query.Id));
        }
    }
}
