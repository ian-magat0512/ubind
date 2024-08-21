// <copyright file="EntityTenantPair.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.Migration
{
    using System;

    /// <summary>
    /// This is a temporary model for migration.
    /// </summary>
    public class EntityTenantPair
    {
        /// <summary>
        /// Gets or sets the id of the entity.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the tenant id of the entity.
        /// </summary>
        public Guid TenantId { get; set; }
    }
}
