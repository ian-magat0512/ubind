// <copyright file="AccessTokenResponse.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765


////    /// <summary>
////    /// DEFT security access response.
////    /// </summary>////    public class AccessTokenResponse
////    {
////        /// <summary>
////        /// Gets the access issued by the DEFT API security token that is to be used to gain access to DEFT resources.
////        /// </summary>////        [JsonProperty(PropertyName = "access_token")]
////        public string AccessToken { get; private set; }

////        /// <summary>
////        /// Gets the token type.
////        /// </summary>////        /// <remarks>Not used. Expected to always be "Bearer".</remarks>
////        [JsonProperty(PropertyName = "token_type")]
////        public string TokenType { get; private set; }

////        /// <summary>
////        /// Gets the lifespan that the token remains active for. Once this time has been exhausted, the token will no longer
////        /// be functional.The value’s unit is in seconds.
////        /// </summary>////        public int ExpiresIn => this.ExpiresInString != null ? int.Parse(this.ExpiresInString) : 0;

////        /// <summary>
////        /// Gets the field of jurisdiction that the Biller has access to using the DEFT API
////        /// security token.
////        /// </summary>////        [JsonProperty(PropertyName = "scope")]
////        public string Scope { get; private set; }

////        /// <summary>
////        /// Gets a value used to ensure that state is maintained between the request and call-back.
////        /// </summary>////        [JsonProperty(PropertyName = "state")]
////        public string State { get; private set; }

////        [JsonProperty(PropertyName = "expires_in")]
////        private string ExpiresInString { get; set; }
////    }
////}
