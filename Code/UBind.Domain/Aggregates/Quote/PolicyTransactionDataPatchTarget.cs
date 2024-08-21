﻿// <copyright file="PolicyTransactionDataPatchTarget.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Specifies a policy transaction to patch.
    /// </summary>
    public class PolicyTransactionDataPatchTarget : DataPatchTargetEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyTransactionDataPatchTarget"/> class.
        /// </summary>
        /// <param name="transactionId">The ID o the transaction to patch.</param>
        public PolicyTransactionDataPatchTarget(Guid transactionId)
        {
            this.TransactionId = transactionId;
        }

        /// <summary>
        /// Gets the ID of the transaction to patch.
        /// </summary>
        [JsonProperty]
        public Guid TransactionId { get; private set; }
    }
}
