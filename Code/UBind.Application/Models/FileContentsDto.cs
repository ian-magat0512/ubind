// <copyright file="FileContentsDto.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Models;

using UBind.Domain;
using NodaTime;

public class FileContentsDto : UBind.Domain.Models.FileInfo
{
    public FileContentsDto(Asset asset, string path)
    {
        this.Path = path;
        this.Content = asset.FileContent.Content;
        this.CreatedTimestamp = asset.CreatedTimestamp;
        this.LastModifiedTimestamp = asset.FileModifiedTimestamp;
    }

    public FileContentsDto(
        string path,
        byte[] contents,
        Instant lastModifiedTimestamp)
    {
        this.Path = path;
        this.Content = contents;
        this.LastModifiedTimestamp = lastModifiedTimestamp;
    }

    public byte[] Content { get; set; }

    public string? ContentType { get; set; }
}
