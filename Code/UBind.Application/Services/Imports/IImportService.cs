// <copyright file="IImportService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Imports
{
    using System.Threading.Tasks;
    using Hangfire;
    using Hangfire.Server;

    /// <summary>
    /// Represents the service for import request.
    /// </summary>
    public interface IImportService
    {
        /// <summary>
        /// Calls the ImportHandler method in background thread and with PerformContext object.
        /// </summary>
        /// <param name="baseParam">The import base parameter that contains environment, tenant and product id.</param>
        /// <param name="json">The json representation data in string.</param>
        /// <param name="updateEnabled">The tag if updates on import is allowed. The default value is false.</param>
        void QueueBackgroundImport(ImportBaseParam baseParam, string json, bool updateEnabled = false);

        /// <summary>
        /// Parse the json payload and queue to persist from import action.
        /// </summary>
        /// <param name="baseParam">The import base parameter that contains environment, tenant and product id.</param>
        /// <param name="json">The json representation data in string.</param>
        /// <param name="updateEnabled">The tag if updates on import is allowed. The default value is false.</param>
        /// <param name="context">Provides information about the context in which the job is performed.</param>
        void ImportHandler(ImportBaseParam baseParam, string json, bool updateEnabled = false, PerformContext context = null);

        /// <summary>
        /// Evaluate and execute whether to create or update customer, policy and claim.
        /// </summary>
        /// <param name="json">The json representation data in string.</param>
        /// <param name="context">Provides information about the context in which the job is performed.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [AutomaticRetry(Attempts = 0)]
        [ContinuationsSupportIncludingFailedStateAttribute]
        Task ImportPersistenceHandler(string json, PerformContext context = null);
    }
}
