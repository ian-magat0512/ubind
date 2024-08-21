// <copyright file="EmailEventExporterAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using MailKit.Net.Smtp;
    using Microsoft.Extensions.Logging;
    using MimeKit;
    using NodaTime;
    using UBind.Application.Queries.Organisation;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Extensions.Domain;
    using UBind.Domain.Factory;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Processing;
    using UBind.Domain.Quote;
    using UBind.Domain.ReadWriteModel.Email;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;

    /// <summary>
    /// Factory for creating email messages.
    /// </summary>
    public class EmailEventExporterAction : EventExporterAction
    {
        private const string JobParamName = "EmailId";
        private readonly IClock clock;
        private readonly IEmailService emailService;
        private readonly ICustomerService customerService;
        private readonly IEmailQueryService emailQueryService;
        private readonly IJobClient jobClient;
        private readonly ILogger logger;
        private readonly Func<SmtpClient> smtpClientFactory;
        private readonly ITextProvider emailTypeProvider;
        private readonly ITextProvider fromAddressProvider;
        private readonly ITextProvider toAddressProvider;
        private readonly ITextProvider ccAddressProvider;
        private readonly ITextProvider bccAddressProvider;
        private readonly ITextProvider subjectProvider;
        private readonly ITextProvider textBodyProvider;
        private readonly ITextProvider htmlBodyProvider;
        private readonly IEnumerable<IAttachmentProvider> attachmentProviders;
        private readonly ICqrsMediator mediator;
        private readonly IFileContentRepository fileContentRepository;

        /// <summary>
        /// Specify if you want to cache email model or get it fresh every time.
        /// If caching is true, retrieve the email from the previous email sent,
        /// if caching is false, generate a brand new email from the product files, templates or settings.
        /// Note: i added this so that if ever we do need to enable caching, we can easily set it here.
        /// </summary>
        private bool cacheEmailModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailEventExporterAction"/> class.
        /// </summary>
        /// <param name="emailService">The email service.</param>
        /// <param name="customerService">The customer service.</param>
        /// <param name="emailQueryService">The email query service.</param>
        /// <param name="jobClient">A client for queuing and parametertizing background jobs.</param>
        /// <param name="logger">The logging service.</param>
        /// <param name="clock">The clock that contains the current timestamp.</param>
        /// <param name="smtpClientFactory">Factory method for creating SMTP clients.</param>
        /// <param name="emailTypeProvider">Factory for generating a string containing the email type.</param>
        /// <param name="fromAddressProvider">Factory for generating a string containing the from address.</param>
        /// <param name="toAddressProvider">Factory for generating a string containing an email address or comma separated list of email addresses to send the email to.</param>
        /// <param name="subjectProvider">Factory for a string containing the email subject.</param>
        /// <param name="textBodyProvider">Factory for the plain text email body.</param>
        /// <param name="ccAddressProvider">Factory for generating a string containing an email address or comma separated list of email addresses to cc the email to.</param>
        /// <param name="bccAddressProvider">Factory for generating a string containing an email address or comma separated list of email addresses to bcc the email to.</param>
        /// <param name="htmlBodyProvider">Factory for the html email body.</param>
        /// <param name="attachmentProviders">Factory for generating email attachments.</param>
        public EmailEventExporterAction(
            IEmailService emailService,
            ICustomerService customerService,
            IEmailQueryService emailQueryService,
            IJobClient jobClient,
            ILogger logger,
            IClock clock,
            Func<SmtpClient> smtpClientFactory,
            IFileContentRepository fileContentRepository,
            ITextProvider emailTypeProvider,
            ITextProvider fromAddressProvider,
            ITextProvider toAddressProvider,
            ITextProvider subjectProvider,
            ITextProvider textBodyProvider,
            ICqrsMediator mediator,
            ITextProvider ccAddressProvider = null,
            ITextProvider bccAddressProvider = null,
            ITextProvider htmlBodyProvider = null,
            IEnumerable<IAttachmentProvider> attachmentProviders = null)
        {
            this.cacheEmailModel = false;
            this.emailService = emailService;
            this.customerService = customerService;
            this.emailQueryService = emailQueryService;
            this.jobClient = jobClient;
            this.logger = logger;
            this.clock = clock;
            this.smtpClientFactory = smtpClientFactory;
            this.emailTypeProvider = emailTypeProvider;
            this.fromAddressProvider = fromAddressProvider;
            this.toAddressProvider = toAddressProvider;
            this.subjectProvider = subjectProvider;
            this.textBodyProvider = textBodyProvider;
            this.ccAddressProvider = ccAddressProvider;
            this.bccAddressProvider = bccAddressProvider;
            this.htmlBodyProvider = htmlBodyProvider;
            this.attachmentProviders = attachmentProviders ?? Enumerable.Empty<IAttachmentProvider>();
            this.fileContentRepository = fileContentRepository;
            this.mediator = mediator;
        }

        /// <summary>
        /// Handle the event by sending an email.
        /// </summary>
        /// <param name="applicationEvent">The event to handle.</param>
        /// <returns>An awaitable task.</returns>
        public override async Task HandleEvent(ApplicationEvent applicationEvent)
        {
            var emailId = this.jobClient.GetJobParameter(applicationEvent.JobId, JobParamName);
            EmailModel emailModel = null;
            if (emailId != null && this.cacheEmailModel)
            {
                Email email = this.emailQueryService.GetWithFilesById(applicationEvent.Aggregate.TenantId, Guid.Parse(emailId));
                emailModel = this.GenerateEmailModel(email, applicationEvent.Aggregate.Environment);
            }
            else
            {
                emailModel = await this.GenerateEmailModel(applicationEvent);
            }

            using (var smtpClient = this.smtpClientFactory())
            {
                var mimeMessage = emailModel.GenerateMailMessage();
                smtpClient.Send(mimeMessage);
                await this.ProcessRecords(applicationEvent, emailModel);
                this.logger.LogInformation("Sending Email Success", new { Recipient = emailModel.To, Sender = emailModel.From, Subject = emailModel.Subject });
            }
        }

        /// <summary>
        /// Process the records that needs to be created after sending the email.
        /// </summary>
        /// <param name="applicationEvent">The application event.</param>
        /// <param name="emailModel">The email model.</param>
        private async Task ProcessRecords(ApplicationEvent applicationEvent, EmailModel emailModel)
        {
            EmailSourceType emailSourceType = applicationEvent.EventType.IsPolicyTransactionCreation() ? EmailSourceType.Policy : EmailSourceType.Quote;
            var quoteAggregate = applicationEvent.Aggregate;
            var quote = quoteAggregate.GetQuoteOrThrow(applicationEvent.QuoteId);
            EmailType emailType = await this.GetEmailType(applicationEvent);
            var customerId = quoteAggregate.HasCustomer && (this.IsEmailBeingSentToCustomer(quoteAggregate, emailModel) || emailType == EmailType.Customer)
                ? quoteAggregate.CustomerId
                : default;
            EmailAndMetadata emailAndMetadata = null;

            Guid personId = default(Guid);
            if (customerId.HasValue)
            {
                var customer = this.customerService.GetCustomerById(quoteAggregate.TenantId, customerId.Value);
                personId = customer.PrimaryPersonId;
            }

            var quoteVersion = quote.GetLatestVersion();

            // if has quoteVersion
            var quoteVersionId
                = quoteVersion != null && applicationEvent.EventType == ApplicationEventType.QuoteVersionCreated
                ? quoteVersion.VersionId : default;

            if (emailSourceType == EmailSourceType.Quote)
            {
                // if has policy.
                var policyId = quoteAggregate.Policy != null ? quoteAggregate.Id : default;
                emailAndMetadata = IntegrationEmailMetadataFactory.CreateForQuote(
                    emailModel, policyId, quote.Id, quoteVersionId, quoteAggregate.OrganisationId, customerId, personId, emailType, this.clock.Now(), this.fileContentRepository);
            }
            else if (emailSourceType == EmailSourceType.Policy)
            {
                var policyTransaction = quoteAggregate.Policy.Transactions.SingleOrDefault(t => t.EventSequenceNumber == applicationEvent.EventSequenceNumber);
                emailAndMetadata = IntegrationEmailMetadataFactory.CreateForPolicy(
                    emailModel,
                    quoteAggregate.Id,
                    policyTransaction.QuoteId,
                    policyTransaction.Id,
                    quoteVersionId,
                    quoteAggregate.OrganisationId,
                    customerId,
                    personId,
                    applicationEvent.EventType,
                    emailType,
                    this.clock.Now(),
                    this.fileContentRepository);
            }

            this.emailService.InsertEmailAndMetadata(emailAndMetadata);
            this.jobClient.SetJobParameter(applicationEvent.JobId, JobParamName, emailAndMetadata.Email.Id.ToString());

            StringBuilder message = new StringBuilder("Email Event Exporter\n");
            message.AppendLine($"TO: {emailModel.To}");
            message.AppendLine($"CC: {emailModel.Cc}");
            message.AppendLine($"BCC: {emailModel.Bcc}");
            message.AppendLine($"FROM: {emailModel.From}");
            message.AppendLine($"SUBJECT: {emailModel.Subject}");
            message.AppendLine($"PLAIN TEXT BODY: {emailModel.PlainTextBody}");
            message.AppendLine($"HTML BODY: {emailModel.HtmlBody}");
            message.AppendLine($"ATTACHMENTS: {string.Join(", ", emailModel.Attachments.Select(a => a.ContentDisposition.FileName))}");
            this.logger.LogInformation(message.ToString());
        }

        private bool IsEmailBeingSentToCustomer(
            Domain.Aggregates.Quote.QuoteAggregate quoteAggregate, EmailModel emailModel)
        {
            var customer = this.customerService.GetCustomerById(
                quoteAggregate.TenantId, quoteAggregate.CustomerId.GetValueOrDefault());
            var emailAddress = customer?.Email?.ToLower() ?? string.Empty;
            var emailAddress2 = customer?.AlternativeEmail?.ToLower() ?? string.Empty;
            var recipient = (emailModel.To ?? string.Empty).ToLower();
            var hasCustomerEmailOnRecipient = false;
            if (!string.IsNullOrEmpty(emailAddress))
            {
                hasCustomerEmailOnRecipient = recipient.Contains(emailAddress);
            }

            if (!string.IsNullOrEmpty(emailAddress2))
            {
                hasCustomerEmailOnRecipient = hasCustomerEmailOnRecipient || recipient.Contains(emailAddress2);
            }

            return hasCustomerEmailOnRecipient;
        }

        private EmailModel GenerateEmailModel(Email model, DeploymentEnvironment? environment)
        {
            var emailModel = new EmailModel(model, environment);
            foreach (var emailAttachment in model.EmailAttachments ?? Enumerable.Empty<EmailAttachment>())
            {
                var content = emailAttachment.DocumentFile.FileContent.Content;
                var attachment = MimeEntity.Load(new MemoryStream(content));
                attachment = attachment.ResolveAttachment(emailAttachment.DocumentFile.Name, content);
                emailModel.Attachments.Add(attachment);
            }

            return emailModel;
        }

        private async Task<EmailType> GetEmailType(ApplicationEvent applicationEvent)
        {
            EmailType emailType = EmailType.Admin;
            if (this.emailTypeProvider != null)
            {
                string type = await this.emailTypeProvider?.Invoke(applicationEvent);
                Enum.TryParse(type, true, out emailType);
            }

            return emailType;
        }

        private async Task<EmailModel> GenerateEmailModel(ApplicationEvent applicationEvent)
        {
            var fromAddress = await this.fromAddressProvider.Invoke(applicationEvent);
            var toAddress = await this.toAddressProvider.Invoke(applicationEvent);
            var ccAddress = this.ccAddressProvider != null
                ? await this.ccAddressProvider.Invoke(applicationEvent)
                : null;
            var bccAddress = this.bccAddressProvider != null
                ? await this.bccAddressProvider.Invoke(applicationEvent)
                : null;
            var subject = await this.subjectProvider.Invoke(applicationEvent);
            var textBody = await this.textBodyProvider.Invoke(applicationEvent);
            var htmlBody = this.htmlBodyProvider != null
                ? await this.htmlBodyProvider.Invoke(applicationEvent)
                : null;

            var tenantId = applicationEvent.Aggregate.TenantId;
            var organisationId = applicationEvent.Aggregate.OrganisationId;
            if (organisationId == default)
            {
                var command = new GetDefaultOrganisationForTenantQuery(tenantId);
                var organisation = await this.mediator.Send(command);
                organisationId = organisation.Id;
            }

            var productId = applicationEvent.Aggregate.ProductId;

            var emailModel = new EmailModel(
                tenantId,
                organisationId,
                productId,
                applicationEvent.Aggregate.Environment,
                fromAddress,
                toAddress,
                subject,
                textBody,
                htmlBody,
                ccAddress,
                bccAddress);
            if (this.attachmentProviders != null)
            {
                emailModel.Attachments = await this.GenerateAttachments(applicationEvent);
            }

            return emailModel;
        }

        private async Task<IList<MimeEntity>> GenerateAttachments(ApplicationEvent applicationEvent)
        {
            List<MimeEntity> attachments = new List<MimeEntity>();
            if (this.attachmentProviders != null)
            {
                var filteredAttachementProviders =
                    await this.attachmentProviders.WhereAsync(async p => await p.IsIncluded(applicationEvent));
                foreach (var provider in filteredAttachementProviders)
                {
                    var attachment = await provider.Invoke(applicationEvent);
                    if (attachment != null)
                    {
                        attachments.Add(attachment);
                    }
                }
            }

            return attachments;
        }
    }
}
