// <copyright file="IClaimReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Claim
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Data transfer object for quotereadmodel
    /// for a specific product.
    /// </summary>
    public interface IClaimReadModelSummary : IEntityReadModel<Guid>
    {
        /// <summary>
        /// Gets or sets the product details.
        /// </summary>
        string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the organisation the claim relates to.
        /// </summary>
        Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the product the claim relates to.
        /// </summary>
        Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets the environment the claim belongs to.
        /// </summary>
        DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets the ID of the policy the claim pertains to.
        /// </summary>
        Guid? PolicyId { get; set; }

        /// <summary>
        /// Gets or sets the policy number of the policy the claim relates to.
        /// </summary>
        string PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets the ID of the customer the quote and claim are for.
        /// </summary>
        Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the person (customer) the quote and claim are for.
        /// </summary>
        Guid? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the full name of the customer the claim is for.
        /// </summary>
        string CustomerFullName { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who owns the claim.
        /// </summary>
        Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the preferred name of the customer the claim is for.
        /// </summary>
        string CustomerPreferredName { get; set; }

        /// <summary>
        /// Gets or sets the customer owner user id.
        /// </summary>
        Guid? CustomerOwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets gets the Claim reference number.
        /// </summary>
        string ClaimReference { get; set; }

        /// <summary>
        /// Gets or sets gets the Claim reference number.
        /// </summary>
        string ClaimNumber { get; set; }

        /// <summary>
        /// Gets or sets the claim amount.
        /// </summary>
        decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the claim description.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets the incident date of the claim.
        /// </summary>
        LocalDateTime? IncidentDateTime { get; }

        Instant? IncidentTimestamp { get; }

        /// <summary>
        /// Gets a value indicating whether claim is a test data.
        /// </summary>
        bool IsTestData { get; }

        /// <summary>
        /// Gets or sets the claim status.
        /// </summary>
        string Status { get; set; }

        /// <summary>
        /// Gets the ID of the latest calculation result.
        /// </summary>
        Guid LatestCalculationResultId { get; }

        /// <summary>
        /// Gets or sets the claim workflow step.
        /// </summary>
        public string WorkflowStep { get; set; }

        /// <summary>
        /// Gets the latest calculation result for the claim.
        /// </summary>
        IClaimCalculationResultReadModel LatestCalculationResult { get; }

        /// <summary>
        /// Gets the ID of the form data used for the latest calculation result.
        /// </summary>
        Guid LatestCalculationResultFormDataId { get; }

        /// <summary>
        /// Gets the latest form data.
        /// </summary>
        string LatestFormData { get; }

        /// <summary>
        /// Gets the timezone which local date times are to be intepreteted using.
        /// </summary>
        DateTimeZone TimeZone { get; }

        /// <summary>
        /// Gets or sets the documents associated with this claim.
        /// </summary>
        IEnumerable<ClaimAttachmentReadModel> Documents { get; set; }

        /// <summary>
        /// Gets the latest form data, generating it on demand if missing.
        /// </summary>
        /// <returns>The latest form data serialized as a json string.</returns>
        string GetFormData();
    }

    /// <summary>
    /// Extension methods for the <see cref="IClaimReadModelSummary"/> interface.
    /// </summary>
    public static class IClaimReadModelSummaryExtensions
    {
        /// <summary>
        /// Get the product context for the claim.
        /// </summary>
        /// <param name="summary">The claim.</param>
        /// <returns>The product context.</returns>
        public static ProductContext GetProductContext(this IClaimReadModelSummary summary) =>
            new ProductContext(summary.TenantId, summary.ProductId, summary.Environment);
    }
}
