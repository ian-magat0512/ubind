// <copyright file="GetFileAttachmentContentQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.FileAttachment
{
    using System;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class GetFileAttachmentContentQuery : IQuery<IFileContentReadModel>
    {
        public GetFileAttachmentContentQuery(Guid tenantId, Guid attachmentId)
        {
            this.TenantId = tenantId;
            this.AttachmentId = attachmentId;
        }

        public Guid TenantId { get; }

        public Guid AttachmentId { get; }
    }
}
