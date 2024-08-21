// <copyright file="ApplicationFileAttachment.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using NodaTime;

    /// <summary>
    /// Stores information of file attachment/s on an application.
    /// </summary>
    public class ApplicationFileAttachment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationFileAttachment"/> class.
        /// </summary>
        /// <param name="id">An ID associating the attachment with a form field.</param>
        /// <param name="name">The file name.</param>
        /// <param name="type">The file type.</param>
        /// <param name="content">The file blob content.</param>
        /// <param name="createdTimestamp">The time this record was created.</param>
        public ApplicationFileAttachment(Guid id, string name, string type, string content, Instant createdTimestamp)
        {
            this.Id = id;
            this.Name = name;
            this.Type = type;
            this.Content = content;
            this.CreatedTimestamp = createdTimestamp;
        }

        /// <summary>
        /// Gets an ID associating this attachments with a form field.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets the file name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the file type.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the file blob content.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Gets the time the attachment was created.
        /// </summary>
        public Instant CreatedTimestamp { get; }
    }
}
