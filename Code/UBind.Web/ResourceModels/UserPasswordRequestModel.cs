// <copyright file="UserPasswordRequestModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using Newtonsoft.Json;
    using UBind.Web.Validation;

    /// <summary>
    /// Resource model for handling user password.
    /// </summary>
    public class UserPasswordRequestModel
    {
        [JsonProperty]
        public string Tenant { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user.
        /// </summary>
        [JsonProperty]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the Id of invitation email.
        /// </summary>
        [JsonProperty]
        public Guid InvitationId { get; set; }

        /// <summary>
        /// Gets or sets the clear text password for the user.
        /// </summary>
        [JsonProperty]
        [StrongPassword]
        public string ClearTextPassword { get; set; }

        /// <summary>
        /// Gets or sets the organisation ID or Alias for the user.
        /// </summary>
        [JsonProperty]
        public string Organisation { get; set; }
    }
}
