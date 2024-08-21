// <copyright file="FileAttachmentInfo.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Attachment;

using Newtonsoft.Json;
using UBind.Application.Automation.Providers.File;
using UBind.Application.JsonConverters;
using UBind.Domain.ValueTypes;

/// <summary>
/// File Attachment Information for FileAttachmentProvider.
/// </summary>
public class FileAttachmentInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileAttachmentInfo"/> class.
    /// </summary>
    /// <param name="fileName">The name of the file attachment.</param>
    /// <param name="fileInfo">The content of the file attachment.</param>
    /// <param name="mimeType">The mime type.</param>
    /// <param name="isIncluded">The flag if attachment to be included or not.</param>
    public FileAttachmentInfo(string fileName, FileInfo fileInfo, string mimeType, bool isIncluded)
    {
        this.FileName = new FileName(fileName);
        this.File = fileInfo;
        this.MimeType = mimeType;
        this.IsIncluded = isIncluded;
    }

    /// <summary>
    /// Gets the file name.
    /// </summary>
    [JsonConverter(typeof(StringObjectConverter<FileName>))]
    public FileName FileName { get; }

    /// <summary>
    /// Gets the mime type.
    /// </summary>
    public string MimeType { get; }

    /// <summary>
    /// Gets the file information.
    /// </summary>
    public FileInfo File { get; }

    /// <summary>
    /// Gets a value indicating whether attachment to be included.
    /// </summary>
    public bool IsIncluded { get; }
}
