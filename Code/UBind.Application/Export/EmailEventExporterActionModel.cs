﻿// <copyright file="EmailEventExporterActionModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Configuration;

    /// <summary>
    /// Model for email action.
    /// </summary>
    public class EmailEventExporterActionModel
        : IExporterModel<EventExporterAction>
    {
        /// <summary>
        /// Gets or sets the text provider for providing the email's type.
        /// </summary>
        public IExporterModel<ITextProvider> EmailType { get; set; }

        /// <summary>
        /// Gets or sets the text provider for providing the email's from
        /// address.
        /// </summary>
        public IExporterModel<ITextProvider> From { get; set; }

        /// <summary>
        /// Gets or sets the text provider for providing the email's to
        /// address.
        /// </summary>
        public IExporterModel<ITextProvider> To { get; set; }

        /// <summary>
        /// Gets or sets the text provider for providing the email's cc
        /// address.
        /// </summary>
        public IExporterModel<ITextProvider> Cc { get; set; }

        /// <summary>
        /// Gets or sets the text provider for providing the email's bcc
        /// address.
        /// </summary>
        public IExporterModel<ITextProvider> Bcc { get; set; }

        /// <summary>
        /// Gets or sets the text provider for providing the email's subject.
        /// </summary>
        public IExporterModel<ITextProvider> Subject { get; set; }

        /// <summary>
        /// Gets or sets the text provider for providing the email's plain text
        /// body.
        /// </summary>
        public IExporterModel<ITextProvider> PlainTextBody { get; set; }

        /// <summary>
        /// Gets or sets the text provider for providing the email's html body.
        /// </summary>
        public IExporterModel<ITextProvider> HtmlBody { get; set; }

        /// <summary>
        /// Gets or sets the attachment provider for providing email's
        /// attachments.
        /// </summary>
        public IEnumerable<IExporterModel<IAttachmentProvider>> Attachments { get; set; }

        /// <summary>
        /// Build the email action.
        /// </summary>
        /// <param name="dependencyProvider">
        /// Container for dependencies required for exporter building.
        /// .</param>
        /// <param name="productConfiguration">
        /// Contains per-product configuration.</param>
        /// <returns>
        /// An email action that can send an email in response to an
        /// application event.
        /// .</returns>
        public EventExporterAction Build(
            IExporterDependencyProvider dependencyProvider,
            IProductConfiguration productConfiguration)
        {
            IList<IAttachmentProvider> attachments = new List<IAttachmentProvider>();

            foreach (IExporterModel<IAttachmentProvider> attachment in this.Attachments
                     ?? Enumerable.Empty<IExporterModel<IAttachmentProvider>>())
            {
                IAttachmentProvider attachmentProvider = attachment?.Build(dependencyProvider, productConfiguration);
                if (attachmentProvider != null)
                {
                    attachments.Add(attachmentProvider);
                }
            }

            var action = new EmailEventExporterAction(
                dependencyProvider.EmailService,
                dependencyProvider.CustomerService,
                dependencyProvider.EmailQueryService,
                dependencyProvider.JobClient,
                dependencyProvider.Logger,
                dependencyProvider.Clock,
                dependencyProvider.SmtpClientFactory,
                dependencyProvider.FileContentRepository,
                this.EmailType?.Build(dependencyProvider, productConfiguration),
                this.From.Build(dependencyProvider, productConfiguration),
                this.To.Build(dependencyProvider, productConfiguration),
                this.Subject.Build(dependencyProvider, productConfiguration),
                this.PlainTextBody.Build(dependencyProvider, productConfiguration),
                dependencyProvider.Mediator,
                this.Cc?.Build(dependencyProvider, productConfiguration),
                this.Bcc?.Build(dependencyProvider, productConfiguration),
                this.HtmlBody?.Build(dependencyProvider, productConfiguration),
                attachments);

            return action;
        }
    }
}
