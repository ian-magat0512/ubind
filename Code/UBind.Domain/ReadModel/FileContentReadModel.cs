// <copyright file="FileContentReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;

    /// <summary>
    /// For representing a file's content.
    /// </summary>
    public class FileContentReadModel : IFileContentReadModel
    {
        /// <inheritdoc/>
        public byte[] FileContent { get; set; }

        /// <inheritdoc/>
        public string ContentType { get; set; }

        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public string GetFileContentString()
        {
            return Convert.ToBase64String(this.FileContent);
        }
    }
}
