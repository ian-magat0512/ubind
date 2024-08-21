// <copyright file="SystemAlertTypeModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Humanizer;
    using Newtonsoft.Json;
    using UBind.Domain;

    /// <summary>
    /// Resource model for systemAlertTypes.
    /// </summary>
    public class SystemAlertTypeModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemAlertTypeModel"/> class.
        /// </summary>
        /// <param name="systemAlertType">The systemAlertType.</param>
        public SystemAlertTypeModel(SystemAlertType systemAlertType)
        {
            this.Id = Guid.Empty.ToString();
            this.Name = systemAlertType.Humanize();
            this.Icon = "stopwatch";
        }

        /// <summary>
        /// Gets the systemAlertType ID.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the systemAlertType name.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the systemAlertType Icon.
        /// </summary>
        /// <remarks>
        /// Public setter required for JSON serialization.
        /// .</remarks>
        [JsonProperty]
        public string Icon { get; private set; }
    }
}
