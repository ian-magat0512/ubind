// <copyright file="UniqueIdentifierService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Repository for quote numbers.
    /// </summary>
    public class UniqueIdentifierService : IUniqueIdentifierService
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IUBindDbContext dbContext;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueIdentifierService"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="cachingResolver">The tenand, product and portal resolver.</param>
        /// <param name="clock">Clock for obtaining the current time.</param>
        public UniqueIdentifierService(
            IUBindDbContext dbContext,
            ICachingResolver cachingResolver,
            IClock clock)
        {
            this.cachingResolver = cachingResolver;
            this.dbContext = dbContext;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<string> ConsumeUniqueIdentifier(IdentifierType type, Tenant tenant, Guid productId, DeploymentEnvironment environment)
        {
            var identifier = this.dbContext.UniqueIdentifiers
                .FirstOrDefault(id =>
                    id.Type == type &&
                    id.TenantId == tenant.Id &&
                    id.ProductId == productId &&
                    id.Environment == environment &&
                    id.Consumption == null);

            if (identifier == null)
            {
                Domain.Product.Product product = await this.cachingResolver.GetProductOrThrow(tenant.Id, productId);
                throw UniqueIdentifierUnavailableException.Create(type, tenant.Details.Alias, product.Details.Alias, environment);
            }

            var number = identifier.Consume(this.clock.GetCurrentInstant());
            this.dbContext.SaveChanges();
            return number;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetAvailableUniqueIdentifiers(
            Guid tenantId, Guid productId, DeploymentEnvironment environment, IdentifierType type)
        {
            return this.dbContext.UniqueIdentifiers
                .Where(id =>
                    id.Type == type &&
                    id.TenantId == tenantId &&
                    id.ProductId == productId &&
                    id.Environment == environment &&
                    id.Consumption == null)
                .Select(id => id.Identifier)
                .ToList();
        }

        /// <inheritdoc/>
        public void LoadUniqueIdentifiers(
            IdentifierType type,
            Tenant tenant,
            UBind.Domain.Product.Product product,
            DeploymentEnvironment environment,
            IEnumerable<string> idenitifiers)
        {
            var createdTimestamp = this.clock.GetCurrentInstant();
            var uniqueIdentifiers = idenitifiers.Select(number =>
            new UniqueIdentifier(tenant.Id, product.Id, environment, type, number, createdTimestamp));
            foreach (var uniqueIdenitifer in uniqueIdentifiers)
            {
                this.dbContext.UniqueIdentifiers.Add(uniqueIdenitifer);
            }

            try
            {
                this.dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                var message = ex.InnerException?.InnerException?.Message ?? string.Empty;
                var parser = new DuplicateEntryNumberKeyExceptionParser(tenant.Id, product.Id, environment, message);
                if (parser.Succeeded)
                {
                    throw new DuplicateUniqueIdentifierException(type, tenant.Details.Alias, product.Details.Alias, environment, parser.Number);
                }

                throw;
            }
        }
    }
}
