// <copyright file="DocumentFile.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Stores information of the document file attachment/s on a quote email.
    /// </summary>
    public class DocumentFile : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFile"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        public DocumentFile()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFile"/> class.
        /// </summary>
        /// <param name="name">The file name.</param>
        /// <param name="type">The file type.</param>
        /// <param name="fileContent">The file content.</param>
        /// <param name="createdTimestamp">The time this record was created.</param>
        public DocumentFile(string name, string type, FileContent fileContent, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.Name = name;
            this.Type = type;
            this.FileContentId = fileContent.Id;
            this.FileContent = fileContent;
        }

        /// <summary>
        /// Gets the file name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the file type.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Gets the file content Id of a document file email attachment.
        /// </summary>
        public Guid FileContentId { get; private set; }

        public FileContent FileContent { get; private set; }
    }
}
