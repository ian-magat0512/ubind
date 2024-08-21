// <copyright file="QuoteDocumentReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Read model for documents attached to a quote or policy.
    /// </summary>
    public class QuoteDocumentReadModel : Aggregates.Quote.QuoteDocument, IReadModel<Guid>
    {
        private Guid quoteOrPolicyTransactionId;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteDocumentReadModel"/> class.
        /// Parameterless constructor for EF.
        /// </summary>
        protected QuoteDocumentReadModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteDocumentReadModel"/> class.
        /// Parameterless constructor for EF.
        /// </summary>
        /// <param name="id">The document Id.</param>
        protected QuoteDocumentReadModel(Guid id)
        {
            this.Id = id;
        }

        private QuoteDocumentReadModel(Guid policyId, Guid quoteOrPolicyTransactionId, DocumentOwnerType ownerType, Aggregates.Quote.QuoteDocument document)
            : base(document.Name, document.Type, document.SizeInBytes, document.FileContentId, document.CreatedTimestamp)
        {
            this.PolicyId = policyId;
            this.QuoteOrPolicyTransactionId = quoteOrPolicyTransactionId;
            this.OwnerType = ownerType;
        }

        /// <summary>
        /// Gets the ID of the policy the document belongs to.
        /// </summary>
        public Guid PolicyId { get; private set; }

        /// <summary>
        /// Gets or sets the environment where the document is created.
        /// </summary>
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets the Id of tenant the document belongs to.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the Id of organisation the document belongs to.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return a test data.
        /// </summary>
        public bool IsTestData { get; set; }

        /// <summary>
        /// Gets or sets the Id of customer the document belongs to.
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets the ID of the quote or policy transaction the document relates to.
        /// </summary>
        public Guid QuoteOrPolicyTransactionId
        {
            get
            {
                if (this.quoteOrPolicyTransactionId == default)
                {
                    return this.PolicyId;
                }

                return this.quoteOrPolicyTransactionId;
            }

            private set
            {
                this.quoteOrPolicyTransactionId = value;
            }
        }

        /// <summary>
        /// Gets the type of entity the document belongs to.
        /// </summary>
        public DocumentOwnerType OwnerType { get; private set; }

        /// <summary>
        /// Creates a quote document read model for a document that belongs to a quote.
        /// </summary>
        /// <param name="policyId">The Id of the policy the document relates to.</param>
        /// <param name="quoteId">The ID of the quote.</param>
        /// <param name="document">The document.</param>
        /// <returns>A new instance of <see cref="QuoteDocumentReadModel"/>.</returns>
        public static QuoteDocumentReadModel CreateQuoteDocumentReadModel(Guid policyId, Guid quoteId, Aggregates.Quote.QuoteDocument document)
        {
            return new QuoteDocumentReadModel(policyId, quoteId, DocumentOwnerType.Quote, document);
        }

        /// <summary>
        /// Creates a quote document read model for a document that belongs to a quote version.
        /// </summary>
        /// <param name="policyId">The Id of the policy the document relates to.</param>
        /// <param name="quoteVersionId">The ID of the quote version.</param>
        /// <param name="document">The document.</param>
        /// <returns>A new instance of <see cref="QuoteDocumentReadModel"/>.</returns>
        public static QuoteDocumentReadModel CreateQuoteVersionDocumentReadModel(Guid policyId, Guid quoteVersionId, Aggregates.Quote.QuoteDocument document)
        {
            return new QuoteDocumentReadModel(policyId, quoteVersionId, DocumentOwnerType.QuoteVersion, document);
        }

        /// <summary>
        /// Creates a quote document read model for a document that belongs to a policy.
        /// </summary>
        /// <param name="policyId">The Id of the policy the document relates to.</param>
        /// <param name="policyTransactionId">The ID of the policy transaction the document relates to.</param>
        /// <param name="document">The document.</param>
        /// <returns>A new instance of <see cref="QuoteDocumentReadModel"/>.</returns>
        public static QuoteDocumentReadModel CreatePolicyDocumentReadModel(
            Guid policyId, Guid policyTransactionId, Aggregates.Quote.QuoteDocument document)
        {
            return new QuoteDocumentReadModel(policyId, policyTransactionId, DocumentOwnerType.Policy, document);
        }

        /// <summary>
        /// Update the read model with the latest document.
        /// </summary>
        /// <param name="document">The new document.</param>
        public void Update(Aggregates.Quote.QuoteDocument document)
        {
            Contract.Assert(document.Name == this.Name);
            this.SizeInBytes = document.SizeInBytes;
            this.FileContentId = document.FileContentId;
            this.CreatedTimestamp = document.CreatedTimestamp;
        }
    }
}
