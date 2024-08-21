// <copyright file="IProductContext.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product
{
    using System;

    /// <summary>
    /// Specifies a product and environment to identify the context a quote/claim belongs to.
    /// </summary>
    public interface IProductContext
    {
        /// <summary>
        /// Gets the ID of the tenant.
        /// </summary>
        Guid TenantId { get; }

        /// <summary>
        /// Gets the ID of the product.
        /// </summary>
        Guid ProductId { get; }

        /// <summary>
        /// Gets the deployment environment.
        /// </summary>
        DeploymentEnvironment Environment { get; }
    }
}
