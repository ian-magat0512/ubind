// <copyright file="IAsynchronousActionHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation;

using Hangfire;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Handler service used for handling execution of asynchronous actions.
/// </summary>
public interface IAsynchronousActionHandler
{
    /// <summary>
    /// Handles execution of asynchronously configured actions using the given automation data and automation config.
    /// </summary>
    /// <param name="automationDataJson">The automation data in JSON format.</param>
    /// <param name="automationAlias">Alias of hte automation for which the action is for.</param>
    /// <param name="actionAlias">Alias of the action to be executed.</param>
    /// <param name="parentActionDataPath">The path of the parent action data, or if this is a top level action, null.</param>
    /// <returns>An awaitable task.</returns>
    [JobDisplayName("Automations Asynchronous Action | TENANT: {3}, ORGANISATION: {4}, PRODUCT: {5}, ENVIRONMENT: {6}, AUTOMATION: {1}, ACTION: {2}")]
    Task HandleAsyncAction(
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
            string? parentActionDataPath = null);
}
