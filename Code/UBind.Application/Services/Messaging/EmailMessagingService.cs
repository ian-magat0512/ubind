// <copyright file="EmailMessagingService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MailKit.Net.Smtp;
    using MimeKit;
    using UBind.Application.Export;
    using UBind.Domain;
    using UBind.Domain.Commands.DkimSettings;
    using UBind.Domain.Patterns.Cqrs;

    /// <inheritdoc />
    public class EmailMessagingService : IMessagingService
    {
        private readonly ISmtpClientConfiguration clientConfiguration;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailMessagingService"/> class.
        /// </summary>
        /// <param name="smtpClientConfiguration">Configuration for SMTP clients.</param>
        public EmailMessagingService(ISmtpClientConfiguration smtpClientConfiguration, ICqrsMediator mediator)
        {
            this.clientConfiguration = smtpClientConfiguration;
            this.mediator = mediator;
        }

        /// <inheritdoc/>
        public void Send(Guid tenantId, MimeMessage mailMessage, Guid organisationId)
        {
            if (mailMessage == null)
            {
                return;
            }

            using (SmtpClient client = this.clientConfiguration.GetSmtpClient())
            {
                var command = new SignEmailWithDkimCommand(tenantId, organisationId, mailMessage, EmailSource.SystemEmail);
                Task.Run(async () => await this.mediator.Send(command, CancellationToken.None));
                client.Send(mailMessage);
            }
        }

        /// <inheritdoc/>
        public async Task SendAsync(Guid tenantId, Guid organisationId, MimeMessage mailMessage)
        {
            using (SmtpClient client = this.clientConfiguration.GetSmtpClient())
            {
                var command = new SignEmailWithDkimCommand(tenantId, organisationId, mailMessage, EmailSource.Automation);
                await this.mediator.Send(command, CancellationToken.None);
                await client.SendAsync(mailMessage);
            }
        }
    }
}
