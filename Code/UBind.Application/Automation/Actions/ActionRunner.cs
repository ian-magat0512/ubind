// <copyright file="ActionRunner.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UBind.Application.Automation.Data;
using UBind.Application.Automation.Enums;
using UBind.Application.Automation.Error;
using UBind.Application.Automation.Extensions;
using UBind.Application.Automation.Providers;
using UBind.Application.Commands.Sentry;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.Extensions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Processing;
using Void = UBind.Domain.Helpers.Void;

/// <inheritdoc/>
public class ActionRunner : IActionRunner
{
    private readonly ICachingResolver cachingResolver;
    private readonly IJobClient backgroundJobClient;
    private readonly ICqrsMediator mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionRunner"/> class.
    /// </summary>
    /// <param name="backgroundJobClient">The background job client.</param>
    public ActionRunner(
        IJobClient backgroundJobClient,
        ICachingResolver cachingResolver,
        ICqrsMediator mediator)
    {
        this.cachingResolver = cachingResolver;
        this.backgroundJobClient = backgroundJobClient;
        this.mediator = mediator;
    }

    /// <inheritdoc/>
    public async Task HandleAsyncAction(
        AutomationData automationData,
        IRunnableAction action,
        string automationAlias,
        ActionData actionData,
        bool isInternal = false,
        string parentActionDataPath = null)
    {
        actionData.UpdateState(ActionState.Unknown);
        var jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };

        string automationSerializedJsonData = JsonConvert.SerializeObject(automationData, jsonSerializerSettings);
        var jobParameters = await this.GetJobParameters(automationData);
        string tenantAlias = jobParameters.First(p => p.Name == JobParameter.TenantParameterName).Value;
        string? organisationAlias = jobParameters.FirstOrDefault(p => p.Name == JobParameter.OrganisationParameterName)?.Value;
        string? productAlias = jobParameters.FirstOrDefault(p => p.Name == JobParameter.ProductParameterName)?.Value;
        string? environment = jobParameters.FirstOrDefault(p => p.Name == JobParameter.EnvironmentParameterName)?.Value;
        Guid? productReleaseId = automationData.ContextManager.ProductRelease?.Id;

