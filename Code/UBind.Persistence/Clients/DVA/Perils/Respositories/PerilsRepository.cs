// <copyright file="PerilsRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Clients.DVA.Perils.Respositories
{
    using System.Linq;
    using UBind.Domain.Clients.DVA.Perils.Entities;
    using UBind.Domain.Clients.DVA.Perils.Interfaces;
    using UBind.Domain.Extensions;
    using UBind.Persistence.Clients.DVA.Migrations;

    /// <inheritdoc/>
    public class PerilsRepository : IPerilsRepository
    {
        private readonly DvaDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerilsRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public PerilsRepository(DvaDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public Peril GetDetailsByPropertyId(string propertyId)
        {
            return this.dbContext.Perils
                .AsNoTracking()
                .Where(p => p.GnafPid == propertyId)
                .OrderByDescending(p => p.EffectiveDate).FirstOrDefault();
        }

        /// <inheritdoc/>
        public Peril GetDetailsByPropertyIdForPolicyStartDate(string propertyId, string policyStartDate)
        {
            var startDate = policyStartDate
                .ToLocalDateFromIso8601OrddMMyyyyOrddMMyy(nameof(policyStartDate)).ToDateTimeUnspecified();
            return this.dbContext.Perils
                .AsNoTracking()
                .Where(p => p.GnafPid == propertyId)
                .Where(p => p.EffectiveDate <= startDate)
                .OrderByDescending(p => p.EffectiveDate)
                .FirstOrDefault();
        }
    }
}
