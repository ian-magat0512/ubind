// <copyright file="MissingCustomerRelationshipMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations
{
    using System;
    using System.Linq;
    using Hangfire;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain.Extensions;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.Migration;

    public class MissingCustomerRelationshipMigration : IMissingCustomerRelationshipMigration
    {
        private readonly IEmailRepository emailRepository;
        private readonly ITenantRepository tenantRepository;
        private readonly IUBindDbContext dbContext;
        private readonly ILogger<MissingCustomerRelationshipMigration> logger;
        private readonly IClock clock;

        public MissingCustomerRelationshipMigration(
            ITenantRepository tenantRepository,
            IEmailRepository emailRepository,
            IUBindDbContext dbContext,
            ILogger<MissingCustomerRelationshipMigration> logger,
            IClock clock)
        {
            this.emailRepository = emailRepository;
            this.tenantRepository = tenantRepository;
            this.dbContext = dbContext;
            this.logger = logger;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public void AddMissingCustomerRelationshipForQuayCommercialHull()
        {
            this.logger.LogInformation($"Migration started for customer relationship update.");
            this.ProcessUpdate(1);
        }

        [JobDisplayName("Startup Job: MissingCustomerRelationshipMigration Process Batch {0}")]
        private void ProcessUpdate(int batch)
        {
            int count = 0;
            var tenant = this.tenantRepository.GetActiveTenants(null, false)
                .FirstOrDefault(x => x.Details.Alias == "quay");

            if (tenant == null)
            {
                return;
            }

            var products = this.dbContext.Products
                .IncludeAllProperties()
                .Where(x => x.TenantId == tenant.Id)
                .ToList();

            foreach (var product in products.Where(x => x.Details.Alias == "commercial-hull"))
            {
                var emailIdsQuery = $"SELECT Id FROM Emails where TenantId = '{tenant.Id}'";
                var emailIds = this.dbContext.Database.SqlQuery<Guid>(emailIdsQuery).ToList();

                foreach (var emailId in emailIds)
                {
                    var tags = this.emailRepository.GetTags(tenant.Id, emailId).ToList();

                    var adminTag = tags
                       .FirstOrDefault(x => x.TagType == Domain.TagType.EmailType && x.Value == "Admin");

                    if (adminTag != null)
                    {
                        // skip
                        continue;
                    }

                    var customerTag = tags
                        .FirstOrDefault(x => x.TagType == Domain.TagType.EmailType && x.Value == "Customer");

                    // has customer tag.
                    if (customerTag != null)
                    {
                        // check if has customer relationship
                        var customerRelationships =
                            this.emailRepository.GetRelationships(tenant.Id, emailId, Domain.EntityType.Customer);

                        var quoteRelationships =
                            this.emailRepository.GetRelationships(tenant.Id, emailId, Domain.EntityType.Quote);

                        // if not
                        if (quoteRelationships.Any(x => x.FromEntityType == Domain.EntityType.Quote)
                            && !customerRelationships.Any(x => x.FromEntityType == Domain.EntityType.Customer))
                        {
                            if (count % 5 == 0)
                            {
                                this.logger.LogInformation($"Updated {count} records with one email being {emailId}");
                            }

                            var quoteRelationship = quoteRelationships.First(x => x.FromEntityType == Domain.EntityType.Quote);

                            var quote = this.dbContext.QuoteReadModels.FirstOrDefault(x => x.Id == quoteRelationship.FromEntityId);
                            var relationship = new Domain.ReadWriteModel.Relationship(
                                    tenant.Id,
                                    Domain.EntityType.Customer,
                                    quote.CustomerId.Value,
                                    Domain.RelationshipType.CustomerMessage,
                                    Domain.EntityType.Message,
                                    emailId,
                                    this.clock.Now());

                            // create customer relationship
                            this.emailRepository.InsertEmailRelationship(relationship);
                            count++;
                        }
                    }
                }
            }
        }
    }
}
