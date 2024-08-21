// <copyright file="ISavedPaymentMethodRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Entities;

    public interface ISavedPaymentMethodRepository
    {
        /// <summary>
        /// Retrieve the saved payment method by the given Id.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant that owns the customer to whom the payment method is for.</param>
        /// <param name="savedPaymentId">The ID of the saved payment method.</param>
        /// <returns>An instance of <see cref="SavedPaymentMethod"/>.</returns>
        /// <remarks>Returned instance should only be used for transactions. DO NOT use this method if intended to display.</remarks>
        SavedPaymentMethod GetByIdForPayment(Guid tenantId, Guid savedPaymentId);

        /// <summary>
        /// Retrieve the saved payment method by the given Id.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant that owns the customer to whom the payment method is for.</param>
        /// <param name="savedPaymentId">The ID of the saved payment method.</param>
        /// <returns>An instance of <see cref="SavedPaymentMethod"/>.</returns>
        /// <remarks>Returned instance should only be used for displaying and not for transactions.</remarks>
        SavedPaymentMethod GetByIdForDisplay(Guid tenantId, Guid savedPaymentId);

        /// <summary>
        /// Retrieves the list of saved payment methods for a given customer.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the customer owning the saved payment methods are for.</param>
        /// <param name="customerId">The ID of the customer who owns the saved payment methods.</param>
        /// <returns>A collection of <see cref="SavedPaymentMethod"/>.</returns>
        /// <remarks>Returned instance should only be used for transactions. DO NOT use this method if intended to display.</remarks>
        IEnumerable<SavedPaymentMethod> GetSavedPaymentMethodsForPaymentByCustomer(Guid tenantId, Guid customerId);

        /// <summary>
        /// Add a new saved payment method.
        /// </summary>
        /// <param name="paymentMethod">The saved payment method to be persisted.</param>
        /// <returns>The saved payment method that has been persisted.</returns>
        SavedPaymentMethod Insert(SavedPaymentMethod paymentMethod);

        /// <summary>
        /// Deletes a saved payment method.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the saved payment method is for.</param>
        /// <param name="customerId">The Id of the customer the saved payment method is for.</param>
        /// <param name="savedPaymentMethodId">The ID of the saved payment method for deletion.</param>
        void Delete(Guid tenantId, Guid customerId, Guid savedPaymentMethodId);

        /// <summary>
        /// Save any changes to saved payment methods.
        /// </summary>
        void SaveChanges();
    }
}
