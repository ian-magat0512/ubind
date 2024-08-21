// <copyright file="IFileContentReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    /// <summary>
    /// For accessing file content.
    /// </summary>
    public interface IFileContentReadModel
    {
        /// <summary>
        /// Gets the file content.
        /// </summary>
        byte[] FileContent { get; }

        /// <summary>
        /// Gets the file content type.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Gets the file name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the file content string.
        /// </summary>
        /// <returns>The file content string value.</returns>
        string GetFileContentString();
    }
}
