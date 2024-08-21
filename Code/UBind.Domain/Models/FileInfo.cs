// <copyright file="FileInfo.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Models;

using NodaTime;

public class FileInfo
{
    /// <summary>
    /// Gets or sets the path to the file, including the file name.
    /// This could be relative or absolute and should be interpreted according to how it was provided.
    /// </summary>
    public string Path { get; set; }

    public Instant CreatedTimestamp { get; set; }

    public Instant LastModifiedTimestamp { get; set; }
}
