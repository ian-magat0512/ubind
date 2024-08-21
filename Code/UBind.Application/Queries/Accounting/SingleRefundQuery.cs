﻿// <copyright file="SingleRefundQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Accounting
{
    using System;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Accounting;

    /// <summary>
    ///  A query to return single financial document by its Id.
    /// </summary>
    public class SingleRefundQuery : IQuery<RefundReadModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleRefundQuery"/> class.
        /// </summary>
        /// <param name="refundId">The financial transaction id.</param>
        public SingleRefundQuery(Guid refundId)
        {
            this.Id = refundId;
        }

        /// <summary>
        /// Gets the financial transaction id.
        /// </summary>
        public Guid Id { get; private set; }
    }
}
