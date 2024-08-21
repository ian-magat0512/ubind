// <copyright file="AsynchronousActionHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UBind.Application.Automation.Actions;
using UBind.Application.Automation.Data;
using UBind.Application.Automation.Enums;
using UBind.Application.Automation.Helper;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.ReadModel;
using UBind.Domain.SerialisedEntitySchemaObject;
using Action = UBind.Application.Automation.Actions.Action;

/// <inheritdoc/>
public class AsynchronousActionHandler : IAsynchronousActionHandler
{
    private readonly ICachingResolver cachingResolver;
    private readonly IServiceProvider serviceProvider;
    private readonly IAutomationConfigurationProvider configurationProvider;
    private readonly IActionRunner actionRunner;
    private readonly ILogger<AsynchronousActionHandler> logger;
    private readonly IQuoteReadModelRepository quoteReadModelRepository;
    private readonly IPolicyReadModelRepository policyReadModelRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsynchronousActionHandler"/> class.
    /// </summary>
    /// <param name="configurationProvider">The automation configuration provider.</param>
    /// <param name="actionRunner">The action runner.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
    public AsynchronousActionHandler(
        IAutomationConfigurationProvider configurationProvider,
        IActionRunner actionRunner,
        IServiceProvider serviceProvider,
        ICachingResolver cachingResolver,
        ILogger<AsynchronousActionHandler> logger,
        IQuoteReadModelRepository quoteReadModelRepository,
        IPolicyReadModelRepository policyReadModelRepository)
    {
        this.cachingResolver = cachingResolver;
        this.serviceProvider = serviceProvider;
        this.configurationProvider = configurationProvider;
        this.actionRunner = actionRunner;
        this.logger = logger;
        this.quoteReadModelRepository = quoteReadModelRepository;
        this.policyReadModelRepository = policyReadModelRepository;
    }

    /// <inheritdoc/>
    [JobDisplayName("Automations Asynchronous Action | TENANT: {3}, ORGANISATION: {4}, PRODUCT: {5}, ENVIRONMENT: {6}, AUTOMATION: {1}, ACTION: {2}")]
    public async Task HandleAsyncAction(
        string automationDataJson,
        string automationAlias,
        string actionAlias,
        string tenantAlias,
        string? organisationAlias,
        string? productAlias,
        string? deploymentEnvironment,
        Guid? productReleaseId,
        CancellationToken cancellationToken,
        bool isInternal = false,
        string? parentActionDataPath = null)
    {
        bool executeAsChild = parentActionDataPath != null && !isInternal;
        var automationData = JsonConvert.DeserializeObject<AutomationData>(
            automationDataJson, AutomationDeserializationConfiguration.DataSettings);

        if (automationData == null)
        {
            var errorData = new JObject()
            {
                { ErrorDataKey.TenantAlias, tenantAlias },
                { ErrorDataKey.ProductAlias, productAlias },
                { ErrorDataKey.Environment, deploymentEnvironment },
                { ErrorDataKey.Automation, automationAlias },
                { ErrorDataKey.ActionAlias, actionAlias },
            };

            throw new ErrorException(Errors.Automation.AutomationDataDeserializationError(errorData));
        }

        automationData.SetServiceProvider(this.serviceProvider);

        var tenantId = automationData.ContextManager.Tenant.Id;

        automationData.Context.TryGetValue("quote", out var quoteEntry);
        var quote = quoteEntry as Quote;
        automationData.Context.TryGetValue("policyTransaction", out var policyTransactionEntry);
        var policyTransaction = policyTransactionEntry as PolicyTransaction;
        if (quote != null)
        {
            productReleaseId = this.quoteReadModelRepository.GetProductReleaseId(tenantId, quote.Id);
        }
        else if (policyTransaction != null)
        {
            productReleaseId = this.policyReadModelRepository.GetProductReleaseId(tenantId, policyTransaction.Id);
        }

        Guid.TryParse(automationData.Automation[AutomationData.ProductId].ToString(), out Guid productId);
        var environment = automationData.System.Environment;
        var automations = await this.configurationProvider.GetAutomationConfigurationOrNull(
            tenantId,
            productId,
            environment,
            productReleaseId);
        var automation = automations?.Automations.FirstOrDefault(auto => auto.Alias.Equals(automationAlias));
        if (automation == null)
        {
            this.logger.LogError(
                "When running this action asynchronously, the original automation which triggered it was no "
                + "longer found. The automation might have been removed or deleted. The execution of this "
                + "asynchronous action will the now cease.");
            return;
        }

        var executableAction = await this.GetExecutableAction(
            tenantId, productId, environment, automation, actionAlias, executeAsChild);
        var actionData = this.GetActionData(automationData, executableAction, executeAsChild);

        // Execute
        await this.actionRunner.HandleAction(
            automationData,
            executableAction,
            actionData,
            cancellationToken,
            isInternal,
            parentActionDataPath);
        if (actionData != null && actionData.Error != null)
        {
            throw new ErrorException(actionData.Error);
        }
    }

