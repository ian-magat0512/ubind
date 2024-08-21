// <copyright file="QuoteDocumentReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Quote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Persistence;

    /// <summary>
    /// Repository for reading Quote and policy document file contents.
    /// </summary>
    public class QuoteDocumentReadModelRepository : IQuoteDocumentReadModelRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteDocumentReadModelRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The UBind database context.</param>
        public QuoteDocumentReadModelRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public IDocumentReadModelWithRelatedEntities GetDocumentWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment environment, Guid id, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForDocumentDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
            return query.FirstOrDefault(c => c.Document.Id == id);
        }

        /// <inheritdoc/>
        public IQueryable<DocumentReadModelWithRelatedEntities> CreateQueryForDocumentDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> relatedEntities)
        {
            var query = from document in this.dbContext.QuoteDocuments
                        where document.TenantId == tenantId && document.Environment == environment
                        select new DocumentReadModelWithRelatedEntities()
                        {
                            Tenant = default,
                            TenantDetails = new TenantDetails[] { },
                            Organisation = default,
                            Document = document,
                        };

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Document.Tenant))))
            {
                query = query.Join(this.dbContext.Tenants.IncludeAllProperties(), e => e.Document.TenantId, t => t.Id, (e, tenant) => new DocumentReadModelWithRelatedEntities
                {
                    Tenant = tenant,
                    TenantDetails = tenant.DetailsCollection,
                    Organisation = e.Organisation,
                    Document = e.Document,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Document.Organisation))))
            {
                query = query.Join(this.dbContext.OrganisationReadModel, e => e.Document.OrganisationId, t => t.Id, (e, organisation) => new DocumentReadModelWithRelatedEntities
                {
                    Tenant = e.Tenant,
                    TenantDetails = e.TenantDetails,
                    Organisation = organisation,
                    Document = e.Document,
                });
            }

            return query;
        }

        /// <inheritdoc/>
        public IFileContentReadModel GetDocumentContent(Guid tenantId, Guid documentId)
        {
            var entities = from qd in this.dbContext.QuoteDocuments
                           join fc in this.dbContext.FileContents on qd.FileContentId equals fc.Id
                           where qd.TenantId == tenantId && qd.Id == documentId
                           select new FileContentReadModel
                           {
                               FileContent = fc.Content,
                               ContentType = qd.Type,
                           };
            return entities.SingleOrDefault();
        }
    }
}
