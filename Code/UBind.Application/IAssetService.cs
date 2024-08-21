// <copyright file="IAssetService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Threading.Tasks;
    using UBind.Domain;

    /// <summary>
    /// An interface for as asset service.
    /// </summary>
    public interface IAssetService
    {
        /// <summary>
        /// Get the asset file from One Drive or Database.
        /// </summary>
        /// <param name="tenantId">The id of the tenant.</param>
        /// <param name="productId">The id of the product.</param>
        /// <param name="filename">The filename of the asset.</param>
        /// <param name="environment">THe deployment environment to load the file for.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task<byte[]> GetAssetFileAsync(
            Guid tenantId,
            Guid productId,
            string filename,
            DeploymentEnvironment environment);
    }
}
