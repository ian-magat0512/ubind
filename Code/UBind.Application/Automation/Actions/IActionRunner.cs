// <copyright file="IActionRunner.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions;

using System.Threading;
using System.Threading.Tasks;
using UBind.Application.Automation.Data;

/// <summary>
/// Handles the execution of an action.
/// </summary>
public interface IActionRunner
{
    /// <summary>
    /// Handles the creation of a Hangfire job for the asynchronous action.
    /// </summary>
    /// <param name="automationData">The automation data.</param>
    /// <param name="action">The asynchronous action.</param>
    /// <param name="automationAlias">The alias of the automation the action to be ran is for.</param>
    /// <param name="actionData">The data-bag of the action to be executed.</param>
    /// <param name="isInternal">The action is internal to another action (e.g. onErrorActions).</param>
    /// <param name="parentActionDataPath">The parent action data's path, or null if this is not a child action.</param>
    Task HandleAsyncAction(
        AutomationData automationData,
        IRunnableAction action,
        string automationAlias,
        ActionData actionData,
        bool isInternal = false,
        string? parentActionDataPath = null);

    /// <summary>
    /// Handles the process of execution for an automation action.
    /// </summary>
    /// <param name="data">The current automation data being used by the automation.</param>
    /// <param name="action">The action to be executed.</param>
    /// <param name="actionData">The data of the action being executed.</param>
    /// <param name="isInternal">The action is internal to another action (e.g. onErrorActions).</param>
    /// <param name="parentActionDataPath">The parent action data's path, or null if this is not a child action.</param>
    /// <returns>An awaitable task.</returns>
    Task HandleAction(
        AutomationData data,
        IRunnableAction action,
        ActionData actionData,
        CancellationToken cancellationToken,
        bool isInternal = false,
        string? parentActionDataPath = null);
}
