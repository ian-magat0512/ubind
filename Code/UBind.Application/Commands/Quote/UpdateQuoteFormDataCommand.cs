// <copyright file="UpdateQuoteFormDataCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote
{
    using System;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Patterns.Cqrs;

    public class UpdateQuoteFormDataCommand : ICommand
    {
        public UpdateQuoteFormDataCommand(Guid tenantId, Guid quoteId, FormData formData)
        {
            this.TenantId = tenantId;
            this.QuoteId = quoteId;
            this.FormData = formData;
        }

        public Guid TenantId { get; }

        public Guid QuoteId { get; }

        public FormData FormData { get; }
    }
}
