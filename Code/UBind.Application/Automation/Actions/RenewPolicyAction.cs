// <copyright file="RenewPolicyAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions;

using CSharpFunctionalExtensions;
using System.Threading.Tasks;
using UBind.Application.Automation.Providers;
using UBind.Application.Automation.Providers.Entity;
using UBind.Application.Automation.Providers.Object;
using UBind.Application.Releases;
using UBind.Domain;
using UBind.Domain.Aggregates.Quote.Workflow;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;
using UBind.Domain.ReferenceNumbers;
using UBind.Domain.Services;
using NodaTime;
using UBind.Application.Automation.Error;
using UBind.Domain.ReadModel;
using Humanizer;
using Newtonsoft.Json.Linq;
using StackExchange.Profiling;
using UBind.Application.Automation.Enums;
using UBind.Domain.Exceptions;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.SerialisedEntitySchemaObject;
using UBind.Application.Automation.Extensions;
using ProviderEntity = UBind.Domain.SerialisedEntitySchemaObject;
using UBind.Domain.ReadWriteModel;
using UBind.Application.Commands.QuoteCalculation;
using System;
using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
using UBind.Domain.Extensions;
using UBind.Domain.ValueTypes;
using UBind.Application.Queries.Quote;
using UBind.Application.Commands.Policy;

/// <summary>
/// Represents an action for renewing a policy within the automation workflow.
/// This action facilitates the renewal process by interacting with quote and policy providers,
/// validating input data, calculating renewal quotes, and persisting changes to the system.
/// </summary>
/// <remarks>
/// The <see cref="RenewPolicyAction"/> class inherits from the base <see cref="Action"/> class
/// and implements the necessary logic to execute the renewal process. It handles scenarios
/// where a policy is renewed with or without an associated quote, ensures data consistency,
/// performs validation, and interacts with various services and repositories to accomplish the renewal.
/// </remarks>
public class RenewPolicyAction : Action
{
    private readonly QuoteEntityProvider? quoteProvider;
    private readonly PolicyEntityProvider? policyProvider;
    private readonly IObjectProvider? inputDataProvider;
    private readonly ICqrsMediator mediator;
    private readonly IClock clock;
    private readonly IProductConfigurationProvider productConfigurationProvider;
    private readonly IQuoteWorkflowProvider quoteWorkflowProvider;
    private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
    private readonly ICachingResolver cachingResolver;
    private readonly IQuoteAggregateResolverService quoteAggregateResolverService;
    private readonly IPolicyService policyService;
    private readonly IReleaseQueryService releaseQueryService;
    private readonly IProductFeatureSettingService productFeatureSettingService;
    private readonly IQuoteAggregateRepository quoteAggregateRepository;
    private readonly IInvoiceNumberRepository invoiceNumberRepository;
    private readonly IPolicyTransactionTimeOfDayScheme timeOfDayScheme;
    private Lazy<Task<JObject>>? debugContextLazy;

    public RenewPolicyAction(
        string name,
        string alias,
        string description,
        bool asynchronous,
        IProvider<Data<bool>>? runCondition,
        IEnumerable<ErrorCondition>? beforeRunConditions,
        IEnumerable<ErrorCondition>? afterRunConditions,
        IEnumerable<IRunnableAction>? errorActions,
        QuoteEntityProvider? quoteProvider,
        PolicyEntityProvider? policyProvider,
        IObjectProvider? inputDataProvider,
        ICqrsMediator mediator,
        IClock clock,
        IProductConfigurationProvider productConfigurationProvider,
        IQuoteWorkflowProvider quoteWorkflowProvider,
        IHttpContextPropertiesResolver httpContextPropertiesResolver,
        ICachingResolver cachingResolver,
        IQuoteAggregateResolverService quoteAggregateResolverService,
        IPolicyService policyService,
        IReleaseQueryService releaseQueryService,
        IProductFeatureSettingService productFeatureSettingService,
        IQuoteAggregateRepository quoteAggregateRepository,
        IInvoiceNumberRepository policyNumberRepository,
        IPolicyTransactionTimeOfDayScheme timeOfDayScheme)
        : base(name, alias, description, asynchronous, runCondition, beforeRunConditions, afterRunConditions, errorActions)
    {
        this.quoteProvider = quoteProvider;
        this.policyProvider = policyProvider;
        this.inputDataProvider = inputDataProvider;
        this.mediator = mediator;
        this.clock = clock;
        this.productConfigurationProvider = productConfigurationProvider;
        this.quoteWorkflowProvider = quoteWorkflowProvider;
        this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        this.cachingResolver = cachingResolver;
        this.quoteAggregateResolverService = quoteAggregateResolverService;
        this.policyService = policyService;
        this.releaseQueryService = releaseQueryService;
        this.productFeatureSettingService = productFeatureSettingService;
        this.quoteAggregateRepository = quoteAggregateRepository;
        this.invoiceNumberRepository = policyNumberRepository;
        this.timeOfDayScheme = timeOfDayScheme;
    }

