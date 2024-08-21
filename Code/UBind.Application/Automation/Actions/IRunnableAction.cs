// <copyright file="IRunnableAction.cs" company="uBind">
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
    /// Represents the properties of an action that makes it available for running.
    /// </summary>
    public interface IRunnableAction
    {
        /// <summary>
        /// Gets the alias of the action.
        /// </summary>
        string Alias { get; }

        /// <summary>
        /// Gets a value indicating whether the action is asychronous or not .
        /// </summary>
        bool Asynchronous { get; }

        /// <summary>
        /// Gets the condition to be evaluated, if any, before running the action.
        /// </summary>
        IProvider<Data<bool>>? RunCondition { get; }

        /// <summary>
        /// Gets a collection representing validation rules applied to Action before running the action.
        /// If any fail, the response will be an error as defined.
        /// </summary>
        IEnumerable<ErrorCondition>? BeforeRunErrorConditions { get; }

        /// <summary>
        /// Gets a collection representing validation rules applied to Action after running the action.
        /// If any fail, the response will be an error as defined.
        /// </summary>
        IEnumerable<ErrorCondition>? AfterRunErrorConditions { get; }

        /// <summary>
        /// Gets a collection representing the list of actions to be run if this action fails to complete successfully.
        /// </summary>
        IEnumerable<IRunnableAction>? OnErrorActions { get; }

        /// <summary>
        /// Executes the action to be performed.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <param name="actionData">The action data.</param>
        /// <param name="isInternal">If the action is being run internally to another action (e.g. onErrorActions) but is not
        /// a child action, then set this to true.</param>
        /// <returns>An awaitable task.</returns>
        Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal = false);

        /// <summary>
        /// Returns an instance of the action data appropriate for te given action.
        /// </summary>
        /// <returns>An instance of <see cref="ActionData"/>.</returns>
        ActionData CreateActionData();

        /// <summary>
        /// Returns a value indicating whether the action will not make any writes to the database or state
        /// when it is executed.
        /// This is needed to determine the RequestIntent, which tells the system whether to direct requests to
        /// the read-write master database instance, or the read-only replica database instance.
        /// </summary>
        bool IsReadOnly();
    }
}
