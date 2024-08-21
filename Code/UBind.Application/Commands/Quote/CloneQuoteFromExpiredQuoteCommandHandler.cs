// <copyright file="CloneQuoteFromExpiredQuoteCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Services;
    using UBind.Domain.Services.QuoteExpiry;

    public class CloneQuoteFromExpiredQuoteCommandHandler : ICommandHandler<CloneQuoteFromExpiredQuoteCommand, NewQuoteReadModel>
    {
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly IQuoteWorkflowProvider quoteWorkflowProvider;
        private readonly IQuoteReadModelRepository quoteReadModelRepository;
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly IQuoteExpirySettingsProvider quoteExpirySettingsProvider;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IClock clock;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IProductFeatureSettingService productFeatureSettingService;
        private readonly IPolicyService policyService;
        private readonly IPolicyTransactionTimeOfDayScheme timeOfDayScheme;
        private readonly IReleaseQueryService releaseQueryService;
        private readonly IAggregateLockingService aggregateLockingService;

        public CloneQuoteFromExpiredQuoteCommandHandler(
            IQuoteAggregateRepository quoteAggregateRepository,
            IQuoteReadModelRepository quoteReadModelRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            IUserReadModelRepository userReadModelRepository,
            IPersonAggregateRepository personAggregateRepository,
            IProductConfigurationProvider productConfigurationProvider,
            IQuoteWorkflowProvider quoteWorkflowProvider,
            IQuoteExpirySettingsProvider quoteExpirySettingsProvider,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IClock clock,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IProductFeatureSettingService productFeatureSettingService,
            IPolicyService policyService,
            IPolicyTransactionTimeOfDayScheme timeOfDayScheme,
            IReleaseQueryService releaseQueryService,
            IAggregateLockingService aggregateLockingService)
        {
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.clock = clock;
            this.quoteExpirySettingsProvider = quoteExpirySettingsProvider;
            this.productConfigurationProvider = productConfigurationProvider;
            this.quoteWorkflowProvider = quoteWorkflowProvider;
            this.quoteReadModelRepository = quoteReadModelRepository;
            this.customerAggregateRepository = customerAggregateRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.personAggregateRepository = personAggregateRepository;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.productFeatureSettingService = productFeatureSettingService;
            this.policyService = policyService;
            this.timeOfDayScheme = timeOfDayScheme;
            this.releaseQueryService = releaseQueryService;
            this.aggregateLockingService = aggregateLockingService;
        }

        public async Task<NewQuoteReadModel> Handle(CloneQuoteFromExpiredQuoteCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Quote newQuote;
            Instant now = this.clock.Now();
            var quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(command.QuoteId);
            using (await this.aggregateLockingService.CreateLockOrThrow(command.TenantId, quoteAggregateId, AggregateType.Quote))
            {
                var quote = this.quoteReadModelRepository.GetQuoteSummary(command.TenantId, command.QuoteId);
                if (quote == null)
                {
                    throw new ErrorException(Errors.General.NotFound("quote", command.QuoteId));
                }

                var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
                var quoteAggregate = this.quoteAggregateRepository.GetById(command.TenantId, quoteAggregateId);
                quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quote aggregate");
                var domainQuote = quoteAggregate.GetQuoteOrThrow(command.QuoteId);
                var releaseContext = this.releaseQueryService.GetReleaseContextForReleaseOrDefaultRelease(
                    command.TenantId,
                    quoteAggregate.ProductId,
                    quoteAggregate.Environment,
                    domainQuote.ProductReleaseId);
                var formDataSchema = this.productConfigurationProvider.GetFormDataSchema(releaseContext, WebFormAppType.Quote);
                await this.productFeatureSettingService.ThrowIfFeatureIsNotEnabled(
                    quoteAggregate.TenantId,
                    quoteAggregate.ProductId,
                    ProductFeatureSettingService.GetQuoteProductFeatureByQuoteType(domainQuote.Type),
                    "copy expired quote");
                if (!domainQuote.IsExpired(this.clock.Now()))
                {
                    throw new InvalidOperationException("Quote should be expired to be able to replicate from the quote");
                }

                // Replacement quote for new business quote are created in new aggregates.
                var quoteExpirySettings = await this.quoteExpirySettingsProvider.Retrieve(quoteAggregate.TenantId, quoteAggregate.ProductId);
                IQuoteWorkflow quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
                if (quote.QuoteType == QuoteType.NewBusiness)
                {
                    var customerId = quoteAggregate.CustomerId;
                    var newBusinessQuoteNumber = await this.policyService.GenerateQuoteNumber(releaseContext);
                    newQuote = QuoteAggregate.CreateNewBusinessQuote(
                        quoteAggregate.TenantId,
                        quoteAggregate.OrganisationId,
                        quoteAggregate.ProductId,
                        quoteAggregate.Environment,
                        quoteExpirySettings,
                        performingUserId,
                        now,
                        quote.ProductReleaseId,
                        quote.TimeZone,
                        this.timeOfDayScheme.AreTimestampsAuthoritative,
                        customerId,
                        quoteAggregate.IsTestData);
                    var newQuoteAggregate = newQuote.Aggregate;

                    if (customerId != null && customerId.Value != default)
                    {
                        var customerAggregate = this.customerAggregateRepository.GetById(newQuoteAggregate.TenantId, customerId.Value);
                        customerAggregate = EntityHelper.ThrowIfNotFound(customerAggregate, customerId.Value, "customer");
                        var personAggregate = this.personAggregateRepository.GetById(customerAggregate.TenantId, customerAggregate.PrimaryPersonId);
                        personAggregate = EntityHelper.ThrowIfNotFound(personAggregate, customerAggregate.PrimaryPersonId, "person");
                        newQuoteAggregate.RecordAssociationWithCustomer(customerAggregate, personAggregate, performingUserId, now);
                        newQuoteAggregate.UpdateCustomerDetails(personAggregate, performingUserId, now, newQuote.Id);
                        newQuote.AssignQuoteNumber(newBusinessQuoteNumber, performingUserId, now);
                    }

                    if (quoteAggregate.OwnerUserId.HasValue)
                    {
                        var ownerUserId = quoteAggregate.OwnerUserId.Value;
                        var user = this.userReadModelRepository.GetUser(command.TenantId, ownerUserId);
                        var personAggregate = this.personAggregateRepository.GetById(command.TenantId, user.PersonId);
                        personAggregate = EntityHelper.ThrowIfNotFound(personAggregate, user.PersonId, "person");
                        newQuoteAggregate.AssignToOwner(personAggregate, performingUserId, now);
                    }

                    var newBusinessQuote = quoteAggregate.GetQuoteOrThrow(command.QuoteId);
                    FormData formData = newBusinessQuote.RemoveFieldValuesThatAreConfiguredToBeReset(formDataSchema);

                    newQuote.UpdateFormData(formData, performingUserId, now);
                    await this.quoteAggregateRepository.Save(newQuoteAggregate);
                    return newQuote.ReadModel;
                }

                // Replacement quotes for adjustment or renewals must be created in the same aggregate.
                var quoteNumber = await this.policyService.GenerateQuoteNumber(releaseContext);
                quoteAggregate.Discard(command.QuoteId, this.httpContextPropertiesResolver.PerformingUserId, now);
                Guid newQuoteId = quoteAggregate.CreateCloneQuoteForExpiredQuote(
                    command.QuoteId,
                    quoteNumber,
                    this.httpContextPropertiesResolver.PerformingUserId,
                    now,
                    quoteWorkflow,
                    formDataSchema);

                quoteAggregate.SetQuoteExpiryFromSettings(command.QuoteId, performingUserId, now, quoteExpirySettings);

                await this.quoteAggregateRepository.Save(quoteAggregate);

                // get current quote.
                newQuote = quoteAggregate.GetQuoteOrThrow(newQuoteId);

                return newQuote.ReadModel;
            }
        }
    }
}
