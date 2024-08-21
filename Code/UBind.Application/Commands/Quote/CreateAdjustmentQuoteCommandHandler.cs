// <copyright file="CreateAdjustmentQuoteCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Quote;

using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using NodaTime;
using UBind.Application.Queries.ProductRelease;
using UBind.Domain;
using UBind.Domain.Aggregates.AdditionalPropertyValue;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Aggregates.Quote.Workflow;
using UBind.Domain.Enums;
using UBind.Domain.Exceptions;
using UBind.Domain.Extensions;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;
using UBind.Domain.ReadModel;
using UBind.Domain.Services;
using UBind.Domain.Services.AdditionalPropertyValue;
using UBind.Domain.Services.QuoteExpiry;
using UBind.Domain.ValueTypes;

public class CreateAdjustmentQuoteCommandHandler : ICommandHandler<CreateAdjustmentQuoteCommand, NewQuoteReadModel>
{
    private readonly ICachingResolver cachingResolver;
    private readonly IProductFeatureSettingService productFeatureSettingService;
    private readonly IQuoteExpirySettingsProvider quoteExpirySettingsProvider;
    private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
    private readonly IClock clock;
    private readonly ICqrsMediator mediator;
    private readonly IQuoteAggregateRepository quoteAggregateRepository;
    private readonly IPolicyService policyService;
    private readonly IProductConfigurationProvider productConfigurationProvider;
    private readonly IQuoteWorkflowProvider quoteWorkflowProvider;
    private readonly IPolicyReadModelRepository policyReadModelRepository;
    private readonly IProductService productService;
    private readonly IAdditionalPropertyTransformHelper additionalPropertyTransformHelper;
    private readonly IAggregateLockingService aggregateLockingService;

    public CreateAdjustmentQuoteCommandHandler(
        IQuoteAggregateRepository quoteAggregateRepository,
        ICachingResolver cachingResolver,
        IProductFeatureSettingService productFeatureSettingService,
        IQuoteExpirySettingsProvider quoteExpirySettingsProvider,
        IHttpContextPropertiesResolver httpContextPropertiesResolver,
        IClock clock,
        ICqrsMediator mediator,
        IPolicyService policyService,
        IProductConfigurationProvider productConfigurationProvider,
        IQuoteWorkflowProvider quoteWorkflowProvider,
        IPolicyReadModelRepository policyReadModelRepository,
        IProductService productService,
        IAdditionalPropertyTransformHelper additionalPropertyValueService,
        IAggregateLockingService aggregateLockingService)
    {
        this.quoteAggregateRepository = quoteAggregateRepository;
        this.cachingResolver = cachingResolver;
        this.productFeatureSettingService = productFeatureSettingService;
        this.quoteExpirySettingsProvider = quoteExpirySettingsProvider;
        this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        this.clock = clock;
        this.mediator = mediator;
        this.policyService = policyService;
        this.productConfigurationProvider = productConfigurationProvider;
        this.quoteWorkflowProvider = quoteWorkflowProvider;
        this.policyReadModelRepository = policyReadModelRepository;
        this.productService = productService;
        this.additionalPropertyTransformHelper = additionalPropertyValueService;
        this.aggregateLockingService = aggregateLockingService;
    }

