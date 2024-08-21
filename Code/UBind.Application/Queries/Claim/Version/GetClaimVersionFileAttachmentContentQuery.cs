// <copyright file="GetClaimVersionFileAttachmentContentQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Claim.Version
{
    using System;
    using CSharpFunctionalExtensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class GetClaimVersionFileAttachmentContentQuery : IQuery<Maybe<IFileContentReadModel>>
    {
        public GetClaimVersionFileAttachmentContentQuery(Guid tenantId, Guid fileAttachmentId)
        {
            this.TenantId = tenantId;
            this.FileAttachmentId = fileAttachmentId;
        }

        public Guid TenantId { get; }

        public Guid FileAttachmentId { get; }
    }
}
