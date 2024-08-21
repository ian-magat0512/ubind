// <copyright file="RefundReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Accounting;
    using UBind.Domain.Repositories;
    using UBind.Domain.ValueTypes;

    /// <inheritdoc/>
    public class RefundReadModelRepository : IFinancialTransactionReadModelRepository<RefundReadModel>
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefundReadModelRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The db context.</param>
        public RefundReadModelRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public IEnumerable<RefundReadModel> GetAllForPayer(Guid payerId, TransactionPartyType payerType)
        {
            return this.dbContext.RefundReadModels.Where(x => x.PayerId == payerId && x.PayerType == payerType);
        }

        /// <inheritdoc/>
        public RefundReadModel GetById(Guid id)
        {
            return this.dbContext.RefundReadModels.Find(id);
        }
    }
}
