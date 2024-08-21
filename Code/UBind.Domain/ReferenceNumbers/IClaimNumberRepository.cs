// <copyright file="IClaimNumberRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReferenceNumbers
{
    using System;

    /// <summary>
    /// Service for managing claim numbers.
    /// </summary>
    public interface IClaimNumberRepository : INumberPoolRepository
    {
        /// <summary>
        /// Assign Claim Number to claim record.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the numbers are for.</param>
        /// <param name="productId">The ID of the product the numbers are for.</param>
        /// <param name="oldNumber">The old Claim number.</param>
        /// <param name="newNumber">The new Claim number.</param>
        /// <param name="environment">The deployment environment of the reference numbers are for.</param>
        /// <returns>A claim number.</returns>
        string AssignClaimNumber(Guid tenantId, Guid productId, string oldNumber, string newNumber, DeploymentEnvironment environment);

        /// <summary>
        /// Unassign a claim number to a claim record.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the numbers are for.</param>
        /// <param name="productId">The ID of the product the numbers are for.</param>
        /// <param name="oldNumber">The old Claim number.</param>
        /// <param name="environment">The deployment environment of the reference numbers are for.</param>
        /// <param name="isRestoreOld">The flag whether to restore or not.</param>
        void UnassignClaimNumber(Guid tenantId, Guid productId, string oldNumber, DeploymentEnvironment environment, bool isRestoreOld = false);

        /// <summary>
        /// Return the old claim number to the claim number pool.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the numbers are for.</param>
        /// <param name="productId">The ID of the product the numbers are for.</param>
        /// <param name="oldClaimNumber">The old Claim number.</param>
        /// <param name="environment">The deployment environment of the reference numbers are for.</param>
        /// <param name="save">Indicates whether to automatically save changes to DB.</param>
        void ReuseOldClaimNumber(Guid tenantId, Guid productId, string oldClaimNumber, DeploymentEnvironment environment, bool save = true);
    }
}
