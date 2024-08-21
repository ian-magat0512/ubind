// <copyright file="EmailConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Email
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using MimeKit;
    using UBind.Application.Automation.Attachment;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Extensions.Domain;
    using UBind.Domain.Helpers;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Defines an email configuration.
    /// </summary>
    public class EmailConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailConfiguration"/> class.
        /// </summary>
        /// <param name="from">The email address of the sender.</param>
        /// <param name="replyTo">The reply to address.</param>
        /// <param name="to">The list of recipient addresses.</param>
        /// <param name="cc">The list of cc addresses.</param>
        /// <param name="bcc">The list of bcc addresses.</param>
        /// <param name="subject">The email subject.</param>
        /// <param name="textBody">The email body in text format.</param>
        /// <param name="htmlBody">The email body in html format.</param>
        /// <param name="attachments">The email attachments.</param>
        /// <param name="comments">The remarks to go in the 'comments' header of the email.</param>
        /// <param name="keywords">The keywords associated with the email content.</param>
        /// <param name="headers">The headers.</param>
        public EmailConfiguration(
            IProvider<Data<string>> from,
            IEnumerable<IProvider<Data<string>>>? to,
            IEnumerable<IProvider<Data<string>>>? replyTo,
            IEnumerable<IProvider<Data<string>>>? cc,
            IEnumerable<IProvider<Data<string>>>? bcc,
            IProvider<Data<string>>? subject,
            IProvider<Data<string>>? textBody,
            IProvider<Data<string>>? htmlBody,
            IEnumerable<IProvider<Data<FileAttachmentInfo>?>>? attachments,
            IProvider<Data<string>>? comments,
            IEnumerable<IProvider<Data<string>>>? keywords,
            IEnumerable<IProvider<KeyValuePair<string, IEnumerable<string>>>>? headers)
        {
            this.From = from;
            this.ReplyTo = replyTo;
            this.To = to;
            this.CC = cc;
            this.BCC = bcc;
            this.Subject = subject;
            this.TextBody = textBody;
            this.HtmlBody = htmlBody;
            this.Attachments = attachments;
            this.Comments = comments;
            this.Keywords = keywords;
            this.Headers = headers;
        }

        /// <summary>
        /// Gets or sets the email address to be used as sender of the email.
        /// </summary>
        public IProvider<Data<string>> From { get; set; }

        /// <summary>
        /// Gets or sets a list of email addresses to which the email will be addressed.
        /// </summary>
        public IEnumerable<IProvider<Data<string>>>? To { get; set; }

        /// <summary>
        /// Gets or sets a list of email addresses that will be used as the reply-to header of the email.
        /// </summary>
        public IEnumerable<IProvider<Data<string>>>? ReplyTo { get; set; } = Enumerable.Empty<IProvider<Data<string>>>();

        /// <summary>
        /// Gets or sets a list of email addresses to which a carbon copy of the email will be sent.
        /// </summary>
        public IEnumerable<IProvider<Data<string>>>? CC { get; set; } = Enumerable.Empty<IProvider<Data<string>>>();

        /// <summary>
        /// Gets or sets a list of email addresses to which a blind carbon copy of the email will be sent.
        /// </summary>
        public IEnumerable<IProvider<Data<string>>>? BCC { get; set; } = Enumerable.Empty<IProvider<Data<string>>>();

        /// <summary>
        /// Gets or sets the subject of the email.
        /// </summary>
        public IProvider<Data<string>>? Subject { get; set; }

        /// <summary>
        /// Gets or sets the text-only version of the email body.
        /// </summary>
        public IProvider<Data<string>>? TextBody { get; set; }

        /// <summary>
        /// Gets or sets the html version of the email body.
        /// </summary>
        public IProvider<Data<string>>? HtmlBody { get; set; }

        /// <summary>
        /// Gets or sets the collection of attachments for the email.
        /// </summary>
        public IEnumerable<IProvider<Data<FileAttachmentInfo>?>>? Attachments { get; set; }

        /// <summary>
        /// Gets or sets the remarks to go in the 'comments' header of the email.
        /// </summary>
        public IProvider<Data<string>>? Comments { get; set; }

        /// <summary>
        /// Gets or sets a collection of keywords associated with the email content.
        /// </summary>
        public IEnumerable<IProvider<Data<string>>>? Keywords { get; set; }

        /// <summary>
        /// Gets or sets a collection of custom headers to be added to the email.
        /// </summary>
        public IEnumerable<IProvider<KeyValuePair<string, IEnumerable<string>>>>? Headers { get; set; }

        /// <summary>
        /// Convert email object to mime message.
        /// </summary>
        /// <param name="emailData">The email object.</param>.
        /// <returns>A MailMessage instance.</returns>
        public MimeMessage ConvertToMailMessage(Email emailData)
        {
            var mailMessage = new MimeMessage();
            mailMessage.Subject = emailData.Subject ?? string.Empty;
            mailMessage.From.Add(MailboxAddress.Parse(emailData.From));
            if (emailData.To != null && emailData.To.Any())
            {
                mailMessage.To.AddRange(InternetAddressHelper.ConvertEmailAddressesToMailBoxAddresses(emailData.To));
            }

            if (emailData.ReplyTo != null && emailData.ReplyTo.Any())
            {
                mailMessage.ReplyTo.AddRange(InternetAddressHelper.ConvertEmailAddressesToMailBoxAddresses(emailData.ReplyTo));
            }

            if (emailData.Bcc != null && emailData.Bcc.Any())
            {
                mailMessage.Bcc.AddRange(InternetAddressHelper.ConvertEmailAddressesToMailBoxAddresses(emailData.Bcc));
            }

            if (emailData.Cc != null && emailData.Cc.Any())
            {
                mailMessage.Cc.AddRange(InternetAddressHelper.ConvertEmailAddressesToMailBoxAddresses(emailData.Cc));
            }

            if (!string.IsNullOrEmpty(emailData.Comments))
            {
                mailMessage.Headers.Add("comments", emailData.Comments.ToString());
            }

            if (emailData.Keywords != null && emailData.Keywords.Any())
            {
                mailMessage.Headers.Add("keywords", string.Join(",", emailData.Keywords));
            }

            if (emailData.Headers != null)
            {
                foreach (var header in emailData.Headers)
                {
                    mailMessage.Headers.Add(header.Key, string.Join(",", header.Value));
                }
            }

            var builder = new BodyBuilder();
            if (emailData.Attachments != null)
            {
                foreach (var file in emailData.Attachments)
                {
                    if (file.IsIncluded)
                    {
                        var memoryStream = new MemoryStream(file.File.Content);
                        ContentType contentType = ContentType.Parse(file.MimeType);
                        var attachment = MimeEntity.Load(contentType, memoryStream);
                        attachment = attachment.ResolveAttachment(file.FileName.ToString(), file.File.Content);
                        builder.Attachments.Add(attachment);
                    }
                }
            }

            if (!string.IsNullOrEmpty(emailData.HtmlBody))
            {
                builder.HtmlBody = emailData.HtmlBody;
            }

            if (!string.IsNullOrEmpty(emailData.TextBody))
            {
                builder.TextBody = emailData.TextBody;
            }

            mailMessage.Body = builder.ToMessageBody();
            return mailMessage;
        }

        /// <summary>
        /// Generates an email object containing resolved properties.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <returns>The email.</returns>
        public async Task<Email> ResolveEmailProperties(IProviderContext providerContext)
        {
            var resolveFrom = (await this.From.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            if (string.IsNullOrEmpty(resolveFrom))
            {
                throw new ErrorException(Errors.Automation.ProviderParameterMissing("from", "email"));
            }
            var from = new EmailAddress(resolveFrom);

            var resolveTo = this.To != null
                ? await this.To.SelectAsync(async x => (await x.ResolveValueIfNotNull(providerContext))?.DataValue)
                : null;
            var to = resolveTo?.Where(t => !string.IsNullOrEmpty(t)).Select(x => new EmailAddress(x!));

            var resolveReplyTo = this.ReplyTo != null
                ? await this.ReplyTo.SelectAsync(async rt => (await rt.ResolveValueIfNotNull(providerContext))?.DataValue)
                : null;
            var replyTo = resolveReplyTo?.Where(r => !string.IsNullOrEmpty(r)).Select(rt => new EmailAddress(rt!)).ToArray();

            var resolveCC = this.CC != null
                ? await this.CC.SelectAsync(async x => (await x.ResolveValueIfNotNull(providerContext))?.DataValue)
                : null;
            var cc = resolveCC?.Where(c => !string.IsNullOrEmpty(c)).Select(x => new EmailAddress(x!)).ToArray();

            var resolveBcc = this.BCC != null
                ? await this.BCC.SelectAsync(async x => (await x.ResolveValueIfNotNull(providerContext))?.DataValue)
                : null;
            var bcc = resolveBcc?.Where(bcc => !string.IsNullOrEmpty(bcc)).Select(x => new EmailAddress(x!)).ToArray();

            var subject = (await this.Subject.ResolveValueIfNotNull(providerContext))?.DataValue;
            var textBody = (await this.TextBody.ResolveValueIfNotNull(providerContext))?.DataValue;
            var htmlBody = (await this.HtmlBody.ResolveValueIfNotNull(providerContext))?.DataValue;
            var comments = (await this.Comments.ResolveValueIfNotNull(providerContext))?.DataValue;
            var resolveKeywords = this.Keywords != null
                ? await this.Keywords.SelectAsync(async key => (await key.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue)
                : null;
            var keywords = resolveKeywords?.ToArray();
            Dictionary<string, object>? headers = null;
            if (this.Headers != null)
            {
                headers = new Dictionary<string, object>();
                foreach (var header in this.Headers)
                {
                    var pair = (await header.Resolve(providerContext)).GetValueOrThrowIfFailed();
                    headers.Add(pair.Key, pair.Value);
                }
            }

            var resolveAttachments = this.Attachments != null
                ? await this.Attachments.SelectAsync(async c => (await c.ResolveValueIfNotNull(providerContext))?.DataValue)
                : null;
            IEnumerable<FileAttachmentInfo>? attachments = resolveAttachments?.OfType<FileAttachmentInfo>()
                .Where(c => c != null && c.IsIncluded);

            return new Email(
                from,
                to,
                replyTo,
                cc,
                bcc,
                subject,
                textBody,
                htmlBody,
                comments,
                keywords,
                headers,
                attachments,
                providerContext?.AutomationData?.System?.Environment);
        }
    }
}
