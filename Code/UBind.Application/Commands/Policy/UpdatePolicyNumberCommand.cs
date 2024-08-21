// <copyright file="UpdatePolicyNumberCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Policy
{
    using System;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Updates the policy number of an existing policy.
    /// The policy number can be a custom policy number or a number from the pool
    /// </summary>
    public class UpdatePolicyNumberCommand : ICommand<IPolicyReadModelDetails>
    {
        public UpdatePolicyNumberCommand(
            Guid tenantId,
            Guid policyId,
            string newPolicyNumber,
            DeploymentEnvironment environment,
            bool returnOldPolicyNumberToPool = false)
        {
            this.TenantId = tenantId;
            this.PolicyId = policyId;
            this.NewPolicyNumber = newPolicyNumber;
            this.Environment = environment;
            this.ReturnOldPolicyNumberToPool = returnOldPolicyNumberToPool;
        }

        public Guid TenantId { get; private set; }

        public Guid PolicyId { get; private set; }

        public string NewPolicyNumber { get; private set; }

        public DeploymentEnvironment Environment { get; private set; }

        public bool ReturnOldPolicyNumberToPool { get; private set; }
    }
}
