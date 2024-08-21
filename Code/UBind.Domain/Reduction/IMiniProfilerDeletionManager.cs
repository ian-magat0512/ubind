// <copyright file="IMiniProfilerDeletionManager.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Reduction
{
    using UBind.Domain.Loggers;

    /// <summary>
    /// The interface for mini profiler reductor.
    /// </summary>
    public interface IMiniProfilerDeletionManager
    {
        /// <summary>
        /// Method that starts the system to truncate the miniprofiler table with progress logger.
        /// </summary>
        /// <param name="logger">The progress logger instance that lets you track your request.</param>
        void Truncate(IProgressLogger logger);
    }
}
