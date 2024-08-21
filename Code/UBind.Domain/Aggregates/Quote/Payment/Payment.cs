// <copyright file="Payment.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.Payment
{
    using System;
    using NodaTime;

    /// <summary>
    /// Record of a payment made for an application.
    /// </summary>
    public class Payment : ApplicationCorollary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Payment"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        public Payment()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Payment"/> class.
        /// </summary>
        /// <param name="formUpdateId">The ID of the form update that was used.</param>
        /// <param name="calculationResultId">The ID of the calculation result that was used.</param>
        /// <param name="createdTimestamp">The time the policy was created.</param>
        public Payment(Guid formUpdateId, Guid calculationResultId, Instant createdTimestamp)
            : base(formUpdateId, calculationResultId, createdTimestamp)
        {
        }
    }
}
