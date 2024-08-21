// <copyright file="EmailModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Quote
{
    using System;
    using System.Collections.Generic;
    using MimeKit;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadWriteModel.Email;

    /// <summary>
    /// Resource model for email model.
    /// </summary>
    public class EmailModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailModel"/> class.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="environment">The environment if has.</param>
        public EmailModel(
            Email email,
            DeploymentEnvironment? environment)
        {
            this.Id = email.Id;
            this.From = email.From;
            this.To = email.To;
            this.Cc = email.CC;
            this.Bcc = email.BCC;
            this.Subject = email.Subject;
            this.PlainTextBody = email.PlainTextBody;
            this.HtmlBody = email.HtmlBody;
            this.TenantId = email.TenantId;
            this.ProductId = email.ProductId;
            this.OrganisationId = email.OrganisationId;
            this.Environment = environment;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailModel"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id if has.</param>
        /// <param name="organisationId">The Id of the organisation.</param>
        /// <param name="productId">the product id if has.</param>
        /// <param name="environment">The environment if has.</param>
        /// <param name="from">The email from value of the entity.</param>
        /// <param name="to">The email to value of the entity.</param>
        /// <param name="subject">The subject value of the entity.</param>
        /// <param name="plainTextBody">The plain text body value of the entity.</param>
        /// <param name="htmlBody">The html body value of the entity.</param>
        /// <param name="cc">The carbon copy email addresses.</param>
        /// <param name="bcc">The blind carbon copy email addresses.</param>
        /// <param name="replyTo">The reply to email addresses.</param>
        public EmailModel(
            Guid tenantId,
            Guid organisationId,
            Guid? productId,
            DeploymentEnvironment? environment,
            string from,
            string to,
            string subject,
            string? plainTextBody,
            string? htmlBody,
            string? cc = null,
            string? bcc = null,
            string? replyTo = null)
        {
            this.Id = Guid.NewGuid();
            this.From = from;
            this.To = to;
            this.Subject = subject;
            this.PlainTextBody = plainTextBody;
            this.HtmlBody = htmlBody;
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.ProductId = productId;
            this.Environment = environment;
            this.Cc = cc;
            this.Bcc = bcc;
            this.ReplyTo = replyTo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailModel"/> class.
        /// </summary>
        private EmailModel(string from, string to, string subject)
        {
            this.From = from;
            this.To = to;
            this.Subject = subject;
        }

        /// <summary>
        /// Gets the unique identifier for email model.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the string tenant id.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the string product id.
        /// Gets the Id of the organisation.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the product id.
        /// </summary>
        public Guid? ProductId { get; private set; }

        /// <summary>
        /// Gets the deployment environment.
        /// </summary>
        public DeploymentEnvironment? Environment { get; private set; }

        /// <summary>
        /// Gets the email from value of the entity.
        /// </summary>
        public string From { get; private set; }

        /// <summary>
        /// Gets the email to value of the entity.
        /// </summary>
        public string To { get; private set; }

        /// <summary>
        /// Gets the subject value of the entity.
        /// </summary>
        public string Subject { get; private set; }

        /// <summary>
        /// Gets the plain text body value of the entity.
        /// </summary>
        public string? PlainTextBody { get; private set; }

        /// <summary>
        /// Gets the html body value of the entity.
        /// </summary>
        public string? HtmlBody { get; private set; }

        /// <summary>
        /// Gets the cc value of the entity.
        /// </summary>
        public string? Cc { get; private set; }

        /// <summary>
        /// Gets the bcc value of the entity.
        /// </summary>
        public string? Bcc { get; private set; }

        /// <summary>
        /// Gets the reply tp of the entity.
        /// </summary>
        public string? ReplyTo { get; private set; }

        /// <summary>
        /// Gets or sets the attachments value of the entity.
        /// </summary>
        public IList<MimeEntity> Attachments { get; set; } = new List<MimeEntity>();

        /// <summary>
        /// separate From into a list.
        /// </summary>
        /// <returns>The list of recipients.</returns>
        public IEnumerable<string> GetSenderList()
        {
            return this.From.Split(',');
        }

        /// <summary>
        /// separate To into a list.
        /// </summary>
        /// <returns>The list of recipients.</returns>
        public IEnumerable<string> GetRecipientList()
        {
            return this.To.Split(',');
        }

        /// <summary>
        /// separate CC into a list.
        /// </summary>
        /// <returns>The list of cc recipients.</returns>
        public IEnumerable<string> GetCCList()
        {
            return this.Cc?.Split(',') ?? Array.Empty<string>();
        }

        /// <summary>
        /// separate BCC into a list.
        /// </summary>
        /// <returns>The list of bcc recipients.</returns>
        public IEnumerable<string> GetBCCList()
        {
            return this.Bcc?.Split(',') ?? Array.Empty<string>();
        }

        /// <summary>
        /// separate ReplyTo into a list.
        /// </summary>
        /// <returns>The list of bcc recipients.</returns>
        public IEnumerable<string> GetReplyToList()
        {
            return this.ReplyTo?.Split(',') ?? Array.Empty<string>();
        }

        /// <summary>
        /// Return the model as mail message class.
        /// </summary>
        /// <returns>The System.Net.Mail.MailMessage class.</returns>
        public MimeMessage GenerateMailMessage()
        {
            if (this.HtmlBody == null && this.PlainTextBody == null)
            {
                throw new InvalidOperationException("System emails must have at least one type of body (html or plain text).");
            }

            // Two Notes:
            // 1. Always put one version in the MailMessage's body, rather than having a null body and two alternate views
            //    since some mail libraries will fail if MailMessage.Body is null.
            // 2. If html and plain text bodies are both available, add the html body last, as the last alternate view is
            //    the one that mail clients should favor according to https://tools.ietf.org/html/rfc2046#section-5.1.4
            var mimeMessage = new MimeMessage();
            mimeMessage.From.AddRange(InternetAddressHelper.ConvertEmailAddressesToMailBoxAddresses(this.GetSenderList()));
            mimeMessage.To.AddRange(InternetAddressHelper.ConvertEmailAddressesToMailBoxAddresses(this.GetRecipientList()));
            mimeMessage.Subject = this.Subject;

            var builder = new BodyBuilder();

            if (this.HtmlBody != null)
            {
                builder.HtmlBody = this.HtmlBody;
            }

            if (this.PlainTextBody != null)
            {
                builder.TextBody = this.PlainTextBody;
            }

            if (!string.IsNullOrWhiteSpace(this.Cc))
            {
                mimeMessage.Cc.AddRange(InternetAddressHelper.ConvertEmailAddressesToMailBoxAddresses(this.GetCCList()));
            }

            if (!string.IsNullOrWhiteSpace(this.Bcc))
            {
                mimeMessage.Bcc.AddRange(InternetAddressHelper.ConvertEmailAddressesToMailBoxAddresses(this.GetBCCList()));
            }

            foreach (MimeEntity attachment in this.Attachments)
            {
                builder.Attachments.Add(attachment);
            }

            mimeMessage.Body = builder.ToMessageBody();

            return mimeMessage;
        }
    }
}
