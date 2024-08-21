// <copyright file="SetPersonAsPrimaryForCustomerCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Person
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for assigning the person as primary for customer.
    /// </summary>
    public class SetPersonAsPrimaryForCustomerCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetPersonAsPrimaryForCustomerCommand"/> class.
        /// </summary>
        [RetryOnDbException(5)]
        public SetPersonAsPrimaryForCustomerCommand(Guid tenantId, Guid customerId, Guid personId)
        {
            this.TenantId = tenantId;
            this.CustomerId = customerId;
            this.PersonId = personId;
        }

        /// <summary>
        /// Gets the ID of the tenant.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the ID of the customer.
        /// </summary>
        public Guid CustomerId { get; private set; }

        /// <summary>
        /// Gets the ID of the person.
        /// </summary>
        public Guid PersonId { get; private set; }
    }
}
