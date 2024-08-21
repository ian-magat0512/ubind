// <copyright file="FileContent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadWriteModel
{
    using System;
    using UBind.Domain.Helpers;

    /// <summary>
    /// For storing file content separate from file details.
    /// </summary>
    public class FileContent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileContent"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="id">The file content ID.</param>
        /// <param name="content">The file content in bytes.</param>
        private FileContent(Guid tenantId, Guid id, byte[] content)
        {
            this.TenantId = tenantId;
            this.Id = id;
            this.Content = content;
            this.HashCode = CryptographyHelper.ComputeHashString(content);
        }

        // Parameterless constructor for EF.
        private FileContent()
        {
        }

        /// <summary>
        /// Gets a unique identifier for the file content.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets the ID of the tenant for which the file content is for.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the file content.
        /// </summary>
        public byte[] Content { get; private set; }

        /// <summary>
        /// Gets the file hash code.
        /// </summary>
        public string HashCode { get; private set; }

        /// <summary>
        /// Gets the file size.
        /// </summary>
        public long Size => this.Content.Length;

        /// <summary>
        /// Factory method to create file content from base-64 string.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="id">The file content ID.</param>
        /// <param name="base64String">The base-64 string file content.</param>
        /// <returns>The file content.</returns>
        public static FileContent CreateFromBase64String(Guid tenantId, Guid id, string base64String)
        {
            return new FileContent(tenantId, id, Convert.FromBase64String(base64String));
        }

        /// <summary>
        /// Factory method to create file content from a byte array.
        /// </summary>
        /// <param name="id">The file content ID.</param>
        /// <param name="byteContent">The file content in byte array.</param>
        /// <returns>The file content.</returns>
        public static FileContent CreateFromBytes(Guid tenantId, Guid id, byte[] byteContent)
        {
            return new FileContent(tenantId, id, byteContent);
        }
    }
}
