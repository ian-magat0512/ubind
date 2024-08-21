// <copyright file="UniqueNumberSequenceGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Linq;
    using UBind.Domain;
    using UBind.Domain.NumberGenerators;

    /// <inheritdoc />
    public class UniqueNumberSequenceGenerator : IUniqueNumberSequenceGenerator
    {
        private readonly string connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueNumberSequenceGenerator"/> class.
        /// </summary>
        /// <param name="connectionString">A connection string for the database where sequence numbers are persisted.</param>
        public UniqueNumberSequenceGenerator(string connectionString)
        {
            this.connectionString = connectionString;
        }

        /// <inheritdoc/>
        public int Next(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            UniqueNumberUseCase useCase)
        {
            using (var dbContext = new UBindDbContext(this.connectionString))
            {
                var currentMax = dbContext
                    .Set<ReferenceNumberSequence>()
                    .Where(qsn => qsn.TenantId == tenantId)
                    .Where(qsn => qsn.ProductId == productId)
                    .Where(qsn => qsn.Environment == environment)
                    .Where(qsn => qsn.UseCase == useCase)
                    .Select(qsn => (int?)qsn.Number)
                    .Max();
                var newSeed = currentMax.HasValue ? currentMax.Value + 1 : 0;
                var newNumber = new ReferenceNumberSequence(tenantId, productId, environment, useCase, newSeed);
                dbContext.Set<ReferenceNumberSequence>().Add(newNumber);
                dbContext.SaveChanges();
                return newNumber.Number;
            }
        }

        /// <inheritdoc/>
        public int Next(
            Guid tenantId,
            DeploymentEnvironment environment,
            UniqueNumberUseCase useCase)
        {
            return this.Next(tenantId, Guid.Empty, environment, useCase);
        }
    }
}
