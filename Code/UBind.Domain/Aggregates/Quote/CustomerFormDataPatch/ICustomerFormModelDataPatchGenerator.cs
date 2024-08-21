// <copyright file="ICustomerFormModelDataPatchGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.CustomerFormDataPatch
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.JsonPatch;
    using UBind.Domain;
    using UBind.Domain.FormDataPatcher;

    /// <summary>
    /// Interface for a customer data patch where it creates
    /// patch for form data when invoke operation is performed.
    /// </summary>
    public interface ICustomerFormModelDataPatchGenerator
    {
        /// <summary>
        /// Gets the patcher for contact email.
        /// </summary>
        CascadingFormModelDatumPatchOperationFactory ContactEmail { get; }

        /// <summary>
        /// Gets the patcher for contact mobile.
        /// </summary>
        CascadingFormModelDatumPatchOperationFactory ContactMobile { get; }

        /// <summary>
        /// Gets the patcher for contact name.
        /// </summary>
        CascadingFormModelDatumPatchOperationFactory ContactName { get; }

        /// <summary>
        /// Gets the patcher for contact phone.
        /// </summary>
        CascadingFormModelDatumPatchOperationFactory ContactPhone { get; }

        /// <summary>
        /// Creates the json patch for form data.
        /// </summary>
        /// <param name="questionMetaData">The question metadata.</param>
        /// <returns>A form data pacth object.</returns>
        JsonPatchDocument Invoke(IEnumerable<IQuestionMetaData> questionMetaData);
    }
}
