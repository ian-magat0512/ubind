// <copyright file="FileModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel
{
    /// <summary>
    /// Representation of a calculation result.
    /// </summary>
    public class FileModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileModel"/> class.
        /// constructor.
        /// </summary>
        /// <param name="fileName">fileName.</param>
        /// <param name="bytes">the file content.</param>
        /// <param name="contentType">content Type.</param>
        public FileModel(string fileName, byte[] bytes, string contentType)
        {
            this.FileName = fileName;
            this.Bytes = bytes;
            this.ContentType = contentType;
        }

        /// <summary>
        /// Gets gets and sets file names.
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets bytes.
        /// </summary>
        public byte[] Bytes { get; private set; }

        /// <summary>
        /// Gets content type.
        /// </summary>
        public string ContentType { get; private set; }
    }
}