    private JObject DebugContext
    {
        get
        {
            if (this.debugContextLazy == null)
            {
                return new JObject();
            }

            return this.debugContextLazy.Value.Result;
        }
    }

    public override ActionData CreateActionData() => new RenewPolicyActionData(this.Name, this.Alias, this.clock);

    public override async Task<Result<Domain.Helpers.Void, Error>> Execute(
        IProviderContext providerContext,
        ActionData actionData,
        bool isInternal = false)
    {
        using (MiniProfiler.Current.Step($"{nameof(RenewPolicyAction)}.{nameof(this.Execute)}"))
        {
            this.debugContextLazy = new Lazy<Task<JObject>>(() => providerContext.GetDebugContext());
            if (this.quoteProvider == null && this.policyProvider == null)
            {
                throw new ErrorException(
                    Errors.Automation.RenewPolicyAction.QuoteAndPolicyNotProvided(this.DebugContext));
            }

            actionData.UpdateState(ActionState.Running);

            var (quote, policy, inputData) = await this.ResolveProviderData(providerContext);
            try
            {
                var tenantId = providerContext.AutomationData.ContextManager.Tenant.Id;
                var productId = providerContext.AutomationData.ContextManager.Product.Id;
                var isProductFeatureSettingEnabled = this.productFeatureSettingService.IsProductFeatureSettingEnabled(
                    tenantId,
                    productId,
                    ProductFeatureSettingItem.RenewalPolicyTransactions);
                if (!isProductFeatureSettingEnabled)
                {
                    var product = await this.cachingResolver.GetProductOrNull(tenantId, productId);
                    throw new ErrorException(
                        Errors.Automation.RenewPolicyAction.TransactionTypeDisabled(
                            productId,
                            product?.Details?.Alias,
                            product?.Details?.Name));
                }

                var environment = providerContext.AutomationData.System.Environment;
                QuoteAggregate? tmpQuoteAggregate = null;
                RenewalQuote? renewalQuote = null;
                Guid? productReleaseId = null;

                if (quote != null)
                {
                    this.ValidateEnvironment(environment, quote.EntityEnvironment, "quote");
                    tmpQuoteAggregate = await this.GetQuoteAggregate(
                        tenantId,
                        productId,
                        quote.Id);
                    var retrievedQuote = tmpQuoteAggregate.GetQuoteOrThrow(quote.Id);
                    var quotes = tmpQuoteAggregate.GetQuotes();
                    environment = quote.EntityEnvironment.ToEnumOrNull<DeploymentEnvironment>() ?? environment;
                    productReleaseId = retrievedQuote.ProductReleaseId;
                    renewalQuote = await this.GetRenewalQuoteOrThrow(tenantId, retrievedQuote);
                }

                var releaseContext = await this.CreateReleaseContextOrThrow(tenantId, productId, environment, productReleaseId, policy);

                var renewalActionData =
                    await this.GetCalculationResultAndFormData(providerContext, releaseContext, inputData, renewalQuote);

                if (policy != null)
                {
                    this.ValidateEnvironment(environment, policy.EntityEnvironment, "policy");
                    var retrievedPolicy = await this.policyService.GetPolicy(tenantId, policy.Id);
                    retrievedPolicy = EntityHelper.ThrowIfNotFound(retrievedPolicy, "Policy");

                    this.ValidatePolicy(retrievedPolicy, renewalQuote);

                    if (quote == null)
                    {
                        var quoteAggregateId = this.quoteAggregateResolverService.GetQuoteAggregateIdForPolicyId(retrievedPolicy.Id);
                        tmpQuoteAggregate = await this.mediator.Send(
                            new RenewPolicyWithoutQuoteCommand(tenantId, quoteAggregateId, retrievedPolicy, renewalActionData.calculationResult, renewalActionData.formData, providerContext.AutomationData.System.TimeZone, releaseContext));
                    }
                }

                QuoteAggregate quoteAggregate = EntityHelper.ThrowIfNotFound(tmpQuoteAggregate, "QuoteAggregate");
                if (quote != null)
                {
                    quoteAggregate = await this.mediator.Send(
                        new RenewPolicyWithQuoteCommand(tenantId, quoteAggregate.Id, renewalActionData.calculationResult, renewalActionData.formData, providerContext.AutomationData.System.TimeZone, releaseContext));
                }

                this.SetActionData(actionData, quoteAggregate, inputData);
                this.SetContext(providerContext, quoteAggregate);
            }
            catch (ErrorException ex)
            {
                if (ex.Error.Code == "calculation.spreadsheet.formula.error")
                {
                    var data = new JObject
                    {
                        { "formData", JToken.FromObject(inputData != null ? JToken.FromObject(inputData) : JValue.CreateNull()) },
                        { "errorDetails", JToken.FromObject(ex.Error.Message) },
                    };
                    throw new ErrorException(
                        Errors.Automation.RenewPolicyAction.CalculationError(data));
                }
                throw;
            }
            return Result.Success<Domain.Helpers.Void, Domain.Error>(default);
        }
    }

