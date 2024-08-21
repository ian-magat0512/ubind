// <copyright file="ISmtpClientConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using MailKit.Net.Smtp;

    /// <summary>
    /// Configuration for SMTP clients.
    /// </summary>
    public interface ISmtpClientConfiguration
    {
        /// <summary>
        /// Gets the host of the SMTP server to connect to.
        /// </summary>
        string Host { get; }

        /// <summary>
        /// Gets the port of the SMTP server to connect to.
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Gets the username of the SMTP server to connect to.
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Gets the password of the SMTP server to connect to.
        /// </summary>
        string Password { get; }

        /// <summary>
        /// retrieves smtpclient.
        /// </summary>
        /// <returns>smtp client.</returns>
        SmtpClient GetSmtpClient();
    }
}
