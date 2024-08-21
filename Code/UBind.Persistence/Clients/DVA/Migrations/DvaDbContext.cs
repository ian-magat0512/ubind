// <copyright file="DvaDbContext.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Clients.DVA.Migrations
{
    using System.Data.Entity;
    using UBind.Domain.Clients.DVA.Perils.Entities;

    /// <summary>
    /// EF database context for DVA application.
    /// </summary>
    public class DvaDbContext : DbContext
    {
        /// <summary>
        /// Name used for the index enforcing unique Gnaf details Id.
        /// </summary>
        internal static readonly string GnafPidIndex = "AK_GnafPidIndex";

        /// <summary>
        /// Initializes a new instance of the <see cref="DvaDbContext"/> class.
        /// </summary>
        public DvaDbContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DvaDbContext"/> class with a particular connection string.
        /// </summary>
        /// <param name="nameOrConnectionString">The connection string or name of connection string in settings.</param>
        public DvaDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        /// <summary>
        /// Gets or sets the set of login attempts.
        /// </summary>
        public virtual DbSet<Peril> Perils { get; set; }

        /// <inheritdoc/>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            this.ApplyCustomPrimaryKeys(modelBuilder);
            this.ApplyUniqueIndexes(modelBuilder);
        }

        private void ApplyCustomPrimaryKeys(DbModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Peril>()
                .HasKey(x => new { x.GnafPid, x.EffectiveDate });
        }

        private void ApplyUniqueIndexes(DbModelBuilder modelBuilder)
        {
            modelBuilder
               .Entity<Peril>()
               .Property(inv => inv.GnafPid)
               .HasColumnType("VARCHAR")
               .HasMaxLength(25)
               .HasUniqueIndexAnnotation(GnafPidIndex, 0);

            modelBuilder
               .Entity<Peril>()
               .Property(inv => inv.EffectiveDate)
               .HasUniqueIndexAnnotation(GnafPidIndex, 1);
        }
    }
}
