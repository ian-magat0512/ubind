// <copyright file="AdditionalPropertyDefinitionFilterResolver.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyDefinition
{
    using System;
    using System.Linq;
    using UBind.Domain.Attributes;
    using UBind.Domain.Enums;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;

    /// <summary>
    /// Resolver that will get the query then update it to filter the context ID's data base on entity types.
    /// </summary>
    public class AdditionalPropertyDefinitionFilterResolver : IAdditionalPropertyDefinitionFilterResolver
    {
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly IClaimReadModelRepository claimReadModelRepository;
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly IPolicyTransactionReadModelRepository policyTransactionReadModelRepository;
        private readonly IClaimVersionReadModelRepository claimVersionReadModelRepository;
        private readonly IQuoteVersionReadModelRepository quoteVersionReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionContextValidator"/> class.
        /// </summary>
        /// <param name="quoteReadModelRepository">quote repository.</param>
        /// <param name="claimReadModelRepository">claim  repository.</param>
        /// <param name="policyReadModelRepository">policy  repository.</param>
        public AdditionalPropertyDefinitionFilterResolver(
            IQuoteReadModelRepository quoteReadModelRepository,
            IClaimReadModelRepository claimReadModelRepository,
            IPolicyReadModelRepository policyReadModelRepository,
            IPolicyTransactionReadModelRepository policyTransactionReadModelRepository,
            IClaimVersionReadModelRepository claimVersionReadModelRepository,
            IQuoteVersionReadModelRepository quoteVersionReadModelRepository)
        {
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.claimReadModelRepository = claimReadModelRepository;
            this.policyReadModelRepository = policyReadModelRepository;
            this.policyTransactionReadModelRepository = policyTransactionReadModelRepository;
            this.claimVersionReadModelRepository = claimVersionReadModelRepository;
            this.quoteVersionReadModelRepository = quoteVersionReadModelRepository;
        }

        /// <summary>
        /// Accept the exiting query then update it to filter the context id to get the only scope of the entity and not get the other product etc.
        /// </summary>
        /// <param name="query">existing query thats t be updated from the text additional property repository.</param>
        /// <param name="tenantId">tenant id.</param>
        /// <param name="entityId">Entity id Take note that the Entity ID could be quotes , claim and policies.</param>
        /// <param name="entityType">Entity type.</param>
        /// <returns> An updated query that will filter the context id.</returns>
        public IQueryable<AdditionalPropertyDefinitionReadModel> FilterAdditionalPropertyByEntityType(IQueryable<AdditionalPropertyDefinitionReadModel> query, Guid tenantId, Guid entityId, AdditionalPropertyEntityType entityType)
        {
            if (this.IsQuoteType(entityType))
            {
                var quotesDetails = this.quoteReadModelRepository.GetById(tenantId, entityId);
                if (quotesDetails != null)
                {
                    query = query.Where(adf =>
                    adf.ContextId == quotesDetails.ProductId ||
                    adf.ContextId == quotesDetails.TenantId ||
                    adf.ContextId == quotesDetails.OrganisationId);
                }
            }
            else if (this.IsQuoteVersion(entityType))
            {
                var quoteVersionDetails = this.quoteVersionReadModelRepository.GetById(tenantId, entityId);

                if (quoteVersionDetails != null)
                {
                    query = query.Where(adf =>
                adf.ContextId == quoteVersionDetails.ProductId ||
                adf.ContextId == quoteVersionDetails.OrganisationId ||
                adf.ContextId == quoteVersionDetails.TenantId);
                }
            }
            else if (this.IsPolicyType(entityType))
            {
                var policyDetails = this.policyReadModelRepository.GetById(tenantId, entityId);

                if (policyDetails != null)
                {
                    query = query.Where(adf =>
                adf.ContextId == policyDetails.ProductId ||
                adf.ContextId == policyDetails.OrganisationId ||
                adf.ContextId == policyDetails.TenantId);
                }
            }
            else if (this.IsPolicyTransactionType(entityType))
            {
                var policyTransactionDetails = this.policyTransactionReadModelRepository.GetById(tenantId, entityId);

                if (policyTransactionDetails != null)
                {
                    query = query.Where(adf =>
                adf.ContextId == policyTransactionDetails.ProductId ||
                adf.ContextId == policyTransactionDetails.OrganisationId ||
                adf.ContextId == policyTransactionDetails.TenantId);
                }
            }
            else if (this.IsClaimVersion(entityType))
            {
                var claimVerionDetails = this.claimVersionReadModelRepository.GetById(tenantId, entityId);

                if (claimVerionDetails != null)
                {
                    query = query.Where(adf =>
                adf.ContextId == claimVerionDetails.ProductId ||
                adf.ContextId == claimVerionDetails.OrganisationId ||
                adf.ContextId == claimVerionDetails.TenantId);
                }
            }
            else if (this.IsClaim(entityType))
            {
                var claimDetails = this.claimReadModelRepository.GetSummaryById(tenantId, entityId);

                if (claimDetails != null)
                {
                    query = query.Where(adf =>
                adf.ContextId == claimDetails.ProductId ||
                adf.ContextId == claimDetails.OrganisationId ||
                adf.ContextId == claimDetails.TenantId);
                }
            }

            return query;
        }

        private bool IsQuoteType(AdditionalPropertyEntityType entityType)
        {
            var category = entityType.GetAttributeOfType<AdditionalPropertyEntityTypeCategoryAttribute>();
            return category?.Category == AdditionalPropertyEntityTypeCategory.Quote;
        }

        private bool IsPolicyTransactionType(AdditionalPropertyEntityType entityType)
        {
            var category = entityType.GetAttributeOfType<AdditionalPropertyEntityTypeCategoryAttribute>();
            return category?.Category == AdditionalPropertyEntityTypeCategory.PolicyTransaction;
        }

        private bool IsPolicyType(AdditionalPropertyEntityType entityType)
        {
            var category = entityType.GetAttributeOfType<AdditionalPropertyEntityTypeCategoryAttribute>();
            return category?.Category == AdditionalPropertyEntityTypeCategory.Policy;
        }

        private bool IsClaim(AdditionalPropertyEntityType entityType)
        {
            var category = entityType.GetAttributeOfType<AdditionalPropertyEntityTypeCategoryAttribute>();
            return category?.Category == AdditionalPropertyEntityTypeCategory.Claim;
        }

        private bool IsClaimVersion(AdditionalPropertyEntityType entityType)
        {
            var category = entityType.GetAttributeOfType<AdditionalPropertyEntityTypeCategoryAttribute>();
            return category?.Category == AdditionalPropertyEntityTypeCategory.ClaimVersion;
        }

        private bool IsQuoteVersion(AdditionalPropertyEntityType entityType)
        {
            var category = entityType.GetAttributeOfType<AdditionalPropertyEntityTypeCategoryAttribute>();
            return category?.Category == AdditionalPropertyEntityTypeCategory.QuoteVersion;
        }
    }
}
