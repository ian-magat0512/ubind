// <copyright file="IssuePolicyAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
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
    using UBind.Application.Commands.QuoteCalculation;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.CustomPipelines.BindPolicy;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.ValueTypes;
    using ProviderEntity = UBind.Domain.SerialisedEntitySchemaObject;
    using Void = UBind.Domain.Helpers.Void;

    public class IssuePolicyAction : Action
    {
        private readonly string formModelPropertyValue = "formModel";
        private readonly IReleaseQueryService releaseQueryService;
        private readonly IProductService productService;
        private readonly IAdditionalPropertyTransformHelper additionalPropertyTransformHelper;
        private readonly IAdditionalPropertyValueService additionalPropertyValueService;
        private readonly IPersonAggregateRepository personAggregateRepository;
        private readonly ICachingResolver cachingResolver;
        private readonly QuoteEntityProvider? quoteProvider;
        private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
        private readonly IProductConfigurationProvider productConfigurationProvider;
        private readonly IQuoteWorkflowProvider quoteWorkflowProvider;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IPolicyService policyService;
        private readonly IQuoteAggregateRepository quoteAggregateRepository;
        private readonly CustomerEntityProvider? customerProvider;
        private readonly OrganisationEntityProvider? organisationProvider;
        private readonly ProductEntityProvider? productProvider;
        private readonly IProvider<Data<string>>? environmentProvider;
        private readonly IObjectProvider? inputDataProvider;
        private readonly IObjectProvider? additionalPropertiesProvider;
        private readonly IProvider<Data<bool>>? testDataProvider;
        private readonly IPolicyTransactionTimeOfDayScheme limitTimesPolicy;
        private readonly ICqrsMediator mediator;
        private readonly IClock clock;
        private readonly IProvider<Data<string>>? customPolicyNumberProvider;
        private readonly IPolicyNumberRepository policyNumberRepository;
        private readonly IUBindDbContext dbContext;
        private ReadOnlyDictionary<string, object>? additionalPropertiesAliasValueMap;
        private UBind.Domain.SerialisedEntitySchemaObject.Product? product;
        private Organisation? organisation;
        private Customer? customer;
        private bool? isTestData;

        /// <summary>
        /// This environment is used for actiondata, to show that there was an override
        /// on the environment.
        /// Null if there is no override.
        /// Note: this is important to be nullable for the actionData so it doesnt show anything
        /// if its null.
        /// </summary>
        private DeploymentEnvironment? deploymentEnvironment;

        public IssuePolicyAction(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>>? runCondition,
            IEnumerable<ErrorCondition>? beforeRunConditions,
            IEnumerable<ErrorCondition>? afterRunConditions,
            IEnumerable<IRunnableAction>? errorActions,
            QuoteEntityProvider? quoteProvider,
            CustomerEntityProvider? customerProvider,
            OrganisationEntityProvider? organisationProvider,
            ProductEntityProvider? productProvider,
            IProvider<Data<string>>? environmentProvider,
            IObjectProvider? inputDataProvider,
            IObjectProvider? additionalPropertiesProvider,
            IProvider<Data<bool>>? testDataProvider,
            IQuoteAggregateResolverService quoteAggregateResolverService,
            IProductConfigurationProvider productConfigurationProvider,
            IQuoteWorkflowProvider quoteWorkflowProvider,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IPolicyService policyService,
            IQuoteAggregateRepository quoteAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            IAdditionalPropertyValueService additionalPropertyValueService,
            IAdditionalPropertyTransformHelper additionalPropertyTransformHelper,
            ICachingResolver cachingResolver,
            IPolicyTransactionTimeOfDayScheme limitTimesPolicy,
            IProductService productService,
            IReleaseQueryService releaseQueryService,
            ICqrsMediator mediator,
            IClock clock,
            IProvider<Data<string>>? customPolicyNumberProvider,
            IPolicyNumberRepository policyNumberRepository,
            IUBindDbContext dbContext)
            : base(name, alias, description, asynchronous, runCondition, beforeRunConditions, afterRunConditions, errorActions)
        {
            this.releaseQueryService = releaseQueryService;
            this.productService = productService;
            this.additionalPropertyTransformHelper = additionalPropertyTransformHelper;
            this.personAggregateRepository = personAggregateRepository;
            this.additionalPropertyValueService = additionalPropertyValueService;
            this.cachingResolver = cachingResolver;
            this.quoteProvider = quoteProvider;
            this.quoteAggregateResolverService = quoteAggregateResolverService;
            this.productConfigurationProvider = productConfigurationProvider;
            this.quoteWorkflowProvider = quoteWorkflowProvider;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.policyService = policyService;
            this.quoteAggregateRepository = quoteAggregateRepository;
            this.customerProvider = customerProvider;
            this.organisationProvider = organisationProvider;
            this.productProvider = productProvider;
            this.environmentProvider = environmentProvider;
            this.inputDataProvider = inputDataProvider;
            this.additionalPropertiesProvider = additionalPropertiesProvider;
            this.testDataProvider = testDataProvider;
            this.limitTimesPolicy = limitTimesPolicy;
            this.mediator = mediator;
            this.clock = clock;
            this.customPolicyNumberProvider = customPolicyNumberProvider;
            this.policyNumberRepository = policyNumberRepository;
            this.dbContext = dbContext;
        }

        public override ActionData CreateActionData() => new IssuePolicyActionData(this.Name, this.Alias, this.clock);

        public override async Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal = false)
        {
            using (MiniProfiler.Current.Step(nameof(IssuePolicyAction) + "." + nameof(this.Execute)))
            {
                actionData.UpdateState(ActionState.Running);
                var additionalDetails = new List<string>
                {
                    "Action Type: " + actionData.Type.Humanize(),
                    "Action Alias: " + actionData.Alias,
                };
                try
                {
                    if (this.quoteProvider == null && this.inputDataProvider == null)
                    {
                        var error = Domain.Errors.Automation.IssuePolicyAction.QuoteAndInputDataNotProvided();
                        throw new ErrorException(error);
                    }
                    QuoteAggregate quoteAggregate = this.quoteProvider != null
                      ? await this.IssuePolicyFromQuote(providerContext)
                      : await this.IssuePolicyFromInputData(providerContext);
                    if (!(actionData is IssuePolicyActionData issuePolicyActionData))
                    {
                        throw new InvalidCastException("The provided actionData is null or cannot be cast to IssuePolicyActionData.");
                    }
                    issuePolicyActionData.PolicyId = quoteAggregate.Policy!.PolicyId;
                    issuePolicyActionData.QuoteId = quoteAggregate.GetLatestQuote()?.Id;
                    issuePolicyActionData.AdditionalProperties = this.additionalPropertiesAliasValueMap;
                    issuePolicyActionData.OrganisationId = this.organisation?.Id;
                    issuePolicyActionData.CustomerId = this.customer?.Id;
                    issuePolicyActionData.ProductId = this.product?.Id;
                    issuePolicyActionData.IsTestData = this.isTestData;
                    issuePolicyActionData.Environment = this.deploymentEnvironment;
                    this.SetContext(quoteAggregate, providerContext);
                }
                catch (ErrorException ex)
                {
                    JObject errorData = await providerContext.GetDebugContext();
                    ex.EnrichAndRethrow(errorData, additionalDetails);
                    throw;
                }
                return Result.Success<Void, Domain.Error>(default);
            }
        }

        public override bool IsReadOnly() => false;

        private void SetContext(QuoteAggregate quoteAggregate, IProviderContext providerContext)
        {
            // Add policy and policyTransaction entities to the context of this automation
            var policy = new ProviderEntity.Policy(quoteAggregate.Policy!.PolicyId);
            providerContext.AutomationData.ContextManager.SetContextEntity(EntityType.Policy, policy);

            foreach (var transaction in quoteAggregate.Policy.Transactions)
            {
                var policyTransaction = new PolicyTransaction(transaction.Id);
                providerContext.AutomationData.ContextManager.SetContextEntity(EntityType.PolicyTransaction, policyTransaction);
            }
        }

        private async Task<JObject> GetFormData(IProviderContext providerContext)
        {
            try
            {
                if (this.inputDataProvider == null)
                {
                    var error = Domain.Errors.Automation.IssuePolicyAction.MissingInputData();
                    throw new ErrorException(error);
                }

                object? inputData = (await this.inputDataProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                var inputDataJObject = inputData == null
                    ? new JObject()
                    : JObject.FromObject(inputData);
                var tmpFormModel = new JObject();
                tmpFormModel[this.formModelPropertyValue] = inputDataJObject;
                return inputDataJObject.ContainsKey(this.formModelPropertyValue) ? inputDataJObject : tmpFormModel;
            }
            catch (ErrorException e) when (e.Error.Code == "automation.providers.property.value.not.found")
            {
                var error = Domain.Errors.Automation.IssuePolicyAction.MissingInputData();
                throw new ErrorException(error);
            }
        }

        private async Task<QuoteAggregate> IssuePolicyFromInputData(IProviderContext providerContext)
        {
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
            var productId = providerContext.AutomationData.ContextManager.Product.Id;
            var organisationId = providerContext.AutomationData.ContextManager.Organisation.Id;
            var environment = providerContext.AutomationData.System.Environment;

            // use this customer if the quotes customer is not specified.
            this.organisation = (Organisation?)(await this.organisationProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            this.customer = (Customer?)(await this.customerProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            this.product = (UBind.Domain.SerialisedEntitySchemaObject.Product?)
                (await this.productProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            string? environmentStr = (await this.environmentProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            this.isTestData = (await this.testDataProvider.ResolveValueIfNotNull(providerContext))?.DataValue ?? false;
            organisationId = this.customer != null
                ? Guid.Parse(this.customer.OrganisationId)
                : this.organisation != null
                    ? this.organisation.Id
                    : organisationId;
            productId = this.product != null ? this.product.Id : productId;
            Enum.TryParse(environmentStr, true, out DeploymentEnvironment deploymentEnvironment);
            if (!string.IsNullOrEmpty(environmentStr))
            {
                this.deploymentEnvironment = deploymentEnvironment;
            }

            environment = deploymentEnvironment != DeploymentEnvironment.None ? deploymentEnvironment : environment;
            var isTestData = this.isTestData != null ? this.isTestData.Value : false;
            await this.ThrowIfProductOrNewBusinessTransactionDisabled(tenantId, productId, organisationId);
            var releaseContext = this.releaseQueryService.GetDefaultReleaseContextOrThrow(tenantId, productId, environment);
            var calculationResult = await this.CreateCalculationFromInputData(providerContext, releaseContext, null, false, false, organisationId);
            var productConfiguration = await this.productConfigurationProvider.GetProductConfiguration(releaseContext, WebFormAppType.Quote);
            if (calculationResult.FinalFormData == null)
            {
                JObject errorData = await providerContext.GetDebugContext();
                throw new ErrorException(Errors.Automation.IssuePolicyAction.MissingFormDataFromCalculation(errorData));
            }

            if (calculationResult.CalculationResultData == null)
            {
                JObject errorData = await providerContext.GetDebugContext();
                throw new ErrorException(Errors.Automation.IssuePolicyAction.MissingResultFromCalculation(errorData));
            }
            var dataRetriever = new StandardQuoteDataRetriever(
                productConfiguration,
                calculationResult.FinalFormData,
                calculationResult.CalculationResultData);

            this.ThrowIfInvalidCalculationResultForInputData(CalculationResult.CreateForNewPolicy(calculationResult.CalculationResultData, dataRetriever));
            var additionalProperties = await this.GenerateAdditionalProperties(providerContext, releaseContext.TenantId, organisationId);
            var externalPolicyNumber = await this.GetExternalPolicyNumber(providerContext, releaseContext.TenantId, releaseContext.ProductId, releaseContext.Environment);

            // Bind the quote.
            var formModelPolicyCreationRequirement = new FormModelPolicyCreationRequirement(
                organisationId,
                this.customer,
                externalPolicyNumber,
                calculationResult.FinalFormData,
                calculationResult.CalculationResultData,
                providerContext.AutomationData.System.TimeZone,
                isTestData);
            var bindCommand = BindPolicyCommand.CreateForBindingWithoutQuote(
                releaseContext, formModelPolicyCreationRequirement, externalPolicyNumber);
            await this.mediator.Send(bindCommand);

            var quoteAggregate = bindCommand.QuoteAggregate!;
            if (additionalProperties != null)
            {
                await this.UpdateAdditionalPropertiesOfPolicy(quoteAggregate, additionalProperties);
                await this.quoteAggregateRepository.Save(quoteAggregate);
            }

            return quoteAggregate;
        }

        private async Task UpdateAdditionalPropertiesOfPolicy(QuoteAggregate quoteAggregate, List<AdditionalPropertyValueUpsertModel> additionalProperties)
        {
            if (additionalProperties != null)
            {
                await this.additionalPropertyValueService.UpsertPropertiesForPolicyThenReturnPolicyNumber(
                    quoteAggregate,
                    additionalProperties);
            }
        }

        private async Task<string?> GetExternalPolicyNumber(
            IProviderContext providerContext,
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment)
        {
            if (this.customPolicyNumberProvider == null)
            {
                return null;
            }

            var externalPolicyNumber = (await this.customPolicyNumberProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            await this.policyService.ThrowIfPolicyNumberInUse(tenantId, productId, environment, externalPolicyNumber);
            return externalPolicyNumber;
        }

        private async Task<DeploymentEnvironment> GetEffectiveEnvironment(IProviderContext providerContext)
        {
            var systemEnvironment = providerContext.AutomationData.System.Environment;
            string? environment = (await this.environmentProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            var deploymentEnvironment = environment.ToEnumOrNull<DeploymentEnvironment>();
            if (deploymentEnvironment != null)
            {
                systemEnvironment = (DeploymentEnvironment)(deploymentEnvironment != DeploymentEnvironment.None
                    ? deploymentEnvironment
                    : systemEnvironment);
            }

            return systemEnvironment;
        }

        private async Task ThrowIfProductOrNewBusinessTransactionDisabled(Guid tenantId, Guid productId, Guid organisationId)
        {
            var product = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenantId), new GuidOrAlias(productId));
            await this.productService.ThrowIfProductNotEnabledForOrganisation(product, organisationId);
            this.productService.ThrowIfNewBusinessPolicyTransactionsDisabled(product);
        }

        private void ThrowIfInvalidCalculationResultForInputData(CalculationResult calculationResult)
        {
            if (calculationResult.Triggers.Any())
            {
                throw new ErrorException(Errors.Policy.Issuance.InputDataHasActiveTriggers());
            }

            if (!calculationResult.IsBindable)
            {
                throw new ErrorException(Errors.Policy.Issuance.InputDataRequiresBindingCalculation(
                    calculationResult.CalculationResultState));
            }
        }

        private void ThrowIfInvalidCalculationResult(CalculationResult calculationResult, bool hasNewFormData = false)
        {
            if (calculationResult.Triggers.Any())
            {
                throw new ErrorException(Errors.Policy.Issuance.IncompleteQuoteHasActiveTriggers());
            }

            if (!calculationResult.IsBindable)
            {
                throw new ErrorException(Errors.Policy.Issuance.IncompleteQuoteRequiresBindableCalculationResult(
                    calculationResult.CalculationResultState, hasNewFormData));
            }
        }

        private async Task ThrowIfNoReferenceNumbersAvailable(Domain.Aggregates.Quote.Quote quote, ReleaseContext releaseContext)
        {
            IQuoteWorkflow quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
            var options = quote.GetBindingOptions(quoteWorkflow);
            if (options.HasFlag(BindOptions.TransactionRecord))
            {
                var calculationResult = quote.LatestCalculationResult.Data;
                if (calculationResult.PayablePrice.TotalPayable > 0)
                {
                    if (!this.policyNumberRepository.GetAvailableForProduct(
                        quote.Aggregate.TenantId,
                        quote.Aggregate.ProductId,
                        quote.Aggregate.Environment).Any())
                    {
                        throw new ReferenceNumberUnavailableException(
                            Errors.NumberPool.NoneAvailable(
                                quote.Aggregate.TenantId.ToString(),
                                quote.Aggregate.ProductId.ToString(),
                                "policy"));
                    }
                }
            }
        }

        private async Task<QuoteAggregate> IssuePolicyFromQuote(IProviderContext providerContext)
        {
            var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
            var productId = providerContext.AutomationData.ContextManager.Product.Id;

            var quoteTmp = (await this.quoteProvider!.Resolve(providerContext)).GetValueOrThrowIfFailed()?.DataValue;

            if (quoteTmp == null)
            {
                var error = Domain.Errors.Automation.IssuePolicyAction.MissingQuote();
                throw new ErrorException(error);
            }

            var quoteId = quoteTmp.Id;
            var quoteAggregate = await this.GetQuoteAggregate(
                tenantId,
                productId,
                quoteId);
            var quote = quoteAggregate.GetQuoteOrThrow(quoteId);

            var newBusinessQuote = quote as NewBusinessQuote;
            bool hasNewFormData = false;

            if (newBusinessQuote != null && newBusinessQuote.PolicyIssued)
            {
                throw new ErrorException(Errors.Policy.Issuance.PolicyIssuanceQuoteAlreadyHasAPolicy(quote.QuoteNumber));
            }

            var releaseContext = this.releaseQueryService.GetReleaseContextForReleaseOrDefaultRelease(
                tenantId,
                productId,
                quote.Aggregate.Environment,
                quote.ProductReleaseId);

            await this.ThrowIfProductOrNewBusinessTransactionDisabled(tenantId, productId, quoteAggregate.OrganisationId);
            await this.ThrowIfInputFieldMismatch(quoteAggregate, quoteId, providerContext);

            // Add new form data to the quote.
            // If this is present, updates the existing quote with this form data.
            if (this.inputDataProvider != null)
            {
                var calculationResult = await this.CreateCalculationFromInputData(providerContext, releaseContext, quote, true, true, quoteAggregate.OrganisationId);
                quote = calculationResult.Quote!;
                newBusinessQuote = (NewBusinessQuote)quote;
                quoteAggregate = newBusinessQuote.Aggregate;
                hasNewFormData = true;
            }

            await this.VerifyQuote(quote, hasNewFormData);

            var additionalProperties = await this.GenerateAdditionalProperties(providerContext, tenantId, quoteAggregate.OrganisationId);
            var externalPolicyNumber = await this.GetExternalPolicyNumber(providerContext, tenantId, productId, quoteAggregate.Environment);

            try
            {
                // Bind the quote.
                var bindCommand = BindPolicyCommand.CreateForBindingWithQuote(
                    releaseContext,
                    quoteId,
                    new Domain.Dto.BindRequirementDto(newBusinessQuote!.LatestCalculationResult.Id),
                    null,
                    null, // null is invoice payment.
                    Guid.Empty,
                    externalPolicyNumber,
                    null,
                    allowBindingForIncompleteQuotes: true);
                await this.mediator.Send(bindCommand);

                quoteAggregate = bindCommand.QuoteAggregate!;
                if (additionalProperties != null && additionalProperties.Any())
                {
                    await this.UpdateAdditionalPropertiesOfPolicy(quoteAggregate, additionalProperties);
                    await this.quoteAggregateRepository.Save(quoteAggregate);
                }
            }
            catch (Exception e)
            {
                if (e.Message.ToLower() == "Cannot issue multiple policies for single application.".ToLower()
                    || e.Message.ToLower().Contains("Policy operation is not applicable in current quote state".ToLower()))
                {
                    throw new ErrorException(Errors.Policy.Issuance.RequiresApprovedOrIncompleteQuoteState(quote.Id));
                }

                throw;
            }

            return quoteAggregate;
        }

        private async Task ThrowIfInputFieldMismatch(QuoteAggregate quoteAggregate, Guid quoteId, IProviderContext providerContext)
        {
            this.product = (UBind.Domain.SerialisedEntitySchemaObject.Product?)
                (await this.productProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (this.product != null && this.product.Id != quoteAggregate.ProductId)
            {
                throw new ErrorException(Errors.Policy.Issuance.ProductMismatch(quoteId, quoteAggregate.ProductId, this.product.Id));
            }

            this.organisation = (Organisation?)(await this.organisationProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (this.organisation != null && this.organisation.Id != quoteAggregate.OrganisationId)
            {
                throw new ErrorException(Errors.Policy.Issuance.OrganisationMismatch(quoteId, quoteAggregate.OrganisationId, this.organisation.Id));
            }

            this.customer = (Customer?)(await this.customerProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (this.customer != null && this.customer.Id != quoteAggregate.CustomerId)
            {
                throw new ErrorException(Errors.Policy.Issuance.CustomerMismatch(quoteId, quoteAggregate.CustomerId, this.customer.Id));
            }

            string? environmentStr = (await this.environmentProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (!string.IsNullOrEmpty(environmentStr))
            {
                Enum.TryParse(environmentStr, true, out DeploymentEnvironment deploymentEnvironment);
                this.deploymentEnvironment = deploymentEnvironment;
                if (quoteAggregate.Environment != deploymentEnvironment)
                {
                    throw new ErrorException(Errors.Policy.Issuance.EnvironmentMismatch(quoteId, quoteAggregate.Environment, deploymentEnvironment));
                }
            }

            this.isTestData = (await this.testDataProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (this.isTestData != null
                && quoteAggregate.IsTestData != this.isTestData.Value)
            {
                throw new ErrorException(Errors.Policy.Issuance.TestDataMismatch(quoteId, quoteAggregate.IsTestData, this.isTestData.Value));
            }
        }

        private async Task<QuoteCalculationCommand> CreateCalculationFromInputData(
            IProviderContext providerContext,
            ReleaseContext releaseContext,
            Domain.Aggregates.Quote.Quote? quote,
            bool hasPremiumFunding,
            bool persistResult,
            Guid organisationId)
        {
            var formModelJObject = await this.GetFormData(providerContext);
            var formModelPropertyValue = formModelJObject[this.formModelPropertyValue];
            if (!(formModelPropertyValue is JObject))
            {
                throw new InvalidOperationException("The form model property value is not a JObject.");
            }

            var formModelPropertyObject = (JObject)formModelPropertyValue;
            var commandContainingResults = new QuoteCalculationCommand(
                releaseContext,
                quote?.Id,
                null,
                quote?.Type,
                releaseContext.ProductReleaseId,
                new CalculationDataModel { FormModel = formModelPropertyObject },
                null,
                hasPremiumFunding,
                persistResult,
                organisationId);
            await this.mediator.Send(commandContainingResults, providerContext.CancellationToken);
            return commandContainingResults;
        }

        private async Task VerifyQuote(Domain.Aggregates.Quote.Quote quote, bool hasNewFormData)
        {
            var quoteState = quote.QuoteStatus;
            if (quoteState.Equals(StandardQuoteStates.Nascent, StringComparison.OrdinalIgnoreCase))
            {
                throw new ErrorException(Errors.Policy.Issuance.RequiresApprovedOrIncompleteQuoteState(quote.Id));
            }

            CalculationResult? latestCalculationResult = quote.LatestCalculationResult?.Data;
            if (latestCalculationResult == null)
            {
                throw new ErrorException(Errors.Policy.Issuance.RequiresCalculation(quote.Id));
            }

            if (quote.TransactionCompleted)
            {
                throw new ErrorException(Errors.Policy.Issuance.PolicyAlreadyIssued());
            }

            if (!(quote is NewBusinessQuote))
            {
                throw new ErrorException(
                    Errors.Policy.Issuance.RequiresNewBusinessQuote(quote.Id, quote.GetType().Name));
            }

            if (!(quoteState.Equals(StandardQuoteStates.Approved, StringComparison.OrdinalIgnoreCase)
                || quoteState.Equals(StandardQuoteStates.Incomplete, StringComparison.OrdinalIgnoreCase)
                || quoteState.Equals(StandardQuoteStates.Complete, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ErrorException(Errors.Policy.Issuance.RequiresApprovedOrIncompleteQuoteState(quote.Id));
            }

            if (quoteState.Equals(StandardQuoteStates.Incomplete, StringComparison.OrdinalIgnoreCase))
            {
                this.ThrowIfInvalidCalculationResult(latestCalculationResult, hasNewFormData);
            }

            var product = await this.cachingResolver.GetProductOrThrow(quote.Aggregate.TenantId, quote.Aggregate.ProductId);
            if (quote.IsExpired(this.clock.Now()) && product.Details.QuoteExpirySetting.Enabled)
            {
                throw new ErrorException(Errors.Policy.Issuance.RequiresApprovedOrIncompleteQuoteState(quote.Id));
            }

            if (!latestCalculationResult.IsBindable)
            {
                throw new ErrorException(Errors.Policy.Issuance.RequiresBindableCalculationResult(
                    latestCalculationResult.CalculationResultState));
            }
        }

        private async Task<List<AdditionalPropertyValueUpsertModel>?> GenerateAdditionalProperties(IProviderContext providerContext, Guid tenantId, Guid organisationId)
        {
            this.additionalPropertiesAliasValueMap
                = (ReadOnlyDictionary<string, object>?)(await this.additionalPropertiesProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (this.additionalPropertiesAliasValueMap == null)
            {
                return null;
            }

            var additionalProperties = await this.additionalPropertyTransformHelper.TransformObjectDictionaryToValueUpsertModels(
                tenantId,
                organisationId,
                Domain.Enums.AdditionalPropertyEntityType.Policy,
                this.additionalPropertiesAliasValueMap);

            return additionalProperties;
        }

        private async Task<QuoteAggregate> GetQuoteAggregate(Guid tenantId, Guid productId, Guid quoteId)
        {
            try
            {
                var quoteAggregate = this.quoteAggregateResolverService.GetQuoteAggregateForQuote(tenantId, quoteId);
                if (quoteAggregate.ProductId != productId)
                {
                    var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(quoteAggregate.TenantId, productId);
                    throw new ErrorException(Errors.Quote.ProductMismatch(productAlias, quoteId));
                }

                return quoteAggregate;
            }
            catch (ErrorException ex)
            when (ex.Error.Code.Equals(Errors.General.NotFound("quote", quoteId, "quoteId").Code))
            {
                throw new ErrorException(Errors.Quote.NotFound(quoteId.ToString()));
            }
        }
    }
}
