// <copyright file="AssociateQuoteWithCustomerUserCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Represents the command for associating a quote to a customer with a user account.
    /// </summary>
    public class AssociateQuoteWithCustomerUserCommand : ICommand
    {
        public AssociateQuoteWithCustomerUserCommand(Guid tenantId, Guid associationInvitationId, Guid quoteId, Guid performingUserId)
        {
            this.TenantId = tenantId;
            this.AssociationInvitationId = associationInvitationId;
            this.QuoteId = quoteId;
            this.PerformingUserId = performingUserId;
        }

        public Guid TenantId { get; private set; }

        public Guid AssociationInvitationId { get; private set; }

        public Guid QuoteId { get; private set; }

        public Guid PerformingUserId { get; private set; }
    }
}
