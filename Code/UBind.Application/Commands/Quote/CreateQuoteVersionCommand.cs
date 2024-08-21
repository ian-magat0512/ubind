// <copyright file="CreateQuoteVersionCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Quote
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using FormData = Domain.Aggregates.Quote.FormData;

    /// <summary>
    /// Represents the command for creating new quote version.
    /// </summary>
    [RetryOnDbException(5)]
    public class CreateQuoteVersionCommand : ICommand<NewQuoteReadModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateQuoteVersionCommand"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productId">The product Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="quoteId">The quote Id.</param>
        /// <param name="formData">The form data.</param>
        public CreateQuoteVersionCommand(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            Guid quoteId,
            FormData formData)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.Environment = environment;
            this.QuoteId = quoteId;
            this.FormData = formData;
        }

        /// <summary>
        /// Gets the tenant Id.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the product Id.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets the deployment environment.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the quote Id.
        /// </summary>
        public Guid QuoteId { get; private set; }

        /// <summary>
        /// Gets the form data.
        /// </summary>
        public FormData FormData { get; private set; }
    }
}
