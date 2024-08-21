// <copyright file="SendDkimTestEmailCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.DkimSettings
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using MimeKit;
    using UBind.Application.Export;
    using UBind.Application.Helpers;
    using UBind.Application.Services.SystemEmail;
    using UBind.Domain;
    using UBind.Domain.Commands.DkimSettings;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Command handler for sending DKIM test email.
    /// </summary>
    public class SendDkimTestEmailCommandHandler : ICommandHandler<SendDkimTestEmailCommand, Unit>
    {
        private readonly IDkimSettingRepository dkimSettingRepository;
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly IMailClientFactory mailClientFactory;
        private readonly ILogger<SendDkimTestEmailCommandHandler> logger;
        private readonly ISmtpClientConfiguration clientConfiguration;
        private readonly ICqrsMediator mediator;

        public SendDkimTestEmailCommandHandler(
            IDkimSettingRepository dkimSettingRepository,
            IOrganisationReadModelRepository organisationReadModelRepositor,
            IMailClientFactory mailClientFactory,
            ILogger<SendDkimTestEmailCommandHandler> logger,
            ICqrsMediator mediator,
            ISmtpClientConfiguration clientConfiguration)
        {
            this.dkimSettingRepository = dkimSettingRepository;
            this.organisationReadModelRepository = organisationReadModelRepositor;
            this.mailClientFactory = mailClientFactory;
            this.logger = logger;
            this.mediator = mediator;
            this.clientConfiguration = clientConfiguration;
        }

        public async Task<Unit> Handle(SendDkimTestEmailCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var dkimSettings = this.dkimSettingRepository.GetDkimSettingById(request.TenantId, request.OrganisationId, request.DkimSettingsId);
            if (dkimSettings == null)
            {
                throw new ErrorException(Errors.General.NotFound("DKIM settings", request.DkimSettingsId));
            }

            var organisation = this.organisationReadModelRepository.Get(request.TenantId, request.OrganisationId);

            if (organisation == null)
            {
                throw new ErrorException(Errors.General.NotFound("Organisation", request.OrganisationId));
            }

            var mimeMessage = new MimeMessage();
            mimeMessage.Subject = $"DKIM test email from {organisation.Name}";
            var builder = new BodyBuilder();
            builder.TextBody = "This email was generated automatically. Please do not reply.";
            mimeMessage.Body = builder.ToMessageBody();
            EmailAddressHelper.ThrowIfEmailAddressIsNotValid(new List<string> { request.SenderEmailAddress, request.RecipientEmailAddress }, "send DKIM test email");
            mimeMessage.From.Add(InternetAddress.Parse(request.SenderEmailAddress));
            mimeMessage.To.Add(InternetAddress.Parse(request.RecipientEmailAddress));

            using (IMailClient client =
                this.mailClientFactory.Invoke(this.clientConfiguration.Host, this.clientConfiguration.Username, this.clientConfiguration.Password, this.clientConfiguration.Port))
            {
                var command = new SignEmailWithDkimCommand(request.TenantId, request.OrganisationId, mimeMessage, EmailSource.TestEmail);
                await this.mediator.Send(command, CancellationToken.None);

                client.Send(mimeMessage);
                this.logger.LogInformation(
                    "Sending Email Success",
                    new { Recipient = mimeMessage.To, Sender = mimeMessage.From, Subject = mimeMessage.Subject });
            }

            return await Task.FromResult(Unit.Value);
        }
    }
}
