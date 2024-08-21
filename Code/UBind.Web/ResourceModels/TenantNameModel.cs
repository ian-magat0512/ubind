// <copyright file="TenantNameModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Resource Model for Tenant Name.
    /// </summary>
    public class TenantNameModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantNameModel"/> class.
        /// </summary>
        /// <param name="tenantName">The tenant name.</param>
        /// <param name="tenantId">The tenant id.</param>
        public TenantNameModel(Guid tenantId, string tenantName)
        {
            this.Name = tenantName;
            this.Id = tenantId;
        }

        /// <summary>
        /// Gets or sets Tenant Name.
        /// </summary>
        [JsonProperty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Tenant Id.
        /// </summary>
        [JsonProperty]
        public Guid Id { get; set; }
    }
}
