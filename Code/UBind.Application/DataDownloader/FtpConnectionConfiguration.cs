// <copyright file="FtpConnectionConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.DataDownloader
{
    /// <summary>
    /// Provides configuration class for ftp connection configuration.
    /// </summary>
    public class FtpConnectionConfiguration : IFtpConnectionConfiguration
    {
        /// <inheritdoc/>
        public string Host { get; set; }

        /// <inheritdoc/>
        public string Username { get; set; }

        /// <inheritdoc/>
        public string Password { get; set; }

        /// <inheritdoc/>
        public string DefaultRemoteDirectory { get; set; }
    }
}
