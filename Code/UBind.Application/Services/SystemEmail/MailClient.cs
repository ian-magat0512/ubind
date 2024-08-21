// <copyright file="MailClient.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.SystemEmail
{
    using System;
    using MailKit.Net.Smtp;
    using MimeKit;
    using UBind.Domain.Extensions;

    /// <summary>
    /// A client for sending email to an SMTP server.
    /// </summary>
    public class MailClient : IMailClient
    {
        private SmtpClient smtpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="MailClient"/> class.
        /// </summary>
        /// <param name="host">The host of the SMTP server to use.</param>
        /// <param name="port">The port of the SMTP server to use.</param>
        /// <param name="username">The username of the SMTP server to use.</param>
        /// <param name="password">The password of the SMTP server to use.</param>
        public MailClient(string host, int port, string username, string password)
        {
            this.smtpClient = new SmtpClient();
            this.smtpClient.Connect(host, port);

            if (!username.IsNullOrEmpty())
            {
                this.smtpClient.Authenticate(username, password);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.smtpClient != null)
            {
                this.smtpClient.Dispose();
                this.smtpClient = null;
            }
        }

        /// <inheritdoc/>
        public void Send(MimeMessage mailMessage)
        {
            if (this.smtpClient == null)
            {
                throw new InvalidOperationException("Cannot use mail client after it has been disposed.");
            }

            this.smtpClient.Send(mailMessage);
        }
    }
}