    public async Task<NewQuoteReadModel> Handle(CreateAdjustmentQuoteCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tenant = await this.cachingResolver.GetTenantOrThrow(command.TenantId);
        TenantHelper.ThrowIfTenantNotActive(command.TenantId, tenant);
        var quoteAggregateId = this.policyReadModelRepository.GetQuoteAggregateIdForPolicyId(command.TenantId, command.PolicyId);
        using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, quoteAggregateId, AggregateType.Quote))
        {
            var quoteAggregate = this.quoteAggregateRepository.GetById(command.TenantId, quoteAggregateId);
            var entityName = string.Empty;
            if (quoteAggregate == null)
            {
                entityName = TenantHelper.CheckAndChangeTextToMutual("policy", false);
                throw new ErrorException(Errors.General.NotFound(entityName, command.PolicyId));
            }

            if (quoteAggregate.TenantId != command.TenantId)
            {
                throw new ErrorException(Errors.General.NotAuthorized("create an adjustment quote", entityName, command.PolicyId));
            }

            if (quoteAggregate.Environment == DeploymentEnvironment.Production && command.ProductRelease != null)
            {
                throw new ErrorException(Errors.Release.ProductReleaseCannotBeSpecified(QuoteType.NewBusiness.Humanize()));
            }

            Product product = await this.cachingResolver.GetProductOrThrow(command.TenantId, quoteAggregate.ProductId);
            var isMutual = TenantHelper.IsMutual(tenant.Details.Alias);
            entityName
                = TenantHelper.CheckAndChangeTextToMutual("policy", isMutual);

            this.productService.ThrowIfProductNotActive(product, product.Details.Alias);

            var policy = quoteAggregate.Policy;
            if (policy == null)
            {
                throw new ErrorException(Errors.General.NotFound(entityName, command.PolicyId));
            }

            var productFeature = this.productFeatureSettingService.GetProductFeature(
                command.TenantId,
                quoteAggregate.ProductId);
            if (!productFeature.AreAdjustmentQuotesEnabled)
            {
                throw new ErrorException(
                    Errors.ProductFeatureSetting.QuoteTypeDisabled(
                        quoteAggregate.ProductId,
                        product.Details.Name,
                        "Adjustment"));
            }

            if (command.DiscardExisting)
            {
                // Check for existing quote and discard
                var latestQuote = quoteAggregate.GetLatestQuote();
                if (latestQuote != null && !(latestQuote is NewBusinessQuote) && !latestQuote.TransactionCompleted && !latestQuote.IsDiscarded)
                {
                    quoteAggregate.Discard(latestQuote.Id, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now());
                }
            }

            List<AdditionalPropertyValueUpsertModel>? additionalPropertyUpserts = null;
            if (command.AdditionalProperties != null)
            {
                additionalPropertyUpserts = await this.additionalPropertyTransformHelper.TransformObjectDictionaryToValueUpsertModels(
                                                command.TenantId,
                                                policy.Aggregate.OrganisationId,
                                                AdditionalPropertyEntityType.Quote,
                                                command.AdditionalProperties);
            }

            Guid productReleaseId = command.ProductRelease != null
                ? await this.cachingResolver.GetProductReleaseIdOrThrow(command.TenantId, product.Id, new GuidOrAlias(command.ProductRelease))
                : await this.mediator.Send(new GetProductReleaseIdQuery(
                    product.TenantId,
                    product.Id,
                    quoteAggregate.Environment,
                    QuoteType.Adjustment,
                    command.PolicyId));
            var releaseContext = new ReleaseContext(
                quoteAggregate.TenantId,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                productReleaseId);
            await this.productService.ThrowWhenResultingStateIsNotSupported(releaseContext, product, command.InitialQuoteState);
            var pastClaims = this.policyService.GetPolicyHistoricalClaims(quoteAggregate.Policy);
            var formDataSchema = this.productConfigurationProvider.GetFormDataSchema(
                releaseContext, WebFormAppType.Quote);
            string? quoteNumber = null;
            if (command.InitialQuoteState != null && command.InitialQuoteState != StandardQuoteStates.Nascent)
            {
                quoteNumber = await this.policyService.GenerateQuoteNumber(releaseContext);
            }

            IQuoteWorkflow quoteWorkflow
                = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
            var quoteExpirySettings = this.quoteExpirySettingsProvider.Retrieve(product);
            var adjustmentQuote = policy.CreateAdjustmentQuote(
                this.clock.Now(),
                quoteNumber,
                pastClaims,
                this.httpContextPropertiesResolver.PerformingUserId,
                quoteWorkflow,
                quoteExpirySettings,
                isMutual,
                productReleaseId,
                formDataSchema,
                command.FormData,
                command.InitialQuoteState,
                additionalPropertyUpserts);
            await this.quoteAggregateRepository.Save(quoteAggregate);
            return adjustmentQuote.ReadModel;
        }
    }
}
