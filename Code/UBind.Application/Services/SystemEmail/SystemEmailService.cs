// <copyright file="SystemEmailService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services.SystemEmail
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::DotLiquid;
    using Humanizer;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Application.Export;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Entities;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Commands.DkimSettings;
    using UBind.Domain.Extensions;
    using UBind.Domain.Factory;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Processing;
    using UBind.Domain.Quote;
    using UBind.Domain.ReadWriteModel.Email;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Messaging Service using DotLiquid.
    /// </summary>
    public class SystemEmailService : ISystemEmailService
    {
        private readonly ILogger<SystemEmailService> logger;
        private readonly IClock clock;
        private readonly ISmtpClientConfiguration clientConfiguration;
        private readonly IEmailTemplateService emailTemplateService;
        private readonly IMailClientFactory mailClientFactory;
        private readonly IEmailService emailService;
        private readonly IJobClient jobClient;
        private readonly ICqrsMediator mediator;
        private readonly IFileContentRepository fileContentRepository;

        /// <summary>
        ///  Initializes a new instance of the <see cref="SystemEmailService"/> class.
        /// </summary>
        /// <param name="emailService">The email repository.</param>
        /// <param name="clientConfiguration">The client config.</param>
        /// <param name="emailTemplateService">The email template service.</param>
        /// <param name="mailClientFactory">A factory for instantiaing an SMTP client.</param>
        /// <param name="jobClient">Client for queuing background jobs.</param>
        /// <param name="logger">The logging service.</param>
        /// <param name="clock">The clock to get the current timestamp.</param>
        public SystemEmailService(
                IEmailService emailService,
                ISmtpClientConfiguration clientConfiguration,
                IEmailTemplateService emailTemplateService,
                IMailClientFactory mailClientFactory,
                IJobClient jobClient,
                ILogger<SystemEmailService> logger,
                ICqrsMediator mediator,
                IClock clock,
                IFileContentRepository fileContentRepository)
        {
            this.emailService = emailService;
            this.clientConfiguration = clientConfiguration;
            this.emailTemplateService = emailTemplateService;
            this.mailClientFactory = mailClientFactory;
            this.jobClient = jobClient;
            this.logger = logger;
            this.mediator = mediator;
            this.clock = clock;
            this.fileContentRepository = fileContentRepository;
        }

        /// <summary>
        /// This static method registers the safe types for the DotLiquid template engine.
        /// It ensures that the DotLiquid template engine can access the properties of the EmailDrop, UserDrop, and PersonDrop classes.
        /// This method is called during the startup of the application.
        /// </summary>
        public static void RegisterTemplateSafeTypes()
        {
            Template.RegisterSafeType(
                typeof(EmailDrop),
                typeof(EmailDrop).GetProperties()
                .Select(email => email?.Name ?? string.Empty)
                .ToArray());

            Template.RegisterSafeType(
                typeof(UserDrop),
                new[] { "FullName", "HomePhone", "WorkPhone", "MobilePhone" });

            Template.RegisterSafeType(
                typeof(PersonDrop),
                new[] { "FullName", "HomePhone", "WorkPhone", "MobilePhone" });
        }

        /// <inheritdoc />
        [DisplayName("Send System Email | TENANT: {0}, ORGANISATION, {1}, TYPE: {2}, EMAIL ADDRESS: {3}")]
        public async Task SendMessage(
            string tenantAlias,
            string organisationAlias,
            string emailType,
            string? emailAddress,
            EmailDrop emailDrop,
            DeploymentEnvironment? environment = null)
        {
            var emailTemplateData = this.emailTemplateService.GenerateTemplateData(
                emailDrop.TenantId, emailDrop.EmailType, emailDrop);
            var mailMessage = this.GenerateEmailModel(emailTemplateData, emailDrop, environment).GenerateMailMessage();
            var smtpServerHost = string.IsNullOrEmpty(emailTemplateData.SmtpServerHost)
                ? this.clientConfiguration.Host
                : emailTemplateData.SmtpServerHost;
            var smtpServerPort = emailTemplateData.SmtpServerPort.HasValue
                ? emailTemplateData.SmtpServerPort.Value
                : this.clientConfiguration.Port;
            var smtpServerUsername = this.clientConfiguration.Username ?? string.Empty;
            var smtpServerPassword = this.clientConfiguration.Password ?? string.Empty;
            using (IMailClient client =
                this.mailClientFactory.Invoke(smtpServerHost, smtpServerUsername, smtpServerPassword, smtpServerPort))
            {
                var command = new SignEmailWithDkimCommand(emailDrop.TenantId, emailDrop.Organisation.Id, mailMessage, EmailSource.SystemEmail);
                await this.mediator.Send(command, CancellationToken.None);

                client.Send(mailMessage);
                this.logger.LogInformation(
                    "Sending Email Success",
                    new { Recipient = mailMessage.To, Sender = mailMessage.From, Subject = mailMessage.Subject });
            }
        }

        /// <inheritdoc />
        public Email SendAndPersistPasswordResetInvitationEmail(
            EmailDrop emailDrop,
            UserAggregate userAggregate)
        {
            var jobId = this.jobClient.Enqueue<SystemEmailService>(
                service => service.SendMessage(
                    emailDrop.Tenant.Alias, emailDrop.Organisation.Alias, emailDrop.EmailType.ToString(), userAggregate.LoginEmail, emailDrop, null),
                emailDrop.TenantId,
                emailDrop.ProductId,
                userAggregate.Environment);

            // Remove invitations after sending, but before persisting.
            emailDrop.MaskInvitationLink();
            SystemEmailTemplateData systemEmailTemplateData = this.emailTemplateService.GenerateTemplateData(
                emailDrop.TenantId, emailDrop.EmailType, emailDrop);
            var isCustomer = userAggregate.UserTypes.Contains(UserType.Customer.Humanize());
            var emailModel = this.GenerateEmailModel(
                systemEmailTemplateData, emailDrop, userAggregate.Environment);
            var emailType = isCustomer ? EmailType.Customer : EmailType.User;
            var createdEmail = this.CreateAndPersistPasswordResetInvitationEmailEntity(emailModel, userAggregate, emailType);
            createdEmail = EntityHelper.ThrowIfNotFound(createdEmail, "Email");
            return createdEmail;
        }

        /// <inheritdoc />
        public Email SendAndPersistAccountActivationInvitationEmail(
            EmailDrop emailDrop,
            UserAggregate userAggregate)
        {
            var jobId = this.jobClient.Enqueue<SystemEmailService>(
                service => service.SendMessage(
                    emailDrop.Tenant.Alias, emailDrop.Organisation.Alias, emailDrop.EmailType.ToString(),
                    userAggregate.LoginEmail ?? string.Empty, emailDrop, userAggregate.Environment),
                emailDrop.TenantId,
                emailDrop.ProductId,
                userAggregate.Environment);

            // Remove invitations after sending, but before persisting.
            emailDrop.MaskInvitationLink();
            SystemEmailTemplateData systemEmailTemplateData = this.emailTemplateService.GenerateTemplateData(
                emailDrop.TenantId, SystemEmailType.AccountActivationInvitation, emailDrop);
            var isCustomer = userAggregate.UserType == UserType.Customer.Humanize();
            Enum.TryParse(emailDrop.User.Environment, out DeploymentEnvironment environment);
            var emailModel = this.GenerateEmailModel(
                systemEmailTemplateData, emailDrop, userAggregate.Environment ?? environment);
            var emailType = isCustomer == true ? EmailType.Customer : EmailType.User;
            return this.CreateAndPersistAccountActivationInvitationEmailEntity(
                emailModel, userAggregate, emailType);
        }

        /// <inheritdoc />
        public Email SendAndPersistAccountAlreadyActivatedEmail(
            EmailDrop emailDrop,
            UserAggregate userAggregate)
        {
            var userLoginEmail = EntityHelper.ThrowIfNotFound(userAggregate.LoginEmail, "UserLoginEmail");
            var jobId = this.jobClient.Enqueue<SystemEmailService>(
                service => service.SendMessage(
                    emailDrop.Tenant.Alias, emailDrop.Organisation.Alias, emailDrop.EmailType.ToString(), userAggregate.LoginEmail ?? string.Empty, emailDrop, null),
                emailDrop.TenantId,
                emailDrop.ProductId,
                userAggregate.Environment);

            emailDrop.MaskInvitationLink();
            SystemEmailTemplateData systemEmailTemplateData = this.emailTemplateService.GenerateTemplateData(
                emailDrop.TenantId, SystemEmailType.AccountAlreadyActivated, emailDrop);
            var isCustomer = userAggregate.UserTypes.Contains(UserType.Customer.Humanize());
            var emailModel = this.GenerateEmailModel(
                systemEmailTemplateData, emailDrop, userAggregate.Environment);
            var emailType = isCustomer == true ? EmailType.Customer : EmailType.User;
            return this.CreateAndPersistAccountActivationInvitationEmailEntity(
                emailModel, userAggregate, emailType);
        }

        /// <inheritdoc />
        public Email SendAndPersistQuoteAssociationInvitationEmail(
            EmailDrop emailDrop,
            UserAggregate userAggregate,
            Guid quoteId)
        {

            var jobId = this.jobClient.Enqueue<SystemEmailService>(
                service => service.SendMessage(
                    emailDrop.Tenant.Alias, emailDrop.Organisation.Alias, emailDrop.EmailType.ToString(), userAggregate.LoginEmail, emailDrop, null),
                emailDrop.TenantId,
                emailDrop.ProductId,
                userAggregate.Environment);

            // Remove invitations after sending, but before persisting.
            emailDrop.MaskInvitationLink();

            var emailTemplateData = this.emailTemplateService.GenerateTemplateData(
                emailDrop.TenantId, SystemEmailType.QuoteAssociationInvitation, emailDrop);
            var emailType = userAggregate.UserTypes.Contains(UserType.Customer.Humanize()) ? EmailType.Customer : EmailType.User;
            var emailModel = this.GenerateEmailModel(
                emailTemplateData, emailDrop, userAggregate.Environment);
            return this.CreateAndPersistQuoteAssociationInvitationEmailEntity(
                emailModel,
                userAggregate,
                emailType,
                quoteId);
        }

        /// <inheritdoc />
        public Email SendAndPersistPolicyRenewalEmail(
            EmailDrop emailDrop,
            QuoteAggregate quoteAggregate,
            Guid customerPersonId,
            PolicyTransaction policyTransaction,
            Quote quote)
        {
            string emailAddress = emailDrop.Person?.Email ?? string.Empty;
            var jobId = this.jobClient.Enqueue<SystemEmailService>(
                service => service.SendMessage(
                    emailDrop.Tenant.Alias, emailDrop.Organisation.Alias, emailDrop.EmailType.ToString(), emailAddress, emailDrop, null),
                quote.ProductContext);

            // Remove invitations after sending, but before persisting.
            emailDrop.MaskInvitationLink();
            var emailTemplateData = this.emailTemplateService.GenerateTemplateData(
                emailDrop.TenantId, emailDrop.EmailType, emailDrop);
            var emailModel = this.GenerateEmailModel(
                emailTemplateData, emailDrop, quoteAggregate.Environment);
            return this.CreateAndPersistPolicyRenewalInvitationEmailEntity(
                emailModel, quoteAggregate, customerPersonId, policyTransaction, quote);
        }

        private static Guid GetUserOrCustomerIdForEmailType(UserAggregate userAggregate, EmailType emailType)
        {
            userAggregate = EntityHelper.ThrowIfNotFound(userAggregate, "UserAggregate");

            if (emailType == EmailType.User)
            {
                return userAggregate.Id;
            }

            if (emailType == EmailType.Customer)
            {
                if (userAggregate.CustomerId.HasValue)
                {
                    return userAggregate.CustomerId.Value;
                }
                else
                {
                    throw new InvalidOperationException("Customer Id is null.");
                }
            }

            throw new ArgumentException("Email Type is not supported.");
        }

        private EmailModel GenerateEmailModel(
            SystemEmailTemplateData templateData, EmailDrop model, DeploymentEnvironment? environment)
        {
            var toAddress = this.RenderDotLiquidTemplate(templateData.ToAddress, model);
            var subject = this.RenderDotLiquidTemplate(templateData.Subject, model);
            var plainTextBody = templateData.PlainTextBody != null
                ? this.RenderDotLiquidTemplate(templateData.PlainTextBody, model)
                : null;
            var htmlBody = templateData.HtmlBody != null
                ? this.RenderDotLiquidTemplate(templateData.HtmlBody, model)
                : null;
            var emailModel = new EmailModel(
                model.TenantId,
                model.Organisation.Id,
                model.ProductId,
                environment,
                templateData.FromAddress,
                toAddress,
                subject,
                plainTextBody,
                htmlBody,
                templateData.Cc != null ? this.RenderDotLiquidTemplate(templateData.Cc, model) : null,
                templateData.Bcc != null ? this.RenderDotLiquidTemplate(templateData.Bcc, model) : null);
            return emailModel;
        }

        private string RenderDotLiquidTemplate(string templateString, EmailDrop model)
        {
            var template = Template.Parse(templateString);
            var result = template.Render(Hash.FromAnonymousObject(model));
            return result;
        }

        private Email? CreateAndPersistPasswordResetInvitationEmailEntity(
           EmailModel model, UserAggregate userAggregate, EmailType emailType)
        {
            Guid userOrCustomerId = GetUserOrCustomerIdForEmailType(userAggregate, emailType);
            Guid organisationId = userAggregate.OrganisationId;
            Guid personId = userAggregate.PersonId;

            EmailAndMetadata? metadata = null;

            if (emailType == EmailType.Customer)
            {
                metadata = SystemEmailMetadataFactory.PasswordReset.CreateForCustomer(
                    model, userOrCustomerId, personId, organisationId, emailType, this.clock.Now(), this.fileContentRepository);
            }
            else
            {
                metadata = SystemEmailMetadataFactory.PasswordReset.CreateForUser(
                    model, userOrCustomerId, personId, organisationId, emailType, this.clock.Now(), this.fileContentRepository);
            }

            this.emailService.InsertEmailAndMetadata(metadata);
            return metadata?.Email;
        }

        private Email CreateAndPersistAccountActivationInvitationEmailEntity(
            EmailModel model, UserAggregate userAggregate, EmailType emailType)
        {
            Guid userOrCustomerId = GetUserOrCustomerIdForEmailType(userAggregate, emailType);
            Guid organisationId = userAggregate.OrganisationId;
            Guid personId = userAggregate.PersonId;

            EmailAndMetadata? emailAndMetadata = null;

            if (emailType == EmailType.User)
            {
                emailAndMetadata = SystemEmailMetadataFactory.AccountActivation.CreateForUser(
                    model, userOrCustomerId, personId, organisationId, emailType, this.clock.Now(), this.fileContentRepository);
            }
            else if (emailType == EmailType.Customer)
            {
                emailAndMetadata = SystemEmailMetadataFactory.AccountActivation.CreateForCustomer(
                    model, userOrCustomerId, personId, organisationId, emailType, this.clock.Now(), this.fileContentRepository);
            }
            emailAndMetadata = EntityHelper.ThrowIfNotFound(emailAndMetadata, "EmailAndMetaData");
            this.emailService.InsertEmailAndMetadata(emailAndMetadata);
            return emailAndMetadata.Email;
        }

        private Email CreateAndPersistQuoteAssociationInvitationEmailEntity(
            EmailModel model, UserAggregate userAggregate, EmailType emailType, Guid quoteId)
        {
            Guid userOrCustomerId = GetUserOrCustomerIdForEmailType(userAggregate, emailType);
            Guid organisationId = userAggregate.OrganisationId;
            Guid personId = userAggregate.PersonId;

            var emailAndMetadata =
                SystemEmailMetadataFactory.QuoteAssociation.Create(model, userOrCustomerId, personId, organisationId, quoteId, emailType, this.clock.Now(), this.fileContentRepository);

            this.emailService.InsertEmailAndMetadata(emailAndMetadata);
            return emailAndMetadata.Email;
        }

        private Email CreateAndPersistPolicyRenewalInvitationEmailEntity(
            EmailModel model, QuoteAggregate quoteAggregate, Guid customerPersonId, PolicyTransaction policyTransaction, Quote quote)
        {
            var emailAndMetadata = IntegrationEmailMetadataFactory.CreateForRenewalInvitation(
                model,
                quoteAggregate.Id,
                quote?.Id,
                policyTransaction.Id,
                quoteAggregate.OrganisationId,
                quoteAggregate.CustomerId,
                customerPersonId,
                EmailType.Customer,
                this.clock.Now(),
                this.fileContentRepository);

            this.emailService.InsertEmailAndMetadata(emailAndMetadata);
            return emailAndMetadata.Email;
        }
    }
}
