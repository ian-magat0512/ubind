// <copyright file="IQuoteDeletionManager.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Reduction
{
    using NodaTime;
    using UBind.Domain.Loggers;

    /// <summary>
    /// The interface for quote deletion manager.
    /// </summary>
    public interface IQuoteDeletionManager
    {
        /// <summary>
        /// Begin to remove nascent records per batch size.
        /// </summary>
        /// <param name="logger">The progress logger instance that lets you track your request.</param>
        /// <param name="size">The batch size of the request.</param>
        /// <param name="limit">The reduction limit of the request.</param>
        /// <param name="expiryDuration">The expiry duration.</param>
        void DeleteNascent(IProgressLogger logger, int size, int limit, Duration expiryDuration);
    }
}
