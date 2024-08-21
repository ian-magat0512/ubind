// <copyright file="TenantAliasModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;

    /// <summary>
    /// Resource Model for Tenant Alias.
    /// </summary>
    public class TenantAliasModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantAliasModel"/> class.
        /// </summary>
        /// <param name="tenantAlias">The tenant alias.</param>
        /// <param name="tenantId">The tenant id.</param>
        public TenantAliasModel(Guid tenantId, string tenantAlias)
        {
            this.Alias = tenantAlias;
            this.Id = tenantId;
        }

        /// <summary>
        /// Gets or sets Tenant Alias.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets or sets Tenant Id.
        /// </summary>
        public Guid Id { get; set; }
    }
}
