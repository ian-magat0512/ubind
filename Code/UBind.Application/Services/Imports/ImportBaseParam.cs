// <copyright file="ImportBaseParam.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Imports
{
    using System;
    using UBind.Domain;

    /// <summary>
    /// Contains the base import parameters.
    /// </summary>
    public class ImportBaseParam
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportBaseParam"/> class.
        /// </summary>
        /// <param name="tenantId">The destinated tenant ID of the json import.</param>
        /// <param name="organisationId">The destinated organisation ID of the json import.</param>
        /// <param name="productId">The destinated product ID of the json import.</param>
        /// <param name="environment">The deployment environment to use.</param>
        public ImportBaseParam(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment environment)
        {
            this.Environment = environment;
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.OrganisationId = organisationId;
        }

        /// <summary>
        /// Gets the deployment environment of the import base param.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the tenant ID of the import base param.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the tenant ID of the import base param.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the product ID of the import base param.
        /// This field is required, maybe future imports dont require productId,
        /// by then we need to edit this.
        /// </summary>
        public Guid ProductId { get; private set; }
    }
}
