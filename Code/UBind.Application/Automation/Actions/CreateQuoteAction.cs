// <copyright file="CreateQuoteAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Humanizer;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Queries.ProductOrganisation;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.Services;
    using UBind.Domain.ValueTypes;

    public class CreateQuoteAction : Action
    {
        private readonly string? policyTransactionType;
        private readonly PolicyEntityProvider? policyReferenceProvider;
        private readonly OrganisationEntityProvider? organisationReferenceProvider;
        private readonly ProductEntityProvider? productReferenceProvider;
        private readonly IProvider<Data<string>>? environmentProvider;
        private readonly IObjectProvider? formDataProvider;
        private readonly CustomerEntityProvider? customerReferenceProvider;
        private readonly IProvider<Data<string>>? initialQuoteStateProvider;
        private readonly IObjectProvider? additionalPropertiesProvider;
        private readonly IProvider<Data<bool>>? testDataProvider;
        private readonly IProductFeatureSettingService productFeatureSettingService;
        private readonly IPolicyReadModelRepository policyReadModelRepository;
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly ICqrsMediator mediator;
        private readonly IClock clock;
        private JObject? errorData;

        public CreateQuoteAction(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>>? runCondition,
            IEnumerable<ErrorCondition>? beforeErrorConditions,
            IEnumerable<ErrorCondition>? afterErrorConditions,
            IEnumerable<Action>? onErrorActions,
            string? policyTransactionType,
            PolicyEntityProvider? policyReferenceProvider,
            OrganisationEntityProvider? organisationReferenceProvider,
            ProductEntityProvider? productReferenceProvider,
            IProvider<Data<string>>? environmentProvider,
            IObjectProvider? formDataProvider,
            CustomerEntityProvider? customerReferenceProvider,
            IProvider<Data<string>>? initialQuoteStateProvider,
            IObjectProvider? additionalPropertiesProvider,
            IProvider<Data<bool>>? testDataProvider,
            IProductFeatureSettingService productFeatureSettingService,
            IPolicyReadModelRepository policyReadModelRepository,
            ICustomerReadModelRepository customerReadModelRepository,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator,
            IClock clock)
            : base(name, alias, description, asynchronous, runCondition, beforeErrorConditions, afterErrorConditions, onErrorActions)
        {
            this.policyTransactionType = policyTransactionType;
            this.policyReferenceProvider = policyReferenceProvider;
            this.organisationReferenceProvider = organisationReferenceProvider;
            this.productReferenceProvider = productReferenceProvider;
            this.environmentProvider = environmentProvider;
            this.formDataProvider = formDataProvider;
            this.customerReferenceProvider = customerReferenceProvider;
            this.initialQuoteStateProvider = initialQuoteStateProvider;
            this.additionalPropertiesProvider = additionalPropertiesProvider;
            this.testDataProvider = testDataProvider;
            this.productFeatureSettingService = productFeatureSettingService;
            this.policyReadModelRepository = policyReadModelRepository;
            this.customerReadModelRepository = customerReadModelRepository;
            this.cachingResolver = cachingResolver;
            this.mediator = mediator;
            this.clock = clock;
        }

        public override ActionData CreateActionData() => new CreateQuoteActionData(this.Name, this.Alias, this.clock);

        public override bool IsReadOnly() => false;

        public override async Task<Result<Void, Domain.Error>> Execute(IProviderContext providerContext, ActionData actionData, bool isInternal = false)
        {
            using (MiniProfiler.Current.Step("CreateQuoteAction.Execute"))
            {
                actionData.UpdateState(ActionState.Running);
                var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
                var organisationId = this.organisationReferenceProvider != null
                    ? (await this.organisationReferenceProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue.Id
                    : providerContext.AutomationData.ContextManager.Organisation.Id;
                var productId = this.productReferenceProvider != null
                    ? (await this.productReferenceProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue.Id
                    : providerContext.AutomationData.ContextManager.Product.Id;

                DeploymentEnvironment environment = default;
                if (this.environmentProvider != null)
                {
                    string inputtedEnvironment = (await this.environmentProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                    if (!Enum.TryParse(inputtedEnvironment, true, out DeploymentEnvironment deploymentEnvironmentResult))
                    {
                        throw new ErrorException(Errors.Automation.EnvironmentNotFound(inputtedEnvironment));
                    }

                    environment = deploymentEnvironmentResult;
                }
                else
                {
                    environment = providerContext.AutomationData.System.Environment;
                }

                this.errorData = await providerContext.GetDebugContext();
                Guid? policyId = (await this.policyReferenceProvider.ResolveValueIfNotNull(providerContext))?.DataValue.Id;
                var policy = policyId != null ? this.policyReadModelRepository.GetPolicyDetails(tenantId, policyId.Value) : null;
                if (policyId != null && policy == null)
                {
                    throw new ErrorException(
                       Errors.Policy.NotFound(policyId.Value));
                }

                var organisation = await this.cachingResolver.GetOrganisationOrThrow(tenantId, organisationId);
                if (!organisation.IsActive)
                {
                    throw new ErrorException(
                        Errors.Quote.Creation.OrganisationDisabled(organisation.Id.ToString(), organisation.Alias, organisation.Name, this.errorData));
                }

                Guid? customerId = (await this.customerReferenceProvider.ResolveValueIfNotNull(providerContext))?.DataValue.Id;
                var customer = customerId != null ? this.customerReadModelRepository.GetCustomerById(tenantId, customerId.Value) : null;
                if (customerId != null && customer == null)
                {
                    throw new ErrorException(
                           Errors.Customer.NotFound(customerId.Value));
                }

                var product = await this.cachingResolver.GetProductOrThrow(tenantId, productId);
                if (product.Details.Disabled)
                {
                    throw new ErrorException(Errors.Quote.Creation.ProductDisabled(product.Id.ToString(), product.Details.Alias, product.Details.Name, this.errorData));
                }

                this.ValidatePolicy(policy, environment, organisation, customer, product);
                this.ValidateCustomer(tenantId, environment, policy, organisation, customer);

                bool transactionTypeInputIsSupported = true;
                TransactionType transactionType = TransactionType.NewBusiness;
                if (!string.IsNullOrEmpty(this.policyTransactionType))
                {
                    transactionTypeInputIsSupported = Enum.TryParse(this.policyTransactionType, true, out TransactionType transactionTypeResult);
                    transactionType = transactionTypeResult;
                }

                string? initialQuoteState = (await this.initialQuoteStateProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
                bool isTestData = (await this.testDataProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? false;
                var productFeatureSetting = this.productFeatureSettingService.GetProductFeature(tenantId, productId);
                if (productFeatureSetting != null && !productFeatureSetting.IsProductFeatureEnabled(this.TransactionTypeToProductFeature(transactionType)))
                {
                    if (transactionType == TransactionType.NewBusiness)
                    {
                        throw new ErrorException(
                            Errors.Quote.Creation.NewBusinessQuoteTypeDisabled(product.Id.ToString(), product.Details.Alias, product.Details.Name, this.errorData));
                    }
                    else if (transactionType == TransactionType.Adjustment)
                    {
                        throw new ErrorException(
                            Errors.Quote.Creation.AdjustmentQuoteTypeDisabled(product.Id.ToString(), product.Details.Alias, product.Details.Name, this.errorData));
                    }
                    else if (transactionType == TransactionType.Renewal)
                    {
                        throw new ErrorException(
                            Errors.Quote.Creation.RenewalQuoteTypeDisabled(product.Id.ToString(), product.Details.Alias, product.Details.Name, this.errorData));
                    }
                    else if (transactionType == TransactionType.Cancellation)
                    {
                        throw new ErrorException(
                            Errors.Quote.Creation.CancellationQuoteTypeDisabled(product.Id.ToString(), product.Details.Alias, product.Details.Name, this.errorData));
                    }
                }

                var productOrganisationSettingQuery = new GetProductOrganisationSettingQuery(tenantId, organisationId, productId);
                var productOrganisationSetting = await this.mediator.Send(productOrganisationSettingQuery);
                if (productOrganisationSetting == null || !productOrganisationSetting.IsNewQuotesAllowed || !transactionTypeInputIsSupported)
                {
                    throw new ErrorException(
                        Errors.Quote.Creation.ProductQuotesDisabledForOrganisation(
                            organisation.Id.ToString(), organisation.Alias, organisation.Name, product.Id.ToString(), product.Details.Alias, product.Details.Name, this.errorData));
                }

                var formModelParam = (await this.formDataProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
                FormData? formData = null;
                if (formModelParam != null)
                {
                    var formDataObject = new
                    {
                        formModel = formModelParam,
                    };
                    formData = new FormData(JObject.FromObject(formDataObject));
                }

                ReadOnlyDictionary<string, object>? additionalProperties = null;
                if (this.additionalPropertiesProvider != null)
                {
                    additionalProperties = (await this.additionalPropertiesProvider.ResolveValueIfNotNull(providerContext))?.GetValueFromGeneric();
                }

                var resultingQuote = await this.CreateQuoteAsPerTransactionType(
                    tenantId, policy, transactionType, environment, isTestData, organisationId, customer?.Id, productId, formData, initialQuoteState, additionalProperties);

                if (resultingQuote != null)
                {
                    this.SetActionData(
                        actionData,
                        policy,
                        transactionType,
                        organisationId,
                        productId,
                        resultingQuote,
                        formModelParam);
                }

                return await Task.FromResult(Result.Success<Void, Domain.Error>(default));
            }
        }

        private async Task<NewQuoteReadModel?> CreateQuoteAsPerTransactionType(
            Guid tenantId,
            IPolicyReadModelDetails? policy,
            TransactionType transactionType,
            DeploymentEnvironment environment,
            bool isTestData,
            Guid organisationId,
            Guid? customerId,
            Guid productId,
            FormData? formData,
            string? initialQuoteState,
            ReadOnlyDictionary<string, object>? additionalProperties)
        {
            NewQuoteReadModel? resultingQuote = null;
            if (transactionType is TransactionType.NewBusiness)
            {
                resultingQuote = await this.CreateNewBusinessQuote(
                    tenantId, environment, formData, customerId, isTestData, organisationId, productId, initialQuoteState, additionalProperties);
            }
            else if (transactionType is TransactionType.Adjustment && policy != null)
            {
                resultingQuote = await this.CreateAdjustmentQuote(tenantId, policy, formData, initialQuoteState, additionalProperties);
            }
            else if (transactionType is TransactionType.Renewal && policy != null)
            {
                resultingQuote = await this.CreateRenewalQuote(tenantId, policy, formData, initialQuoteState, additionalProperties);
            }
            else if (transactionType is TransactionType.Cancellation && policy != null)
            {
                resultingQuote = await this.CreateCancellationQuote(tenantId, policy, formData, initialQuoteState, additionalProperties);
            }

            return resultingQuote;
        }

        private void ValidatePolicy(
            IPolicyReadModelDetails? policy,
            DeploymentEnvironment environment,
            OrganisationReadModel organisation,
            CustomerReadModelDetail? customer,
            Domain.Product.Product product)
        {
            if (policy == null)
            {
                return;
            }

            var policyStatus = policy.GetPolicyStatus(this.clock.Now());
            if (policyStatus == PolicyStatus.Expired || policyStatus == PolicyStatus.Cancelled)
            {
                throw new ErrorException(Errors.Quote.Creation.PolicyStatusInvalid(
                   policy.Id.ToString(),
                   policy.PolicyNumber,
                   policy.PolicyState,
                   policyStatus.ToString(),
                   this.policyTransactionType,
                   this.errorData));
            }

            if (policy.Transactions.Any(tq =>
                string.Equals(
                    tq.PolicyTransaction.GetTransactionStatus(policy.AreTimestampsAuthoritative, Timezones.AET, this.clock.Now()),
                    "pending",
                    StringComparison.OrdinalIgnoreCase)))
            {
                var pendingTransaction = policy.Transactions.First(tq =>
                    string.Equals(
                        tq.PolicyTransaction.GetTransactionStatus(policy.AreTimestampsAuthoritative, Timezones.AET, this.clock.Now()),
                        "pending",
                        StringComparison.OrdinalIgnoreCase));
                throw new ErrorException(Errors.Quote.Creation.PolicyHasPendingTransaction(
                    policy.Id.ToString(), policy.PolicyNumber, pendingTransaction.Quote.Type.ToString(), this.errorData));
            }

            if (policy.ProductId != product.Id)
            {
                throw new ErrorException(Errors.Quote.Creation.ProductMismatchWithPolicy(
                    policy.Id.ToString(),
                    policy.ProductId.ToString(),
                    policy.ProductName,
                    policy.PolicyNumber,
                    this.policyTransactionType,
                    product.Id.ToString(),
                    product.Details.Name,
                    this.errorData));
            }

            if (policy.OrganisationId != organisation.Id)
            {
                throw new ErrorException(Errors.Quote.Creation.OrganisationMismatchWithPolicy(
                    policy.Id.ToString(),
                    policy.OrganisationId.ToString(),
                    policy.OrganisationName,
                    policy.PolicyNumber,
                    this.policyTransactionType,
                    organisation.Id.ToString(),
                    organisation.Name,
                    this.errorData));
            }

            if (customer != null && policy.CustomerId != customer.Id)
            {
                throw new ErrorException(Errors.Quote.Creation.CustomerMismatchWithPolicy(
                    policy.Id.ToString(),
                    policy.CustomerId?.ToString(),
                    policy.CustomerPreferredName ?? policy.CustomerFullName,
                    policy.PolicyNumber,
                    this.policyTransactionType,
                    customer.Id.ToString(),
                    customer.DisplayName ?? customer.FullName,
                    this.errorData));
            }

            if (policy.Environment != environment)
            {
                throw new ErrorException(Errors.Quote.Creation.EnvironmentMismatchWithPolicy(
                    policy.Id.ToString(), policy.Environment.ToString(), policy.PolicyNumber, this.policyTransactionType, environment.ToString(), this.errorData));
            }
        }

        private void ValidateCustomer(Guid tenantId, DeploymentEnvironment environment, IPolicyReadModelDetails? policy, OrganisationReadModel organisation, CustomerReadModelDetail? customer)
        {
            var policyCustomer = policy != null && policy.CustomerId.HasValue ? this.customerReadModelRepository.GetCustomerById(tenantId, policy.CustomerId.Value) : null;
            if (customer != null)
            {
                if (customer.OrganisationId != organisation.Id)
                {
                    throw new ErrorException(
                        Errors.Quote.Creation.CustomerMismatchWithOrganisation(
                            organisation.Id.ToString(),
                            organisation.Name,
                            customer.Id.ToString(),
                            customer.DisplayName ?? customer.PreferredName ?? customer.FullName,
                            customer.OrganisationId.ToString(),
                            customer.OrganisationName,
                            this.errorData));
                }

                if (customer.Environment != environment)
                {
                    throw new ErrorException(
                        Errors.Quote.Creation.EnvironmentMismatchWithCustomer(
                            customer.Id.ToString(),
                            customer.DisplayName ?? customer.PreferredName ?? customer.FullName,
                            customer.Environment.ToString(),
                            environment.ToString(),
                            this.errorData));
                }
            }
        }

        private async Task<NewQuoteReadModel> CreateNewBusinessQuote(
            Guid tenantId,
            DeploymentEnvironment environment,
            FormData? formData,
            Guid? customerId,
            bool isTestData,
            Guid organisationId,
            Guid productId,
            string? initialQuoteState,
            ReadOnlyDictionary<string, object>? additionalProperties)
        {
            var createNewBusinessQuoteCommand = new CreateNewBusinessQuoteCommand(
                tenantId,
                organisationId,
                null,
                productId,
                environment,
                isTestData,
                customerId,
                null,
                formData?.JObject,
                initialQuoteState?.Humanize() ?? StandardQuoteStates.Nascent,
                additionalProperties);
            var newBusinessQuote = await this.mediator.Send(createNewBusinessQuoteCommand);
            return newBusinessQuote;
        }

        private async Task<NewQuoteReadModel> CreateAdjustmentQuote(
            Guid tenantId,
            IPolicyReadModelDetails policy,
            FormData? formData,
            string? initialQuoteState,
            ReadOnlyDictionary<string, object>? additionalProperties)
        {
            var createAdjustmentQuoteCommand = new CreateAdjustmentQuoteCommand(
                       tenantId, policy.Id, true, formData, initialQuoteState?.Humanize() ?? StandardQuoteStates.Nascent, additionalProperties);
            var adjustmentQuote = await this.mediator.Send(createAdjustmentQuoteCommand);
            return adjustmentQuote;
        }

        private async Task<NewQuoteReadModel> CreateRenewalQuote(
            Guid tenantId,
            IPolicyReadModelDetails policy,
            FormData? formData,
            string? initialQuoteState,
            ReadOnlyDictionary<string, object>? additionalProperties)
        {
            var createRenewalQuoteCommand = new CreateRenewalQuoteCommand(
                       tenantId, policy.Id, true, formData, initialQuoteState?.Humanize() ?? StandardQuoteStates.Nascent, additionalProperties);
            var renewalQuote = await this.mediator.Send(createRenewalQuoteCommand);
            return renewalQuote;
        }

        private async Task<NewQuoteReadModel> CreateCancellationQuote(
            Guid tenantId,
            IPolicyReadModelDetails policy,
            FormData? formData,
            string? initialQuoteState,
            ReadOnlyDictionary<string, object>? additionalProperties)
        {
            var createCancellationQuoteCommand = new CreateCancellationQuoteCommand(
                       tenantId, policy.Id, true, formData, initialQuoteState?.Humanize() ?? StandardQuoteStates.Nascent, additionalProperties);
            var canellationQuote = await this.mediator.Send(createCancellationQuoteCommand);
            return canellationQuote;
        }

        private ProductFeatureSettingItem TransactionTypeToProductFeature(TransactionType trasactionType)
        {
            switch (trasactionType)
            {
                case TransactionType.NewBusiness:
                    return ProductFeatureSettingItem.NewBusinessQuotes;
                case TransactionType.Renewal:
                    return ProductFeatureSettingItem.RenewalQuotes;
                case TransactionType.Adjustment:
                    return ProductFeatureSettingItem.AdjustmentQuotes;
                case TransactionType.Cancellation:
                    return ProductFeatureSettingItem.CancellationQuotes;
                default:
                    throw new ArgumentException($"Transaction type {trasactionType} is not supported");
            }
        }

        private void SetActionData(
            ActionData actionData,
            IPolicyReadModelDetails? policy,
            TransactionType transactionType,
            Guid organisationId,
            Guid productId,
            NewQuoteReadModel quote,
            object? formDataParam)
        {
            var createQuoteActionData = (CreateQuoteActionData)actionData;
            createQuoteActionData.QuoteId = quote.Id;
            createQuoteActionData.PolicyTransactionType = transactionType.ToString().Camelize();
            if (policy != null)
            {
                createQuoteActionData.PolicyId = policy.Id.ToString();
            }
            if (quote.CustomerId != null)
            {
                createQuoteActionData.CustomerId = quote.CustomerId.Value.ToString();
            }
            createQuoteActionData.OrganisationId = organisationId;
            createQuoteActionData.ProductId = productId;
            createQuoteActionData.InitialQuoteState = quote.QuoteState.Camelize();
            createQuoteActionData.Environment = quote.Environment.ToString().Camelize();
            if (formDataParam != null)
            {
                createQuoteActionData.FormData = formDataParam;
            }
        }
    }
}
