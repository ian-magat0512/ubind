// <copyright file="IQuoteVersionReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;

    /// <summary>
    /// Details of a quote version.
    /// </summary>
    public interface IQuoteVersionReadModelSummary : IEntityReadModel<Guid>
    {
        /// <summary>
        /// Gets the ID of the policy the quote belongs to.
        /// </summary>
        /// TODO: Rename this to PolicyId
        Guid AggregateId { get; }

        /// <summary>
        /// Gets the ID of the organisation this quote was created under.
        /// </summary>
        Guid OrganisationId { get; }

        /// <summary>
        /// Gets the calculation result json for the quote version.
        /// </summary>
        string CalculationResultJson { get; }

        /// <summary>
        /// Gets customer data.
        /// </summary>
        string CustomerAlternativeEmail { get; }

        /// <summary>
        /// Gets customer data.
        /// </summary>
        string CustomerEmail { get; }

        /// <summary>
        /// Gets customer data.
        /// </summary>
        string CustomerFullName { get; }

        /// <summary>
        /// Gets customer data.
        /// </summary>
        string CustomerHomePhone { get; }

        /// <summary>
        /// Gets customer data.
        /// </summary>
        Guid? CustomerId { get; }

        /// <summary>
        /// Gets customer data.
        /// </summary>
        string CustomerMobilePhone { get; }

        /// <summary>
        /// Gets customer data.
        /// </summary>
        Guid? CustomerPersonId { get; }

        /// <summary>
        /// Gets customer data.
        /// </summary>
        string CustomerPreferredName { get; }

        /// <summary>
        /// Gets customer data.
        /// </summary>
        string CustomerWorkPhone { get; }

        /// <summary>
        /// Gets the latest form data for the quote version.
        /// </summary>
        string LatestFormData { get; }

        /// <summary>
        /// Gets owner data.
        /// </summary>
        string OwnerFullName { get; }

        /// <summary>
        /// Gets owner data.
        /// </summary>
        Guid? OwnerPersonId { get; }

        /// <summary>
        /// Gets owner data.
        /// </summary>
        Guid? OwnerUserId { get; }

        /// <summary>
        /// Gets ID of the quote this is a version of.
        /// </summary>
        Guid QuoteId { get; }

        /// <summary>
        /// Gets the quote number of the quote this version is of.
        /// </summary>
        string QuoteNumber { get; }

        /// <summary>
        /// Gets the ID of this quote version.
        /// </summary>
        Guid QuoteVersionId { get; }

        /// <summary>
        /// Gets the version number of this quote version.
        /// </summary>
        int QuoteVersionNumber { get; }

        /// <summary>
        /// Gets the quote state at the time this quote version was created.
        /// </summary>
        string State { get; }

        /// <summary>
        /// Gets the quote workflow step at the time this quote version was created.
        /// </summary>
        string WorkflowStep { get; }
    }
}
