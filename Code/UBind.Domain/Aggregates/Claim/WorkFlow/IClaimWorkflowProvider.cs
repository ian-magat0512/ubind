// <copyright file="IClaimWorkflowProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Claim.Workflow
{
    using System.Threading.Tasks;
    using UBind.Domain.Product;

    /// <summary>
    /// For providing product-specific quote workflow configuration.
    /// </summary>
    public interface IClaimWorkflowProvider
    {
        /// <summary>
        /// Gets the configurable quote workflow logic from a product's configuration for a given environment.
        /// </summary>
        /// <param name="releaseContext">The context indicating which product/environment to use.</param>
        /// <returns>A task from which the product configuration's configurable workflow for a given environment can be retrieved.</returns>
        Task<IClaimWorkflow> GetConfigurableClaimWorkflow(ReleaseContext releaseContext);
    }
}
