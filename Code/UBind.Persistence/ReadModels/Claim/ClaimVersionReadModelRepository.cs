// <copyright file="ClaimVersionReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Claim
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
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Persistence;

    /// <inheritdoc/>
    public class ClaimVersionReadModelRepository : IClaimVersionReadModelRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimVersionReadModelRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public ClaimVersionReadModelRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private Expression<Func<ClaimVersionReadModel, IClaimVersionReadModelDetails>> DetailSelector =>
            (cv) => new ClaimVersionReadModelDetails
            {
                Id = cv.Id,
                AggregateId = cv.AggregateId,
                ClaimId = cv.ClaimId,
                ClaimVersionNumber = cv.ClaimVersionNumber,
                LatestFormData = cv.LatestFormData,
                ClaimReferenceNumber = cv.ClaimReferenceNumber,
                SerializedCalculationResult = cv.SerializedCalculationResult,
                LastModifiedTicksSinceEpoch = cv.LastModifiedTicksSinceEpoch,
                CreatedTicksSinceEpoch = cv.CreatedTicksSinceEpoch,
            };

        public ClaimVersionReadModel GetById(Guid tenantId, Guid claimVersionId)
        {
            return this.dbContext.ClaimVersions
                .SingleOrDefault(cv => cv.TenantId == tenantId && cv.Id == claimVersionId);
        }

        /// <inheritdoc/>
        public IClaimVersionReadModelDetails GetDetailsById(Guid tenantId, Guid versionId)
        {
            var claimVersion = this.dbContext.ClaimVersions
                .Where(cv => cv.TenantId == tenantId && cv.Id == versionId)
                .Select(this.DetailSelector)
                .SingleOrDefault();
            claimVersion.Documents = this.dbContext.ClaimAttachment.Where(c => c.ClaimOrClaimVersionId == claimVersion.Id).ToList();
            return claimVersion;
        }

        /// <inheritdoc/>
        public IClaimVersionReadModelDetails GetDetailsByVersionNumber(Guid tenantId, Guid claimId, int versionNumber)
        {
            var claimVersion = this.dbContext.ClaimVersions
                .Where(cv => cv.TenantId == tenantId && cv.ClaimId == claimId && cv.ClaimVersionNumber == versionNumber)
                .Select(this.DetailSelector)
                .SingleOrDefault();
            claimVersion.Documents = this.dbContext.ClaimAttachment.Where(c => c.ClaimOrClaimVersionId == claimVersion.Id).ToList();
            return claimVersion;
        }

        /// <inheritdoc/>
        public IEnumerable<IClaimVersionReadModelDetails> GetVersionsOfClaim(Guid tenantId, Guid claimId)
        {
            return this.dbContext.ClaimVersions
                .Where(v => v.TenantId == tenantId && v.ClaimId == claimId)
                .OrderByDescending(x => x.CreatedTicksSinceEpoch)
                .Take(100)
                .Select(this.DetailSelector)
                .ToList();
        }

        /// <inheritdoc />
        public IClaimVersionReadModelWithRelatedEntities GetClaimVersionWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, Guid claimVersionId, IEnumerable<string> relatedEntities)
        {
            var query =
                this.CreateQueryForClaimVersionDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
            return query.FirstOrDefault(c => c.ClaimVersion.Id == claimVersionId);
        }

        /// <inheritdoc />
        public IClaimVersionReadModelWithRelatedEntities GetClaimVersionWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, Guid claimId, int versionNumber, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForClaimVersionDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
            return query.FirstOrDefault(
                c => c.ClaimVersion.ClaimId == claimId && c.ClaimVersion.ClaimVersionNumber == versionNumber);
        }

        /// <inheritdoc />
        public IClaimVersionReadModelWithRelatedEntities GetClaimVersionWithRelatedEntitiesByReference(
            Guid tenantId,
            string claimReference,
            DeploymentEnvironment? environment,
            int versionNumber,
            IEnumerable<string> relatedEntities)
        {
            var query =
                this.CreateQueryForClaimVersionDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
            return query.FirstOrDefault(
                c => c.ClaimVersion.ClaimReferenceNumber == claimReference &&
                c.ClaimVersion.Environment == environment &&
                c.ClaimVersion.ClaimVersionNumber == versionNumber);
        }

        /// <inheritdoc />
        public IClaimVersionReadModelWithRelatedEntities GetClaimVersionWithRelatedEntitiesByNumber(
            Guid tenantId,
            string claimNumber,
            DeploymentEnvironment? environment,
            int versionNumber,
            IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForClaimVersionDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
            return query.FirstOrDefault(
                c => c.ClaimVersion.ClaimNumber == claimNumber &&
                c.ClaimVersion.ClaimVersionNumber == versionNumber);
        }

        /// <inheritdoc/>
        public List<Guid> GetAllClaimVersionIdsByEntityFilters(EntityFilters entityFilters)
        {
            var query = this.dbContext.ClaimVersions.Where(cv => cv.TenantId == entityFilters.TenantId);

            if (entityFilters.ProductId.HasValue)
            {
                query = query.Where(cv => cv.ProductId == entityFilters.ProductId.Value);
            }

            if (entityFilters.OrganisationId.HasValue)
            {
                query = query.Where(cv => cv.OrganisationId == entityFilters.OrganisationId.Value);
            }

            if (entityFilters.Skip.HasValue && entityFilters.PageSize.HasValue)
            {
                query = query.OrderByDescending(
                    cv => cv.CreatedTicksSinceEpoch).Skip(entityFilters.Skip.Value).Take(
                    entityFilters.PageSize.Value);
            }

            return query.Select(cv => cv.Id).ToList();
        }

        /// <inheritdoc />
        public IQueryable<ClaimVersionReadModelWithRelatedEntities> CreateQueryForClaimVersionDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, IEnumerable<string> relatedEntities)
        {
            var query = this.dbContext.ClaimVersions.Where(c => c.TenantId == tenantId);
            if (environment.HasValue)
            {
                query = query.Where(c => c.Environment == environment);
            }
            var queryRelatedEntities = from claimVersion in query
                                       select new ClaimVersionReadModelWithRelatedEntities
                                       {
                                           ClaimVersion = claimVersion,
                                           Claim = default,
                                           Customer = default,
                                           Tenant = default,
                                           TenantDetails = new TenantDetails[] { },
                                           Product = default,
                                           ProductDetails = new ProductDetails[] { },
                                           Owner = default,
                                           Documents = new ClaimAttachmentReadModel[] { },
                                           Emails = new Domain.ReadWriteModel.Email.Email[] { },
                                           Organisation = default,
                                           Policy = default,
                                           PolicyTransactions = new Domain.ReadModel.Policy.PolicyTransaction[] { },
                                           Sms = new Sms[] { },
                                           ToRelationships = new Relationship[] { },
                                           FromRelationships = new Relationship[] { },
                                           TextAdditionalPropertiesValues = new TextAdditionalPropertyValueReadModel[] { },
                                           StructuredDataAdditionalPropertyValues = new StructuredDataAdditionalPropertyValueReadModel[] { }
                                       };

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.ClaimVersion.Tenant))))
            {
                queryRelatedEntities = queryRelatedEntities.Join(this.dbContext.Tenants, c => c.ClaimVersion.TenantId, t => t.Id, (c, tenant) =>
                new ClaimVersionReadModelWithRelatedEntities
                {
                    ClaimVersion = c.ClaimVersion,
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = tenant,
                    TenantDetails = tenant.DetailsCollection,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.ClaimVersion.Product))))
            {
                queryRelatedEntities = queryRelatedEntities.Join(
                    this.dbContext.Products.IncludeAllProperties(),
                    c => new { tenantId = c.ClaimVersion.TenantId, productId = c.ClaimVersion.ProductId },
                    p => new { tenantId = p.TenantId, productId = p.Id },
                    (c, product) => new ClaimVersionReadModelWithRelatedEntities
                    {
                        ClaimVersion = c.ClaimVersion,
                        Claim = c.Claim,
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Product = product,
                        ProductDetails = product.DetailsCollection,
                        Owner = c.Owner,
                        Documents = c.Documents,
                        Emails = c.Emails,
                        Organisation = c.Organisation,
                        Policy = c.Policy,
                        PolicyTransactions = c.PolicyTransactions,
                        Sms = c.Sms,
                        ToRelationships = c.ToRelationships,
                        FromRelationships = c.FromRelationships,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.ClaimVersion.Customer))))
            {
                queryRelatedEntities = queryRelatedEntities.Join(
                    this.dbContext.CustomerReadModels,
                    c => c.ClaimVersion.CustomerId,
                    c => c.Id,
                    (c, customer) => new ClaimVersionReadModelWithRelatedEntities
                    {
                        ClaimVersion = c.ClaimVersion,
                        Claim = c.Claim,
                        Customer = customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Product = c.Product,
                        ProductDetails = c.ProductDetails,
                        Owner = c.Owner,
                        Documents = c.Documents,
                        Emails = c.Emails,
                        Organisation = c.Organisation,
                        Policy = c.Policy,
                        PolicyTransactions = c.PolicyTransactions,
                        Sms = c.Sms,
                        ToRelationships = c.ToRelationships,
                        FromRelationships = c.FromRelationships,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.ClaimVersion.Owner))))
            {
                queryRelatedEntities = queryRelatedEntities
                    .GroupJoin(
                        this.dbContext.Users,
                        c => c.ClaimVersion.OwnerUserId,
                        u => u.Id,
                        (c, users) => new { c, users })
                    .SelectMany(
                        x => x.users.DefaultIfEmpty(),
                        (x, user) => new ClaimVersionReadModelWithRelatedEntities
                        {
                            ClaimVersion = x.c.ClaimVersion,
                            Claim = x.c.Claim,
                            Customer = x.c.Customer,
                            Tenant = x.c.Tenant,
                            TenantDetails = x.c.TenantDetails,
                            Product = x.c.Product,
                            ProductDetails = x.c.ProductDetails,
                            Owner = x.c.ClaimVersion.OwnerUserId != null ? user : null,
                            Documents = x.c.Documents,
                            Emails = x.c.Emails,
                            Organisation = x.c.Organisation,
                            Policy = x.c.Policy,
                            PolicyTransactions = x.c.PolicyTransactions,
                            Sms = x.c.Sms,
                            ToRelationships = x.c.ToRelationships,
                            FromRelationships = x.c.FromRelationships,
                            TextAdditionalPropertiesValues = x.c.TextAdditionalPropertiesValues,
                            StructuredDataAdditionalPropertyValues = x.c.StructuredDataAdditionalPropertyValues,
                        });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.ClaimVersion.Claim))))
            {
                queryRelatedEntities = queryRelatedEntities.Join(this.dbContext.ClaimReadModels, c => c.ClaimVersion.ClaimId, p => p.Id, (c, claim) => new ClaimVersionReadModelWithRelatedEntities
                {
                    ClaimVersion = c.ClaimVersion,
                    Claim = claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.ClaimVersion.Policy))))
            {
                queryRelatedEntities = queryRelatedEntities.Join(this.dbContext.Policies, c => c.ClaimVersion.PolicyId, p => p.Id, (c, policy) => new ClaimVersionReadModelWithRelatedEntities
                {
                    ClaimVersion = c.ClaimVersion,
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
                queryRelatedEntities = queryRelatedEntities.GroupJoin(this.dbContext.PolicyTransactions, c => c.Policy.Id, pt => pt.PolicyId, (c, pt) => new ClaimVersionReadModelWithRelatedEntities
                {
                    ClaimVersion = c.ClaimVersion,
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = pt,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.ClaimVersion.Organisation))))
            {
                queryRelatedEntities = queryRelatedEntities.Join(this.dbContext.OrganisationReadModel, c => c.ClaimVersion.OrganisationId, t => t.Id, (c, organisation) => new ClaimVersionReadModelWithRelatedEntities
                {
                    ClaimVersion = c.ClaimVersion,
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    Emails = c.Emails,
                    Organisation = organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Claim.Documents))))
            {
                queryRelatedEntities = queryRelatedEntities.GroupJoin(this.dbContext.ClaimAttachment, c => c.ClaimVersion.Id, c => c.ClaimOrClaimVersionId, (c, documents) => new ClaimVersionReadModelWithRelatedEntities
                {
                    ClaimVersion = c.ClaimVersion,
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = documents,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.ClaimVersion.Messages))))
            {
                var emailQuery = from email in this.dbContext.Emails
                                 join relationship in this.dbContext.Relationships on new { EmailId = email.Id, Type = RelationshipType.ClaimVersionMessage, FromEntityType = EntityType.ClaimVersion } equals new { EmailId = relationship.ToEntityId, relationship.Type, relationship.FromEntityType }
                                 select new
                                 {
                                     Email = email,
                                     RelationShip = relationship,
                                 };

                queryRelatedEntities = queryRelatedEntities.GroupJoin(emailQuery, c => c.ClaimVersion.Id, c => c.RelationShip.FromEntityId, (c, emails) => new ClaimVersionReadModelWithRelatedEntities
                {
                    ClaimVersion = c.ClaimVersion,
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    Emails = emails.Select(a => a.Email),
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });

                var smsQuery = from sms in this.dbContext.Sms
                               join relationship in this.dbContext.Relationships on new { SmsId = sms.Id, Type = RelationshipType.ClaimVersionMessage, FromEntityType = EntityType.ClaimVersion } equals new { SmsId = relationship.ToEntityId, Type = relationship.Type, FromEntityType = relationship.FromEntityType }
                               select new
                               {
                                   Sms = sms,
                                   RelationShip = relationship,
                               };

                queryRelatedEntities = queryRelatedEntities.GroupJoin(smsQuery, c => c.ClaimVersion.Id, c => c.RelationShip.FromEntityId, (c, sms) => new ClaimVersionReadModelWithRelatedEntities
                {
                    ClaimVersion = c.ClaimVersion,
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = sms.Select(s => s.Sms),
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.ClaimVersion.Relationships))))
            {
                queryRelatedEntities = queryRelatedEntities.GroupJoin(this.dbContext.Relationships, c => c.ClaimVersion.Id, r => r.FromEntityId, (c, relationships) => new ClaimVersionReadModelWithRelatedEntities
                {
                    ClaimVersion = c.ClaimVersion,
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = relationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });

                queryRelatedEntities = queryRelatedEntities.GroupJoin(this.dbContext.Relationships, c => c.ClaimVersion.Id, r => r.ToEntityId, (c, relationships) => new ClaimVersionReadModelWithRelatedEntities
                {
                    ClaimVersion = c.ClaimVersion,
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = relationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.ClaimVersion.AdditionalProperties))))
            {
                queryRelatedEntities = queryRelatedEntities.GroupJoin(
                    this.dbContext.TextAdditionalPropertValues.IncludeAllProperties(),
                    c => c.ClaimVersion.Id,
                    c => c.EntityId,
                    (c, apv) => new ClaimVersionReadModelWithRelatedEntities
                    {
                        ClaimVersion = c.ClaimVersion,
                        Claim = c.Claim,
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Product = c.Product,
                        ProductDetails = c.ProductDetails,
                        Owner = c.Owner,
                        Documents = c.Documents,
                        Emails = c.Emails,
                        Organisation = c.Organisation,
                        Policy = c.Policy,
                        PolicyTransactions = c.PolicyTransactions,
                        Sms = c.Sms,
                        ToRelationships = c.ToRelationships,
                        FromRelationships = c.FromRelationships,
                        TextAdditionalPropertiesValues = (IEnumerable<TextAdditionalPropertyValueReadModel>)apv
                            .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                    })
                    .GroupJoin(
                    this.dbContext.StructuredDataAdditionalPropertyValues.IncludeAllProperties(),
                    c => c.ClaimVersion.Id,
                    c => c.EntityId,
                    (c, apv) => new ClaimVersionReadModelWithRelatedEntities
                    {
                        ClaimVersion = c.ClaimVersion,
                        Claim = c.Claim,
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Product = c.Product,
                        ProductDetails = c.ProductDetails,
                        Owner = c.Owner,
                        Documents = c.Documents,
                        Emails = c.Emails,
                        Organisation = c.Organisation,
                        Policy = c.Policy,
                        PolicyTransactions = c.PolicyTransactions,
                        Sms = c.Sms,
                        ToRelationships = c.ToRelationships,
                        FromRelationships = c.FromRelationships,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = (IEnumerable<StructuredDataAdditionalPropertyValueReadModel>)apv
                            .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                    });
            }

            return queryRelatedEntities;
        }
    }
}
