// <copyright file="CreateClaimForPolicyCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Claim
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Creates a claim against a policy.
    /// </summary>
    public class CreateClaimForPolicyCommand : ICommand<Guid>
    {
        public CreateClaimForPolicyCommand(Guid tenantId, Guid policyId, Guid? ownerUserId)
        {
            this.TenantId = tenantId;
            this.PolicyId = policyId;
            this.OwnerUserId = ownerUserId;
        }

        /// <summary>
        /// Gets the tenant ID.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the policy ID.
        /// </summary>
        public Guid PolicyId { get; }

        public Guid? OwnerUserId { get; }
    }
}
