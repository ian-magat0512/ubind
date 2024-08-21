// <copyright file="QuoteVersionReadModelSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Quote
{
    using System;
    using UBind.Domain;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// For representing details of quote versions.
    /// </summary>
    public class QuoteVersionReadModelSummary : EntityReadModel<Guid>, IQuoteVersionReadModelSummary
    {
        private Guid aggregateId;
        private Guid quoteVersionId;

        /// <summary>
        /// Gets or sets the ID of the quote version.
        /// </summary>
        public Guid QuoteVersionId
        {
            get => this.quoteVersionId == default
                ? this.Id
                : this.quoteVersionId;

            set => this.quoteVersionId = value;
        }

        /// <summary>
        /// Gets or sets the ID of the aggregate the quote belongs to.
        /// </summary>
        public Guid AggregateId
        {
            get => this.aggregateId != default ? this.aggregateId : this.QuoteId;
            set => this.aggregateId = value;
        }

        /// <summary>
        /// Gets or sets the ID of the organisation this quote was created under.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the quote's ID.
        /// </summary>
        public Guid QuoteId { get; set; }

        /// <summary>
        /// Gets or sets the Version Number for the current quote.
        /// </summary>
        public int QuoteVersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the latest form data for the quote.
        /// </summary>
        public string LatestFormData { get; set; }

        /// <summary>
        /// Gets or sets the ID of the customer the quote is assigned to if any, otherwise default.
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the person who is the customer for the quote if any, otherwise default.
        /// </summary>
        public Guid? CustomerPersonId { get; set; }

        /// <summary>
        /// Gets or sets the Customer's full name.
        /// </summary>
        public string CustomerFullName { get; set; }

        /// <summary>
        /// Gets or sets the Customer's preferred name.
        /// </summary>
        public string CustomerPreferredName { get; set; }

        /// <summary>
        /// Gets or sets the customer's email address.
        /// </summary>
        public string CustomerEmail { get; set; }

        /// <summary>
        /// Gets or sets the customer's alternative email address.
        /// </summary>
        public string CustomerAlternativeEmail { get; set; }

        /// <summary>
        /// Gets or sets the customer's mobile phone number.
        /// </summary>
        public string CustomerMobilePhone { get; set; }

        /// <summary>
        /// Gets or sets the customer's home phone number.
        /// </summary>
        public string CustomerHomePhone { get; set; }

        /// <summary>
        /// Gets or sets the customer's work phone number.
        /// </summary>
        public string CustomerWorkPhone { get; set; }

        /// <summary>
        /// Gets or sets the quote number.
        /// </summary>
        public string QuoteNumber { get; set; }

        /// <summary>
        /// Gets or sets the user ID of the current owner of the quote.
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the person who owns this quote.
        /// </summary>
        public Guid? OwnerPersonId { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person who owns this quote.
        /// </summary>
        public string OwnerFullName { get; set; }

        /// <summary>
        /// Gets or sets the calculation result json string related to the quote version.
        /// </summary>
        public string CalculationResultJson { get; set; }

        /// <inheritdoc/>
        public string State { get; set; }

        /// <inheritdoc/>
        public string WorkflowStep { get; set; }
    }
}
