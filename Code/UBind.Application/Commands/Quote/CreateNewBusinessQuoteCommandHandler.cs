// <copyright file="CreateNewBusinessQuoteCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote;

using System;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using NodaTime;
using UBind.Application.Queries.ProductRelease;
using UBind.Domain;
using UBind.Domain.Aggregates.AdditionalPropertyValue;
using UBind.Domain.Aggregates.Person;
using UBind.Domain.Aggregates.Quote;
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

public class CreateNewBusinessQuoteCommandHandler : ICommandHandler<CreateNewBusinessQuoteCommand, NewQuoteReadModel>
{
    private readonly ICachingResolver cachingResolver;
    private readonly IQuoteExpirySettingsProvider quoteExpirySettingsProvider;
    private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
    private readonly IClock clock;
    private readonly ICqrsMediator mediator;
    private readonly IQuoteAggregateRepository quoteAggregateRepository;
    private readonly IProductService productService;
    private readonly IPersonReadModelRepository personReadModelRepository;
    private readonly IPolicyTransactionTimeOfDayScheme timeOfDayScheme;
    private readonly IAdditionalPropertyTransformHelper additionalPropertyTransformHelper;
    private readonly IPolicyService policyService;
    private readonly IAggregateLockingService aggregateLockingService;

    public CreateNewBusinessQuoteCommandHandler(
        IQuoteAggregateRepository quoteAggregateRepository,
        ICachingResolver cachingResolver,
        IQuoteExpirySettingsProvider quoteExpirySettingsProvider,
        IHttpContextPropertiesResolver httpContextPropertiesResolver,
        IClock clock,
        ICqrsMediator mediator,
        IProductService productService,
        IPersonReadModelRepository personReadModelRepository,
        IPolicyTransactionTimeOfDayScheme timeOfDayScheme,
        IAdditionalPropertyTransformHelper additionalPropertyTransformHelper,
        IAggregateLockingService aggregateLockingService,
        IPolicyService policyService)
    {
        this.quoteAggregateRepository = quoteAggregateRepository;
        this.cachingResolver = cachingResolver;
        this.quoteExpirySettingsProvider = quoteExpirySettingsProvider;
        this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        this.clock = clock;
        this.mediator = mediator;
        this.productService = productService;
        this.personReadModelRepository = personReadModelRepository;
        this.timeOfDayScheme = timeOfDayScheme;
        this.additionalPropertyTransformHelper = additionalPropertyTransformHelper;
        this.policyService = policyService;
        this.aggregateLockingService = aggregateLockingService;
    }

    public async Task<NewQuoteReadModel> Handle(CreateNewBusinessQuoteCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tenant = await this.cachingResolver.GetTenantOrThrow(command.TenantId);
        TenantHelper.ThrowIfTenantNotActive(command.TenantId, tenant);
        var product = await this.cachingResolver.GetProductOrThrow(command.TenantId, command.ProductId);
        this.productService.ThrowIfProductNotActive(product, product.Details.Alias);
        await this.productService.ThrowIfProductNotEnabledForOrganisation(product, command.OrganisationId);
        this.productService.ThrowIfProductTransactionDisabledForTheLoadOperation(product);
        if (command.PortalId.HasValue)
        {
            this.productService.ThrowIfProductNotEnabledForPortal(command.TenantId, product, command.PortalId.Value);
        }

        if (command.Environment == DeploymentEnvironment.Production && command.ProductRelease != null)
        {
            throw new ErrorException(Errors.Release.ProductReleaseCannotBeSpecified(QuoteType.NewBusiness.Humanize()));
        }

        Guid productReleaseId = command.ProductRelease != null
            ? await this.cachingResolver.GetProductReleaseIdOrThrow(command.TenantId, product.Id, new GuidOrAlias(command.ProductRelease))
            : await this.mediator.Send(new GetProductReleaseIdQuery(
                tenant.Id,
                product.Id,
                command.Environment,
                QuoteType.NewBusiness,
                default));
        var releaseContext = new ReleaseContext(
            tenant.Id,
            product.Id,
            command.Environment,
            productReleaseId);
        await this.productService.ThrowWhenResultingStateIsNotSupported(releaseContext, product, command.InitialQuoteState);
        List<AdditionalPropertyValueUpsertModel>? additionalPropertyUpserts = null;
        if (command.AdditionalProperties != null)
        {
            additionalPropertyUpserts = await this.additionalPropertyTransformHelper.TransformObjectDictionaryToValueUpsertModels(
                command.TenantId,
                command.OrganisationId,
                AdditionalPropertyEntityType.Quote,
                command.AdditionalProperties);
        }

        string? quoteNumber = null;
        if (quoteNumber == null && command.InitialQuoteState != null && command.InitialQuoteState != StandardQuoteStates.Nascent)
        {
            quoteNumber = await this.policyService.GenerateQuoteNumber(releaseContext);
        }

        // TODO: consider whether we allow this to be passed in, or we get it from the initial form data.
        // For now we'll default it to AET. It can be changed as part of a FormUpdate operation though
        // so it's not a big deal.
        DateTimeZone timeZone = Timezones.AET;
        var quoteExpirySettings = this.quoteExpirySettingsProvider.Retrieve(product);
        var quote = QuoteAggregate.CreateNewBusinessQuote(
            tenant.Id,
            command.OrganisationId,
            product.Id,
            command.Environment,
            quoteExpirySettings,
            this.httpContextPropertiesResolver.PerformingUserId,
            this.clock.Now(),
            productReleaseId,
            timeZone,
            this.timeOfDayScheme.AreTimestampsAuthoritative,
            command.CustomerId,
            command.IsTestData,
            command.FormData,
            quoteNumber,
            command.InitialQuoteState,
            additionalPropertyUpserts);
        using (await this.aggregateLockingService.CreateLockOrThrow(tenant.Id, quote.Aggregate.Id, AggregateType.Quote))
        {
            QuoteAggregate quoteAggregate = quote.Aggregate;
            if (command.OwnerUserId.HasValue)
            {
                await this.mediator.Send(
                    new AssignOwnerCommand(quoteAggregate, command.OwnerUserId.Value));
            }

            if (quote.CustomerId.HasValue)
            {
                var person = this.personReadModelRepository.GetPersonAssociatedWithPrimaryPersonByCustmerId(quoteAggregate.TenantId, quote.CustomerId.Value);
                if (person == null)
                {
                    throw new ErrorException(Errors.Customer.NotFound(quote.CustomerId.Value));
                }
                var personalDetails = new PersonalDetails(quoteAggregate.TenantId, person);
                quoteAggregate.UpdateCustomerDetails(personalDetails, this.httpContextPropertiesResolver.PerformingUserId, this.clock.Now(), quote.Id);
            }

            await this.quoteAggregateRepository.Save(quoteAggregate);
            return quote.ReadModel;
        }
    }
}
