// <copyright file="CreateCustomerForQuoteCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Customer;

using UBind.Domain.Aggregates.Person;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;

public class CreateCustomerForQuoteCommand : ICommand<NewQuoteReadModel>
{
    public CreateCustomerForQuoteCommand(Guid tenantId, Guid quoteId, IPersonalDetails customerDetails, Guid? portalId)
    {
        this.TenantId = tenantId;
        this.QuoteId = quoteId;
        this.CustomerDetails = customerDetails;
        this.PortalId = portalId;
    }

    public Guid TenantId { get; }

    public Guid QuoteId { get; }

    public IPersonalDetails CustomerDetails { get; }

    public Guid? PortalId { get; }
}
