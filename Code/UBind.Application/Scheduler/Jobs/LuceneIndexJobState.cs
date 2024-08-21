// <copyright file="LuceneIndexJobState.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Scheduler.Jobs
{
    using System.Threading.Tasks;

    /// <summary>
    /// The static variables related to search indexes.
    /// </summary>
    public static class LuceneIndexJobState
    {
        /// <summary>
        /// Gets or sets status of quote search index generation from DB.
        /// </summary>
        public static TaskStatus QuoteIndexGenerationStatus { get; set; } = TaskStatus.WaitingForActivation;

        /// <summary>
        /// Gets or sets status of quote earch index regeneration from DB.
        /// </summary>
        public static TaskStatus QuoteIndexRegenerationStatus { get; set; } = TaskStatus.WaitingForActivation;

        /// <summary>
        /// Gets or sets status of policy search index generation from DB.
        /// </summary>
        public static TaskStatus PolicyIndexGenerationStatus { get; set; } = TaskStatus.WaitingForActivation;

        /// <summary>
        /// Gets or sets status of policy earch index regeneration from DB.
        /// </summary>
        public static TaskStatus PolicyIndexRegenerationStatus { get; set; } = TaskStatus.WaitingForActivation;
    }
}
