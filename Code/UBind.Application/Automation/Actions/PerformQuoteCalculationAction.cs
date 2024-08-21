// <copyright file="PerformQuoteCalculationAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;
    using NodaTime;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Commands.QuoteCalculation;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Payment;
    using UBind.Domain.Product;
    using Void = UBind.Domain.Helpers.Void;

    public class PerformQuoteCalculationAction : Action
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IProvider<Data<string>>? quoteProvider;
        private readonly IProvider<Data<string>>? policyProvider;
        private readonly IProvider<Data<string>>? productProvider;
        private readonly IProvider<Data<string>>? environmentProvider;
        private readonly IProvider<Data<string>>? policyTransactionTypeProvider;
        private readonly IObjectProvider inputDataProvider;
        private readonly IObjectProvider? paymentDataProvider;
        private readonly IProvider<Data<bool>>? persistResultsProvider;
        private readonly IClock clock;
        private readonly ICqrsMediator mediator;

        public PerformQuoteCalculationAction(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>>? runCondition,
            IEnumerable<ErrorCondition>? beforeRunConditions,
            IEnumerable<ErrorCondition>? afterRunConditions,
            IEnumerable<IRunnableAction>? errorActions,
            IProvider<Data<string>>? quoteProvider,
            IProvider<Data<string>>? policyProvider,
            IProvider<Data<string>>? productProvider,
            IProvider<Data<string>>? environmentProvider,
            IProvider<Data<string>>? policyTransactionTypeProvider,
            IObjectProvider inputDataProvider,
            IObjectProvider? paymentDataProvider,
            IProvider<Data<bool>>? persistResultsProvider,
            ICachingResolver cachingResolver,
            IClock clock,
            ICqrsMediator mediator)
            : base(name, alias, description, asynchronous, runCondition, beforeRunConditions, afterRunConditions, errorActions)
        {
            this.cachingResolver = cachingResolver;
            this.quoteProvider = quoteProvider;
            this.policyProvider = policyProvider;
            this.productProvider = productProvider;
            this.environmentProvider = environmentProvider;
            this.policyTransactionTypeProvider = policyTransactionTypeProvider;
            this.inputDataProvider = inputDataProvider;
            this.paymentDataProvider = paymentDataProvider;
            this.persistResultsProvider = persistResultsProvider;
            this.clock = clock;
            this.mediator = mediator;
        }

        public override ActionData CreateActionData() => new PerformQuoteCalculationActionData(this.Name, this.Alias, this.clock);

        public override async Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext, ActionData actionData, bool isInternal = false)
        {
            PerformQuoteCalculationActionData calcActionData = (PerformQuoteCalculationActionData)actionData;
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
            var organisationId = providerContext.AutomationData.ContextManager.Organisation.Id;
            var productId = await this.ResolveProductId(tenantId, providerContext);
            calcActionData.Product = productId.ToString();
            var environment = await this.ResolveEnvironment(providerContext);
            calcActionData.Environment = environment.ToString();
            string? policyTransactionTypeString = (await this.policyTransactionTypeProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            string? quoteIdString = (await this.quoteProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            string? policyIdString = (await this.policyProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            Guid? quoteId = quoteIdString?.ParseGuidOrThrow();
            calcActionData.Quote = quoteId?.ToString();
            Guid? policyId = policyIdString?.ParseGuidOrThrow();
            calcActionData.Policy = policyId?.ToString();
            QuoteType? quoteType = null;
            if (policyTransactionTypeString != null)
            {
                quoteType = policyTransactionTypeString.ToEnumOrThrow<QuoteType>();
            }

            var inputData = (await this.inputDataProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (inputData == null)
            {
                throw new ErrorException(Errors.Calculation.InputDataMissing());
            }

            calcActionData.PolicyTransactionType = quoteType?.ToString().ToCamelCase();
            calcActionData.InputData = JObject.FromObject(inputData);
            CalculationDataModel calculationDataModel = new CalculationDataModel { FormModel = calcActionData.InputData };

            var paymentData = (await this.paymentDataProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            PaymentData? paymentDataModel = null;
            if (paymentData != null)
            {
                var jObject = JObject.FromObject(paymentData);
                paymentDataModel = jObject.ToObject<PaymentData>();
            }

            bool persistResults = (await this.persistResultsProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? false;
            var productContext = new ProductContext(tenantId, productId, environment);
            var command = new QuoteCalculationCommand(
                productContext,
                quoteId,
                policyId,
                quoteType,
                null,
                calculationDataModel,
                paymentDataModel,
                true,
                persistResults,
                organisationId);

            var source = CancellationTokenSource.CreateLinkedTokenSource(providerContext.CancellationToken);
            var calculationResponseModel = await this.mediator.Send(command, source.Token);
            calcActionData.Quote = command.QuoteId.ToString();
            calcActionData.Policy = command.PolicyId.ToString();
            calcActionData.PolicyTransactionType = command.QuoteType?.ToString();
            calcActionData.CalculationResult = JObject.FromObject(
                calculationResponseModel,
                JsonSerializer.Create(new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new CamelCaseNamingStrategy(),
                    },
                    Formatting = Formatting.Indented,
                }));

            return Result.Success<Void, Domain.Error>(default);
        }

        public override bool IsReadOnly() => false;

        private async Task<Guid> ResolveProductId(Guid tenantId, IProviderContext providerContext)
        {
            var productString = (await this.productProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (productString != null)
            {
                var product = await this.cachingResolver.GetProductOrThrow(tenantId, new Domain.Helpers.GuidOrAlias(productString));
                return product.Id;
            }

            return providerContext.AutomationData.ContextManager.Product.Id;
        }

        private async Task<DeploymentEnvironment> ResolveEnvironment(IProviderContext providerContext)
        {
            string? environment = (await this.environmentProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (environment != null)
            {
                return environment.ToEnumOrThrow<DeploymentEnvironment>();
            }

            return providerContext.AutomationData.System.Environment;
        }
    }
}
