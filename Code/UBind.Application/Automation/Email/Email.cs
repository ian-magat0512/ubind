// <copyright file="Email.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Email
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using UBind.Application.Automation.Attachment;
    using UBind.Domain;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Defines an email configuration with the resolved properties.
    /// </summary>
    public class Email
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Email"/> class.
        /// </summary>
        /// <param name="from">The from address.</param>
        /// <param name="replyTo">The replyTo address/es.</param>
        /// <param name="to">The to address/es.</param>
        /// <param name="cc">The cc address/es.</param>
        /// <param name="bcc">The bcc address/es.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="textBody">The text body of the email.</param>
        /// <param name="htmlBody">The html body of the email.</param>
        /// <param name="comments">The commentary to be added to the email.</param>
        /// <param name="keywords">The keywords to be tagged to the email.</param>
        /// <param name="headers">The headers to be added to the email.</param>
        /// <param name="attachments">The attachments added to the email.</param>
        /// <param name="environment">The environment of the email.</param>
        public Email(
            EmailAddress from,
            IEnumerable<EmailAddress>? to,
            IEnumerable<EmailAddress>? replyTo,
            IEnumerable<EmailAddress>? cc,
            IEnumerable<EmailAddress>? bcc,
            string? subject,
            string? textBody,
            string? htmlBody,
            string? comments,
            IEnumerable<string>? keywords,
            Dictionary<string, object>? headers,
            IEnumerable<FileAttachmentInfo>? attachments,
            DeploymentEnvironment? environment)
        {
            this.From = from.ToString();
            this.ReplyTo = replyTo != null && replyTo.Any() ? replyTo.Select(x => x.ToString()) : null;
            this.To = to != null && to.Any() ? to.Select(x => x.ToString()) : null;
            this.Cc = cc != null && cc.Any() ? cc.Select(x => x.ToString()) : null;
            this.Bcc = bcc != null && bcc.Any() ? bcc.Select(x => x.ToString()) : null;
            this.Subject = subject;
            this.TextBody = textBody;
            this.HtmlBody = htmlBody;
            this.Comments = comments;
            this.Keywords = keywords != null && keywords.Any() ? keywords : null;
            this.Headers = headers != null && headers.Any() ? headers : null;
            this.Attachments = attachments != null && attachments.Any() ? attachments : null;
            this.Environment = environment;
            this.HasAttachments = attachments?.Any() ?? false;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        [JsonConstructor]
        public Email()
        {
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets the email address to be used as sender of the email.
        /// </summary>
        [JsonProperty(PropertyName = "from")]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets a list of email addresses to which the email will be addressed.
        /// </summary>
        [JsonProperty(PropertyName = "to", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string>? To { get; set; }

        /// <summary>
        /// Gets or sets a list of email addresses that will be used as the reply-to header of the email.
        /// </summary>
        [JsonProperty(PropertyName = "replyTo", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string>? ReplyTo { get; set; }

        /// <summary>
        /// Gets or sets a list of email addresses to which a carbon copy of the email will be sent.
        /// </summary>
        [JsonProperty(PropertyName = "cc", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string>? Cc { get; set; }

        /// <summary>
        /// Gets or sets a list of email addresses to which a blind carbon copy of the email will be sent.
        /// </summary>
        [JsonProperty(PropertyName = "bcc", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string>? Bcc { get; set; }

        /// <summary>
        /// Gets or sets the subject of the email.
        /// </summary>
        [JsonProperty(PropertyName = "subject", NullValueHandling = NullValueHandling.Ignore)]
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets the text-only version of the email body.
        /// </summary>
        [JsonProperty(PropertyName = "textBody", NullValueHandling = NullValueHandling.Ignore)]
        public string? TextBody { get; set; }

        /// <summary>
        /// Gets or sets the html version of the email body.
        /// </summary>
        [JsonProperty(PropertyName = "htmlBody", NullValueHandling = NullValueHandling.Ignore)]
        public string? HtmlBody { get; set; }

        /// <summary>
        /// Gets or sets the collection of attachments for the email.
        /// </summary>
        [JsonProperty(PropertyName = "attachments", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<FileAttachmentInfo>? Attachments { get; set; }

        /// <summary>
        /// Gets or sets the remarks to go in the 'comments' header of the email.
        /// </summary>
        [JsonProperty(PropertyName = "comments", NullValueHandling = NullValueHandling.Ignore)]
        public string? Comments { get; set; }

        /// <summary>
        /// Gets or sets a collection of keywords associated with the email content.
        /// </summary>
        [JsonProperty(PropertyName = "keywords", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<string>? Keywords { get; set; }

        /// <summary>
        /// Gets or sets a collection of custom headers to be added to the email.
        /// </summary>
        [JsonProperty(PropertyName = "headers", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object>? Headers { get; set; }

        /// <summary>
        /// Gets or sets environment of the email.
        /// </summary>
        [JsonProperty("environment")]
        public DeploymentEnvironment? Environment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether if the email has an attachment or not.
        /// </summary>
        [JsonProperty("hasAttachments")]
        public bool HasAttachments { get; set; }
    }
}
