// <copyright file="IClaimReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Claim
{
    using System;
    using NodaTime;

    /// <summary>
    /// Read model for claims.
    /// </summary>
    public interface IClaimReadModel : IEntityReadModel<Guid>
    {
        /// <summary>
        /// Gets the amount of the claim, if known, otherwise null.
        /// </summary>
        decimal? Amount { get; }

        /// <summary>
        /// Gets the claim number if known, otherwise null.
        /// </summary>
        string ClaimNumber { get; }

        /// <summary>
        /// Gets the claim reference.
        /// </summary>
        string ClaimReference { get; }

        /// <summary>
        /// Gets the full name of the customer the claim belongs to.
        /// </summary>
        string CustomerFullName { get; }

        /// <summary>
        /// Gets the ID of the customer the claim belongs to.
        /// </summary>
        Guid? CustomerId { get; }

        /// <summary>
        /// Gets the preferred name of the customer the claim belongs to.
        /// </summary>
        string CustomerPreferredName { get; }

        /// <summary>
        /// Gets a description of the claim if known, otherwise null.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the environment the claim is in.
        /// </summary>
        DeploymentEnvironment Environment { get; }

        LocalDateTime? IncidentDateTime { get; }

        /// <summary>
        /// Gets the timestamp of the incident the claim is for if known, otherwise null.
        /// </summary>
        Instant? IncidentTimestamp { get; }

        /// <summary>
        /// Gets the latest calculation result.
        /// </summary>
        IClaimCalculationResultReadModel LatestCalculationResult { get; }

        /// <summary>
        /// Gets the ID of the form data used for the last calculation.
        /// </summary>
        Guid LatestCalculationResultFormDataId { get; }

        /// <summary>
        /// Gets the ID of the latest calculation result.
        /// </summary>
        Guid LatestCalculationResultId { get; }

        /// <summary>
        /// Gets the latest form data for the claim.
        /// </summary>
        string LatestFormData { get; }

        /// <summary>
        /// Gets the ID of the person who owns the claim.
        /// </summary>
        Guid? PersonId { get; }

        /// <summary>
        /// Gets the policy number for the policy the claim is under.
        /// </summary>
        string? PolicyNumber { get; }

        /// <summary>
        /// Gets the ID of the product the claim relates to.
        /// </summary>
        Guid ProductId { get; }

        /// <summary>
        /// Gets the current state of the claim.
        /// </summary>
        string Status { get; }

        /// <summary>
        /// Gets the ID of the organisation the claim belongs to.
        /// </summary>
        Guid OrganisationId { get; }

        /// <summary>
        /// Gets the current workflow step for the claim.
        /// </summary>
        string WorkflowStep { get; }

        /// <summary>
        /// Gets the ID of the policy the claim pertains to.
        /// </summary>
        Guid? PolicyId { get; }

        public LocalDateTime? LodgedDateTime { get; }

        /// <summary>
        ///  Gets the timestamp claim is lodged.
        /// </summary>
        public Instant? LodgedTimestamp { get; }

        public LocalDateTime? SettledDateTime { get; }

        /// <summary>
        ///  Gets the timestamp the claim state chanegs to complete.
        /// </summary>
        public Instant? SettledTimestamp { get; }

        public LocalDateTime? DeclinedDateTime { get; }

        /// <summary>
        ///  Gets the timestamp the claim is declined.
        /// </summary>
        public Instant? DeclinedTimestamp { get; }
    }
}