    public override bool IsReadOnly() => false;

    private CalculationResult GetQuoteCalculationResultOrThrow(RenewalQuote renewalQuote)
    {
        var calculationResult = renewalQuote.LatestCalculationResult.Data;
        this.ValidateQuoteCalculationResult(calculationResult, renewalQuote.Id, renewalQuote.QuoteNumber);
        return calculationResult;
    }

    private void ValidateQuoteCalculationResult(CalculationResult calculationResult, Guid quoteId, string quoteNumber)
    {
        if (!calculationResult.IsBindable)
        {
            throw new ErrorException(
                Errors.Automation.RenewPolicyAction.QuoteCalculationStateInvalid(
                quoteId, quoteNumber, calculationResult.CalculationResultState, this.DebugContext));
        }

        if (calculationResult.Triggers.Any())
        {
            throw new ErrorException(
                Errors.Automation.RenewPolicyAction.QuoteCalculationTriggersActive(
                quoteId, quoteNumber, this.DebugContext));
        }
    }

    private record ProviderData(IEntity? quote, IEntity? policy, object? inputData);

    private async Task<ProviderData> ResolveProviderData(IProviderContext providerContext)
    {
        var quote = this.quoteProvider != null
            ? (await this.quoteProvider!.Resolve(providerContext)).GetValueOrThrowIfFailed()?.DataValue
            : null;

        var policy = this.policyProvider != null
            ? (await this.policyProvider!.Resolve(providerContext)).GetValueOrThrowIfFailed()?.DataValue
            : null;

        var inputData = this.inputDataProvider != null
            ? (await this.inputDataProvider!.Resolve(providerContext)).GetValueOrThrowIfFailed()?.DataValue
            : null;

        return new ProviderData(quote, policy, inputData);
    }

    private record RenewalActionData(CalculationResult calculationResult, FormData formData);

    private async Task<RenewalActionData> GetCalculationResultAndFormData(
        IProviderContext providerContext, ReleaseContext releaseContext, object? inputData, RenewalQuote? renewalQuote)
    {
        CalculationResult? calculationResult = null;
        FormData? formData = null;

        if (inputData != null)
        {
            var organisationId = providerContext.AutomationData.ContextManager.Organisation.Id;
            var calculation = await this.CreateCalculationFromInputData(
                providerContext, releaseContext, renewalQuote, false, false, organisationId);
            var productConfiguration =
                await this.productConfigurationProvider.GetProductConfiguration(releaseContext, WebFormAppType.Quote);
            if (calculation.FinalFormData == null)
            {
                throw new ErrorException(Errors.Automation.RenewPolicyAction.MissingFormDataFromCalculation(this.DebugContext));
            }

            if (calculation.CalculationResultData == null)
            {
                throw new ErrorException(Errors.Automation.RenewPolicyAction.MissingResultFromCalculation(this.DebugContext));
            }
            var dataRetriever = new StandardQuoteDataRetriever(
                productConfiguration,
                calculation.FinalFormData,
                calculation.CalculationResultData);

            calculationResult = CalculationResult.CreateForNewPolicy(calculation.CalculationResultData, dataRetriever);
            formData = calculation.FinalFormData;
            if (calculationResult.Triggers.Any())
            {
                throw new ErrorException(
                    Errors.Automation.RenewPolicyAction.CalculationTriggersActive(formData, calculationResult));
            }
            if (!calculationResult.IsBindable)
            {
                var data = new JObject
                {
                    { "formData", JToken.FromObject(formData) },
                    { "calculationResult", JToken.FromObject(calculationResult) },
                };
                throw new ErrorException(
                    Errors.Automation.RenewPolicyAction.CalculationStateInvalid(calculationResult, data));
            }

            calculationResult.FormDataId = Guid.NewGuid();
        }
        else if (renewalQuote != null)
        {
            calculationResult = this.GetQuoteCalculationResultOrThrow(renewalQuote!);
            formData = renewalQuote.LatestFormData.Data;
        }

        calculationResult = EntityHelper.ThrowIfNotFound(calculationResult, "calculation result");
        formData = EntityHelper.ThrowIfNotFound(formData, "formData");
        return new RenewalActionData(calculationResult, formData);
    }

