// <copyright file="PolicyTransactionReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading;
    using Dapper;
    using LinqKit;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class PolicyTransactionReadModelRepository : IPolicyTransactionReadModelRepository
    {
        private readonly IUBindDbContext dbContext;
        private readonly IConnectionConfiguration connection;
        private readonly Func<IUBindDbContext, Guid, DeploymentEnvironment, IQueryable<PolicyTransactionReadModelWithRelatedEntities>> baseQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyTransactionReadModelRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public PolicyTransactionReadModelRepository(
            IUBindDbContext dbContext,
            IConnectionConfiguration connection)
        {
            this.dbContext = dbContext;
            this.connection = connection;
            this.baseQuery = this.CreateCompiledQuery();
        }

        /// <summary>
        /// Gets an expression for instantiating summaries from QuoteReadModels for use in EF projections.
        /// </summary>
        private Expression<Func<PolicyTransactionQuoteReadModel, IPolicyTransactionReadModelSummary>> SummarySelector =>
            pt => new PolicyTransactionReadModelSummary
            {
                ProductName = pt.Product.DetailsCollection
                    .OrderByDescending(d => d.CreatedTicksSinceEpoch).FirstOrDefault().Name,
                CreatedTicksSinceEpoch = pt.PolicyTransaction.CreatedTicksSinceEpoch,
                PolicyNumber = pt.Policy.PolicyNumber,
                InvoiceNumber = pt.Quote.InvoiceNumber,
                CreditNoteNumber = pt.Quote.CreditNoteNumber,
                CustomerFullName = pt.Policy.CustomerFullName,
                CustomerEmail = pt.Policy.CustomerEmail,
                IsTestData = pt.Policy.IsTestData,
                Transaction = pt.PolicyTransaction as PolicyTransaction,
                SerializedLatestCalculationResult = pt.PolicyTransaction.PolicyData.SerializedCalculationResult,
                PaymentGateway = pt.Quote.PaymentGateway,
                PaymentResponseJson = pt.Quote.PaymentResponseJson,
                OrganisationName = pt.Organisation.Name,
                OrganisationAlias = pt.Organisation.Alias,
                AgentName = pt.Policy.OwnerFullName,
            };

        public PolicyTransaction GetById(Guid tenantId, Guid policyTransactionId)
        {
            return this.dbContext.PolicyTransactions.SingleOrDefault(
                pt => pt.TenantId == tenantId && pt.Id == policyTransactionId);
        }

        /// <inheritdoc/>
        public IEnumerable<IPolicyTransactionReadModelSummary> GetPolicyTransactions(
            Guid tenantId,
            Guid organisationId,
            IEnumerable<Guid> products,
            DeploymentEnvironment environment,
            Instant from,
            Instant to,
            bool includeTestData,
            IEnumerable<TransactionType> transactionFilter)
        {
            var query = this.QueryPolicyTransactions(
                tenantId, organisationId, products, environment, from, to, includeTestData, transactionFilter);
            return query.Select(this.SummarySelector);
        }

        /// <inheritdoc/>
        public IPolicyTransactionReadModelWithRelatedEntities GetPolicyTransactionWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment environment, Guid policyTransactionId, IEnumerable<string> relatedEntities)
        {
            var query =
                this.CreateQueryForPolicyTransactionDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
            return query.FirstOrDefault(c => c.PolicyTransaction.Id == policyTransactionId);
        }

        /// <inheritdoc/>
        public List<Guid> GetPolicyTransactionIdsByEntityFilters(
            EntityFilters entityFilters, Type type = null)
        {
            var queryable = this.dbContext.PolicyTransactions.Where(pt => pt.TenantId == entityFilters.TenantId);

            if (type != null)
            {
                if (type == typeof(NewBusinessTransaction))
                {
                    queryable = queryable.Where(pt => (pt as NewBusinessTransaction) != null);
                }

                if (type == typeof(CancellationTransaction))
                {
                    queryable = queryable.Where(pt => (pt as CancellationTransaction) != null);
                }

                if (type == typeof(RenewalTransaction))
                {
                    queryable = queryable.Where(pt => (pt as RenewalTransaction) != null);
                }

                if (type == typeof(AdjustmentTransaction))
                {
                    queryable = queryable.Where(pt => (pt as AdjustmentTransaction) != null);
                }
            }

            if (entityFilters.ProductId.HasValue)
            {
                queryable = queryable.Where(pt => pt.ProductId == entityFilters.ProductId);
            }

            if (entityFilters.OrganisationId.HasValue)
            {
                queryable = queryable.Where(pt => pt.OrganisationId == entityFilters.OrganisationId);
            }

            if (entityFilters.Skip.HasValue && entityFilters.PageSize.HasValue)
            {
                queryable = queryable.OrderByDescending(pt => pt.CreatedTicksSinceEpoch)
                    .Skip(entityFilters.Skip.Value).Take(entityFilters.PageSize.Value);
            }

            return queryable.Select(pt => pt.Id).ToList();
        }

        /// <inheritdoc/>
        public IQueryable<PolicyTransactionReadModelWithRelatedEntities> CreateQueryForPolicyTransactionDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> relatedEntities)
        {
            var query = this.baseQuery(this.dbContext, tenantId, environment);

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.PolicyTransaction.Policy))))
            {
                query = query.Join(this.dbContext.Policies, pt => pt.PolicyTransaction.PolicyId, p => p.Id, (pt, policy) => new PolicyTransactionReadModelWithRelatedEntities
                {
                    PolicyTransaction = pt.PolicyTransaction,
                    Quote = pt.Quote,
                    Policy = policy,
                    PolicyTransactions = pt.PolicyTransactions,
                    Customer = pt.Customer,
                    Tenant = pt.Tenant,
                    TenantDetails = pt.TenantDetails,
                    Organisation = pt.Organisation,
                    Product = pt.Product,
                    ProductDetails = pt.ProductDetails,
                    Owner = pt.Owner,
                    Documents = pt.Documents,
                    Emails = pt.Emails,
                    Sms = pt.Sms,
                    FromRelationships = pt.FromRelationships,
                    ToRelationships = pt.ToRelationships,
                    TextAdditionalPropertiesValues = pt.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = pt.StructuredDataAdditionalPropertyValues,
                    TimeZoneId = policy.TimeZoneId,
                });

                query = query.GroupJoin(this.dbContext.PolicyTransactions, pt => pt.PolicyTransaction.PolicyId, pts => pts.PolicyId, (pt, pts) => new PolicyTransactionReadModelWithRelatedEntities
                {
                    PolicyTransaction = pt.PolicyTransaction,
                    Quote = pt.Quote,
                    Policy = pt.Policy,
                    PolicyTransactions = pts,
                    Customer = pt.Customer,
                    Tenant = pt.Tenant,
                    TenantDetails = pt.TenantDetails,
                    Organisation = pt.Organisation,
                    Product = pt.Product,
                    ProductDetails = pt.ProductDetails,
                    Owner = pt.Owner,
                    Documents = pt.Documents,
                    Emails = pt.Emails,
                    Sms = pt.Sms,
                    FromRelationships = pt.FromRelationships,
                    ToRelationships = pt.ToRelationships,
                    TextAdditionalPropertiesValues = pt.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = pt.StructuredDataAdditionalPropertyValues,
                    TimeZoneId = pt.TimeZoneId,
                });
            }
            else
            {
                query = query.Join(this.dbContext.Policies, pt => pt.PolicyTransaction.PolicyId, p => p.Id, (pt, policy) => new PolicyTransactionReadModelWithRelatedEntities
                {
                    PolicyTransaction = pt.PolicyTransaction,
                    Quote = pt.Quote,
                    Policy = pt.Policy,
                    PolicyTransactions = pt.PolicyTransactions,
                    Customer = pt.Customer,
                    Tenant = pt.Tenant,
                    TenantDetails = pt.TenantDetails,
                    Organisation = pt.Organisation,
                    Product = pt.Product,
                    ProductDetails = pt.ProductDetails,
                    Owner = pt.Owner,
                    Documents = pt.Documents,
                    Emails = pt.Emails,
                    Sms = pt.Sms,
                    FromRelationships = pt.FromRelationships,
                    ToRelationships = pt.ToRelationships,
                    TextAdditionalPropertiesValues = pt.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = pt.StructuredDataAdditionalPropertyValues,
                    TimeZoneId = policy.TimeZoneId,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.PolicyTransaction.Tenant))))
            {
                query = query.Join(this.dbContext.Tenants, pt => pt.PolicyTransaction.TenantId, t => t.Id, (pt, tenant) => new PolicyTransactionReadModelWithRelatedEntities
                {
                    PolicyTransaction = pt.PolicyTransaction,
                    Quote = pt.Quote,
                    Policy = pt.Policy,
                    PolicyTransactions = pt.PolicyTransactions,
                    Customer = pt.Customer,
                    Tenant = tenant,
                    TenantDetails = tenant.DetailsCollection,
                    Organisation = pt.Organisation,
                    Product = pt.Product,
                    ProductDetails = pt.ProductDetails,
                    Owner = pt.Owner,
                    Documents = pt.Documents,
                    Emails = pt.Emails,
                    Sms = pt.Sms,
                    FromRelationships = pt.FromRelationships,
                    ToRelationships = pt.ToRelationships,
                    TextAdditionalPropertiesValues = pt.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = pt.StructuredDataAdditionalPropertyValues,
                    TimeZoneId = pt.TimeZoneId,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.PolicyTransaction.Organisation))))
            {
                query = query.Join(this.dbContext.OrganisationReadModel, pt => pt.PolicyTransaction.OrganisationId, t => t.Id, (pt, organisation) => new PolicyTransactionReadModelWithRelatedEntities
                {
                    PolicyTransaction = pt.PolicyTransaction,
                    Quote = pt.Quote,
                    Policy = pt.Policy,
                    PolicyTransactions = pt.PolicyTransactions,
                    Customer = pt.Customer,
                    Tenant = pt.Tenant,
                    TenantDetails = pt.TenantDetails,
                    Organisation = organisation,
                    Product = pt.Product,
                    ProductDetails = pt.ProductDetails,
                    Owner = pt.Owner,
                    Documents = pt.Documents,
                    Emails = pt.Emails,
                    Sms = pt.Sms,
                    FromRelationships = pt.FromRelationships,
                    ToRelationships = pt.ToRelationships,
                    TextAdditionalPropertiesValues = pt.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = pt.StructuredDataAdditionalPropertyValues,
                    TimeZoneId = pt.TimeZoneId,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.PolicyTransaction.Product))))
            {
                query = query.Join(
                    this.dbContext.Products.IncludeAllProperties(),
                    pt => new { tenantId = pt.PolicyTransaction.TenantId, productId = pt.PolicyTransaction.ProductId },
                    t => new { tenantId = t.TenantId, productId = t.Id },
                    (pt, product) => new PolicyTransactionReadModelWithRelatedEntities
                    {
                        PolicyTransaction = pt.PolicyTransaction,
                        Quote = pt.Quote,
                        Policy = pt.Policy,
                        PolicyTransactions = pt.PolicyTransactions,
                        Customer = pt.Customer,
                        Tenant = pt.Tenant,
                        TenantDetails = pt.TenantDetails,
                        Organisation = pt.Organisation,
                        Product = product,
                        ProductDetails = product.DetailsCollection.OrderByDescending(p => p.CreatedTicksSinceEpoch).Take(1),
                        Owner = pt.Owner,
                        Documents = pt.Documents,
                        Emails = pt.Emails,
                        Sms = pt.Sms,
                        FromRelationships = pt.FromRelationships,
                        ToRelationships = pt.ToRelationships,
                        TextAdditionalPropertiesValues = pt.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = pt.StructuredDataAdditionalPropertyValues,
                        TimeZoneId = pt.TimeZoneId,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.PolicyTransaction.Customer))))
            {
                query = query.Join(this.dbContext.CustomerReadModels, pt => pt.PolicyTransaction.CustomerId, c => c.Id, (pt, customer) => new PolicyTransactionReadModelWithRelatedEntities
                {
                    PolicyTransaction = pt.PolicyTransaction,
                    Quote = pt.Quote,
                    Policy = pt.Policy,
                    PolicyTransactions = pt.PolicyTransactions,
                    Customer = customer,
                    Tenant = pt.Tenant,
                    TenantDetails = pt.TenantDetails,
                    Organisation = pt.Organisation,
                    Product = pt.Product,
                    ProductDetails = pt.ProductDetails,
                    Owner = pt.Owner,
                    Documents = pt.Documents,
                    Emails = pt.Emails,
                    Sms = pt.Sms,
                    FromRelationships = pt.FromRelationships,
                    ToRelationships = pt.ToRelationships,
                    TextAdditionalPropertiesValues = pt.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = pt.StructuredDataAdditionalPropertyValues,
                    TimeZoneId = pt.TimeZoneId,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.PolicyTransaction.Owner))))
            {
                query = query.GroupJoin(this.dbContext.Users, pt => pt.PolicyTransaction.OwnerUserId, u => u.Id, (pt, users) => new { pt, users })
                 .SelectMany(
                     x => x.users.DefaultIfEmpty(),
                     (x, user) => new PolicyTransactionReadModelWithRelatedEntities
                     {
                         PolicyTransaction = x.pt.PolicyTransaction,
                         Quote = x.pt.Quote,
                         Policy = x.pt.Policy,
                         PolicyTransactions = x.pt.PolicyTransactions,
                         Customer = x.pt.Customer,
                         Tenant = x.pt.Tenant,
                         TenantDetails = x.pt.TenantDetails,
                         Organisation = x.pt.Organisation,
                         Product = x.pt.Product,
                         ProductDetails = x.pt.ProductDetails,
                         Owner = x.pt.PolicyTransaction.OwnerUserId != null ? user : null,
                         Documents = x.pt.Documents,
                         Emails = x.pt.Emails,
                         Sms = x.pt.Sms,
                         FromRelationships = x.pt.FromRelationships,
                         ToRelationships = x.pt.ToRelationships,
                         TextAdditionalPropertiesValues = x.pt.TextAdditionalPropertiesValues,
                         StructuredDataAdditionalPropertyValues = x.pt.StructuredDataAdditionalPropertyValues,
                         TimeZoneId = x.pt.TimeZoneId,
                     });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.PolicyTransaction.Quote))))
            {
                // policy transactions can be created without a quote, so we need to do a left join (e.g. use case is Policy Importation)
                query = query.GroupJoin(this.dbContext.QuoteReadModels, pt => pt.PolicyTransaction.QuoteId, t => t.Id, (pt, quote) => new PolicyTransactionReadModelWithRelatedEntities
                {
                    PolicyTransaction = pt.PolicyTransaction,
                    Quote = quote.FirstOrDefault(),
                    Policy = pt.Policy,
                    PolicyTransactions = pt.PolicyTransactions,
                    Customer = pt.Customer,
                    Tenant = pt.Tenant,
                    TenantDetails = pt.TenantDetails,
                    Organisation = pt.Organisation,
                    Product = pt.Product,
                    ProductDetails = pt.ProductDetails,
                    Owner = pt.Owner,
                    Documents = pt.Documents,
                    Emails = pt.Emails,
                    Sms = pt.Sms,
                    FromRelationships = pt.FromRelationships,
                    ToRelationships = pt.ToRelationships,
                    TextAdditionalPropertiesValues = pt.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = pt.StructuredDataAdditionalPropertyValues,
                    TimeZoneId = pt.TimeZoneId,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.PolicyTransaction.Documents))))
            {
                query = query.GroupJoin(this.dbContext.QuoteDocuments, pt => pt.PolicyTransaction.Id, c => c.QuoteOrPolicyTransactionId, (pt, documents) => new PolicyTransactionReadModelWithRelatedEntities
                {
                    PolicyTransaction = pt.PolicyTransaction,
                    Quote = pt.Quote,
                    Policy = pt.Policy,
                    PolicyTransactions = pt.PolicyTransactions,
                    Customer = pt.Customer,
                    Tenant = pt.Tenant,
                    TenantDetails = pt.TenantDetails,
                    Organisation = pt.Organisation,
                    Product = pt.Product,
                    ProductDetails = pt.ProductDetails,
                    Owner = pt.Owner,
                    Documents = documents,
                    Emails = pt.Emails,
                    Sms = pt.Sms,
                    FromRelationships = pt.FromRelationships,
                    ToRelationships = pt.ToRelationships,
                    TextAdditionalPropertiesValues = pt.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = pt.StructuredDataAdditionalPropertyValues,
                    TimeZoneId = pt.TimeZoneId,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.PolicyTransaction.Messages))))
            {
                var emailQuery = from email in this.dbContext.Emails
                                 join relationship in this.dbContext.Relationships
                                 on new { EmailId = email.Id, Type = RelationshipType.PolicyTransactionMessage, FromEntityType = EntityType.PolicyTransaction }
                                 equals new { EmailId = relationship.ToEntityId, Type = relationship.Type, FromEntityType = relationship.FromEntityType }
                                 select new
                                 {
                                     Email = email,
                                     RelationShip = relationship,
                                 };

                query = query.GroupJoin(emailQuery, pt => pt.PolicyTransaction.Id, c => c.RelationShip.FromEntityId, (pt, emails) => new PolicyTransactionReadModelWithRelatedEntities
                {
                    PolicyTransaction = pt.PolicyTransaction,
                    Quote = pt.Quote,
                    Policy = pt.Policy,
                    PolicyTransactions = pt.PolicyTransactions,
                    Customer = pt.Customer,
                    Tenant = pt.Tenant,
                    TenantDetails = pt.TenantDetails,
                    Organisation = pt.Organisation,
                    Product = pt.Product,
                    ProductDetails = pt.ProductDetails,
                    Owner = pt.Owner,
                    Documents = pt.Documents,
                    Emails = emails.Select(c => c.Email),
                    Sms = pt.Sms,
                    FromRelationships = pt.FromRelationships,
                    ToRelationships = pt.ToRelationships,
                    TextAdditionalPropertiesValues = pt.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = pt.StructuredDataAdditionalPropertyValues,
                    TimeZoneId = pt.TimeZoneId,
                });

                var smsQuery = from sms in this.dbContext.Sms
                               join relationship in this.dbContext.Relationships
                               on new { SmsId = sms.Id, Type = RelationshipType.PolicyTransactionMessage, FromEntityType = EntityType.PolicyTransaction }
                               equals new { SmsId = relationship.ToEntityId, Type = relationship.Type, FromEntityType = relationship.FromEntityType }
                               select new
                               {
                                   Sms = sms,
                                   RelationShip = relationship,
                               };

                query = query.GroupJoin(smsQuery, pt => pt.PolicyTransaction.Id, c => c.RelationShip.FromEntityId, (pt, sms) => new PolicyTransactionReadModelWithRelatedEntities
                {
                    PolicyTransaction = pt.PolicyTransaction,
                    Quote = pt.Quote,
                    Policy = pt.Policy,
                    PolicyTransactions = pt.PolicyTransactions,
                    Customer = pt.Customer,
                    Tenant = pt.Tenant,
                    TenantDetails = pt.TenantDetails,
                    Organisation = pt.Organisation,
                    Product = pt.Product,
                    ProductDetails = pt.ProductDetails,
                    Owner = pt.Owner,
                    Documents = pt.Documents,
                    Emails = pt.Emails,
                    Sms = sms.Select(s => s.Sms),
                    FromRelationships = pt.FromRelationships,
                    ToRelationships = pt.ToRelationships,
                    TextAdditionalPropertiesValues = pt.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = pt.StructuredDataAdditionalPropertyValues,
                    TimeZoneId = pt.TimeZoneId,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.PolicyTransaction.Relationships))))
            {
                query = query.GroupJoin(this.dbContext.Relationships, pt => pt.PolicyTransaction.Id, r => r.FromEntityId, (pt, relationships) => new PolicyTransactionReadModelWithRelatedEntities
                {
                    PolicyTransaction = pt.PolicyTransaction,
                    Quote = pt.Quote,
                    Policy = pt.Policy,
                    PolicyTransactions = pt.PolicyTransactions,
                    Customer = pt.Customer,
                    Tenant = pt.Tenant,
                    TenantDetails = pt.TenantDetails,
                    Organisation = pt.Organisation,
                    Product = pt.Product,
                    ProductDetails = pt.ProductDetails,
                    Owner = pt.Owner,
                    Documents = pt.Documents,
                    Emails = pt.Emails,
                    Sms = pt.Sms,
                    FromRelationships = relationships,
                    ToRelationships = pt.ToRelationships,
                    TextAdditionalPropertiesValues = pt.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = pt.StructuredDataAdditionalPropertyValues,
                    TimeZoneId = pt.TimeZoneId,
                });

                query = query.GroupJoin(this.dbContext.Relationships, pt => pt.PolicyTransaction.Id, r => r.ToEntityId, (pt, relationships) => new PolicyTransactionReadModelWithRelatedEntities
                {
                    PolicyTransaction = pt.PolicyTransaction,
                    Quote = pt.Quote,
                    Policy = pt.Policy,
                    PolicyTransactions = pt.PolicyTransactions,
                    Customer = pt.Customer,
                    Tenant = pt.Tenant,
                    TenantDetails = pt.TenantDetails,
                    Organisation = pt.Organisation,
                    Product = pt.Product,
                    ProductDetails = pt.ProductDetails,
                    Owner = pt.Owner,
                    Documents = pt.Documents,
                    Emails = pt.Emails,
                    Sms = pt.Sms,
                    FromRelationships = pt.FromRelationships,
                    ToRelationships = relationships,
                    TextAdditionalPropertiesValues = pt.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = pt.StructuredDataAdditionalPropertyValues,
                    TimeZoneId = pt.TimeZoneId,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.PolicyTransaction.AdditionalProperties))))
            {
                query = query.GroupJoin(
                    this.dbContext.TextAdditionalPropertValues.IncludeAllProperties(),
                    pt => pt.PolicyTransaction.Id,
                    apv => apv.EntityId,
                    (pt, apv) => new PolicyTransactionReadModelWithRelatedEntities
                    {
                        PolicyTransaction = pt.PolicyTransaction,
                        Quote = pt.Quote,
                        Policy = pt.Policy,
                        PolicyTransactions = pt.PolicyTransactions,
                        Customer = pt.Customer,
                        Tenant = pt.Tenant,
                        TenantDetails = pt.TenantDetails,
                        Organisation = pt.Organisation,
                        Product = pt.Product,
                        ProductDetails = pt.ProductDetails,
                        Owner = pt.Owner,
                        Documents = pt.Documents,
                        Emails = pt.Emails,
                        Sms = pt.Sms,
                        FromRelationships = pt.FromRelationships,
                        ToRelationships = pt.ToRelationships,
                        TextAdditionalPropertiesValues = (IEnumerable<TextAdditionalPropertyValueReadModel>)apv
                            .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                        StructuredDataAdditionalPropertyValues = pt.StructuredDataAdditionalPropertyValues,
                        TimeZoneId = pt.TimeZoneId,
                    })
                    .GroupJoin(
                    this.dbContext.StructuredDataAdditionalPropertyValues.IncludeAllProperties(),
                    pt => pt.PolicyTransaction.Id,
                    apv => apv.EntityId,
                    (pt, apv) => new PolicyTransactionReadModelWithRelatedEntities
                    {
                        PolicyTransaction = pt.PolicyTransaction,
                        Quote = pt.Quote,
                        Policy = pt.Policy,
                        PolicyTransactions = pt.PolicyTransactions,
                        Customer = pt.Customer,
                        Tenant = pt.Tenant,
                        TenantDetails = pt.TenantDetails,
                        Organisation = pt.Organisation,
                        Product = pt.Product,
                        ProductDetails = pt.ProductDetails,
                        Owner = pt.Owner,
                        Documents = pt.Documents,
                        Emails = pt.Emails,
                        Sms = pt.Sms,
                        FromRelationships = pt.FromRelationships,
                        ToRelationships = pt.ToRelationships,
                        TextAdditionalPropertiesValues = pt.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = (IEnumerable<StructuredDataAdditionalPropertyValueReadModel>)apv
                            .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                        TimeZoneId = pt.TimeZoneId,
                    });
            }

            return query;
        }

        /// <inheritdoc/>
        /// <inheritdoc/>
        public async Task<IEnumerable<PolicyTransactionDashboardSummaryModel>> ListPolicyTransactionForPeriodicSummary(Guid tenantId, PolicyReadModelFilters filters, CancellationToken cancellation)
        {
            filters.TenantId = tenantId;
            return await this.QueryPolicyTransactionForPeriodicSummary(filters, cancellation);
        }

        /// <inheritdoc/>
        public IQueryable<PolicyTransaction> GetByPolicyId(Guid policyId)
        {
            return this.dbContext.PolicyTransactions.Where(pt => pt.PolicyId == policyId);
        }

        private Func<IUBindDbContext, Guid, DeploymentEnvironment, IQueryable<PolicyTransactionReadModelWithRelatedEntities>> CreateCompiledQuery()
        {
            Expression<Func<IUBindDbContext, Guid, DeploymentEnvironment, IQueryable<PolicyTransactionReadModelWithRelatedEntities>>> expression =
                (db, tenantId, environment) =>
                from polTransaction in db.PolicyTransactions
                where polTransaction.TenantId == tenantId && polTransaction.Environment == environment
                select new PolicyTransactionReadModelWithRelatedEntities
                {
                    PolicyTransaction = polTransaction,
                    Quote = default,
                    Policy = default,
                    PolicyTransactions = new PolicyTransaction[] { },
                    Customer = default,
                    Tenant = default,
                    TenantDetails = new TenantDetails[] { },
                    Organisation = default,
                    Product = default,
                    ProductDetails = new ProductDetails[] { },
                    Owner = default,
                    Documents = new QuoteDocumentReadModel[] { },
                    Emails = new UBind.Domain.ReadWriteModel.Email.Email[] { },
                    Sms = new Sms[] { },
                    FromRelationships = new Relationship[] { },
                    ToRelationships = new Relationship[] { },
                    TextAdditionalPropertiesValues = new TextAdditionalPropertyValueReadModel[] { },
                    StructuredDataAdditionalPropertyValues = new StructuredDataAdditionalPropertyValueReadModel[] { },
                    TimeZoneId = null,
                };

            return expression.Compile();
        }

        /// <summary>
        /// This is a temporary hardcode method specific for ride-protect sub-organisation,
        /// to be removed after the implementation of UB-8372.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns>A query for policies.</returns>
        private IQueryable<PolicyTransaction> GetPolicyTransactionsByEntityFiltersForRideProtectProductFromOtherOrganisation(Guid tenantId, PolicyReadModelFilters filters)
        {
            var transactionQuery = this.dbContext.PolicyTransactions
                .Where(pt => pt.TenantId == tenantId)
                .Where(pt => !pt.IsTestData);

            if (filters.Environment.HasValue)
            {
                transactionQuery = transactionQuery.Where(pt => pt.Environment == filters.Environment.Value);
            }

            if (filters.ProductIds.Any())
            {
                ExpressionStarter<PolicyTransaction> productIdsPredicate =
                    PredicateBuilder.New<PolicyTransaction>(false);
                foreach (var productId in filters.ProductIds)
                {
                    productIdsPredicate = productIdsPredicate.Or(rm => rm.ProductId.Equals(productId));
                }

                transactionQuery = transactionQuery.Where(productIdsPredicate);
            }

            bool hasOrganisationFilter = filters.OrganisationIds != null && filters.OrganisationIds.Any();
            if (hasOrganisationFilter)
            {
                transactionQuery = transactionQuery.Where(pt => filters.OrganisationIds.Contains(pt.OrganisationId));
            }

            if (filters.OwnerUserId.HasValue)
            {
                transactionQuery = transactionQuery.Where(pt => pt.OwnerUserId == filters.OwnerUserId.Value);
            }

            if (filters.QuoteTypes.Any())
            {
                var transactionPredicate = PredicateBuilder.New<PolicyTransaction>(false);
                foreach (var quoteType in filters.QuoteTypes)
                {
                    if (Enum.TryParse<QuoteType>(quoteType, true, out QuoteType type))
                    {
                        transactionPredicate = transactionPredicate.Or(this.GetExpressionForTransactionTypeMatching(type));
                    }
                }

                transactionQuery = transactionQuery.Where(transactionPredicate);
            }

            if (filters.DateIsAfterTicks.HasValue)
            {
                transactionQuery = transactionQuery.Where(rm => rm.CreatedTicksSinceEpoch > filters.DateIsAfterTicks);
            }

            if (filters.DateIsBeforeTicks.HasValue)
            {
                transactionQuery = transactionQuery.Where(rm => rm.CreatedTicksSinceEpoch < filters.DateIsBeforeTicks);
            }

            return transactionQuery;
        }

        private IQueryable<PolicyTransactionQuoteReadModel> QueryPolicyTransactions(
            Guid tenantId,
            Guid organisationId,
            IEnumerable<Guid> productIds,
            DeploymentEnvironment environment,
            Instant fromTimestamp,
            Instant toTimestamp,
            bool includeTestData,
            IEnumerable<TransactionType> transactionFilter)
        {
            var query = this.JoinPolicyTransactionAndQuote(tenantId, environment);

            if (organisationId != default)
            {
                query = query.Where(j => j.Policy.OrganisationId == organisationId);
            }

            if (productIds != null && productIds.Any())
            {
                query = query.Where(j => productIds.Contains(j.Policy.ProductId));
            }

            if (fromTimestamp != default)
            {
                var fromTicks = fromTimestamp.ToUnixTimeTicks();
                query = query.Where(j => j.PolicyTransaction.CreatedTicksSinceEpoch >= fromTicks);
            }

            if (toTimestamp != default)
            {
                var toTicks = toTimestamp.ToUnixTimeTicks();
                query = query.Where(j => j.PolicyTransaction.CreatedTicksSinceEpoch <= toTicks);
            }

            if (!includeTestData)
            {
                query = query.Where(j => j.Policy.IsTestData == false);
            }

            if (transactionFilter.Any())
            {
                var transactionPredicate = PredicateBuilder.New<PolicyTransactionQuoteReadModel>(false);

                foreach (var transactionType in transactionFilter)
                {
                    transactionPredicate = transactionPredicate.Or(this.GetExpressionForTransactionTypeMatching(transactionType));
                }

                query = query.Where(transactionPredicate);
            }

            return query;
        }

        private Expression<Func<PolicyTransaction, bool>> GetExpressionForTransactionTypeMatching(QuoteType transactionType)
        {
            if (transactionType == QuoteType.NewBusiness)
            {
                return model => model is NewBusinessTransaction;
            }
            else if (transactionType == QuoteType.Adjustment)
            {
                return model => model is AdjustmentTransaction;
            }
            else if (transactionType == QuoteType.Renewal)
            {
                return model => model is RenewalTransaction;
            }
            else if (transactionType == QuoteType.Cancellation)
            {
                return model => model is CancellationTransaction;
            }

            throw new NotSupportedException($"Unsupported quote: {transactionType}.");
        }

        private Expression<Func<PolicyTransactionQuoteReadModel, bool>> GetExpressionForTransactionTypeMatching(TransactionType transactionType)
        {
            if (transactionType == TransactionType.NewBusiness)
            {
                return model => model.PolicyTransaction is NewBusinessTransaction;
            }
            else if (transactionType == TransactionType.Renewal)
            {
                return model => model.PolicyTransaction is RenewalTransaction;
            }
            else if (transactionType == TransactionType.Adjustment)
            {
                return model => model.PolicyTransaction is AdjustmentTransaction;
            }
            else if (transactionType == TransactionType.Cancellation)
            {
                return model => model.PolicyTransaction is CancellationTransaction;
            }

            throw new NotSupportedException($"Unsupported policy transaction type: {transactionType}.");
        }

        private string GetPolicyTransactionTypeFromQuoteType(string quoteTypeString)
        {
            var quoteType = QuoteType.NewBusiness;
            if (!Enum.TryParse(quoteTypeString, true, out quoteType))
            {
                throw new NotSupportedException($"Unsupported quote type: {quoteTypeString}.");
            }

            if (quoteType == QuoteType.NewBusiness)
            {
                return nameof(NewBusinessTransaction);
            }
            else if (quoteType == QuoteType.Renewal)
            {
                return nameof(RenewalTransaction);
            }
            else if (quoteType == QuoteType.Adjustment)
            {
                return nameof(AdjustmentTransaction);
            }
            else if (quoteType == QuoteType.Cancellation)
            {
                return nameof(CancellationTransaction);
            }

            throw new NotSupportedException($"Unsupported policy transaction type: {quoteType}.");
        }

        private IQueryable<PolicyTransactionQuoteReadModel> JoinPolicyTransactionAndQuote(Guid tenantId, DeploymentEnvironment environment)
        {
            var query = this.dbContext.Policies
                .Where(policy => policy.TenantId == tenantId && policy.Environment == environment)
                .GroupJoin(this.dbContext.PolicyTransactions, p => p.Id, pt => pt.PolicyId, (p, qpt) => new { p, qpt })
                .SelectMany(t => t.qpt.DefaultIfEmpty(), (t, p1) => new { t.p, p1 })
                .GroupJoin(this.dbContext.QuoteReadModels, t => t.p1 != null ? t.p1.QuoteId : t.p.QuoteId, q => q.Id, (t, qpolicy) => new { t.p, t.p1, qpolicy })
                .SelectMany(t => t.qpolicy.DefaultIfEmpty(), (t, quote) => new { t.p, t.p1, quote })
                .Join(
                    this.dbContext.Products.IncludeAllProperties(),
                    t => new { productId = t.p.ProductId, tenantId = t.p.TenantId },
                    product => new { productId = product.Id, tenantId = product.TenantId },
                    (t, product) => new { t.p, t.p1, t.quote, product })
                .GroupJoin(this.dbContext.OrganisationReadModel, t => t.p.OrganisationId, org => org.Id, (t, organisation) => new { t.p, t.p1, t.quote, t.product, organisation })
                .SelectMany(
                    t => t.organisation.DefaultIfEmpty(),
                    (t, orgFirst) => new PolicyTransactionQuoteReadModel
                    {
                        Quote = t.quote,
                        Policy = t.p,
                        PolicyTransaction = t.p1,
                        Product = t.product,
                        Organisation = orgFirst,
                    }).OrderBy(j => j.PolicyTransaction.CreatedTicksSinceEpoch);
            return query;
        }

        /// <summary>
        /// Generates a query using filters provided.
        /// </summary>
        /// <param name="filters">filters for policy transactions.</param>
        /// <returns>List of policy transactions that matches the filters.</returns>
        private async Task<IEnumerable<PolicyTransactionDashboardSummaryModel>> QueryPolicyTransactionForPeriodicSummary(PolicyReadModelFilters filters, CancellationToken cancellation)
        {
            StringBuilder queryBuilder = new StringBuilder();
            var parameters = new DynamicParameters();
            parameters.Add("@tenantId", filters.TenantId);
            parameters.Add("@environment", filters.Environment ?? DeploymentEnvironment.Production);
            var policyTransactionTypes = new List<string>();
            foreach (var quoteType in filters.QuoteTypes)
            {
                policyTransactionTypes.Add(this.GetPolicyTransactionTypeFromQuoteType(quoteType));
            }

            parameters.Add("@policyTransactionTypes", policyTransactionTypes);
            queryBuilder.Append(@"
                SELECT 
                pt.Id AS Id, 
				pt.Discriminator AS [PolicyTransactionType],
                pt.ProductId AS ProductId, 
                pt.CreatedTicksSinceEpoch AS [CreatedTicksSinceEpoch],
				pt.LastModifiedTicksSinceEpoch AS [LastModifiedTicksSinceEpoch],
				pt.TotalPayable AS [Amount]
                FROM dbo.PolicyTransactions AS pt
                WHERE (pt.IsTestData <> 1)
                AND (pt.TotalPayable is not null)
                AND (pt.TenantId = @tenantId) 
                AND (pt.Environment = @environment)
                AND (pt.Discriminator in @policyTransactionTypes)");

            if (filters.DateIsAfterTicks.HasValue || filters.DateIsBeforeTicks.HasValue)
            {
                if (filters.DateIsAfterTicks.HasValue)
                {
                    queryBuilder.Append(@" AND (pt.CreatedTicksSinceEpoch > @dateIsAfterTicks)");
                    parameters.Add("@dateIsAfterTicks", filters.DateIsAfterTicks);
                }

                if (filters.DateIsBeforeTicks.HasValue)
                {
                    queryBuilder.Append(@" AND (pt.CreatedTicksSinceEpoch < @dateIsBeforeTicks)");
                    parameters.Add("@dateIsBeforeTicks", filters.DateIsBeforeTicks);
                }
            }

            var organisationCondition = string.Empty;
            var organisationConditionForRideProtect = string.Empty;
            bool hasOrganisationFilter = filters.OrganisationIds != null && filters.OrganisationIds.Any();
            if (hasOrganisationFilter)
            {
                parameters.Add("@OrganisationIds", filters.OrganisationIds);
                organisationCondition = " AND (pt.OrganisationId IN @OrganisationIds)";
                organisationConditionForRideProtect = " AND (pt.OrganisationId NOT IN @OrganisationIds)";
            }

            bool queryIsForRideProtectOrganisation = filters.IsRideProtectOrganisation;
            bool hasQueryProductIds = filters.ProductIds.Any();
            if (queryIsForRideProtectOrganisation)
            {
                queryIsForRideProtectOrganisation = (hasQueryProductIds && filters.ProductIds.Contains(filters.RideProtectProductId.Value)) || !hasQueryProductIds;
            }

            if (queryIsForRideProtectOrganisation)
            {
                parameters.Add("@productIdRideProtect", filters.RideProtectProductId.Value);
                var productAndOrganisationCondition = $"(pt.ProductId = @productIdRideProtect {organisationConditionForRideProtect})";
                if (hasQueryProductIds)
                {
                    // policy transactions from ride-protect organisation of specified productIDs and ride-protect quote from any organisation
                    parameters.Add("@productIds", filters.ProductIds);
                    productAndOrganisationCondition = $"AND ({productAndOrganisationCondition} OR (pt.ProductId in @productIds {organisationCondition}))";
                }
                else
                {
                    // policy transactions from ride-protect organisation and ride-protect quote from any organisation
                    productAndOrganisationCondition = $"AND ({productAndOrganisationCondition} {organisationCondition.Replace("AND", "OR")})";
                }

                queryBuilder.Append(productAndOrganisationCondition);
            }
            else
            {
                queryBuilder.Append(organisationCondition);
                if (hasQueryProductIds)
                {
                    parameters.Add("@productIds", filters.ProductIds);
                    queryBuilder.Append(" AND (pt.ProductId in @productIds)");
                }
            }

            // filters for user permission to view policy transactions
            if (filters.OwnerUserId != null)
            {
                parameters.Add("@OwnerUserId", filters.OwnerUserId);
                queryBuilder.Append(" AND (pt.OwnerUserId = @OwnerUserId)");
            }

            if (filters.CustomerId != null)
            {
                parameters.Add("@CustomerId", filters.CustomerId);
                queryBuilder.Append(" AND (pt.CustomerId = @CustomerId)");
            }

            var sql = queryBuilder.ToString();
            sql = $"SELECT * from ({sql}) as qResult ORDER BY qResult.CreatedTicksSinceEpoch ASC";

            using (var connection = new SqlConnection(this.connection.UBind))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
                {
                    sql = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; " + sql;
                    var command = new CommandDefinition(
                        sql,
                        parameters,
                        transaction,
                        180,
                        System.Data.CommandType.Text,
                        CommandFlags.Buffered,
                        cancellation);
                    var result = await connection.QueryAsync<PolicyTransactionDashboardSummaryModel>(command);
                    transaction.Commit();
                    return result;
                }
            }
        }
    }
}