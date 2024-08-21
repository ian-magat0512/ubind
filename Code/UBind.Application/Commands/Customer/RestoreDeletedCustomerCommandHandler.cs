// <copyright file="RestoreDeletedCustomerCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Handler for restoring deleted customer.
    /// </summary>
    public class RestoreDeletedCustomerCommandHandler : ICommandHandler<RestoreDeletedCustomerCommand, Unit>
    {
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;

        public RestoreDeletedCustomerCommandHandler(
            ICustomerAggregateRepository customerAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock)
        {
            this.customerAggregateRepository = customerAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.clock = clock;
        }

        public async Task<Unit> Handle(RestoreDeletedCustomerCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var customerAggregate = this.customerAggregateRepository.GetById(request.TenantId, request.CustomerId);

            if (customerAggregate == null)
            {
                return Unit.Value;
            }

            if (customerAggregate.IsDeleted)
            {
                customerAggregate.MarkAsUndeleted(this.httpContextPropertiesResolver.PerformingUserId, this.clock.GetCurrentInstant());
                await this.customerAggregateRepository.Save(customerAggregate);
            }

            return Unit.Value;
        }
    }
}
