// <copyright file="LogModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    /// Resource model for Logging.
    /// </summary>
    public class LogModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogModel"/> class.
        /// Constructor.
        /// </summary>
        [JsonConstructor]
        public LogModel()
        {
        }

        /// <summary>
        /// Gets or sets the ID of the application the new claim is for, or the ID of the claim to be edited.
        /// </summary>
        [JsonProperty]
        [Required]
        public LogLevel Level { get; set; }

        /// <summary>
        /// Gets or sets the description of the log..
        /// </summary>
        [JsonProperty]
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the value of the log.
        /// </summary>
        [JsonProperty]
        [Required]
        public string Value { get; set; }
    }
}
