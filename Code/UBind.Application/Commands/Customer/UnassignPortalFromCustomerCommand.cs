// <copyright file="UnassignPortalFromCustomerCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for unassigning portal to a customer.
    /// </summary>
    [RetryOnDbException(5)]
    public class UnassignPortalFromCustomerCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnassignPortalFromCustomerCommand"/> class.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="customerId">The Id of the customer.</param>
        public UnassignPortalFromCustomerCommand(Guid tenantId, Guid customerId)
        {
            this.TenantId = tenantId;
            this.CustomerId = customerId;
        }

        public Guid TenantId { get; }

        /// <summary>
        /// Gets the Id of the customer where to assign the portal to.
        /// </summary>
        public Guid CustomerId { get; }
    }
}