    private async Task<bool> IsMutualTenant(Guid tenantId)
    {
        var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
        return TenantHelper.IsMutual(tenantAlias);
    }

    private async Task ProgressQuoteState(
        QuoteAggregate quoteAggregate,
        QuoteAction quoteActions,
        ReleaseContext releaseContext)
    {
        var quoteWorkflow = await this.quoteWorkflowProvider.GetConfigurableQuoteWorkflow(releaseContext);
        var quoteStatus = QuoteStatus.Complete.Humanize();
        var resultingState = quoteWorkflow.GetResultingState(quoteActions, quoteStatus);
        var stateChangeEvent = new QuoteAggregate.QuoteStateChangedEvent(
            quoteAggregate.TenantId,
            quoteAggregate.Id,
            quoteAggregate.GetLatestQuote().Id,
            quoteActions,
            this.httpContextPropertiesResolver.PerformingUserId,
            quoteStatus,
            resultingState,
            this.clock.Now());
        quoteAggregate.ApplyNewEvent(stateChangeEvent);
    }

    private void SetActionData(ActionData actionData, QuoteAggregate quoteAggregate, object? inputData)
    {
        if (actionData is not RenewPolicyActionData renewPolicyActionData)
        {
            throw new InvalidCastException("The provided actionData is null or cannot be cast to RenewPolicyActionData.");
        }

        renewPolicyActionData.QuoteId = quoteAggregate.GetLatestQuote()?.Id;
        renewPolicyActionData.PolicyId = quoteAggregate.Policy?.PolicyId;
        renewPolicyActionData.PolicyTransactionId = quoteAggregate.Policy?.Transactions?
            .OrderByDescending(t => t.CreatedTimestamp)
            .FirstOrDefault()?.Id;
        renewPolicyActionData.InputData = inputData;
    }

    private void SetContext(IProviderContext providerContext, QuoteAggregate quoteAggregate)
    {
        var contextManager = providerContext.AutomationData.ContextManager;
        var quote = quoteAggregate.GetLatestQuote();
        if (quote != null)
        {
            contextManager.SetContextEntity(
                EntityType.Quote, new ProviderEntity.Quote(quote.Id));
        }

        var policy = quoteAggregate.Policy;
        if (policy != null)
        {
            contextManager.SetContextEntity(EntityType.Policy, new ProviderEntity.Policy(policy.PolicyId));
            policy.Transactions?.ToList().ForEach(transaction =>
                contextManager.SetContextEntity(EntityType.PolicyTransaction, new ProviderEntity.PolicyTransaction(transaction.Id)));
        }
    }

