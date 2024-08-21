// <copyright file="IBaseReportReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    /// <summary>
    /// Base Report Read Model.
    /// </summary>
    public interface IBaseReportReadModel
    {
        /// <summary>
        /// Gets the gateway used for payment.
        /// </summary>
        string PaymentGateway { get; }

        /// <summary>
        /// Gets the payment details.
        /// </summary>
        string PaymentResponseJson { get; }

        /// <summary>
        /// Gets the latest calculation result json.
        /// </summary>
        string SerializedLatestCalculationResult { get; }
    }
}
