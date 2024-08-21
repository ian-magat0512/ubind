// <copyright file="ThrowIfHasNewProductDirectoryExistsQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Tenant
{
    using System;
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Check if new product directory exists.
    /// </summary>
    [RetryOnDbException(5)]
    public class ThrowIfHasNewProductDirectoryExistsQuery : IQuery<Unit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThrowIfHasNewProductDirectoryExistsQuery"/> class.
        /// </summary>
        /// <param name="tenantId">The tenantId.</param>
        /// <param name="performingUserId">The performing user id.</param>
        public ThrowIfHasNewProductDirectoryExistsQuery(
            Guid tenantId,
            string newTenantAlias,
            Guid? performingUserId)
        {
            this.NewTenantAlias = newTenantAlias;
            this.TenantId = tenantId;
            this.PerformingUserId = performingUserId;
        }

        /// <summary>
        /// Gets the tenantId.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the new tenant alias.
        /// </summary>
        public string NewTenantAlias { get; }

        /// <summary>
        /// Gets the performing user Id.
        /// </summary>
        public Guid? PerformingUserId { get; }
    }
}
