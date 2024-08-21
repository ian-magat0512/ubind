// <copyright file="QuoteSearchIndexWriteModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Search
{
    using System;
    using UBind.Domain;

    /// <summary>
    /// For projecting quote lucene write models from the database.
    /// </summary>
    public class QuoteSearchIndexWriteModel : EntitySearchIndexWriteModel, IQuoteSearchIndexWriteModel
    {
        /// <inheritdoc/>
        public Guid? CustomerId { get; set; }

        /// <inheritdoc/>
        public Guid OrganisationId { get; set; }

        /// <inheritdoc/>
        public Guid ProductId { get; set; }

        /// <inheritdoc/>
        public string QuoteTitle { get; set; }

        /// <inheritdoc/>
        public string QuoteNumber { get; set; }

        /// <inheritdoc/>
        public string FormDataJson { get; set; }

        /// <inheritdoc/>
        public Guid? OwnerUserId { get; set; }

        /// <inheritdoc/>
        public Guid? OwnerPersonId { get; set; }

        /// <inheritdoc/>
        public string OwnerFullname { get; set; }

        /// <inheritdoc/>
        public string CustomerFullname { get; set; }

        /// <inheritdoc/>
        public string CustomerPreferredName { get; set; }

        /// <inheritdoc/>
        public string CustomerEmail { get; set; }

        /// <inheritdoc/>
        public string CustomerAlternativeEmail { get; set; }

        /// <inheritdoc/>
        public string CustomerHomePhone { get; set; }

        /// <inheritdoc/>
        public string CustomerWorkPhone { get; set; }

        /// <inheritdoc/>
        public string CustomerMobilePhone { get; set; }

        /// <inheritdoc/>
        public string QuoteState { get; set; }

        /// <inheritdoc/>
        public QuoteType QuoteType { get; set; }

        /// <inheritdoc/>
        public bool IsDiscarded { get; set; }

        /// <inheritdoc/>
        public bool IsTestData { get; set; }

        /// <inheritdoc/>
        public long? ExpiryTicksSinceEpoch { get; set; }

        public string ProductName { get; set; }

        /// <inheritdoc/>
        public string PolicyNumber { get; set; }
    }
}
