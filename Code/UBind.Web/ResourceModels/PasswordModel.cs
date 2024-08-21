// <copyright file="PasswordModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using Newtonsoft.Json;

    /// <summary>
    /// Password for an application.
    /// </summary>
    public class PasswordModel
    {
        /// <summary>
        /// Gets or sets the clear text password for the application.
        /// </summary>
        [JsonProperty(PropertyName = "savePassword")]
        public string ClearTextPassword { get; set; }
    }
}
