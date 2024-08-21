// <copyright file="PasswordInvitationRequestModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using Newtonsoft.Json;
    using UBind.Domain;

    /// <summary>
    /// Resource model for Sending Password Invitation Email Notification.
    /// </summary>
    public class PasswordInvitationRequestModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordInvitationRequestModel"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for model binding.</remarks>
        [JsonConstructor]
        public PasswordInvitationRequestModel()
        {
        }

        /// <summary>
        /// Gets or sets the Tenant ID or Alias.
        /// </summary>
        public string Tenant { get; set; }

        /// <summary>
        /// Gets or sets the Email.
        /// </summary>
        [JsonProperty]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the Organisation ID or Alias.
        /// </summary>
        public string Organisation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the password expired or not.
        /// </summary>
        [JsonProperty]
        public bool IsPasswordExpired { get; set; }

        /// <summary>
        /// Gets or sets the environment which determines the portal location where the person will activate their
        /// account.
        /// </summary>
        [JsonProperty]
        public DeploymentEnvironment Environment { get; set; }
    }
}
