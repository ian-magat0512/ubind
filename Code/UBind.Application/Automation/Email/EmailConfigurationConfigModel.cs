// <copyright file="EmailConfigurationConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Email
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Application.Automation.Attachment;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// Model for creating an <see cref="EmailConfiguration"/>.
    /// </summary>
    public class EmailConfigurationConfigModel : IBuilder<EmailConfiguration>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        /// <summary>
        /// Gets or sets the email address to be used as sender of the email.
        /// </summary>
        public IBuilder<IProvider<Data<string>>> From { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Gets or sets a list of email addresses to which the email will be addressed.
        /// </summary>
        public IEnumerable<IBuilder<IProvider<Data<string>>>>? To { get; set; }

        /// <summary>
        /// Gets or sets a list of email addresses that will be used as the reply-to header of the email.
        /// </summary>
        public IEnumerable<IBuilder<IProvider<Data<string>>>>? ReplyTo { get; set; }

        /// <summary>
        /// Gets or sets a list of email addresses to which a carbon copy of the email will be sent.
        /// </summary>
        public IEnumerable<IBuilder<IProvider<Data<string>>>>? CC { get; set; }

        /// <summary>
        /// Gets or sets a list of email addresses to which a blind carbon copy of the email will be sent.
        /// </summary>
        public IEnumerable<IBuilder<IProvider<Data<string>>>>? BCC { get; set; }

        /// <summary>
        /// Gets or sets the subject of the email.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? Subject { get; set; }

        /// <summary>
        /// Gets or sets the text-only version of the email body.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? TextBody { get; set; }

        /// <summary>
        /// Gets or sets the html version of the email body.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? HtmlBody { get; set; }

        /// <summary>
        /// Gets or sets the collection of attachments for the email.
        /// </summary>
        public IEnumerable<FileAttachmentProviderConfigModel>? Attachments { get; set; }

        /// <summary>
        /// Gets or sets the remarks to go in the 'comments' header of the email.
        /// </summary>
        public IBuilder<IProvider<Data<string>>>? Comments { get; set; }

        /// <summary>
        /// Gets or sets a collection of keywords associated with the email content.
        /// </summary>
        public IEnumerable<IBuilder<IProvider<Data<string>>>>? Keywords { get; set; }

        /// <summary>
        /// Gets or sets a collection of custom headers to be added to the email.
        /// </summary>
        public IEnumerable<IBuilder<IProvider<KeyValuePair<string, IEnumerable<string>>>>>? Headers { get; set; }

        /// <inheritdoc/>
        public EmailConfiguration Build(IServiceProvider dependencyProvider)
        {
            return new EmailConfiguration(
                this.From.Build(dependencyProvider),
                this.To?.Select(t => t.Build(dependencyProvider)),
                this.ReplyTo?.Select(r => r.Build(dependencyProvider)),
                this.CC?.Select(c => c.Build(dependencyProvider)),
                this.BCC?.Select(b => b.Build(dependencyProvider)),
                this.Subject?.Build(dependencyProvider),
                this.TextBody?.Build(dependencyProvider),
                this.HtmlBody?.Build(dependencyProvider),
                this.Attachments?.Select(c => c.Build(dependencyProvider)).ToList(),
                this.Comments?.Build(dependencyProvider),
                this.Keywords?.Select(k => k.Build(dependencyProvider)),
                this.Headers?.Select(h => h.Build(dependencyProvider)));
        }
    }
}
