// <copyright file="CardPaymentCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.Quote
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using MediatR;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Payment;
    using UBind.Application.Payment.Stripe;
    using UBind.Application.Queries.Person;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Configuration;
    using UBind.Domain.CustomPipelines.BindPolicy;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Payment;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;

    public class CardPaymentCommandHandler
        : ICommandHandler<CardPaymentCommand, Unit>, IPipelineBehavior<BindPolicyCommand, ValueTuple<NewQuoteReadModel, PolicyReadModel>>
    {
        private readonly IClock clock;
        private readonly IProductFeatureSettingService productFeatureSettingService;
        private readonly IPaymentConfigurationProvider paymentConfigurationProvider;
        private readonly PaymentGatewayFactory paymentGatewayFactory;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly ISavedPaymentMethodRepository paymentMethodRepository;
        private readonly IQuoteWorkflowProvider quoteWorkflowProvider;
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly ICqrsMediator mediator;
        private readonly IAggregateLockingService aggregateLockingService;

        public CardPaymentCommandHandler(
            IProductFeatureSettingService productFeature,
            IPaymentConfigurationProvider paymentConfiguration,
            PaymentGatewayFactory paymentGatewayFactory,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IQuoteAggregateRepository quoteAggregateRepo,
            IQuoteAggregateResolverService quoteAggregateResolver,
            IQuoteWorkflowProvider quoteWorkflow,
            ISavedPaymentMethodRepository paymentMethodRepository,
            IProductConfigurationProvider productConfigurationProvider,
            ICqrsMediator mediator,
            IClock clock,
            IAggregateLockingService aggregateLockingService)
        {
            this.clock = clock;
            this.productFeatureSettingService = productFeature;
            this.paymentConfigurationProvider = paymentConfiguration;
            this.paymentGatewayFactory = paymentGatewayFactory;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.quoteAggregateRepository = quoteAggregateRepo;
            this.quoteAggregateResolverService = quoteAggregateResolver;
            this.quoteWorkflowProvider = quoteWorkflow;
            this.paymentMethodRepository = paymentMethodRepository;
            this.productConfigurationProvider = productConfigurationProvider;
            this.mediator = mediator;
            this.aggregateLockingService = aggregateLockingService;
        }

        public async Task<Unit> Handle(CardPaymentCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForQuoteId(command.QuoteId);
            using (await this.aggregateLockingService.CreateLockOrThrow(command.ReleaseContext.TenantId, quoteAggregateId, AggregateType.Quote))
            {
                var quoteAggregate = this.quoteAggregateRepository.GetById(command.ReleaseContext.TenantId, quoteAggregateId);
                quoteAggregate = EntityHelper.ThrowIfNotFound(quoteAggregate, quoteAggregateId, "quote aggregate");
                var quote = quoteAggregate.GetQuoteOrThrow(command.QuoteId);

                if (quote.IsPaidFor || quote.IsFunded)
                {
                    throw new ErrorException(Errors.Payment.PaymentAlreadyMade());
                }

                IQuoteWorkflow quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(
                    command.ReleaseContext);
                if (!quote.IsPaymentAdmissible(quoteWorkflow))
                {
                    var operation = quoteWorkflow.GetOperation(QuoteAction.Payment);
                    throw new ErrorException(Errors.Quote.OperationNotPermittedForState(
                        operation.Action.Humanize(),
                        quote.QuoteStatus,
                        operation.ResultingState,
                        operation.RequiredStates));
                }
                quote.ThrowIfGivenCalculationIdNotMatchingTheLatest(command.CalculationResultId);
                var calculationResult = quote.LatestCalculationResult.Data;
                var productConfiguration = await this.productConfigurationProvider.GetProductConfiguration(
                    command.ReleaseContext, WebFormAppType.Quote);
                var dataRetriever = new StandardQuoteDataRetriever(productConfiguration, command.LatestFormData, calculationResult);

                var paymentResult = await this.ProcessPayment(
                    command.ReleaseContext,
                    quoteAggregate,
                    command.QuoteId,
                    command.LatestFormData,
                    command.CalculationResultId,
                    productConfiguration,
                    command.PaymentTokenId,
                    command.SavedPaymentMethodId.GetValueOrDefault(),
                    command.CreditCardDetails);

                quoteAggregate = await ConcurrencyPolicy.ExecuteWithRetriesAsync(() => this.UpdateQuoteAggregate(command, quoteAggregate.Id, paymentResult));
                var persistPaymentDetails = dataRetriever.Retrieve<bool>(StandardQuoteDataField.SaveCustomerPaymentDetails);
                if (persistPaymentDetails
                    && command.CreditCardDetails != null
                    && paymentResult.Success)
                {
                    var expiryDate = this.RetrieveExpiryDate(command.CreditCardDetails);
                    quote = quoteAggregate.GetQuoteOrThrow(command.QuoteId);
                    if (!quote.CustomerId.HasValue)
                    {
                        throw new ErrorException(Errors.Quote.CustomerDetailsNotFound(command.QuoteId));
                    }

                    if (quote.LatestPaymentAttemptResult.PaymentDetails.Request == null)
                    {
                        throw new ErrorException(Errors.Payment.PaymentGatewayResponseNotFound("credit card"));
                    }

                    this.PersistUsedPaymentMethod(command.ReleaseContext.TenantId, quote.CustomerId.Value, expiryDate, quote.LatestPaymentAttemptResult.PaymentDetails.Request);
                }

                return Unit.Value;
            }
        }

        public async Task<ValueTuple<NewQuoteReadModel, PolicyReadModel>> Handle(
            BindPolicyCommand command,
            RequestHandlerDelegate<ValueTuple<NewQuoteReadModel, PolicyReadModel>> next,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var quoteAggregate = command.QuoteAggregate;
            if (quoteAggregate != null)
            {
                if (!command.QuoteId.HasValue)
                {
                    throw new ErrorException(Errors.General.Unexpected("Unable to retrieve the quote Id used for binding."));
                }
                var quote = quoteAggregate.GetQuoteOrThrow(command.QuoteId.Value);

                // already paid or funded in a previous operation.
                if (quote.IsPaidFor || quote.IsFunded)
                {
                    return await next();
                }

                // Since 'HaveTriedPersistingCommandBefore' flag is set to true, then this command has been
                // handled initially for persisting by the SaveBindCommandHandler.
                // We do not want the payment to be reprocessed (even if it's a fail) as every payment attempt needs to be recorded.
                // Instead, we are skipping processing of payment here, and allow SaveBindCommandHandler to process the payment attempt
                // from the previous retry.
                // If payment attempt is a failure, persist and throw error. See line 100 of SaveBindCommandHandler.
                // If payment attempt is a success, proceed with binding.
                if (command.HaveTriedPersistingCommandBefore && command.PaymentResult != null)
                {
                    return await next();
                }

                // already funded
                if (command.AcceptedFundingProposal != null)
                {
                    return await next();
                }

                // no card details for payment, skip.
                if (command.PaymentToken == null && command.SavedPaymentId == default && command.PaymentMethodDetails == null)
                {
                    return await next();
                }

                quote.ThrowIfGivenCalculationIdNotMatchingTheLatest(command.BindRequirements?.CalculationResultId);
                var calculationResult = quote.LatestCalculationResult.Data;
                IQuoteWorkflow quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(command.ReleaseContext);
                if (calculationResult.PayablePrice.TotalPayable <= 0
                    && (!quoteWorkflow.IsSettlementRequired || !quoteWorkflow.IsSettlementSupported))
                {
                    return await next();
                }

                var productConfiguration = await this.productConfigurationProvider.GetProductConfiguration(
                    command.ReleaseContext, WebFormAppType.Quote);
                var formData = quote.LatestFormData.Data;
                var dataRetriever = new StandardQuoteDataRetriever(productConfiguration, formData, calculationResult);
                if (command.BindRequirements == null)
                {
                    throw new ErrorException(Errors.Operations.Bind.CalculationIdNotProvided());
                }
                var creditCardDetails = command.PaymentMethodDetails as CreditCardDetails;
                var paymentResult = await this.ProcessPayment(
                    command.ReleaseContext,
                    quoteAggregate,
                    command.QuoteId.Value,
                    formData,
                    command.BindRequirements.CalculationResultId,
                    productConfiguration,
                    command.PaymentToken,
                    command.SavedPaymentId,
                    creditCardDetails);

                command.PaymentResult = paymentResult;
                var persistPaymentDetails = dataRetriever.Retrieve<bool>(StandardQuoteDataField.SaveCustomerPaymentDetails);
                if (persistPaymentDetails && paymentResult.Success)
                {
                    if (creditCardDetails == null)
                    {
                        throw new ErrorException(Errors.Payment.PaymentDetailsNotFound("credit card"));
                    }

                    var expiryDate = this.RetrieveExpiryDate(creditCardDetails);
                    if (!quote.CustomerId.HasValue)
                    {
                        throw new ErrorException(Errors.Quote.CustomerDetailsNotFound(command.QuoteId.Value));
                    }

                    if (paymentResult.Details.Request == null)
                    {
                        throw new ErrorException(Errors.Payment.PaymentGatewayResponseNotFound("credit card"));
                    }

                    this.PersistUsedPaymentMethod(command.ReleaseContext.TenantId, quote.CustomerId.Value, expiryDate, paymentResult.Details.Request);
                }

                return await next();
            }

            return await next();
        }

        private async Task<PaymentGatewayResult> ProcessPayment(
            ReleaseContext releaseContext,
            QuoteAggregate quoteAggregate,
            Guid quoteId,
            FormData latestFormData,
            Guid calculationResultId,
            IProductConfiguration productConfiguration,
            string? paymentToken = null,
            Guid savedPaymentMethodId = default,
            CreditCardDetails? cardDetails = null)
        {
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);
            var productPaymentConfigResult =
                await this.paymentConfigurationProvider.GetConfigurationAsync(releaseContext);
            if (productPaymentConfigResult.HasNoValue)
            {
                throw new ErrorException(Errors.Payment.NotConfigured(quote.ProductContext));
            }

            quote.ThrowIfGivenCalculationIdNotMatchingTheLatest(calculationResultId);
            var calculationResult = quote.LatestCalculationResult.Data;
            var productPaymentConfig = productPaymentConfigResult.Value;
            var gateway = this.paymentGatewayFactory.Create(productPaymentConfig);
            var priceBreakdown = calculationResult.PayablePrice;
            var dataRetriever = new StandardQuoteDataRetriever(
                productConfiguration, latestFormData, calculationResult);

            var useSavedPaymentDetails = dataRetriever.Retrieve<bool>(StandardQuoteDataField.UseSavedPaymentMethod);
            PaymentGatewayResult? paymentResult = null;

            // Using stripe token.
            if (!string.IsNullOrEmpty(paymentToken))
            {
                if (quoteAggregate.IsTestData)
                {
                    if (gateway is StripePaymentGateway)
                    {
                        // The minimum amount for stripe is AUD $0.50.
                        priceBreakdown = PriceBreakdown.OneMinorUnits(priceBreakdown.CurrencyCode) * 50;
                    }
                    else
                    {
                        priceBreakdown = PriceBreakdown.OneMinorUnits(priceBreakdown.CurrencyCode);
                    }
                }

                var personName = await this.GetPersonNameForReference(releaseContext, quote, latestFormData);
                var reference = $"{quote.QuoteNumber} "
                + $"/ {personName ?? string.Empty} "
                + $"/ {calculationResultId}";
                paymentResult = await gateway.MakePayment(
                    priceBreakdown,
                    paymentToken,
                    reference);
            } // Using a saved payment method of the customer.
            else if (useSavedPaymentDetails)
            {
                if (!quote.CustomerId.HasValue)
                {
                    throw new ErrorException(Errors.Quote.CustomerDetailsNotFound(quote.Id));
                }

                SavedPaymentMethod? savedPaymentMethod = null;
                if (savedPaymentMethodId != default)
                {
                    savedPaymentMethod = this.paymentMethodRepository.GetByIdForPayment(quoteAggregate.TenantId, savedPaymentMethodId);
                    if (savedPaymentMethod == null)
                    {
                        throw new ErrorException(Errors.Payment.SavedPayments.SavedPaymentMethodNotFound(savedPaymentMethodId));
                    }
                }
                else
                {
                    savedPaymentMethod = this.paymentMethodRepository
                        .GetSavedPaymentMethodsForPaymentByCustomer(quoteAggregate.TenantId, quote.CustomerId.Value)
                        .FirstOrDefault();
                    if (savedPaymentMethod == null)
                    {
                        throw new ErrorException(Errors.Payment.SavedPayments.CustomerHasNoSavedPayments());
                    }
                }

                if (quoteAggregate.IsTestData)
                {
                    priceBreakdown = PriceBreakdown.OneMinorUnits(priceBreakdown.CurrencyCode);
                }

                paymentResult = await gateway.MakePayment(quote, priceBreakdown, savedPaymentMethod);
            } // Using a credit card entered.
            else
            {
                if (quoteAggregate.IsTestData)
                {
                    priceBreakdown = PriceBreakdown.OneMinorUnits(priceBreakdown.CurrencyCode);
                }

                if (cardDetails == null)
                {
                    throw new ErrorException(Errors.Payment.PaymentDetailsNotFound("credit card"));
                }

                paymentResult = await gateway.MakePayment(
                    quote,
                    priceBreakdown,
                    cardDetails,
                    calculationResult.GetType().ToString());
            }

            return paymentResult;
        }

        private async Task<QuoteAggregate> UpdateQuoteAggregate(
            CardPaymentCommand command,
            Guid quoteAggregateId,
            PaymentGatewayResult paymentResult)
        {
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var aggregate = this.quoteAggregateRepository.GetById(command.ReleaseContext.TenantId, quoteAggregateId);
            aggregate = EntityHelper.ThrowIfNotFound(aggregate, quoteAggregateId, "quote aggregate");
            var quote = aggregate.GetQuoteOrThrow(command.QuoteId);
            quote.UpdateFormData(command.LatestFormData, performingUserId, this.clock.Now());
            var paymentResultErrors = paymentResult.Errors ?? Enumerable.Empty<string>();
            if (paymentResult.Success)
            {
                aggregate.RecordPaymentMade(paymentResult.Reference, paymentResult.Details, performingUserId, this.clock.Now(), command.QuoteId);
            }
            else
            {
                aggregate.RecordPaymentFailed(paymentResultErrors, performingUserId, this.clock.Now(), command.QuoteId);
            }

            await this.quoteAggregateRepository.Save(aggregate);
            if (!paymentResult.Success)
            {
                if (this.httpContextPropertiesResolver.IsIpAddressWhitelisted)
                {
                    throw new ErrorException(Errors.Payment.CardPaymentFailed(paymentResultErrors, paymentResult.Details));
                }
                else
                {
                    throw new ErrorException(Errors.Payment.CardPaymentFailed(paymentResultErrors));
                }
            }

            return aggregate;
        }

        private async Task<string?> GetPersonNameForReference(ReleaseContext releaseContext, Quote quote, FormData formData)
        {
            IPersonalDetails? personDetails = quote.LatestCustomerDetails?.Data;
            if (personDetails == null && quote.CustomerId.HasValue)
            {
                personDetails = await this.mediator.Send(new GetPrimaryPersonForCustomerQuery(quote.Aggregate.TenantId, quote.CustomerId.Value));
            }

            string? personName = personDetails?.FullName;
            if (string.IsNullOrEmpty(personName))
            {
                var productConfiguration = await this.productConfigurationProvider.GetProductConfiguration(
                    releaseContext, WebFormAppType.Quote);
                var dataRetriever = new StandardQuoteDataRetriever(productConfiguration, formData, quote.LatestCalculationResult.Data);
                personName = dataRetriever.Retrieve<string>(StandardQuoteDataField.InsuredName)
                    ?? dataRetriever.Retrieve<string>(StandardQuoteDataField.TradingName)
                    ?? dataRetriever.Retrieve<string>(StandardQuoteDataField.CustomerName);
            }

            return personName;
        }

        private LocalDate RetrieveExpiryDate(CreditCardDetails cardDetails)
        {
            return new LocalDate(int.Parse(cardDetails.ExpiryYear), int.Parse(cardDetails.ExpiryMonth), 1)
                        .PlusMonths(1);
        }

        private void PersistUsedPaymentMethod(Guid tenantId, Guid customerId, LocalDate expiryDate, string serializedPaymentInformation)
        {
            var identificationDataObject = JObject.Parse(serializedPaymentInformation);
            var cardDetails = JsonConvert.SerializeObject(identificationDataObject.GetToken("cardAccount"));
            identificationDataObject.Remove("cardAccount");
            var authentificationInformation = JsonConvert.SerializeObject(identificationDataObject);

            var customerPaymentMethod = new SavedPaymentMethod(
                tenantId,
                customerId,
                PaymentMethod.GetZaiPaymentMethod().Id,
                expiryDate,
                cardDetails,
                authentificationInformation,
                this.clock.Now());
            this.paymentMethodRepository.Insert(customerPaymentMethod);
            this.paymentMethodRepository.SaveChanges();
        }
    }
}
