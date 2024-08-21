// <copyright file="AttachFilesToEntityCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Document
{
    using UBind.Application.Automation.Attachment;
    using UBind.Application.Automation.DocumentAttacher;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Represents the command for attaching files to an entity.
    /// </summary>
    public class AttachFilesToEntityCommand : ICommand
    {
        public AttachFilesToEntityCommand(
            Guid tenantId,
            IDocumentAttacher documentAttacher,
            List<FileAttachmentInfo> attachments)
        {
            this.TenantId = tenantId;
            this.DocumentAttacher = documentAttacher;
            this.Attachments = attachments;
        }

        public Guid TenantId { get; }

        public IDocumentAttacher DocumentAttacher { get; }

        public List<FileAttachmentInfo> Attachments { get; }
    }
}
