// <copyright file="ImportHangfireRequestModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Imports
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the hangfire request model for import service.
    /// </summary>
    public class ImportHangfireRequestModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportHangfireRequestModel"/> class.
        /// </summary>
        /// <param name="jobId">The hangfire job ID to use for enqueueing.</param>
        public ImportHangfireRequestModel(string jobId)
        {
            this.JobId = jobId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportHangfireRequestModel"/> class.
        /// </summary>
        [JsonConstructor]
        public ImportHangfireRequestModel()
        {
        }

        /// <summary>
        /// Gets the import hangfire job ID.
        /// </summary>
        [JsonProperty]
        public string JobId { get; private set; }
    }
}
