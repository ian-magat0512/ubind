// <copyright file="CreatePersonForCustomerCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Person
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for creating person record.
    /// </summary>
    public class CreatePersonForCustomerCommand : ICommand<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreatePersonForCustomerCommand"/> class.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="personDetails">The person details.</param>
        /// <param name="customerId">The ID of the customer.</param>
        public CreatePersonForCustomerCommand(Guid tenantId, IPersonalDetails personDetails, Guid customerId)
        {
            if (customerId == default)
            {
                throw new ErrorException(Errors.Customer.NotFound(customerId));
            }

            this.TenantId = tenantId;
            this.PersonDetails = personDetails;
            this.CustomerId = customerId;
        }

        /// <summary>
        /// Gets the ID of the customer.
        /// </summary>
        public Guid CustomerId { get; private set; }

        /// <summary>
        /// Gets the ID of the tenant.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the person details.
        /// </summary>
        public IPersonalDetails PersonDetails { get; private set; }
    }
}
