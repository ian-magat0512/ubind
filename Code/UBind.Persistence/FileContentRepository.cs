// <copyright file="FileContentRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence
{
    using System;
    using System.Data.SqlClient;
    using System.Linq;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class FileContentRepository : IFileContentRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileContentRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The UBind Database context.</param>
        public FileContentRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public FileContent? GetFileContentById(Guid id)
        {
            return this.dbContext.FileContents.SingleOrDefault(fc => fc.Id == id);
        }

        /// <inheritdoc/>
        public FileContent? GetFileContentByQuoteFileAttachmentId(Guid tenantId, Guid attachmentId)
        {
            var file = this.dbContext.QuoteFileAttachments
                .Where(q => q.TenantId == tenantId && q.Id == attachmentId)
                .Join(this.dbContext.FileContents, q => q.FileContentId, f => f.Id, (q, f) => new { f.Id, f.Content })
                .SingleOrDefault();
            if (file == null)
            {
                return null;
            }
            return FileContent.CreateFromBytes(tenantId, file.Id, file.Content);
        }

        /// <inheritdoc/>
        public FileContent? GetFileContent(Guid tenantId, Guid quoteAttachmentOrFileContentId)
        {
            return this.GetFileContentById(quoteAttachmentOrFileContentId)
                ?? this.GetFileContentByQuoteFileAttachmentId(tenantId, quoteAttachmentOrFileContentId);
        }

        /// <inheritdoc/>
        public Guid Insert(FileContent fileContent)
        {
            var dbFileContentId = this.dbContext.FileContents
                .Where(f => f.TenantId == fileContent.TenantId && f.HashCode == fileContent.HashCode)
                .Select(f => f.Id)
                .FirstOrDefault();

            if (dbFileContentId != default)
            {
                return dbFileContentId;
            }

            var newFileContent = this.dbContext.FileContents.Add(fileContent);
            return newFileContent.Id;
        }

        /// <inheritdoc/>
        public void UpdateFileContentsAndHashCodes()
        {
            var ids = this.dbContext.ClaimFileAttachments.Select(c => c.FileContentId).Distinct().ToArray();

            foreach (var fileContentId in ids)
            {
                var updateSql = "UPDATE FileContents " +
                    "SET Content = CAST(CAST(CAST(content as XML).value('.','varbinary(max)') AS varchar(max)) AS varbinary(max)), HashCode='' " +
                    "WHERE Id = '" + fileContentId + "' AND HashCode IS NULL";
                try
                {
                    this.dbContext.Database.ExecuteSqlCommand(updateSql);
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex);
                }
            }

            // Update by a batch of 10 to avoid timeout
            var updateHashCode =
                "UPDATE TOP (10) FileContents SET HashCode=CONVERT(varchar(max), HASHBYTES('SHA2_256', content), 2) " +
                "WHERE ISNULL(HashCode, '')='' AND Content IS NOT NULL";

            int result;
            do
            {
                result = this.dbContext.Database.ExecuteSqlCommand(updateHashCode);
            }
            while (result > 0);
        }

        public FileContent? GetFileContentByHashCode(Guid tenantId, string hashCode)
        {
            return this.dbContext.FileContents
                .FirstOrDefault(f => f.TenantId == tenantId && f.HashCode == hashCode);
        }

        public bool HasFileContentWithHashCode(Guid tenantId, string hashCode)
        {
            return this.dbContext.FileContents.Any(f => f.TenantId == tenantId && f.HashCode == hashCode);
        }

        public Guid? GetFileContentIdOrNullForHashCode(Guid tenantId, string hashCode)
        {
            return this.dbContext.FileContents
                .Where(f => f.TenantId == tenantId && f.HashCode == hashCode)
                .Select(f => f.Id)
                .Cast<Guid?>()
                .FirstOrDefault();
        }
    }
}
