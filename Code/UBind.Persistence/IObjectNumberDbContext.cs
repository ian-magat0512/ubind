// <copyright file="IObjectNumberDbContext.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System.Data.Entity;
    using UBind.Domain;

    /// <summary>
    /// Interface to allow DI to instantiate separate instance of DbContext for policy numbers,
    /// so that it can save chanegs independently to the request aggregate.
    /// </summary>
    public interface IObjectNumberDbContext
    {
        /// <summary>
        /// Gets policy numbers DbSet.
        /// </summary>
        DbSet<PolicyNumber> PolicyNumbers { get; }

        /// <summary>
        /// Gets or sets the set of invoice number records.
        /// </summary>
        DbSet<InvoiceNumber> InvoiceNumbers { get; set; }

        /// <summary>
        /// Gets or sets the set of invoice number records.
        /// </summary>
        DbSet<ClaimNumber> ClaimNumbers { get; set; }

        /// <summary>
        /// Persist in memory changes to the database.
        /// </summary>
        /// <returns>Number of changes persisted.</returns>
        int SaveChanges();
    }
}
