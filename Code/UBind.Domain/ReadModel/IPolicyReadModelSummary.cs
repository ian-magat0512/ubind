// <copyright file="IPolicyReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using NodaTime;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Policy summary data for use in policy lists.
    /// </summary>
    public interface IPolicyReadModelSummary : IPolicy, IPolicyOwnershipDetails, IEntityReadModel<Guid>
    {
        /// <summary>
        /// Gets Quote Id.
        /// </summary>
        Guid? QuoteId { get; }

        /// <summary>
        /// Gets the policy title.
        /// </summary>
        string PolicyTitle { get; }

        /// <summary>
        /// Gets the policy state.
        /// </summary>
        string PolicyState { get; }

        /// <summary>
        /// Gets quote number.
        /// </summary>
        string QuoteNumber { get; }

        /// <summary>
        /// Gets or sets invoice number.
        /// </summary>
        string InvoiceNumber { get; set; }

        /// <summary>
        /// Gets or sets invoice time.
        /// </summary>
        Instant InvoiceTimestamp { get; set; }

        /// <summary>
        /// Gets or sets quote submission time.
        /// </summary>
        Instant SubmissionTimestamp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether quote has been invoice.
        /// </summary>
        bool IsInvoiced { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether quote is paid for.
        /// </summary>
        bool IsPaidFor { get; set; }

        /// <summary>
        /// Gets or sets the product feature setting.
        /// </summary>
        ProductFeatureSetting ProductFeatureSetting { get; set; }

        /// <summary>
        /// Gets policy number.
        /// </summary>
        string PolicyNumber { get; }

        /// <summary>
        /// Gets a value indicating whether it has been submitted.
        /// </summary>
        bool IsSubmitted { get; }

        /// <summary>
        /// Gets a value indicating whether the quote is test data.
        /// </summary>
        bool IsTestData { get; }

        /// <summary>
        /// Gets or sets product name.
        /// </summary>
        string ProductName { get; set; }

        /// <summary>
        /// Gets customer full name.
        /// </summary>
        string CustomerFullName { get; }

        /// <summary>
        /// Gets customer preferredName.
        /// </summary>
        string CustomerPreferredName { get; }

        /// <summary>
        /// Gets the quote's status.
        /// </summary>
        string QuoteState { get; }

        /// <summary>
        /// Gets the policy issue date time.
        /// </summary>
        Instant IssuedTimestamp { get; }

        /// <summary>
        /// Gets the policy latest renewal effective date time.
        /// </summary>
        Instant? LatestRenewalEffectiveTimestamp { get; }

        /// <summary>
        /// Gets quote number.
        /// </summary>
        QuoteType QuoteType { get; }

        /// <summary>
        /// Gets the deployment environment.
        /// </summary>
        DeploymentEnvironment Environment { get; }

        /// <summary>
        /// Gets a string containing the serialized calculation result used for the policy.
        /// </summary>
        string SerializedCalculationResult { get; }

        /// <summary>
        /// Gets the calculation result used for the policy.
        /// </summary>
        CalculationResultReadModel CalculationResult { get; }
    }
}
