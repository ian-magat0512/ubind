// <copyright file="ProductPresentationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using System;

    /// <summary>
    /// Data transfer object for report entity.
    /// </summary>
    public class ProductPresentationModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductPresentationModel"/> class.
        /// </summary>
        /// <param name="tenantId">the tenant id.</param>
        /// <param name="id">The id of the product.</param>
        /// <param name="alias">The alias of the product.</param>
        /// <param name="name">The name of the report.</param>
        public ProductPresentationModel(Guid tenantId, Guid id, string alias, string name)
        {
            this.Id = id;
            this.Alias = alias;
            this.Name = name;
            this.TenantId = tenantId;
        }

        /// <summary>
        /// Gets  the Id of the product.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets  the Id of the product.
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Gets the name of the product.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets  the id of the tenant the product is for.
        /// </summary>
        public Guid TenantId { get; private set; }
    }
}
