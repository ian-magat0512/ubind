// <copyright file="CustomerFormModelDataPatchGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.CustomerFormDataPatch
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.JsonPatch.Operations;
    using Newtonsoft.Json.Serialization;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.FormDataPatcher;

    /// <summary>
    /// Helper for customer form model data patch where it creates
    /// the patch when invoke operation is performed.
    /// </summary>
    public class CustomerFormModelDataPatchGenerator : ICustomerFormModelDataPatchGenerator
    {
        private readonly IQuoteDatumLocations productQuoteDatumLocation;
        private readonly IQuoteDatumLocations defaultQuoteDatumLocation;
        private readonly IPersonalDetails personalDetails;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerFormModelDataPatchGenerator"/> class.
        /// </summary>
        /// <param name="productQuoteDatumLocation">The product quote datum locations.</param>
        /// <param name="defaultQuoteDatumLocation">The default quote datum locations.</param>
        /// <param name="personalDetails">The personal details.</param>
        public CustomerFormModelDataPatchGenerator(
            IQuoteDatumLocations productQuoteDatumLocation,
            IQuoteDatumLocations defaultQuoteDatumLocation,
            IPersonalDetails personalDetails)
        {
            this.productQuoteDatumLocation = productQuoteDatumLocation;
            this.defaultQuoteDatumLocation = defaultQuoteDatumLocation;
            this.personalDetails = personalDetails;
        }

        /// <inheritdoc/>
        public CascadingFormModelDatumPatchOperationFactory ContactEmail =>
            new CascadingFormModelDatumPatchOperationFactory(
                this.personalDetails.Email,
                this.productQuoteDatumLocation.ContactEmail?.Path,
                this.defaultQuoteDatumLocation.ContactEmail.Path);

        /// <inheritdoc/>
        public CascadingFormModelDatumPatchOperationFactory ContactName =>
            new CascadingFormModelDatumPatchOperationFactory(
               this.personalDetails.DisplayName,
               this.productQuoteDatumLocation.ContactName?.Path,
               this.defaultQuoteDatumLocation.ContactName.Path);

        /// <inheritdoc/>
        public CascadingFormModelDatumPatchOperationFactory ContactMobile =>
            new CascadingFormModelDatumPatchOperationFactory(
                this.personalDetails.MobilePhone,
                this.productQuoteDatumLocation.ContactMobile?.Path,
                this.defaultQuoteDatumLocation.ContactMobile.Path);

        /// <inheritdoc/>
        public CascadingFormModelDatumPatchOperationFactory ContactPhone =>
            new CascadingFormModelDatumPatchOperationFactory(
                this.personalDetails.HomePhone,
                this.productQuoteDatumLocation.ContactPhone?.Path,
                this.defaultQuoteDatumLocation.ContactPhone.Path);

        /// <inheritdoc/>
        public JsonPatchDocument Invoke(IEnumerable<IQuestionMetaData> questionMetaData)
        {
            var operations = new List<Operation>();
            operations.AddIfNotNull(this.ContactName.ToUpsertPatchOperation(questionMetaData));
            operations.AddIfNotNull(this.ContactEmail.ToUpsertPatchOperation(questionMetaData));
            operations.AddIfNotNull(this.ContactMobile.ToUpsertPatchOperation(questionMetaData));
            operations.AddIfNotNull(this.ContactPhone.ToUpsertPatchOperation(questionMetaData));
            return new JsonPatchDocument(operations, new DefaultContractResolver());
        }
    }
}
