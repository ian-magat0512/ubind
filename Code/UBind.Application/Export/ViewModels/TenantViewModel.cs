// <copyright file="TenantViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export.ViewModels
{
    using System;
    using UBind.Domain;

    /// <summary>
    /// View model for presenting the tenant for razor templates.
    /// </summary>
    public class TenantViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TenantViewModel"/> class.
        /// </summary>
        /// <param name="tenant">The tenant.</param>
        public TenantViewModel(Tenant tenant)
        {
            if (tenant != null)
            {
                this.Id = tenant.Id.ToString();
                this.Name = tenant?.Details?.Name;
                this.Alias = tenant?.Details?.Alias;
            }
        }

        /// <summary>
        /// Gets the Id of the tenant.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the Name or Title of the tenant.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the Alias of the tenant.
        /// </summary>
        public string Alias { get; private set; }

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
