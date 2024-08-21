// <copyright file="AttachFileToQuoteCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote;

using System;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;

/// <summary>
/// Command to attach a file to a quote. This is used by the attachment operation from the quotes form.
/// </summary>
public class AttachFileToQuoteCommand : ICommand<QuoteFileAttachmentReadModel>
{
    public AttachFileToQuoteCommand(
        Guid tenantId,
        Guid quoteId,
        Guid attachmentId,
        string fileName,
        string fileType,
        string fileData)
    {
        this.TenantId = tenantId;
        this.QuoteId = quoteId;
        this.AttachmentId = attachmentId;
        this.FileName = fileName;
        this.FileType = fileType;
        this.FileData = fileData;
    }

    public Guid TenantId { get; private set; }

    public Guid AttachmentId { get; private set; }

    public string FileName { get; private set; }

    public string FileType { get; private set; }

    public string FileData { get; private set; }

    public Guid QuoteId { get; private set; }
}