    private async Task<ReleaseContext> CreateReleaseContextOrThrow(
        Guid tenantId, Guid productId, DeploymentEnvironment environment, Guid? productReleaseId, IEntity? policy)
    {
        if (productReleaseId == null && policy != null)
        {
            environment = policy.EntityEnvironment.ToEnumOrNull<DeploymentEnvironment>() ?? environment;
            var relContext = this.releaseQueryService.GetDefaultReleaseContextOrNull(tenantId, productId, environment);
            if (relContext == null)
            {
                var product = await this.cachingResolver.GetProductOrNull(tenantId, productId);
                throw new ErrorException(Errors.Automation.RenewPolicyAction.EnvironmentDefaultProductReleaseNotSet(
                    product?.Details?.Name, environment, this.DebugContext));
            }
            return relContext.Value;
        }
        return new ReleaseContext(tenantId, productId, environment, productReleaseId.GetValueOrDefault());
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

    private async Task<JObject> GetFormModel(IProviderContext providerContext)
    {
        if (this.inputDataProvider == null)
        {
            throw new ErrorException(Errors.Automation.IssuePolicyAction.MissingInputData());
        }

        try
        {
            var inputDataResult = await this.inputDataProvider.Resolve(providerContext);
            var inputDataValue = inputDataResult.GetValueOrThrowIfFailed().DataValue;
            var jObject = inputDataValue != null ? JObject.FromObject(new { formModel = inputDataValue }) : new JObject();
            return new FormData(jObject).FormModel;
        }
        catch (ErrorException e) when (e.Error.Code == "automation.providers.property.value.not.found")
        {
            throw new ErrorException(Errors.Automation.IssuePolicyAction.MissingInputData());
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
        var formModel = await this.GetFormModel(providerContext);
        var command = new QuoteCalculationCommand(
            releaseContext,
            quote?.Id,
            null,
            quote?.Type,
            releaseContext.ProductReleaseId,
            new CalculationDataModel { FormModel = formModel },
            null,
            hasPremiumFunding,
            persistResult,
            organisationId);

        await this.mediator.Send(command, providerContext.CancellationToken);

        return command;
    }

    private async Task<RenewalQuote?> GetRenewalQuoteOrThrow(Guid tenantId, Domain.Aggregates.Quote.Quote retrievedQuote)
    {
        if (retrievedQuote is not RenewalQuote renewalQuote)
        {
            throw new ErrorException(Errors.Automation.RenewPolicyAction.QuoteTypeInvalid(
                retrievedQuote.Id, retrievedQuote.QuoteNumber, retrievedQuote.Type, this.DebugContext));
        }

        var getQuoteCommand = new GetQuoteByIdQuery(tenantId, renewalQuote.Id);
        var quote = await this.mediator.Send(getQuoteCommand);
        var quoteState = quote?.QuoteState;
        var validStatuses = new[] { StandardQuoteStates.Incomplete, StandardQuoteStates.Approved };
        if (!validStatuses.Contains(quoteState, StringComparer.OrdinalIgnoreCase))
        {
            throw new ErrorException(Errors.Automation.RenewPolicyAction.QuoteStateInvalid(
                retrievedQuote.Id, retrievedQuote.QuoteNumber, quoteState, this.DebugContext));
        }

        this.ValidateCalculationResult(renewalQuote.LatestCalculationResult);
        return renewalQuote;
    }

    private void ValidateEnvironment(DeploymentEnvironment environment, string entityEnvironment, string entityName)
    {
        var parsedEnvironment = entityEnvironment.ToEnumOrNull<DeploymentEnvironment>();
        if (environment != parsedEnvironment)
        {
            throw new ErrorException(Errors.Operations.EnvironmentMisMatch(entityName));
        }
    }

    private void ValidateCalculationResult(QuoteDataUpdate<CalculationResult> calculationResult)
    {
        if (calculationResult == null)
        {
            throw new ErrorException(
                Errors.Automation.RenewPolicyAction.MissingLatestCalculationResult(this.DebugContext));
        }
    }

    private void ValidatePolicy(IPolicyReadModelDetails policy, RenewalQuote? renewalQuote)
    {
        if (renewalQuote != null && renewalQuote.PolicyId != policy.Id)
        {
            throw new ErrorException(
                Errors.Automation.RenewPolicyAction.QuoteMismatchWithPolicy(
                    policy.Id,
                    policy.QuoteId,
                    renewalQuote.Id,
                    policy.PolicyNumber,
                    renewalQuote.QuoteNumber,
                    policy.QuoteNumber,
                    this.DebugContext));
        }

        var validPolicyStates = new[] { "issued", "active", "expired" };
        if (!validPolicyStates.Contains(policy.PolicyState, StringComparer.OrdinalIgnoreCase))
        {
            throw new ErrorException(
                Errors.Automation.RenewPolicyAction.PolicyStatusInvalid(
                    policy.Id, policy.PolicyNumber, policy.PolicyState, this.DebugContext));
        }
    }
}
