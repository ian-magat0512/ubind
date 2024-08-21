// <copyright file="ClaimNumberRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;

    /// <summary>
    /// retrieve claim number records.
    /// </summary>
    public class ClaimNumberRepository : NumberPoolRepositoryBase<ClaimNumber>, IClaimNumberRepository
    {
        private readonly IUBindDbContext dbContext;
        private readonly DbSet<ClaimNumber> dbSet;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimNumberRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="connectionConfiguration">SQL connection configuration.</param>
        /// <param name="clock">Clock for obtaining the curent time.</param>
        public ClaimNumberRepository(IUBindDbContext dbContext, IConnectionConfiguration connectionConfiguration, IClock clock)
            : base(dbContext, connectionConfiguration, clock)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<ClaimNumber>();
            this.clock = clock;
        }

        /// <inheritdoc/>
        public override string Prefix
        {
            get { return "C-"; }
        }

        /// <inheritdoc/>
        public string AssignClaimNumber(Guid tenantId, Guid productId, string oldNumber, string newNumber, DeploymentEnvironment environment)
        {
            var claimNumberRecord = this.dbSet
                .Where(
                    cn => cn.TenantId == tenantId &&
                    cn.ProductId == productId &&
                    cn.Environment == environment &&
                    cn.Number == newNumber).FirstOrDefault();

            if (claimNumberRecord == null)
            {
                var newClaimNumber = (ClaimNumber)Activator.CreateInstance(typeof(ClaimNumber), tenantId, productId, environment, newNumber, this.clock.GetCurrentInstant());
                newClaimNumber.Consume();
                this.dbSet.Add(newClaimNumber);
            }
            else
            {
                claimNumberRecord.Consume();
            }

            this.dbContext.SaveChanges();

            return newNumber;
        }

        /// <inheritdoc/>
        public void UnassignClaimNumber(Guid tenantId, Guid productId, string oldNumber, DeploymentEnvironment environment, bool isRestoreOld = false)
        {
            var forUnassignClaimNumber = this.dbSet
               .Where(
                   cn => cn.TenantId == tenantId &&
                   cn.ProductId == productId &&
                   cn.Environment == environment &&
                   cn.Number == oldNumber &&
                   cn.IsAssigned == true).FirstOrDefault();

            if (forUnassignClaimNumber != null && !isRestoreOld)
            {
                this.dbSet.Remove(forUnassignClaimNumber);
                this.dbContext.SaveChanges();
            }
        }

        /// <inheritdoc/>
        public void ReuseOldClaimNumber(Guid tenantId, Guid productId, string oldClaimNumber, DeploymentEnvironment environment, bool save = true)
        {
            var originalClaimNumber = this.dbSet
                .Where(
                    cn => cn.TenantId == tenantId &&
                    cn.ProductId == productId &&
                    cn.Environment == environment &&
                    cn.Number == oldClaimNumber &&
                    cn.IsAssigned == true)
                .FirstOrDefault();
            if (originalClaimNumber != null)
            {
                originalClaimNumber.UnConsume();
            }

            if (save)
            {
                this.dbContext.SaveChanges();
            }
        }
    }
}
