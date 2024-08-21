// <copyright file="Action.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using Void = UBind.Domain.Helpers.Void;

    /// <summary>
    /// Defines the actions that can be ran by automations.
    /// </summary>
    public abstract class Action : IRunnableAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Action"/> class.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <param name="alias">The alias of the action.</param>
        /// <param name="description">The action description.</param>
        /// <param name="asynchronous">The asynchronous to be used.</param>
        /// <param name="runCondition">An optional condition.</param>
        /// <param name="beforeRunConditions">The validation rules before the action.</param>
        /// <param name="afterRunConditions">The validation rules after the action.</param>
        /// <param name="errorActions">The list of non successful actions.</param>
        protected Action(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>>? runCondition,
            IEnumerable<ErrorCondition>? beforeRunConditions,
            IEnumerable<ErrorCondition>? afterRunConditions,
            IEnumerable<IRunnableAction>? errorActions)
        {
            this.Name = name;
            this.Alias = alias;
            this.Description = description;
            this.RunCondition = runCondition;
            this.Asynchronous = asynchronous;
            this.BeforeRunErrorConditions = beforeRunConditions;
            this.AfterRunErrorConditions = afterRunConditions;
            this.OnErrorActions = errorActions;
        }

        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        public string Name { get; }

        /// <inheritdoc/>
        public string Alias { get; }

        /// <summary>
        /// Gets the description of the action.
        /// </summary>
        public string Description { get; }

        /// <inheritdoc/>
        public bool Asynchronous { get; }

        /// <inheritdoc/>
        public IProvider<Data<bool>>? RunCondition { get; }

        /// <inheritdoc/>
        public IEnumerable<ErrorCondition>? BeforeRunErrorConditions { get; }

        /// <inheritdoc/>
        public IEnumerable<ErrorCondition>? AfterRunErrorConditions { get; }

        /// <inheritdoc/>
        public IEnumerable<IRunnableAction>? OnErrorActions { get; }

        /// <inheritdoc/>
        public abstract Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal = false);

        /// <inheritdoc/>
        public abstract ActionData CreateActionData();

        /// <summary>
        /// Determines if the action is read-only.
        /// Derived classes that are read-only should call <see cref="AreAllOnErrorActionsReadOnly"/>
        /// to ensure all error actions are also read-only.
        /// Else, return false for actions that are not read-only.
        /// </summary>
        /// <returns>True if the action and all its error actions are read-only; otherwise, false.</returns>
        public abstract bool IsReadOnly();

        /// <summary>
        /// Determines if all actions in <see cref="OnErrorActions"/> are read-only.
        /// </summary>
        /// <returns>True if <see cref="OnErrorActions"/> is null or all actions within are read-only; otherwise, false.</returns>
        public bool AreAllOnErrorActionsReadOnly()
        {
            return this.OnErrorActions == null || this.AreAllActionsReadOnly(this.OnErrorActions);
        }

        /// <summary>
        /// Determines if all actions given are read-only.
        /// </summary>
        /// <param name="runnableActions"></param>
        /// <returns></returns>
        public bool AreAllActionsReadOnly(IEnumerable<IRunnableAction> runnableActions)
        {
            return runnableActions.All(a => a.IsReadOnly());
        }
    }
}
