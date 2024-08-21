// <copyright file="FileDocumentEventAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Export
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UBind.Application.Commands.Document;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Factory for saving documents.
    /// </summary>
    public class FileDocumentEventAction : EventExporterAction
    {
        private readonly IEnumerable<IAttachmentProvider> attachmentProviders;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDocumentEventAction"/> class.
        /// </summary>
        /// <param name="attachmentProviders">Factory for generating email attachments.</param>
        public FileDocumentEventAction(
            ICqrsMediator mediator,
            IEnumerable<IAttachmentProvider> attachmentProviders)
        {
            this.attachmentProviders = attachmentProviders;
            this.mediator = mediator;
        }

        /// <summary>
        /// Handle the event by saving an attachment.
        /// </summary>
        /// <param name="applicationEvent">The event to handle.</param>
        /// <returns>An awaitable Task.</returns>
        public override async Task HandleEvent(ApplicationEvent applicationEvent)
        {
            var attachments = await (await this.attachmentProviders
                .WhereAsync(async p => await p.IsIncluded(applicationEvent)))
                .SelectAsync(async p => await p.Invoke(applicationEvent));
            if (attachments != null && attachments.Any())
            {
                await this.mediator.Send(new AttachDocumentsToQuoteCommand(applicationEvent, attachments));
            }
        }
    }
}
