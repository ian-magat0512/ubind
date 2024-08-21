// <copyright file="AssociateQuoteWithCustomerCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote
{
    using System;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// A command that is responsible for associating quote with an existing customer record.
    /// </summary>
    public class AssociateQuoteWithCustomerCommand : ICommand<Unit>
    {
        public AssociateQuoteWithCustomerCommand(Guid tenantId, Guid quoteId, Guid customerId)
        {
            this.TenantId = tenantId;
            this.QuoteId = quoteId;
            this.CustomerId = customerId;
        }

        public Guid TenantId { get; }

        public Guid QuoteId { get; }

        public Guid CustomerId { get; }
    }
}
