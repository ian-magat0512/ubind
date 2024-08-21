// <copyright file="ThirdPartyDataSetsDbContext.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ThirdPartyDataSets
{
    using System.Data.Entity;
    using UBind.Domain.ThirdPartyDataSets;

    /// <summary>
    /// The EF database context for third party data sets.
    /// </summary>
    public class ThirdPartyDataSetsDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThirdPartyDataSetsDbContext"/> class with a particular connection string.
        /// </summary>
        /// <param name="nameOrConnectionString">The connection string for dbContext.</param>
        public ThirdPartyDataSetsDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThirdPartyDataSetsDbContext"/> class.
        /// </summary>
        public ThirdPartyDataSetsDbContext()
        {
        }

        /// <summary>
        /// Gets or sets state machine jobs DbSet.
        /// </summary>
        public DbSet<StateMachineJob> StateMachineJob { get; set; }
    }
}
