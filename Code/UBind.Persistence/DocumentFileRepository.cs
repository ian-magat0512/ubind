// <copyright file="DocumentFileRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class DocumentFileRepository : IDocumentFileRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFileRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public DocumentFileRepository(IUBindDbContext dbContext)
        {
            Contract.Assert(dbContext != null);
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public IFileContentReadModel GetFileContent(Guid id)
        {
            return this.dbContext.DocumentFile
                .Where(x => x.Id == id)
                .Select(x =>
                     new FileContentReadModel
                     {
                         FileContent = x.FileContent.Content,
                         ContentType = x.Type,
                     }).FirstOrDefault();
        }
    }
}
