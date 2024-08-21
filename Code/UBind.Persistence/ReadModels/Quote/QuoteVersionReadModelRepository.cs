// <copyright file="QuoteVersionReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Quote
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;

    /// <inheritdoc />
    public class QuoteVersionReadModelRepository : IQuoteVersionReadModelRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteVersionReadModelRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public QuoteVersionReadModelRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private Expression<Func<QuoteVersionReadModel, IQuoteVersionReadModelSummary>> SummarySelector =>
            (qv) => new QuoteVersionReadModelSummary
            {
                WorkflowStep = qv.WorkflowStep,
                State = qv.State,
                QuoteVersionId = qv.QuoteVersionId,
                Id = qv.Id,
                AggregateId = qv.AggregateId,
                QuoteId = qv.QuoteId,
                QuoteVersionNumber = qv.QuoteVersionNumber,
                LatestFormData = qv.LatestFormData,
                CustomerId = qv.CustomerId,
                CustomerPersonId = qv.CustomerPersonId,
                CustomerFullName = qv.CustomerFullName,
                CustomerEmail = qv.CustomerEmail,
                CustomerAlternativeEmail = qv.CustomerAlternativeEmail,
                CustomerMobilePhone = qv.CustomerMobilePhone,
                CustomerHomePhone = qv.CustomerHomePhone,
                CustomerWorkPhone = qv.CustomerWorkPhone,
                QuoteNumber = qv.QuoteNumber,
                OwnerUserId = qv.OwnerUserId,
                OwnerPersonId = qv.OwnerPersonId,
                OwnerFullName = qv.OwnerFullName,
                CalculationResultJson = qv.CalculationResultJson,
                LastModifiedTicksSinceEpoch = qv.LastModifiedTicksSinceEpoch,
                CreatedTicksSinceEpoch = qv.CreatedTicksSinceEpoch,
            };

        private Expression<Func<QuoteVersionReadModel, IQuoteVersionReadModelDetails>> DetailSelector =>
            (qv) => new QuoteVersionReadModelDetails
            {
                WorkflowStep = qv.WorkflowStep,
                State = qv.State,
                QuoteVersionId = qv.QuoteVersionId,
                Id = qv.Id,
                AggregateId = qv.AggregateId,
                QuoteId = qv.QuoteId,
                QuoteVersionNumber = qv.QuoteVersionNumber,
                LatestFormData = qv.LatestFormData,
                CustomerId = qv.CustomerId,
                CustomerPersonId = qv.CustomerPersonId,
                CustomerFullName = qv.CustomerFullName,
                CustomerEmail = qv.CustomerEmail,
                CustomerAlternativeEmail = qv.CustomerAlternativeEmail,
                CustomerMobilePhone = qv.CustomerMobilePhone,
                CustomerHomePhone = qv.CustomerHomePhone,
                CustomerWorkPhone = qv.CustomerWorkPhone,
                QuoteNumber = qv.QuoteNumber,
                OwnerUserId = qv.OwnerUserId,
                OwnerPersonId = qv.OwnerPersonId,
                OwnerFullName = qv.OwnerFullName,
                CalculationResultJson = qv.CalculationResultJson,
                SerializedCalculationResult = qv.SerializedCalculationResult,
                LastModifiedTicksSinceEpoch = qv.LastModifiedTicksSinceEpoch,
                CreatedTicksSinceEpoch = qv.CreatedTicksSinceEpoch,
                Documents = this.dbContext.QuoteDocuments.Where(d => d.QuoteOrPolicyTransactionId == qv.QuoteVersionId),
            };

        public QuoteVersionReadModel GetById(Guid tenantId, Guid quoteVersionId)
        {
            return this.dbContext.QuoteVersions.Where(v => v.TenantId == tenantId && v.Id == quoteVersionId)
                .SingleOrDefault();
        }

        /// <inheritdoc />
        public IEnumerable<IQuoteVersionReadModelSummary> GetDetailVersionsOfQuote(Guid tenantId, Guid quoteId)
        {
            var query = this.dbContext.QuoteVersions.Where(v => v.QuoteId == quoteId);

            // TODO: We are not checking tenant ID yet since the read model writer sends null here.
            // we will fix this by making the tenant ID available along with events, in a separate ticket.
            if (tenantId != default)
            {
                query = query.Where(v => v.TenantId == tenantId);
            }

            var query2 = query
                       .Where(v => v.QuoteId == quoteId)
                        .OrderByDescending(x => x.CreatedTicksSinceEpoch)
                        .Take(() => 100)
                        .Select(this.SummarySelector);

            return query2.ToList();
        }

        /// <inheritdoc />
        public IQuoteVersionReadModelDetails GetVersionDetailsByVersionNumber(Guid tenantId, Guid quoteId, int versionNumber)
        {
            return this.dbContext.QuoteVersions
                .Where(v => v.TenantId == tenantId && v.QuoteId == quoteId)
                .Where(v => v.QuoteVersionNumber == versionNumber)
                .Select(this.DetailSelector)
                .SingleOrDefault();
        }

        /// <inheritdoc />
        public IQuoteVersionReadModelDetails GetVersionDetailsById(Guid tenantId, Guid versionId)
        {
            var quoteVersion = this.dbContext.QuoteVersions
                .Where(v => v.TenantId == tenantId && v.Id == versionId)
                .Select(this.DetailSelector)
                .SingleOrDefault();

            return quoteVersion;
        }

        /// <inheritdoc />
        public IQuoteVersionReadModelWithRelatedEntities GetQuoteVersionWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, Guid quoteVersionId, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForQuoteDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
            return query.FirstOrDefault(q => q.QuoteVersion.QuoteVersionId == quoteVersionId);
        }

        /// <inheritdoc />
        public IQuoteVersionReadModelWithRelatedEntities GetQuoteVersionWithRelatedEntities(
            Guid tenantId,
            DeploymentEnvironment? environment,
            Guid quoteId,
            int versionNumber,
            IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForQuoteDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
            return query.FirstOrDefault(
                q => q.QuoteVersion.QuoteId == quoteId && q.QuoteVersion.QuoteVersionNumber == versionNumber);
        }

        /// <inheritdoc />
        public IQuoteVersionReadModelWithRelatedEntities GetQuoteVersionWithRelatedEntities(
            Guid tenantId,
            string quoteReference,
            DeploymentEnvironment? environment,
            int versionNumber,
            IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForQuoteDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
            return query.FirstOrDefault(
                q => q.QuoteVersion.QuoteNumber == quoteReference &&
                q.QuoteVersion.QuoteVersionNumber == versionNumber);
        }

        /// <inheritdoc />
        public IQueryable<QuoteVersionReadModelWithRelatedEntities> CreateQueryForQuoteDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, IEnumerable<string> relatedEntities)
        {
            var query = this.dbContext.QuoteVersions.Where(qv => qv.TenantId == tenantId);
            if (environment.HasValue)
            {
                query = query.Where(qv => qv.Environment == environment);
            }
            var querRelatedEntities = from quoteVersion in query
                                      select new QuoteVersionReadModelWithRelatedEntities
                                      {
                                          QuoteVersion = quoteVersion,
                                          Customer = default,
                                          Tenant = default,
                                          Organisation = default,
                                          TenantDetails = new TenantDetails[] { },
                                          Product = default,
                                          ProductDetails = new ProductDetails[] { },
                                          Owner = default,
                                          Documents = new QuoteDocumentReadModel[] { },
                                          Emails = new Domain.ReadWriteModel.Email.Email[] { },
                                          Quote = default,
                                          Sms = new Sms[] { },
                                          FromRelationships = new Relationship[] { },
                                          ToRelationships = new Relationship[] { },
                                          TextAdditionalPropertiesValues = new TextAdditionalPropertyValueReadModel[] { },
                                          StructuredDataAdditionalPropertyValues = new StructuredDataAdditionalPropertyValueReadModel[] { },
                                      };

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.QuoteVersion.Quote))))
            {
                querRelatedEntities = querRelatedEntities.Join(this.dbContext.QuoteReadModels, qv => qv.QuoteVersion.QuoteId, t => t.Id, (qv, quote) => new QuoteVersionReadModelWithRelatedEntities
                {
                    QuoteVersion = qv.QuoteVersion,
                    Customer = qv.Customer,
                    Tenant = qv.Tenant,
                    Organisation = qv.Organisation,
                    TenantDetails = qv.TenantDetails,
                    Product = qv.Product,
                    ProductDetails = qv.ProductDetails,
                    Owner = qv.Owner,
                    Documents = qv.Documents,
                    Emails = qv.Emails,
                    Quote = quote,
                    Sms = qv.Sms,
                    FromRelationships = qv.FromRelationships,
                    ToRelationships = qv.ToRelationships,
                    TextAdditionalPropertiesValues = qv.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = qv.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.QuoteVersion.Tenant))))
            {
                querRelatedEntities = querRelatedEntities.Join(this.dbContext.Tenants, qv => qv.QuoteVersion.TenantId, t => t.Id, (qv, tenant) => new QuoteVersionReadModelWithRelatedEntities
                {
                    QuoteVersion = qv.QuoteVersion,
                    Customer = qv.Customer,
                    Tenant = tenant,
                    Organisation = qv.Organisation,
                    TenantDetails = tenant.DetailsCollection,
                    Product = qv.Product,
                    ProductDetails = qv.ProductDetails,
                    Owner = qv.Owner,
                    Documents = qv.Documents,
                    Emails = qv.Emails,
                    Quote = qv.Quote,
                    Sms = qv.Sms,
                    FromRelationships = qv.FromRelationships,
                    ToRelationships = qv.ToRelationships,
                    TextAdditionalPropertiesValues = qv.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = qv.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.QuoteVersion.Organisation))))
            {
                querRelatedEntities = querRelatedEntities.Join(this.dbContext.OrganisationReadModel, qv => qv.QuoteVersion.OrganisationId, t => t.Id, (qv, organisation) => new QuoteVersionReadModelWithRelatedEntities
                {
                    QuoteVersion = qv.QuoteVersion,
                    Customer = qv.Customer,
                    Tenant = qv.Tenant,
                    Organisation = organisation,
                    TenantDetails = qv.TenantDetails,
                    Product = qv.Product,
                    ProductDetails = qv.ProductDetails,
                    Owner = qv.Owner,
                    Documents = qv.Documents,
                    Emails = qv.Emails,
                    Quote = qv.Quote,
                    Sms = qv.Sms,
                    FromRelationships = qv.FromRelationships,
                    ToRelationships = qv.ToRelationships,
                    TextAdditionalPropertiesValues = qv.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = qv.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.QuoteVersion.Product))))
            {
                querRelatedEntities = querRelatedEntities.Join(this.dbContext.Products, qv => new { tenantId = qv.QuoteVersion.TenantId, productId = qv.QuoteVersion.ProductId }, t => new { tenantId = t.TenantId, productId = t.Id }, (qv, product) => new QuoteVersionReadModelWithRelatedEntities
                {
                    QuoteVersion = qv.QuoteVersion,
                    Customer = qv.Customer,
                    Tenant = qv.Tenant,
                    Organisation = qv.Organisation,
                    TenantDetails = qv.TenantDetails,
                    Product = product,
                    ProductDetails = product.DetailsCollection,
                    Owner = qv.Owner,
                    Documents = qv.Documents,
                    Emails = qv.Emails,
                    Quote = qv.Quote,
                    Sms = qv.Sms,
                    FromRelationships = qv.FromRelationships,
                    ToRelationships = qv.ToRelationships,
                    TextAdditionalPropertiesValues = qv.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = qv.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.QuoteVersion.Customer))))
            {
                querRelatedEntities = querRelatedEntities.Join(this.dbContext.CustomerReadModels, qv => qv.QuoteVersion.CustomerId, c => c.Id, (qv, customer) => new QuoteVersionReadModelWithRelatedEntities
                {
                    QuoteVersion = qv.QuoteVersion,
                    Customer = customer,
                    Tenant = qv.Tenant,
                    Organisation = qv.Organisation,
                    TenantDetails = qv.TenantDetails,
                    Product = qv.Product,
                    ProductDetails = qv.ProductDetails,
                    Owner = qv.Owner,
                    Documents = qv.Documents,
                    Emails = qv.Emails,
                    Quote = qv.Quote,
                    Sms = qv.Sms,
                    FromRelationships = qv.FromRelationships,
                    ToRelationships = qv.ToRelationships,
                    TextAdditionalPropertiesValues = qv.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = qv.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.QuoteVersion.Owner))))
            {
                {
                    querRelatedEntities = querRelatedEntities.GroupJoin(this.dbContext.Users, qv => qv.QuoteVersion.OwnerUserId, u => u.Id, (qv, users) => new { qv, users })
                        .SelectMany(
                            x => x.users.DefaultIfEmpty(),
                            (x, user) => new QuoteVersionReadModelWithRelatedEntities
                            {
                                QuoteVersion = x.qv.QuoteVersion,
                                Customer = x.qv.Customer,
                                Tenant = x.qv.Tenant,
                                Organisation = x.qv.Organisation,
                                TenantDetails = x.qv.TenantDetails,
                                Product = x.qv.Product,
                                ProductDetails = x.qv.ProductDetails,
                                Owner = x.qv.QuoteVersion.OwnerUserId != null ? user : null,
                                Documents = x.qv.Documents,
                                Emails = x.qv.Emails,
                                Quote = x.qv.Quote,
                                Sms = x.qv.Sms,
                                FromRelationships = x.qv.FromRelationships,
                                ToRelationships = x.qv.ToRelationships,
                                TextAdditionalPropertiesValues = x.qv.TextAdditionalPropertiesValues,
                                StructuredDataAdditionalPropertyValues = x.qv.StructuredDataAdditionalPropertyValues,
                            });
                }
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.QuoteVersion.Documents))))
            {
                querRelatedEntities = querRelatedEntities.GroupJoin(this.dbContext.QuoteDocuments, qv => qv.QuoteVersion.QuoteVersionId, c => c.QuoteOrPolicyTransactionId, (qv, documents) => new QuoteVersionReadModelWithRelatedEntities
                {
                    QuoteVersion = qv.QuoteVersion,
                    Customer = qv.Customer,
                    Tenant = qv.Tenant,
                    Organisation = qv.Organisation,
                    TenantDetails = qv.TenantDetails,
                    Product = qv.Product,
                    ProductDetails = qv.ProductDetails,
                    Owner = qv.Owner,
                    Documents = documents,
                    Emails = qv.Emails,
                    Quote = qv.Quote,
                    Sms = qv.Sms,
                    FromRelationships = qv.FromRelationships,
                    ToRelationships = qv.ToRelationships,
                    TextAdditionalPropertiesValues = qv.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = qv.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.QuoteVersion.Messages))))
            {
                var emailQuery = from email in this.dbContext.Emails
                                 join relationship in this.dbContext.Relationships on new { EmailId = email.Id, Type = RelationshipType.QuoteVersionMessage, FromEntityType = EntityType.QuoteVersion } equals new { EmailId = relationship.ToEntityId, relationship.Type, relationship.FromEntityType }
                                 select new
                                 {
                                     Email = email,
                                     RelationShip = relationship,
                                 };

                querRelatedEntities = querRelatedEntities.GroupJoin(emailQuery, qv => qv.QuoteVersion.QuoteVersionId, c => c.RelationShip.FromEntityId, (qv, emails) => new QuoteVersionReadModelWithRelatedEntities
                {
                    QuoteVersion = qv.QuoteVersion,
                    Customer = qv.Customer,
                    Tenant = qv.Tenant,
                    Organisation = qv.Organisation,
                    TenantDetails = qv.TenantDetails,
                    Product = qv.Product,
                    ProductDetails = qv.ProductDetails,
                    Owner = qv.Owner,
                    Documents = qv.Documents,
                    Emails = emails.Select(c => c.Email),
                    Quote = qv.Quote,
                    Sms = qv.Sms,
                    FromRelationships = qv.FromRelationships,
                    ToRelationships = qv.ToRelationships,
                    TextAdditionalPropertiesValues = qv.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = qv.StructuredDataAdditionalPropertyValues,
                });

                var smsQuery = from sms in this.dbContext.Sms
                               join relationship in this.dbContext.Relationships on new { SmsId = sms.Id, Type = RelationshipType.QuoteVersionMessage, FromEntityType = EntityType.QuoteVersion } equals new { SmsId = relationship.ToEntityId, relationship.Type, relationship.FromEntityType }
                               select new
                               {
                                   Sms = sms,
                                   RelationShip = relationship,
                               };

                querRelatedEntities = querRelatedEntities.GroupJoin(smsQuery, qv => qv.QuoteVersion.QuoteVersionId, c => c.RelationShip.FromEntityId, (qv, sms) => new QuoteVersionReadModelWithRelatedEntities
                {
                    QuoteVersion = qv.QuoteVersion,
                    Customer = qv.Customer,
                    Tenant = qv.Tenant,
                    Organisation = qv.Organisation,
                    TenantDetails = qv.TenantDetails,
                    Product = qv.Product,
                    ProductDetails = qv.ProductDetails,
                    Owner = qv.Owner,
                    Documents = qv.Documents,
                    Emails = qv.Emails,
                    Quote = qv.Quote,
                    Sms = sms.Select(s => s.Sms),
                    FromRelationships = qv.FromRelationships,
                    ToRelationships = qv.ToRelationships,
                    TextAdditionalPropertiesValues = qv.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = qv.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.QuoteVersion.Relationships))))
            {
                querRelatedEntities = querRelatedEntities.GroupJoin(this.dbContext.Relationships, qv => qv.QuoteVersion.QuoteVersionId, r => r.FromEntityId, (qv, relationships) => new QuoteVersionReadModelWithRelatedEntities
                {
                    QuoteVersion = qv.QuoteVersion,
                    Customer = qv.Customer,
                    Tenant = qv.Tenant,
                    Organisation = qv.Organisation,
                    TenantDetails = qv.TenantDetails,
                    Product = qv.Product,
                    ProductDetails = qv.ProductDetails,
                    Owner = qv.Owner,
                    Documents = qv.Documents,
                    Emails = qv.Emails,
                    Quote = qv.Quote,
                    Sms = qv.Sms,
                    FromRelationships = relationships,
                    ToRelationships = qv.ToRelationships,
                    TextAdditionalPropertiesValues = qv.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = qv.StructuredDataAdditionalPropertyValues,
                });

                querRelatedEntities = querRelatedEntities.GroupJoin(this.dbContext.Relationships, qv => qv.QuoteVersion.QuoteVersionId, r => r.ToEntityId, (qv, relationships) => new QuoteVersionReadModelWithRelatedEntities
                {
                    QuoteVersion = qv.QuoteVersion,
                    Customer = qv.Customer,
                    Tenant = qv.Tenant,
                    Organisation = qv.Organisation,
                    TenantDetails = qv.TenantDetails,
                    Product = qv.Product,
                    ProductDetails = qv.ProductDetails,
                    Owner = qv.Owner,
                    Documents = qv.Documents,
                    Emails = qv.Emails,
                    Quote = qv.Quote,
                    Sms = qv.Sms,
                    FromRelationships = qv.FromRelationships,
                    ToRelationships = relationships,
                    TextAdditionalPropertiesValues = qv.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = qv.StructuredDataAdditionalPropertyValues,
                });
            }

            return querRelatedEntities;
        }

        /// <inheritdoc/>
        public List<Guid> GetQuoteVersionIdsByEntityFilters(EntityFilters entityFilters)
        {
            var query = this.dbContext.QuoteVersions.Where(qv => qv.TenantId == entityFilters.TenantId);

            if (entityFilters.ProductId.HasValue)
            {
                query = query.Where(qv => qv.ProductId == entityFilters.ProductId.Value);
            }

            if (entityFilters.OrganisationId.HasValue)
            {
                query = query.Where(qv => qv.OrganisationId == entityFilters.OrganisationId);
            }

            if (entityFilters.Skip.HasValue && entityFilters.PageSize.HasValue)
            {
                query = query.OrderByDescending(
                    qv => qv.CreatedTicksSinceEpoch).Skip(entityFilters.Skip.Value).Take(
                    entityFilters.PageSize.Value);
            }

            return query.Select(qv => qv.QuoteVersionId).ToList();
        }
    }
}
