// <copyright file="IFtpClientFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.DataDownloader
{
    using FluentFTP;

    /// <summary>
    /// Provide a contract for the FTP client factory to be used for creating an FTP connection.
    /// </summary>
    public interface IFtpClientFactory
    {
        /// <summary>
        /// Get a new instance of FTP client given the FTP configuration.
        /// </summary>
        /// <param name="ftpConnectionConfiguration">The ftp connection configuration.</param>
        /// <returns>Return a new instance of ftp client.</returns>
        IFtpClient GetNewFtpClient(IFtpConnectionConfiguration ftpConnectionConfiguration);
    }
}