    private async Task<Action> GetExecutableAction(
        Guid tenantId,
        Guid productId,
        DeploymentEnvironment environment,
        Automation automation,
        string actionAlias,
        bool isChildAction)
    {
        // Grab the action to be executed from the automation.
        Action? executableAction = null;
        if (isChildAction)
        {
            executableAction = this.FindActionInAutomationHierarchy(automation, actionAlias);
        }
        else
        {
            executableAction = automation.Actions.SingleOrDefault(act => act.Alias.Equals(actionAlias));
        }

        if (executableAction == null)
        {
            var errorData = new JObject()
            {
                { ErrorDataKey.Automation, automation.Alias },
                { ErrorDataKey.ActionAlias, actionAlias },
            };

            var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
            var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(tenantId, productId);

            throw new ErrorException(Errors.Automation.AutomationActionNotFound(
                tenantAlias, productAlias, environment, automation.Alias, actionAlias, errorData));
        }

        return executableAction;
    }

    private Action? FindActionInAutomationHierarchy(Automation automation, string actionAlias)
    {
        foreach (var action in automation.Actions)
        {
            if (action.Alias.Equals(actionAlias))
            {
                return action;
            }

            if (action is GroupAction)
            {
                var foundAction = this.RetrieveChildFromParentAction(action, actionAlias);
                if (foundAction.HasValue)
                {
                    return foundAction.Value;
                }
            }

            if (action is IRunnableParentAction parentAction)
            {
                var childAction = parentAction.ChildActions.FirstOrDefault(c => c.Alias.Equals(actionAlias));
                if (childAction != null)
                {
                    return (Action)childAction;
                }
            }
        }

        return null;
    }

    private Maybe<Action> RetrieveChildFromParentAction(Action action, string childAlias)
    {
        // Get either Actions or ChildActions property from the action.
        var actionsProperty = action.GetType().GetProperty("Actions") ?? action.GetType().GetProperty("ChildActions");
        if (actionsProperty == null)
        {
            return default;
        }

        var actionPropertyValue =
            actionsProperty.GetValue(action) as IEnumerable<Action> ?? Enumerable.Empty<Action>();
        foreach (var childAction in actionPropertyValue)
        {
            if (childAction.Alias.Equals(childAlias))
            {
                return childAction;
            }

            if (childAction is GroupAction)
            {
                var sub = this.RetrieveChildFromParentAction(childAction, childAlias);
                if (sub.HasValue)
                {
                    return sub.Value;
                }
            }
        }

        return default;
    }

    private ActionData GetActionData(AutomationData automationData, Action action, bool isChildAction)
    {
        ActionData? actionData = null;

        // if isChildAction = action data must be present within a parent action data.
        if (isChildAction)
        {
            actionData = this.GetChildActionData(automationData.Actions, action.Alias);
            if (actionData == null)
            {
                var errorDetails = GenericErrorDataHelper.GetGeneralErrorDetails(
                    automationData.ContextManager.Tenant.Id,
                    automationData.ContextManager?.Product.Id,
                    automationData.System.Environment);
                throw new ErrorException(
                    Errors.Automation.ActionExecutionErrorEncountered(action.Alias, data: errorDetails));
            }
        }
        else
        {
            actionData = automationData.Actions.FirstOrDefault(x => x.Key.Equals(action.Alias)).Value
                ?? action.CreateActionData();
            automationData.AddActionData(actionData);
        }

        return actionData;
    }

    private ActionData? GetChildActionData(Dictionary<string, ActionData>? actionDataDictionary, string actionAlias)
    {
        if (actionDataDictionary == null)
        {
            return null;
        }

        foreach (var actionData in actionDataDictionary.Values)
        {
            if (actionData.Alias.Equals(actionAlias))
            {
                return actionData;
            }

            if (actionData.Type.Equals(ActionType.GroupAction))
            {
                // Check if the Actions property exists
                var childActionsProperty = actionData.GetType().GetProperty("Actions");
                if (childActionsProperty != null)
                {
                    var childActionPropertyValue = childActionsProperty.GetValue(actionData) as Dictionary<string, ActionData>;
                    var result = this.GetChildActionData(childActionPropertyValue, actionAlias);

                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            if (actionData.Type.Equals(ActionType.IterateAction))
            {
                var iterateActionData = (IterateActionData)actionData;
                if (iterateActionData.CurrentIteration?.Actions.TryGetValue(actionAlias, out ActionData? result) ?? false)
                {
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
        }

        // If it got here - action does not exist.
        return null;
    }
}
