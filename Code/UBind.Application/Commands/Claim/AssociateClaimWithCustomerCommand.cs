// <copyright file="AssociateClaimWithCustomerCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Claim
{
    using System;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for associating claim with an existing customer record.
    /// </summary>
    public class AssociateClaimWithCustomerCommand : ICommand<Unit>
    {
        public AssociateClaimWithCustomerCommand(Guid tenantId, Guid policyId, Guid customerId)
        {
            this.TenantId = tenantId;
            this.PolicyId = policyId;
            this.CustomerId = customerId;
        }

        public Guid TenantId { get; }

        public Guid PolicyId { get; }

        public Guid CustomerId { get; }
    }
}
