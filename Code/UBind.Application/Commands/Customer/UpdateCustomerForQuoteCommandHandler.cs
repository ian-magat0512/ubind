// <copyright file="UpdateCustomerForQuoteCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer;

using MediatR;
using UBind.Application.Services;
using UBind.Domain.Patterns.Cqrs;

public class UpdateCustomerForQuoteCommandHandler : ICommandHandler<UpdateCustomerForQuoteCommand, Unit>
{
    private readonly IApplicationQuoteService applicationQuoteService;

    public UpdateCustomerForQuoteCommandHandler(IApplicationQuoteService applicationQuoteService)
    {
        this.applicationQuoteService = applicationQuoteService;
    }

    public async Task<Unit> Handle(UpdateCustomerForQuoteCommand command, CancellationToken cancellationToken)
    {
        var aggregate = await this.applicationQuoteService.UpdateCustomerForApplication(
            command.TenantId,
            command.QuoteId,
            command.CustomerId,
            command.CustomerDetails,
            command.PortalId);
        return Unit.Value;
    }
}
