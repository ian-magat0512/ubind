// <copyright file="IPolicyNumberRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReferenceNumbers
{
    /// <summary>
    /// Repository for managing policy numbers.
    /// </summary>
    public interface IPolicyNumberRepository : INumberPoolRepository
    {
        /// <summary>
        /// Consumes a policy number, adding it (if needed) to the pool and marking it as consumed.
        /// </summary>
        string ConsumePolicyNumber(Guid tenantId, Guid productId, string newNumber, DeploymentEnvironment environment);

        /// <summary>
        /// Deletes a policy number from the pool
        /// when the user sets "Reuse previous policy number" to false upon updating the policy number.
        /// </summary>
        void DeletePolicyNumber(Guid tenantId, Guid productId, string policyNumber, DeploymentEnvironment environment);

        /// <summary>
        /// Returns an old policy number to the policy number pool
        /// when the user sets "Reuse previous policy number" to true upon updating the policy number.
        /// </summary>
        void ReturnOldPolicyNumberToPool(Guid tenantId, Guid productId, string oldpolicyNumber, DeploymentEnvironment environment);
    }
}
