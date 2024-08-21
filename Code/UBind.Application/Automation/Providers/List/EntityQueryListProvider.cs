// <copyright file="EntityQueryListProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using MorseCode.ITask;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Domain;
    using UBind.Domain.Automation;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Sms;

    /// <summary>
    /// For providing entity collections queried from the database.
    /// </summary>
    public class EntityQueryListProvider : IDataListProvider<object>, IExpressionProvider
    {
        private readonly IEntityQueryService queryService;
        private readonly IProvider<Data<string>> entityTypeProvider;
        private readonly IProvider<Data<long>>? pageSizeProvider;
        private readonly IProvider<Data<long>>? pageNumberProvider;
        private readonly ICachingResolver cacheResolver;
        private readonly Dictionary<EntityType, Func<IEntityQueryService, AutomationData, List<string>, long?, long?, IDataList<object>>> entityCollectionFactoryMap =
        new Dictionary<EntityType, Func<IEntityQueryService, AutomationData, List<string>, long?, long?, IDataList<object>>>()
        {
            { EntityType.Quote, (s, t, p, ps, pn) => CreateQuoteCollection(s, t, p, q => q.OrderBy(c => c.Quote.CreatedTicksSinceEpoch), ps, pn) },
            { EntityType.QuoteVersion, (s, t, p, ps, pn) => CreateQuoteVersionCollection(s, t, p, q => q.OrderBy(c => c.QuoteVersion.CreatedTicksSinceEpoch), ps, pn) },
            { EntityType.Claim, (s, t, p, ps, pn) => CreateClaimCollection(s, t, p, q => q.OrderBy(c => c.Claim.CreatedTicksSinceEpoch), ps, pn) },
            { EntityType.ClaimVersion, (s, t, p, ps, pn) => CreateClaimVersionCollection(s, t, p, q => q.OrderBy(c => c.ClaimVersion.CreatedTicksSinceEpoch), ps, pn) },
            { EntityType.Policy, (s, t, p, ps, pn) => CreatePolicyCollection(s, t, p, q => q.OrderBy(c => c.Policy.CreatedTicksSinceEpoch), ps, pn) },
            { EntityType.PolicyTransaction, (s, t, p, ps, pn) => CreatePolicyTransactionCollection(s, t, p, q => q.OrderBy(c => c.PolicyTransaction.CreatedTicksSinceEpoch), ps, pn) },
            { EntityType.Organisation, (s, t, p, ps, pn) => CreateOrganisationCollection(s, t, p, q => q.OrderBy(c => c.Organisation.CreatedTicksSinceEpoch), ps, pn) },
            { EntityType.Product, (s, t, p, ps, pn) => CreateProductCollection(s, t, p, q => q.OrderBy(c => c.Product.CreatedTicksSinceEpoch), ps, pn) },
            { EntityType.Tenant, (s, t, p, ps, pn) => CreateTenantCollection(s, t, p, q => q.OrderBy(c => c.Tenant.CreatedTicksSinceEpoch), ps, pn) },
            { EntityType.Customer, (s, t, p, ps, pn) => CreateCustomerCollection(s, t, p, q => q.OrderBy(c => c.Customer.CreatedTicksSinceEpoch), ps, pn) },
            { EntityType.User, (s, t, p, ps, pn) => CreateUserCollection(s, t, p, q => q.OrderBy(c => c.User.CreatedTicksSinceEpoch), ps, pn) },
            { EntityType.Document, (s, t, p, ps, pn) => CreateDocumentCollection(s, t, p, q => q.OrderBy(c => c.Document.CreatedTicksSinceEpoch), ps, pn) },
            { EntityType.EmailMessage, (s, t, p, ps, pn) => CreateEmailCollection(s, t, p, q => q.OrderBy(c => c.Email.CreatedTicksSinceEpoch), ps, pn) },
            { EntityType.SmsMessage, (s, t, p, ps, pn) => CreateSmsCollection(s, t, p, q => q.OrderBy(c => c.Sms.CreatedTicksSinceEpoch), ps, pn) },
            { EntityType.Portal, (s, t, p, ps, pn) => CreatePortalCollection(s, t, p, q => q.OrderBy(c => c.Portal.CreatedTicksSinceEpoch), ps, pn) },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityQueryListProvider"/> class.
        /// </summary>
        /// <param name="queryService">A service for querying entities from the database.</param>
        /// <param name="entityTypeProvider">A provider for the type of entity to query.</param>
        /// <param name="cachingResolver">The caching resolver.</param>
        public EntityQueryListProvider(IEntityQueryService queryService,
            IProvider<Data<string>> entityTypeProvider,
            ICachingResolver cachingResolver,
            IProvider<Data<long>>? pageSizeProvider = null,
            IProvider<Data<long>>? pageNumberProvider = null)
        {
            this.queryService = queryService;
            this.entityTypeProvider = entityTypeProvider;
            this.cacheResolver = cachingResolver;
            this.pageSizeProvider = pageSizeProvider;
            this.pageNumberProvider = pageNumberProvider;
        }

        /// <inheritdoc/>
        public List<string> IncludedProperties { get; set; } = new List<string>();

        public string SchemaReferenceKey => "entityQueryList";

        /// <inheritdoc/>
        public async ITask<IProviderResult<IDataList<object>>> Resolve(IProviderContext providerContext)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            string type = (await this.entityTypeProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
            long? pageSizeValue = null, pageNumberValue = null;
            pageSizeValue = this.pageSizeProvider != null
            ? (await this.pageSizeProvider.ResolveValueIfNotNull(providerContext))?.DataValue
            : null;
            if (pageSizeValue != null)
            {
                pageNumberValue = this.pageNumberProvider != null
                    ? (await this.pageNumberProvider.ResolveValueIfNotNull(providerContext))?.DataValue
                    : 1; // Surely the default should be page 0 ?!!!
            }

            if (Enum.TryParse(type, true, out EntityType entityType))
            {
                if (entityType == EntityType.Event)
                {
                    return ProviderResult<IDataList<object>>.Success(CreateEventCollection(
                        tenantId,
                        this.queryService,
                        q => q.OrderBy(@event => @event.CreatedTicksSinceEpoch)));
                }

                if (entityType == EntityType.Tenant)
                {
                    var tenant = await this.cacheResolver.GetTenantOrThrow(tenantId);
                    if (!tenant.Id.Equals(Tenant.MasterTenantId))
                    {
                        throw new ErrorException(Errors.General.AccessDeniedToResource("tenants list", entityType.ToString()));
                    }
                }

                if (this.entityCollectionFactoryMap.TryGetValue(
                    entityType, out Func<IEntityQueryService, AutomationData, List<string>, long?, long?, IDataList<object>>? factory))
                {
                    providerContext.CancellationToken.ThrowIfCancellationRequested();
                    var result = factory.Invoke(
                        this.queryService, providerContext.AutomationData, this.IncludedProperties, pageSizeValue, pageNumberValue);
                    return ProviderResult<IDataList<object>>.Success(result);
                }
            }

            throw new NotSupportedException($"Collections not supported for entities of type '{type}'.");
        }

        /// <inheritdoc/>
        public async Task<Expression> Invoke(IProviderContext providerContext, ExpressionScope scope)
        {
            var data = (await this.Resolve(providerContext)).GetValueOrThrowIfFailed();
            return data.Query.Expression;
        }

        private static IDataList<object> CreateEventCollection(
            Guid tenantId,
            IEntityQueryService queryService,
            Func<IQueryable<Event>, IOrderedQueryable<Event>> order) =>
            new EntityQueryList<Event>(queryService.QueryEvents(tenantId), order);

        private static IDataList<object> CreateQuoteCollection(
            IEntityQueryService queryService,
            AutomationData automationData,
            List<string> includedProperties,
            Func<IQueryable<IQuoteReadModelWithRelatedEntities>, IOrderedQueryable<IQuoteReadModelWithRelatedEntities>> order,
            long? pageSize,
            long? pageNumber)
        {
            var collection = new EntityQueryList<IQuoteReadModelWithRelatedEntities>(
                queryService.QueryQuotes(
                    automationData.ContextManager.Tenant.Id, automationData.System.Environment, includedProperties),
                order, pageSize, pageNumber);

            return collection;
        }

        private static IDataList<object> CreateQuoteVersionCollection(
            IEntityQueryService queryService,
            AutomationData automationData,
            List<string> includedProperties,
            Func<IQueryable<IQuoteVersionReadModelWithRelatedEntities>,
            IOrderedQueryable<IQuoteVersionReadModelWithRelatedEntities>> order,
            long? pageSize,
            long? pageNumber) =>
            new EntityQueryList<IQuoteVersionReadModelWithRelatedEntities>(
                queryService.QueryQuoteVersions(
                    automationData.ContextManager.Tenant.Id, automationData.System.Environment, includedProperties),
                order,
                pageSize,
                pageNumber);

        private static IDataList<object> CreateClaimCollection(
            IEntityQueryService queryService,
            AutomationData automationData,
            List<string> includedProperties,
            Func<IQueryable<IClaimReadModelWithRelatedEntities>, IOrderedQueryable<IClaimReadModelWithRelatedEntities>> order,
            long? pageSize,
            long? pageNumber) =>
            new EntityQueryList<IClaimReadModelWithRelatedEntities>(
                queryService.QueryClaims(
                    automationData.ContextManager.Tenant.Id, automationData.System.Environment, includedProperties),
                order,
                pageSize,
                pageNumber);

        private static IDataList<object> CreateClaimVersionCollection(
            IEntityQueryService queryService,
            AutomationData automationData,
            List<string> includedProperties,
            Func<IQueryable<IClaimVersionReadModelWithRelatedEntities>, IOrderedQueryable<IClaimVersionReadModelWithRelatedEntities>> order,
            long? pageSize,
            long? pageNumber) =>
            new EntityQueryList<IClaimVersionReadModelWithRelatedEntities>(
                queryService.QueryClaimVersions(
                    automationData.ContextManager.Tenant.Id, automationData.System.Environment, includedProperties),
                order,
                pageSize,
                pageNumber);

        private static IDataList<object> CreatePolicyCollection(
            IEntityQueryService queryService,
            AutomationData automationData,
            List<string> includedProperties,
            Func<IQueryable<IPolicyReadModelWithRelatedEntities>, IOrderedQueryable<IPolicyReadModelWithRelatedEntities>> order,
            long? pageSize,
            long? pageNumber) =>
           new EntityQueryList<IPolicyReadModelWithRelatedEntities>(
                queryService.QueryPolicies(
                    automationData.ContextManager.Tenant.Id, automationData.System.Environment, includedProperties),
                order,
                pageSize,
                pageNumber);

        private static IDataList<object> CreatePolicyTransactionCollection(
            IEntityQueryService queryService,
            AutomationData automationData,
            List<string> includedProperties,
            Func<IQueryable<IPolicyTransactionReadModelWithRelatedEntities>, IOrderedQueryable<IPolicyTransactionReadModelWithRelatedEntities>> order,
            long? pageSize,
            long? pageNumber) =>
            new EntityQueryList<IPolicyTransactionReadModelWithRelatedEntities>(
                queryService.QueryPolicyTransactions(
                    automationData.ContextManager.Tenant.Id, automationData.System.Environment, includedProperties),
                order,
                pageSize,
                pageNumber);

        private static IDataList<object> CreateOrganisationCollection(
            IEntityQueryService queryService,
            AutomationData automationData,
            List<string> includedProperties,
            Func<IQueryable<IOrganisationReadModelWithRelatedEntities>, IOrderedQueryable<IOrganisationReadModelWithRelatedEntities>> order,
            long? pageSize,
            long? pageNumber) =>
            new EntityQueryList<IOrganisationReadModelWithRelatedEntities>(
                queryService.QueryOrganisations(automationData.ContextManager.Tenant.Id, includedProperties), order, pageSize, pageNumber);

        private static IDataList<object> CreateProductCollection(
            IEntityQueryService queryService,
            AutomationData automationData,
            List<string> includedProperties,
            Func<IQueryable<IProductWithRelatedEntities>, IOrderedQueryable<IProductWithRelatedEntities>> order,
            long? pageSize,
            long? pageNumber) =>
            new EntityQueryList<IProductWithRelatedEntities>(
                queryService.QueryProducts(automationData.ContextManager.Tenant.Id, includedProperties), order, pageSize, pageNumber);

        private static IDataList<object> CreateTenantCollection(
            IEntityQueryService queryService,
            AutomationData automationData,
            List<string> includedProperties,
            Func<IQueryable<ITenantWithRelatedEntities>, IOrderedQueryable<ITenantWithRelatedEntities>> order,
            long? pageSize,
            long? pageNumber) =>
            new EntityQueryList<ITenantWithRelatedEntities>(
                queryService.QueryTenants(automationData.ContextManager.Tenant.Id, includedProperties), order, pageSize, pageNumber);

        private static IDataList<object> CreateCustomerCollection(
            IEntityQueryService queryService,
            AutomationData automationData,
            List<string> includedProperties,
            Func<IQueryable<ICustomerReadModelWithRelatedEntities>, IOrderedQueryable<ICustomerReadModelWithRelatedEntities>> order,
            long? pageSize,
            long? pageNumber) =>
            new EntityQueryList<ICustomerReadModelWithRelatedEntities>(
                queryService.QueryCustomers(
                    automationData.ContextManager.Tenant.Id, automationData.System.Environment, includedProperties),
                order,
                pageSize,
                pageNumber);

        private static IDataList<object> CreateUserCollection(
            IEntityQueryService queryService,
            AutomationData automationData,
            List<string> includedProperties,
            Func<IQueryable<IUserReadModelWithRelatedEntities>, IOrderedQueryable<IUserReadModelWithRelatedEntities>> order,
            long? pageSize,
            long? pageNumber) =>
            new EntityQueryList<IUserReadModelWithRelatedEntities>(
                queryService.QueryUsers(automationData.ContextManager.Tenant.Id, automationData.System.Environment, includedProperties),
                order,
                pageSize,
                pageNumber);

        private static IDataList<object> CreateDocumentCollection(
            IEntityQueryService queryService,
            AutomationData automationData,
            List<string> includedProperties,
            Func<IQueryable<IDocumentReadModelWithRelatedEntities>, IOrderedQueryable<IDocumentReadModelWithRelatedEntities>> order,
            long? pageSize,
            long? pageNumber) =>
            new EntityQueryList<IDocumentReadModelWithRelatedEntities>(
                queryService.QueryDocuments(
                    automationData.ContextManager.Tenant.Id, automationData.System.Environment, includedProperties),
                order, pageSize, pageNumber);

        private static IDataList<object> CreateEmailCollection(
            IEntityQueryService queryService,
            AutomationData automationData,
            List<string> includedProperties,
            Func<IQueryable<IEmailReadModelWithRelatedEntities>, IOrderedQueryable<IEmailReadModelWithRelatedEntities>> order,
            long? pageSize,
            long? pageNumber) =>
            new EntityQueryList<IEmailReadModelWithRelatedEntities>(
                queryService.QueryEmails(
                    automationData.ContextManager.Tenant.Id, automationData.System.Environment, includedProperties),
                order, pageSize, pageNumber);

        private static IDataList<object> CreateSmsCollection(
           IEntityQueryService queryService,
           AutomationData automationData,
           List<string> includedProperties,
           Func<IQueryable<ISmsReadModelWithRelatedEntities>, IOrderedQueryable<ISmsReadModelWithRelatedEntities>> order,
           long? pageSize,
           long? pageNumber) =>
           new EntityQueryList<ISmsReadModelWithRelatedEntities>(
               queryService.QuerySms(automationData.ContextManager.Tenant.Id, includedProperties), order, pageSize, pageNumber);

        private static IDataList<object> CreatePortalCollection(
            IEntityQueryService queryService,
            AutomationData automationData,
            List<string> includedProperties,
            Func<IQueryable<IPortalWithRelatedEntities>, IOrderedQueryable<IPortalWithRelatedEntities>> order,
            long? pageSize,
            long? pageNumber) =>
            new EntityQueryList<IPortalWithRelatedEntities>(
                queryService.QueryPortals(automationData.ContextManager.Tenant.Id, includedProperties), order, pageSize, pageNumber);
    }
}
