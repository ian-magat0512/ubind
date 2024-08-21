// <copyright file="AttachDocumentsToQuoteCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Document
{
    using MimeKit;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    public class AttachDocumentsToQuoteCommand : ICommand
    {
        public AttachDocumentsToQuoteCommand(
            ApplicationEvent applicationEvent,
            IEnumerable<MimeEntity> attachments)
        {
            this.ApplicationEvent = applicationEvent;
            this.Attachments = attachments;
        }

        public ApplicationEvent ApplicationEvent { get; }

        public IEnumerable<MimeEntity> Attachments { get; }
    }
}
