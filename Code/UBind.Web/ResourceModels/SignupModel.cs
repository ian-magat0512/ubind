// <copyright file="SignupModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;

    /// <summary>
    /// Resource model for login requests.
    /// </summary>
    public class SignupModel : PersonalDetails
    {
        /// <summary>
        /// Gets or sets the ID or alias of the tenant which the user should be created in.
        /// </summary>
        public string Tenant { get; set; }

        /// <summary>
        /// Gets or sets ID of the user.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the deployment environment of the user, or null if the user has access to all environments.
        /// </summary>
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets the ID of the portal which the user should login to.
        /// </summary>
        public Guid? PortalId { get; set; }

        /// <summary>
        /// Gets or sets activation invitation ID.
        /// </summary>
        public Guid ActivationInvitationId { get; set; }

        /// <summary>
        /// Gets or sets the send activation invitation.
        /// </summary>
        public bool? SendActivationInvitation { get; set; } = true;

        /// <summary>
        /// Gets or sets user initial password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the role of the user.
        /// </summary>
        public string UserType { get; set; }

        /// <summary>
        /// Gets or sets the initial role of the new user.
        /// </summary>
        public Guid[] InitialRoles { get; set; }

        /// <summary>
        /// Gets or sets additional property values.
        /// </summary>
        [JsonProperty]
        public List<AdditionalPropertyValueUpsertModel> Properties { get; set; }
    }
}
