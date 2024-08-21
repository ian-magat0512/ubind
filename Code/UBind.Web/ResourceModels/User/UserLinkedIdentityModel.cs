// <copyright file="UserLinkedIdentityModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.User
{
    using Newtonsoft.Json;
    using UBind.Domain.ReadModel.User;

    public class UserLinkedIdentityModel
    {
        /// <summary>
        /// Resource model which represents the user's link to an external identity provider.
        /// </summary>
        public UserLinkedIdentityModel(UserLinkedIdentityReadModel readModel)
        {
            this.AuthenticationMethodId = readModel.AuthenticationMethodId;
            this.AuthenticationMethodName = readModel.AuthenticationMethodName;
            this.AuthenticationMethodTypeName = readModel.AuthenticationMethodTypeName;
            this.UniqueId = readModel.UniqueId;
        }

        /// <summary>
        /// Gets or sets the authentication method ID.
        /// </summary>
        [JsonProperty("authenticationMethodId")]
        public Guid AuthenticationMethodId { get; set; }

        /// <summary>
        /// Gets or sets the authentication method name.
        /// </summary>
        [JsonProperty("authenticationMethodName")]
        public string AuthenticationMethodName { get; set; }

        /// <summary>
        /// Gets or sets the authentication method type name.
        /// </summary>
        [JsonProperty("authenticationMethodTypeName")]
        public string AuthenticationMethodTypeName { get; set; }

        /// <summary>
        /// Gets or sets the unique ID for the Organisation in the external identity provider's system.
        /// </summary>
        [JsonProperty("uniqueId")]
        public string UniqueId { get; set; }
    }
}
