// <copyright file="RenameProductDirectoryCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Tenant
{
    using System;
    using NodaTime;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Renames product directories for the tenant and all its products.
    /// </summary>
    [RetryOnDbException(5)]
    public class RenameProductDirectoryCommand : CommandBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenameProductDirectoryCommand"/> class.
        /// </summary>
        /// <param name="tenantId">ID of the tenant.</param>
        /// <param name="oldTenantAlias">The old tenant alias, the old path of the product directory.</param>
        /// <param name="newTenantAlias">The new tenant alias, the new path of the product directory.</param>
        /// <param name="performingUserId">ID of the user.</param>
        /// <param name="createdTimestamp">Created timestamp.</param>
        public RenameProductDirectoryCommand(
            Guid tenantId,
            string oldTenantAlias,
            string newTenantAlias,
            Guid? performingUserId,
            Instant createdTimestamp)
            : base(performingUserId, createdTimestamp)
        {
            this.TenantId = tenantId;
            this.OldTenantAlias = oldTenantAlias;
            this.NewTenantAlias = newTenantAlias;
        }

        /// <summary>
        /// Gets the tenantId.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the old tenant alias, the old path of the product directory.
        /// </summary>
        public string OldTenantAlias { get; }

        /// <summary>
        /// Gets the new tenant alias, the new path of the product directory.
        /// </summary>
        public string NewTenantAlias { get; }
    }
}
