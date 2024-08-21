// <copyright file="EmailAccount.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Email
{
    using Newtonsoft.Json;

    /// <summary>
    /// Defines the details and credentials needed to receive emails from a remote server.
    /// </summary>
    public class EmailAccount
    {
        /// <summary>
        /// Gets or sets the protocol being used for connecting to the incoming mail server.
        /// </summary>
        [JsonProperty]
        public string Protocol { get; set; }

        /// <summary>
        /// Gets or sets the encryption method used. If omitted, the default encryption method is 'none'.
        /// </summary>
        [JsonProperty]
        public string EncryptionMethod { get; set; }

        /// <summary>
        /// Gets or sets the host that will be connected to.
        /// </summary>
        [JsonProperty]
        public string HostName { get; set; }

        /// <summary>
        /// Gets or sets the port that will be used.
        /// </summary>
        /// <remarks>If omitted, the default port will be based on the protocol and encryption method.</remarks>
        [JsonProperty]
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the username to be used for authentication.
        /// </summary>
        [JsonProperty]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password to be used for authentication.
        /// </summary>
        [JsonProperty]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the value defining the frequency, in seconds, that the incoming mail server can be polled.
        /// </summary>
        /// <remarks>If omitted, default value is 60.</remarks>
        [JsonProperty]
        public int PollingIntervalSeconds { get; set; }
    }
}
