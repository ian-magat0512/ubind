// <copyright file="IPaymentMethodDetailsModel{TPaymentMethodDetails}.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using UBind.Application.Services.Encryption;
    using UBind.Domain.Aggregates.Quote.Payment;

    /// <summary>
    /// Interface for payment details models.
    /// </summary>
    /// <typeparam name="TPaymentMethodDetails">The type of the payment method details.</typeparam>
    public interface IPaymentMethodDetailsModel<TPaymentMethodDetails>
        where TPaymentMethodDetails : IPaymentMethodDetails
    {
#pragma warning disable 1723
        /// <summary>
        /// Create a new instance of <see cref="TPaymentMethodDetails"/>, with the data from this model.
        /// </summary>
        /// <param name="encryptionService">The encryption/decryption service.</param>
        /// <returns>A new instance of <see cref="TPaymentMethodDetails"/>, with the data from this model.</returns>
        TPaymentMethodDetails Map(IAsymmetricEncryptionService encryptionService);
    }
#pragma warning restore 1723
}
