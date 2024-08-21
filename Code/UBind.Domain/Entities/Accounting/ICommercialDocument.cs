// <copyright file="ICommercialDocument.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Accounting
{
    using System;
    using NodaTime;

    /// <summary>
    /// Base class of commercial documents (invoice/creditNote) for basic accounting.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity identifier.</typeparam>
    public interface ICommercialDocument<TKey> : IEntity<TKey>
    {
        /// <summary>
        /// Gets the instant in time of the due date.
        /// </summary>
        Instant DueTimestamp { get; }

        /// <summary>
        /// Gets the Invoice Number.
        /// </summary>
        IReferenceNumber ReferenceNumber { get; }

        /// <summary>
        /// Gets the breakdown Id.
        /// </summary>
        Guid? BreakdownId { get; }

        /// <summary>
        /// Gets the tenant Id.
        /// </summary>
        Guid TenantId { get; }
    }
}
