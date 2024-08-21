// <copyright file="ApplicationImportResult.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Results of batch application import attempt.
    /// </summary>
    public class ApplicationImportResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationImportResult"/> class.
        /// </summary>
        /// <param name="succeeded">The IDs of applications that were successfully imported.</param>
        /// <param name="failed">The IDs of applications that were not successfully imported.</param>
        public ApplicationImportResult(IEnumerable<Tuple<Guid, string>> succeeded, IEnumerable<Tuple<Guid, string>> failed)
        {
            this.Succeeded = succeeded;
            this.Failed = failed;
        }

        /// <summary>
        /// Gets the IDs of successfully imported applications.
        /// </summary>
        [JsonProperty]
        public IEnumerable<Tuple<Guid, string>> Succeeded { get; private set; }

        /// <summary>
        /// Gets the IDs of applications that were not successfully imported.
        /// </summary>
        [JsonProperty]
        public IEnumerable<Tuple<Guid, string>> Failed { get; private set; }
    }
}
