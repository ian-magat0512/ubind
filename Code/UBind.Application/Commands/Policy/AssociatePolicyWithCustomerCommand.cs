// <copyright file="AssociatePolicyWithCustomerCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Policy
{
    using System;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Associates a policy with a customer. The policy can be:
    /// - an issued policy from a completed quote
    /// - an imported policy without a quote
    /// This also associates related claims to the customer.
    /// </summary>
    public class AssociatePolicyWithCustomerCommand : ICommand<Unit>
    {
        public AssociatePolicyWithCustomerCommand(Guid tenantId, Guid policyId, Guid customerId)
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
