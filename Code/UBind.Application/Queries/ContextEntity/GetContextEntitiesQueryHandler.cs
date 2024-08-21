// <copyright file="GetContextEntitiesQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ContextEntity
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using StackExchange.Profiling;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.SerialisedEntitySchemaObject;

    public class GetContextEntitiesQueryHandler : IQueryHandler<GetContextEntitiesQuery, object>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly IClaimReadModelRepository claimReadModelRepository;
        private readonly IReleaseQueryService releaseQueryService;
        private readonly ICachingResolver cachingResolver;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly string[] relatedEntities = new string[] { "quote", "customer", "policy", "claim" };

        public GetContextEntitiesQueryHandler(
            IServiceProvider serviceProvider,
            IQuoteReadModelRepository quoteReadModelRepository,
            IClaimReadModelRepository claimReadModelRepository,
            IReleaseQueryService releaseQueryService,
            ICachingResolver cachingResolver,
            IHttpContextPropertiesResolver httpContextPropertiesResolver)
        {
            this.serviceProvider = serviceProvider;
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.claimReadModelRepository = claimReadModelRepository;
            this.releaseQueryService = releaseQueryService;
            this.cachingResolver = cachingResolver;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        }

        public async Task<object> Handle(GetContextEntitiesQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (MiniProfiler.Current.Step($"{nameof(GetContextEntitiesQueryHandler)}.{nameof(this.Handle)}"))
            {
                var tenantId = query.ReleaseContext.TenantId;
                var environment = query.ReleaseContext.Environment;
                var productId = query.ReleaseContext.ProductId;
                var contextEntitiesResolver = this.GenerateContextEntitiesObjectProvider(query);
                var automationData = AutomationData.CreateForContextEntities(
                    query.ReleaseContext,
                    query.OrganisationId,
                    this.serviceProvider);

                if (query.WebFormAppType == WebFormAppType.Quote)
                {
                    this.SetContextEntitiesForQuote(automationData, query);
                }
                else if (query.WebFormAppType == WebFormAppType.Claim)
                {
                    this.SetContextEntitiesForClaim(automationData, query);
                }

                var performingUserId = this.httpContextPropertiesResolver.PerformingUserId.GetValueOrDefault();
                if (performingUserId != default)
                {
                    automationData.ContextManager.SetContextEntity<User>("performingUser", new User(performingUserId));
                }
                var data = await contextEntitiesResolver.Resolve(new ProviderContext(automationData));
                return data.GetValueOrThrowIfFailed().DataValue;
            }
        }

        private ContextEntitiesObjectProvider GenerateContextEntitiesObjectProvider(GetContextEntitiesQuery query)
        {
            using (MiniProfiler.Current.Step(nameof(GetContextEntitiesQueryHandler) + "." + nameof(this.GenerateContextEntitiesObjectProvider)))
            {
                var release = this.releaseQueryService.GetRelease(query.ReleaseContext);
                var config = release.GetProductComponentConfigurationOrThrow(query.WebFormAppType);
                if (config.Component.ContextEntities == null)
                {
                    var tenantAlias = this.cachingResolver.GetTenantAliasOrThrow(query.ReleaseContext.TenantId);
                    var productAlias = this.cachingResolver.GetProductAliasOrThrow(query.ReleaseContext.TenantId, query.ReleaseContext.ProductId);
                    throw new ErrorException(Errors.ContextEntities.NotConfigured(tenantAlias, productAlias, query.WebFormAppType));
                }
                string[] includeContextEntities;
                if (query.WebFormAppType == WebFormAppType.Claim)
                {
                    if (config.Component.ContextEntities.Claims == null)
                    {
                        var tenantAlias = this.cachingResolver.GetTenantAliasOrThrow(query.ReleaseContext.TenantId);
                        var productAlias = this.cachingResolver.GetProductAliasOrThrow(query.ReleaseContext.TenantId, query.ReleaseContext.ProductId);
                        throw new ErrorException(Errors.ContextEntities.NotConfiguredForClaims(tenantAlias, productAlias));
                    }
                    includeContextEntities = config.Component.ContextEntities.Claims?.IncludeContextEntities;
                }
                else if (query.WebFormAppType == WebFormAppType.Quote)
                {
                    if (query.QuoteType == null)
                    {
                        throw new ArgumentException("An attempt was made to get context entities for a quote without specifying the quote type");
                    }

                    var quoteContextEntities = config.Component.ContextEntities.Quotes?.IncludeContextEntities.ToList() ?? new List<string>();
                    switch (query.QuoteType)
                    {
                        case QuoteType.NewBusiness:
                            if (config.Component.ContextEntities.NewBusinessQuotes != null)
                            {
                                quoteContextEntities.AddRange(config.Component.ContextEntities.NewBusinessQuotes?.IncludeContextEntities);
                            }
                            break;
                        case QuoteType.Renewal:
                            if (config.Component.ContextEntities.RenewalQuotes != null)
                            {
                                quoteContextEntities.AddRange(config.Component.ContextEntities.RenewalQuotes?.IncludeContextEntities);
                            }
                            break;
                        case QuoteType.Adjustment:
                            if (config.Component.ContextEntities.AdjustmentQuotes != null)
                            {
                                quoteContextEntities.AddRange(config.Component.ContextEntities.AdjustmentQuotes?.IncludeContextEntities);
                            }
                            break;
                        case QuoteType.Cancellation:
                            if (config.Component.ContextEntities.CancellationQuotes != null)
                            {
                                quoteContextEntities.AddRange(config.Component.ContextEntities.CancellationQuotes?.IncludeContextEntities);
                            }
                            break;
                        default:
                            throw new ErrorException(Errors.General.UnexpectedEnumValue(query.QuoteType, typeof(QuoteType)));
                    }

                    if (quoteContextEntities.Count == 0)
                    {
                        var tenantAlias = this.cachingResolver.GetTenantAliasOrThrow(query.ReleaseContext.TenantId);
                        var productAlias = this.cachingResolver.GetProductAliasOrThrow(query.ReleaseContext.TenantId, query.ReleaseContext.ProductId);
                        throw new ErrorException(Errors.ContextEntities.NotConfiguredForQuoteType(tenantAlias, productAlias, query.QuoteType.Value));
                    }

                    includeContextEntities = quoteContextEntities.ToArray();
                }
                else
                {
                    throw new ErrorException(Errors.General.UnexpectedEnumValue(query.WebFormAppType, typeof(WebFormAppType)));
                }

                var entityPathProviders = includeContextEntities.Select(entityPath => new StaticProvider<Data<string>>(entityPath));
                return new ContextEntitiesObjectProvider(entityPathProviders);
            }
        }

        private void SetContextEntitiesForQuote(AutomationData automationData, GetContextEntitiesQuery query)
        {
            using (MiniProfiler.Current.Step(nameof(GetContextEntitiesQueryHandler) + "." + nameof(this.SetContextEntitiesForQuote)))
            {
                var quoteId = query.EntityId;
                var quote = this.quoteReadModelRepository.GetQuoteWithRelatedEntities(
                    query.ReleaseContext.TenantId, query.ReleaseContext.Environment, quoteId, this.relatedEntities);

                if (quote == null)
                {
                    return;
                }

                if (quote.Quote.IsActualised || quote.Quote.Type == QuoteType.Adjustment)
                {
                    automationData.ContextManager.SetContextEntity(EntityType.Quote, new Quote(quoteId));
                }

                if (quote.Customer != null)
                {
                    automationData.ContextManager.SetContextEntity(EntityType.Customer, new Customer(quote.Customer.Id));
                }

                if (quote.Policy != null)
                {
                    automationData.ContextManager.SetContextEntity(EntityType.Policy, new Policy(quote.Policy.Id));
                }
            }
        }

        private void SetContextEntitiesForClaim(AutomationData automationData, GetContextEntitiesQuery query)
        {
            using (MiniProfiler.Current.Step(nameof(GetContextEntitiesQueryHandler) + "." + nameof(this.SetContextEntitiesForClaim)))
            {
                var claimId = query.EntityId;
                var claim = this.claimReadModelRepository.GetClaimWithRelatedEntities(
                    query.ReleaseContext.TenantId, query.ReleaseContext.Environment, claimId, this.relatedEntities);

                if (claim == null)
                {
                    return;
                }

                automationData.ContextManager.SetContextEntity(EntityType.Claim, new Claim(claimId));

                if (claim.Customer != null)
                {
                    automationData.ContextManager.SetContextEntity(EntityType.Customer, new Customer(claim.Customer.Id));
                }

                if (claim.Policy != null)
                {
                    automationData.ContextManager.SetContextEntity(EntityType.Policy, new Policy(claim.Policy.Id));
                }
            }
        }
    }
}
