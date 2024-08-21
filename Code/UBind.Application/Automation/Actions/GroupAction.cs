// <copyright file="GroupAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Processing;
    using Void = UBind.Domain.Helpers.Void;

    /// <summary>
    /// Represents a group of automation actions that are executed together as a single unit.
    /// </summary>
    /// <remarks>
    /// This class is designed to execute a collection of actions as a cohesive group. It can be configured
    /// to run its child actions either in parallel or sequentially, providing flexibility in automation
    /// workflow management. If configured to run in parallel, all child actions will execute asynchronously
    /// and then wait for all others in the group to complete. If configured to run sequentially, each child
    /// action will execute one after the other.
    /// </remarks>
    public class GroupAction : Action, IRunnableParentAction
    {
        private readonly IActionRunner actionRunner;
        private readonly IJobClient backgroundJobClient;
        private readonly IClock clock;

        public GroupAction(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>> runCondition,
            IEnumerable<ErrorCondition> beforeRunConditions,
            IEnumerable<ErrorCondition> afterRunConditions,
            IEnumerable<IRunnableAction> errorActions,
            IEnumerable<IRunnableAction> actions,
            bool parallel,
            IActionRunner actionRunner,
            IJobClient backgroundJobClient,
            IClock clock)
            : base(name, alias, description, asynchronous, runCondition, beforeRunConditions, afterRunConditions, errorActions)
        {
            this.ChildActions = actions;
            this.Parallel = parallel;
            this.actionRunner = actionRunner;
            this.backgroundJobClient = backgroundJobClient;
            this.clock = clock;
        }

        public IEnumerable<IRunnableAction> ChildActions { get; } = Enumerable.Empty<IRunnableAction>();

        /// <summary>
        /// Gets a value indicating whether the actions in this group will be run in parallel. Default value is false.
        /// </summary>
        /// <remarks>
        /// If true, then the \"asynchronous\" property of each of the actions will be ignored, and they will each be set to run asynchronously,
        /// then wait for all other actions in this groupAction to complete.
        /// NOTE: To be implemented by a separate ticket: UB-5860.
        /// </remarks>
        public bool Parallel { get; }

        public override ActionData CreateActionData() => new GroupActionData(this.Name, this.Alias, this.clock);

        /// <summary>
        /// Executes the group of actions configured as a whole.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <param name="actionData">The action data for this action.</param>
        /// <returns>An awaitable task.</returns>
        public override async Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal = false)
        {
            using (MiniProfiler.Current.Step($"{nameof(GroupAction)}.{nameof(this.Execute)}"))
            {
                actionData.UpdateState(ActionState.Running);
                var childActionDataList = new Dictionary<string, ActionData>();
                var actionTaskList = new Dictionary<string, Func<Task>>();

                foreach (var action in this.ChildActions)
                {
                    providerContext.CancellationToken.ThrowIfCancellationRequested();

                    var childActionData = action.CreateActionData();
                    if (!childActionDataList.ContainsKey(childActionData.Alias))
                    {
                        childActionDataList.Add(childActionData.Alias, childActionData);
                    }

                    // Update parent action of new action execution
                    (actionData as GroupActionData).Actions = childActionDataList;

                    if (action.Asynchronous)
                    {
                        providerContext.AutomationData.Automation.TryGetValue("automation", out object automationAlias);
                        if (automationAlias != null)
                        {
                            childActionData.ToggleStatusValuesForAsyncActions();
                            if (!actionTaskList.ContainsKey(childActionData.Alias))
                            {
                                actionTaskList.Add(
                                    childActionData.Alias,
                                    () => this.actionRunner.HandleAsyncAction(
                                            providerContext.AutomationData,
                                            action,
                                            automationAlias.ToString(),
                                            childActionData,
                                            isInternal,
                                            providerContext.CurrentActionDataPath + "/actions"));
                            }
                        }
                        else
                        {
                            // This is a system error, as automation alias should be foremost persisted in automation data after data has been created
                            throw new ErrorException(Domain.Errors.General.Unexpected("Automation alias is not found in data"));
                        }
                    }
                    else
                    {
                        if (!actionTaskList.ContainsKey(childActionData.Alias))
                        {
                            actionTaskList.Add(
                                childActionData.Alias,
                                () => this.actionRunner.HandleAction(
                                        providerContext.AutomationData,
                                        action,
                                        childActionData,
                                        providerContext.CancellationToken,
                                        isInternal,
                                        providerContext.CurrentActionDataPath + "/actions"));
                        }
                    }
                }

                // Execute the tasks concurrently if this.Parallel is true; otherwise, execute them sequentially
                Result<Void, Error> result = new Result<Void, Error>();
                if (this.Parallel)
                {
                    result = await this.ExecuteActionsParallel(childActionDataList, actionTaskList);
                }
                else
                {
                    result = await this.ExecuteActionsSequentially(childActionDataList, actionTaskList);
                }

                if (result.IsFailure)
                {
                    providerContext.AutomationData.SetError(result.Error);
                }

                return result;
            }
        }

        public override bool IsReadOnly()
        {
            return this.AreAllActionsReadOnly(this.ChildActions) && this.AreAllOnErrorActionsReadOnly();
        }

        private async Task<Result<Void, Error>> ExecuteActionsParallel(
            Dictionary<string, ActionData> childActionDataList,
            Dictionary<string, Func<Task>> actionTaskList)
        {
            var listOfTask = actionTaskList.Values.Select(actionTask => actionTask.Invoke());
            await Task.WhenAll(listOfTask);

            if (childActionDataList.Values.Any(actionData => actionData.Error != null))
            {
                return Result.Failure<Void, Error>(childActionDataList.Values.First(actionData => actionData.Error != null).Error);
            }

            return Result.Success<Void, Error>(default);
        }

        private async Task<Result<Void, Error>> ExecuteActionsSequentially(
            Dictionary<string, ActionData> childActionDataList,
            Dictionary<string, Func<Task>> actionTaskList)
        {
            // Check for errors in the results and return the appropriate result
            foreach (var actionTask in actionTaskList)
            {
                await actionTask.Value.Invoke();
                var childActionData = childActionDataList[actionTask.Key];
                if (childActionData.Error != null)
                {
                    return Result.Failure<Void, Error>(childActionData.Error);
                }
            }
            return Result.Success<Void, Error>(default);
        }
    }
}
