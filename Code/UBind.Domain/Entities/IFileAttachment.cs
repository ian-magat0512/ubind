// <copyright file="IFileAttachment.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Entities
{
    using System;
    using NodaTime;

    /// <summary>
    /// The file attachment interface.
    /// </summary>
    public interface IFileAttachment
    {
        /// <summary>
        /// Gets the ID for the document.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the document name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the document type.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the document size.
        /// </summary>
        long FileSize { get; }

        /// <summary>
        /// Gets the time the document was created.
        /// </summary>
        Instant CreatedTimestamp { get; }

        /// <summary>
        /// Gets the ID of the file content.
        /// </summary>
        Guid FileContentId { get; }
    }
}
