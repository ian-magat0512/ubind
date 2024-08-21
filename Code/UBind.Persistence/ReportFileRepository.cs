// <copyright file="ReportFileRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using UBind.Domain;
    using UBind.Domain.Repositories;

    /// <summary>
    /// The report file repository.
    /// </summary>
    public class ReportFileRepository : IReportFileRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportFileRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public ReportFileRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public void Insert(ReportFile reportFile)
        {
            this.dbContext.ReportFiles.Add(reportFile);
        }
    }
}