        // Note: in the following Enqueue call we are passing in CancellationToken.None, however this will be
        // replaced by hangfire with it's own cancellation token, a real one.
        // See https://docs.hangfire.io/en/latest/background-methods/using-cancellation-tokens.html
        var jobId = this.backgroundJobClient.Enqueue<AsynchronousActionHandler>(
            aa => aa.HandleAsyncAction(
                automationSerializedJsonData,
                automationAlias,
                action.Alias,
                tenantAlias,
                organisationAlias,
                productAlias,
                environment,
                productReleaseId,
                CancellationToken.None,
                isInternal,
                parentActionDataPath),
            jobParameters.ToArray());
    }

    /// <inheritdoc/>
    public async Task HandleAction(
        AutomationData automationData,
        IRunnableAction action,
        ActionData actionData,
        CancellationToken cancellationToken,
        bool isInternal = false,
        string parentActionDataPath = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        actionData.UpdateState(ActionState.Started);
        string currentActionDataPath = isInternal
            ? parentActionDataPath
            : parentActionDataPath != null
                ? parentActionDataPath + "/" + action.Alias
                : "/actions/" + action.Alias;
        var providerContext = new ProviderContext(automationData, cancellationToken, currentActionDataPath);
        var runCondition = action.RunCondition != null ? (await action.RunCondition.Resolve(providerContext)).GetValueOrThrowIfFailed() : null;
        var runConditionMet = runCondition?.DataValue ?? true;
        if (runConditionMet)
        {
            var result = await this.PerformBeforeRunErrorConditionVerification(providerContext, actionData, action);
            if (result.IsSuccess)
            {
                try
                {
                    var actionResult = await action.Execute(providerContext, actionData, isInternal);
                    if (actionResult.IsFailure)
                    {
                        await this.HandleOnErrorActionRequest(
                            automationData,
                            actionData,
                            cancellationToken,
                            action,
                            actionResult.Error,
                            parentActionDataPath);
                        actionData.UpdateState(ActionState.Completed);
                        return;
                    }
                }
                catch (ErrorException ex)
                {
                    var error = await this.GenerateError(action.Alias, providerContext, ex);
                    await this.HandleOnErrorActionRequest(
                        automationData, actionData, cancellationToken, action, error, parentActionDataPath);
                    actionData.UpdateState(ActionState.Completed);
                    return;
                }

                result = await this.PeformAfterRunErrorConditionVerification(providerContext, actionData, action);
                if (result.IsSuccess)
                {
                    actionData.UpdateState(ActionState.Completed);
                    return;
                }
            }

            await this.HandleOnErrorActionRequest(
                automationData, actionData, cancellationToken, action, result.Error.ToError(), parentActionDataPath);
            actionData.UpdateState(ActionState.Completed);
        }
    }

    private async Task HandleOnErrorActionRequest(
        AutomationData data,
        ActionData actionData,
        CancellationToken cancellationToken,
        IRunnableAction action,
        Error error,
        string parentActionDataPath)
    {
        if (action.OnErrorActions?.Any() != true)
        {
            actionData.AppendRaisedError(error);
            return;
        }

        data.SetError(error);
        foreach (var onErrorAction in action.OnErrorActions)
        {
            var onErrorActionData = actionData.OnErrorActions
                ?.FirstOrDefault(act => act.Key == onErrorAction.Alias).Value ?? onErrorAction.CreateActionData();

            if (!onErrorAction.Asynchronous)
            {
                actionData.AddRaisedOnErrorAction(onErrorActionData);
                await this.HandleAction(
                    data,
                    onErrorAction,
                    onErrorActionData,
                    cancellationToken,
                    true,
                    parentActionDataPath);
            }
            else
            {
                actionData.ToggleStatusValuesForAsyncActions();
                data.Automation.TryGetValue("automation", out object automationAlias);
                if (automationAlias != null)
                {
                    await this.HandleAsyncAction(
                        data, onErrorAction, automationAlias.ToString(), onErrorActionData, true, parentActionDataPath);
                }
                else
                {
                    // this is a system error, as automation alias should be foremost persisted in automation data after data has been created
                    throw new ErrorException(Errors.General.Unexpected("Automation alias is not found in data"));
                }
            }
        }

        data.SetError(null);

        // This checks if none of the OnErrorActions were executed due to run conditions. If so, then error should be re-raised.
        // Note that asynchronously ran actions will also be flagged as not finished, and also fall in the same requirement.
        if (actionData.OnErrorActions == null || !actionData.OnErrorActions.Values.Any(act => act.Finished.Value))
        {
            actionData.AppendRaisedError(error);
        }

        if (actionData.OnErrorActions != null && actionData.OnErrorActions.Values.Any(act => act.Error != default))
        {
            var raisedError = actionData.OnErrorActions.Values.LastOrDefault(act => act.Error != default).Error;
            actionData.AppendRaisedError(raisedError);
        }
    }

    private async Task<Domain.Error> GenerateError(string actionAlias, IProviderContext providerContext, Exception ex)
    {
        if (ex is ErrorException err)
        {
            return err.Error;
        }
        else
        {
            JObject errorData = await providerContext.GetDebugContext();
            errorData.Add("errorMessage", ex.Message);
            errorData.Add("source", ex.Source);

            // Errors from automations should always be ErrorExceptions, so this is a system error, and we should
            // send it to sentry
            await this.mediator.Send(new CaptureSentryExceptionCommand(
                ex,
                providerContext.AutomationData.System.Environment,
                errorData));

            return Errors.Automation.ActionExecutionErrorEncountered(actionAlias, errorData);
        }
    }

    /// <summary>
    /// Verifies if the before-run error conditions are met, and raises an error if so.
    /// </summary>
    /// <param name="providerContext">The data and path to perform resolutions with.</param>
    /// <param name="actionData">The action data for this action.</param>
    /// <param name="action">The action that owns the before run error conditions.</param>
    /// <returns>A value indicating whether the run conditions are met.</returns>
    private async Task<Result<Void, ConfiguredError>> PerformBeforeRunErrorConditionVerification(
        IProviderContext providerContext, ActionData actionData, IRunnableAction action)
    {
        actionData.UpdateState(ActionState.BeforeRunErrorChecking);
        return await this.EvaluateRunErrorConditions(providerContext, action.BeforeRunErrorConditions);
    }

    /// <summary>
    /// Verifies if the before-run error conditions are met, and raises an error if so.
    /// </summary>
    /// <param name="providerContext">The data and path to perform resolutions with.</param>
    /// <param name="actionData">The action data for this action.</param>
    /// <param name="action">The action that owns the before run error conditions.</param>
    /// <returns>A value indicating whether the run conditions are met.</returns>
    private async Task<Result<Void, ConfiguredError>> PeformAfterRunErrorConditionVerification(
        IProviderContext providerContext, ActionData actionData, IRunnableAction action)
    {
        actionData.UpdateState(ActionState.AfterRunErrorChecking);
        return await this.EvaluateRunErrorConditions(providerContext, action.AfterRunErrorConditions);
    }

    private async Task<Result<Void, ConfiguredError>> EvaluateRunErrorConditions(
        IProviderContext providerContext, IEnumerable<ErrorCondition> errorConditions)
    {
        var resolveErrorCondition = await errorConditions.SelectAsync(async x => await x.Evaluate(providerContext) != null ? x : null);
        var satisfiedCondition = resolveErrorCondition.FirstOrDefault(x => x != null);
        if (satisfiedCondition != default)
        {
            return Result.Failure<Void, ConfiguredError>((await satisfiedCondition.Error.Resolve(providerContext)).GetValueOrThrowIfFailed());
        }

        return Result.Success<Void, ConfiguredError>(default);
    }

    private async Task<List<JobParameter>> GetJobParameters(AutomationData automationData)
    {
        var tenantId = automationData.ContextManager.Tenant.Id;
        var tenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
        var organisationId = automationData.ContextManager.Organisation?.Id;
        var organisationAlias = organisationId != null
            ? (await this.cachingResolver.GetOrganisationOrThrow(tenantId, organisationId.Value)).Alias
            : null;
        var productId = Guid.Parse(automationData.Automation[AutomationData.ProductId].ToString());
        var productAlias = await this.cachingResolver.GetProductAliasOrThrowAsync(tenantId, productId);

        var jobParameters = new List<JobParameter>
        {
            JobParameter.Tenant(tenantAlias),
            JobParameter.Product(productAlias),
            JobParameter.Environment(automationData.System.Environment),
        };

        if (organisationId != null)
        {
            jobParameters.Add(JobParameter.Organisation(organisationAlias));
        }

        return jobParameters;
    }
}
