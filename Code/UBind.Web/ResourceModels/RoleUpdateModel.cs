// <copyright file="RoleUpdateModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;

    /// <summary>
    /// Resource model for creating/updating a role.
    /// </summary>
    public class RoleUpdateModel
    {
        /// <summary>
        /// Gets the name of the role.
        /// </summary>
        [Required(ErrorMessage = "Role name is required.")]
        [RegularExpression(".*[a-zA-Z0-9].*", ErrorMessage = "Role name must contain at least one alphanumeric character.")]
        [JsonProperty]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the role.
        /// </summary>
        [Required(ErrorMessage = "Role description is required.")]
        [RegularExpression(".*[a-zA-Z0-9].*", ErrorMessage = "Role description must contain at least one alphanumeric character.")]
        [JsonProperty]
        public string Description { get; private set; }
    }
}
