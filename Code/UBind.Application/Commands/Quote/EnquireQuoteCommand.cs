// <copyright file="EnquireQuoteCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote
{
    using System;
    using UBind.Domain.Patterns.Cqrs;
    using FormData = Domain.Aggregates.Quote.FormData;

    /// <summary>
    /// Represents the command for lodging a quote enquiry.
    /// </summary>
    public class EnquireQuoteCommand : ICommand
    {
        public EnquireQuoteCommand(
            Guid tenantId,
            Guid productId,
            Guid quoteId,
            FormData formData)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.QuoteId = quoteId;
            this.FormData = formData;
        }

        public Guid TenantId { get; private set; }

        public Guid ProductId { get; private set; }

        public Guid QuoteId { get; private set; }

        public FormData FormData { get; private set; }
    }
}
