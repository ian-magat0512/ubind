// <copyright file="ImportDataContainer.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Imports
{
    using System;
    using UBind.Application.Services.Imports.MappingObjects;
    using UBind.Domain;

    /// <summary>
    /// Model that contains target reference for import data.
    /// </summary>
    public class ImportDataContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportDataContainer"/> class.
        /// </summary>
        /// <param name="baseParam">The import base parameter that contains environment, tenant and product id.</param>
        /// <param name="data">The mapping data values.</param>
        /// <param name="config">The mapping configuration definition.</param>
        public ImportDataContainer(ImportBaseParam baseParam, ImportData data, ImportConfiguration config)
        {
            this.TenantId = baseParam.TenantId;
            this.ProductId = baseParam.ProductId;
            this.Environment = baseParam.Environment;
            this.OrganisationId = baseParam.OrganisationId;
            this.Data = data;
            this.Configuration = config;
        }

        /// <summary>
        /// Gets the import target tenant ID.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the import target organisation ID.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the import target product ID.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets the import target deployment environment.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the import data object.
        /// </summary>
        public ImportData Data { get; private set; }

        /// <summary>
        /// Gets the import configuration object.
        /// </summary>
        public ImportConfiguration Configuration { get; private set; }
    }
}
