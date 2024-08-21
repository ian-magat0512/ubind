﻿// <copyright file="AssignPortalToCustomerCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for assigning portal to a customer.
    /// </summary>
    [RetryOnDbException(5)]
    public class AssignPortalToCustomerCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssignPortalToCustomerCommand"/> class.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="customerId">The Id of the customer.</param>
        /// <param name="portalId">The Id of the portal ("null" value for unassigned).</param>
        public AssignPortalToCustomerCommand(Guid tenantId, Guid customerId, Guid portalId)
        {
            this.TenantId = tenantId;
            this.CustomerId = customerId;
            this.PortalId = portalId;
        }

        public Guid TenantId { get; }

        /// <summary>
        /// Gets the Id of the customer where to assign the portal to.
        /// </summary>
        public Guid CustomerId { get; }

        /// <summary>
        /// Gets the Id of the portal.
        /// </summary>
        /// <remarks>
        /// Assigning "null" value to this property implies that the customer has no portal assigned to it.
        /// </remarks>
        public Guid PortalId { get; }
    }
}
