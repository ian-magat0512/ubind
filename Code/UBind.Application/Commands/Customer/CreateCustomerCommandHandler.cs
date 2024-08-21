// <copyright file="CreateCustomerCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Services;

    /// <summary>
    /// Creates a customer.
    /// </summary>
    public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, Guid>
    {
        private readonly ICustomerService customerService;

        public CreateCustomerCommandHandler(ICustomerService customerService)
        {
            this.customerService = customerService;
        }

        public async Task<Guid> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var customerAggregate = await this.customerService.CreateCustomerForNewPerson(
                request.TenantId,
                request.Environment,
                request.PersonDetails,
                request.OwnerId,
                request.PortalId,
                request.IsTestData,
                request.AdditionalProperties);
            return customerAggregate.Id;
        }
    }
}
