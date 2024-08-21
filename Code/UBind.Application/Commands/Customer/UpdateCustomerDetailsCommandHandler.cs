// <copyright file="UpdateCustomerDetailsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Events;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    public class UpdateCustomerDetailsCommandHandler : ICommandHandler<UpdateCustomerDetailsCommand>
    {
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly ICustomerService customerService;
        private readonly ICustomerSystemEventEmitter customerSystemEventEmitter;

        public UpdateCustomerDetailsCommandHandler(
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            ICustomerService customerService,
            ICustomerSystemEventEmitter customerSystemEventEmitter)
        {
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
            this.customerService = customerService;
            this.customerSystemEventEmitter = customerSystemEventEmitter;
        }

        public async Task<Unit> Handle(UpdateCustomerDetailsCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.customerService.UpdateCustomerDetails(
                command.TenantId,
                command.CustomerId,
                command.Details,
                command.PortalId,
                command.AdditionalPropertyValueUpsertModels);
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            await this.customerSystemEventEmitter.CreateAndEmitSystemEvents(
                command.TenantId,
                command.CustomerId,
                new List<SystemEventType> { SystemEventType.CustomerEdited, SystemEventType.CustomerModified },
                performingUserId,
                this.clock.GetCurrentInstant());
            return Unit.Value;
        }
    }
}
