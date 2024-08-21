// <copyright file="TenantDrop.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.SystemEmail
{
    using System;
    using global::DotLiquid;

    /// <summary>
    /// A drop model for tenant.
    /// </summary>
    public class TenantDrop : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantDrop"/> class.
        /// </summary>
        /// <param name="id">The tenant.</param>
        /// <param name="name">The tenant name.</param>
        /// <param name="alias">The tenant alias.</param>
        public TenantDrop(Guid id, string name, string alias)
        {
            this.Id = id;
            this.Name = name;
            this.Alias = alias;
        }

        /// <summary>
        /// Gets the tenant ID.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets or sets the tenant name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the tenant's alias.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Gets the Abbreviation of the tenant, for backwards compatibility.
        /// </summary>
        [Obsolete("Please use Alias instead of Abbreviation")]
        public string Abbreviation
        {
            get
            {
                return this.Alias;
            }
        }
    }
}
