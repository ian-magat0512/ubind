// <copyright file="AutomationEntityQueryService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain;
    using UBind.Domain.Automation;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.ReadModel.Sms;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class AutomationEntityQueryService : IEntityQueryService
    {
        private readonly ISystemEventRepository systemEventRepository;
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly IQuoteVersionReadModelRepository quoteVersionReadModelRepository;
        private readonly IClaimReadModelRepository claimReadModelRepository;
        private readonly IClaimVersionReadModelRepository claimVersionReadModelRepository;
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly IPolicyTransactionReadModelRepository policyTransactionReadModelRepository;
        private readonly IOrganisationReadModelRepository organisationRepository;
        private readonly IProductRepository productRepository;
        private readonly ITenantRepository tenantRepository;
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IQuoteDocumentReadModelRepository documentRepository;
        private readonly IEmailRepository emailRepository;
        private readonly ISmsRepository smsRepository;
        private readonly IPortalReadModelRepository portalRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationEntityQueryService"/> class.
        /// </summary>
        /// <param name="systemEventRepository">The system event repository.</param>
        /// <param name="quoteReadModelRepository">The quote read model repository.</param>
        /// <param name="quoteVersionReadModelRepository">The quote version read model repository.</param>
        /// <param name="claimReadModelRepository">The claim read model repository.</param>
        /// <param name="claimVersionReadModelRepository">The claim version read model repository.</param>
        /// <param name="policyReadModelRepository">The policy read model repository.</param>
        /// <param name="policyTransactionReadModelRepository">The policy transaction read model repository.</param>
        /// <param name="organisationRepository">The organisation read model repository.</param>
        /// <param name="productRepository">The product repository.</param>
        /// <param name="tenantRepository">The tenant repository.</param>
        /// <param name="customerReadModelRepository">The customer read model repository.</param>
        /// <param name="userReadModelRepository">The user read model repository.</param>
        /// <param name="documentRepository">The document repository.</param>
        /// <param name="emailRepository">The email repository.</param>
        /// <param name="smsRepository">The sms repository.</param>
        /// <param name="portalRepository">The portal repository.</param>
        public AutomationEntityQueryService(
            ISystemEventRepository systemEventRepository,
            IQuoteReadModelRepository quoteReadModelRepository,
            IQuoteVersionReadModelRepository quoteVersionReadModelRepository,
            IClaimReadModelRepository claimReadModelRepository,
            IClaimVersionReadModelRepository claimVersionReadModelRepository,
            IPolicyReadModelRepository policyReadModelRepository,
            IPolicyTransactionReadModelRepository policyTransactionReadModelRepository,
            IOrganisationReadModelRepository organisationRepository,
            IProductRepository productRepository,
            ITenantRepository tenantRepository,
            ICustomerReadModelRepository customerReadModelRepository,
            IUserReadModelRepository userReadModelRepository,
            IQuoteDocumentReadModelRepository documentRepository,
            IEmailRepository emailRepository,
            ISmsRepository smsRepository,
            IPortalReadModelRepository portalRepository)
        {
            this.systemEventRepository = systemEventRepository;
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.quoteVersionReadModelRepository = quoteVersionReadModelRepository;
            this.claimReadModelRepository = claimReadModelRepository;
            this.claimVersionReadModelRepository = claimVersionReadModelRepository;
            this.policyReadModelRepository = policyReadModelRepository;
            this.policyTransactionReadModelRepository = policyTransactionReadModelRepository;
            this.organisationRepository = organisationRepository;
            this.productRepository = productRepository;
            this.tenantRepository = tenantRepository;
            this.customerReadModelRepository = customerReadModelRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.documentRepository = documentRepository;
            this.emailRepository = emailRepository;
            this.smsRepository = smsRepository;
            this.portalRepository = portalRepository;
        }

        /// <inheritdoc/>
        public IQueryable<Event> QueryEvents(Guid tenantId)
        {
            return this.systemEventRepository.CreateQueryForAutomations(tenantId);
        }

        /// <inheritdoc/>
        public IQueryable<IQuoteReadModelWithRelatedEntities> QueryQuotes(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties) =>
            this.quoteReadModelRepository.CreateQueryForQuoteDetailsWithRelatedEntities(tenantId, environment, includedProperties);

        /// <inheritdoc/>
        public IQueryable<IQuoteVersionReadModelWithRelatedEntities> QueryQuoteVersions(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties) =>
            this.quoteVersionReadModelRepository.CreateQueryForQuoteDetailsWithRelatedEntities(tenantId, environment, includedProperties);

        /// <inheritdoc/>
        public IQueryable<IClaimReadModelWithRelatedEntities> QueryClaims(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties) =>
            this.claimReadModelRepository.CreateQueryForClaimDetailsWithRelatedEntities(tenantId, environment, includedProperties);

        /// <inheritdoc/>
        public IQueryable<IClaimVersionReadModelWithRelatedEntities> QueryClaimVersions(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties) =>
             this.claimVersionReadModelRepository.CreateQueryForClaimVersionDetailsWithRelatedEntities(tenantId, environment, includedProperties);

        /// <inheritdoc/>
        public IQueryable<IPolicyReadModelWithRelatedEntities> QueryPolicies(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties) =>
            this.policyReadModelRepository.CreateQueryForPolicyDetailsWithRelatedEntities(tenantId, environment, includedProperties);

        /// <inheritdoc/>
        public IQueryable<IPolicyTransactionReadModelWithRelatedEntities> QueryPolicyTransactions(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties) =>
            this.policyTransactionReadModelRepository.CreateQueryForPolicyTransactionDetailsWithRelatedEntities(tenantId, environment, includedProperties);

        /// <inheritdoc/>
        public IQueryable<IOrganisationReadModelWithRelatedEntities> QueryOrganisations(
            Guid tenantId, IEnumerable<string> includedProperties) =>
            this.organisationRepository.CreateQueryForOrganisationDetailsWithRelatedEntities(tenantId, includedProperties);

        /// <inheritdoc/>
        public IQueryable<IProductWithRelatedEntities> QueryProducts(
            Guid tenantId, IEnumerable<string> includedProperties) =>
            this.productRepository.CreateQueryForProductDetailsWithRelatedEntities(tenantId, includedProperties);

        /// <inheritdoc/>
        public IQueryable<ITenantWithRelatedEntities> QueryTenants(
            Guid tenantId, IEnumerable<string> includedProperties) =>
            this.tenantRepository.CreateQueryForTenantWithRelatedEntities(tenantId, includedProperties);

        /// <inheritdoc/>
        public IQueryable<ICustomerReadModelWithRelatedEntities> QueryCustomers(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties) =>
            this.customerReadModelRepository.CreateQueryForCustomerDetailsWithRelatedEntities(tenantId, environment, includedProperties);

        /// <inheritdoc/>
        public IQueryable<IUserReadModelWithRelatedEntities> QueryUsers(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties) =>
            this.userReadModelRepository.CreateQueryForUserDetailsWithRelatedEntities(tenantId, includedProperties);

        /// <inheritdoc/>
        public IQueryable<IDocumentReadModelWithRelatedEntities> QueryDocuments(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties) =>
            this.documentRepository.CreateQueryForDocumentDetailsWithRelatedEntities(tenantId, environment, includedProperties);

        /// <inheritdoc/>
        public IQueryable<IEmailReadModelWithRelatedEntities> QueryEmails(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> includedProperties) =>
            this.emailRepository.CreateQueryForEmailDetailsWithRelatedEntities(tenantId, environment, includedProperties);

        /// <inheritdoc/>
        public IQueryable<IPortalWithRelatedEntities> QueryPortals(
            Guid tenantId, IEnumerable<string> includedProperties) =>
            this.portalRepository.CreateQueryForPortalWithRelatedEntities(tenantId, includedProperties);

        public IQueryable<ISmsReadModelWithRelatedEntities> QuerySms(
            Guid tenantId, IEnumerable<string> includedProperties) =>
            this.smsRepository.CreateQueryForSmsDetailsWithRelatedEntities(tenantId, includedProperties);
    }
}
