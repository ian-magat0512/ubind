// <copyright file="RaiseErrorAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using Void = UBind.Domain.Helpers.Void;

    /// <summary>
    /// This class represents an automation action that results in a new error to be raised.
    /// </summary>
    public class RaiseErrorAction : Action
    {
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="RaiseErrorAction"/> class.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <param name="alias">The alias of the action.</param>
        /// <param name="description">The action description.</param>
        /// <param name="runCondition">An optional condition.</param>
        /// <param name="beforeRunErrorConditions">The validation rules before the action.</param>
        /// <param name="afterRunErrorConditions">The validation rules after the action.</param>
        /// <param name="onErrorActions">A list of actions to run if an error is encountered on processing.</param>
        /// <param name="error">The error that will be raised by this action.</param>
        public RaiseErrorAction(
            string name,
            string alias,
            string description,
            IProvider<Data<bool>> runCondition,
            IEnumerable<ErrorCondition> beforeRunErrorConditions,
            IEnumerable<ErrorCondition> afterRunErrorConditions,
            IEnumerable<Action> onErrorActions,
            IProvider<ConfiguredError> error,
            IClock clock)
            : base(name, alias, description, false, runCondition, beforeRunErrorConditions, afterRunErrorConditions, onErrorActions)
        {
            Contract.Assert(error != null);
            this.Error = error;
            this.clock = clock;
        }

        /// <summary>
        /// Gets the error to be raised by the action.
        /// </summary>
        public IProvider<ConfiguredError> Error { get; }

        /// <inheritdoc/>
        public override ActionData CreateActionData() => new RaiseErrorActionData(this.Name, this.Alias, this.clock);

        /// <inheritdoc/>
        public override async Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal)
        {
            using (MiniProfiler.Current.Step(nameof(RaiseErrorAction) + "." + nameof(this.Execute)))
            {
                actionData.UpdateState(ActionState.Running);
                var error = (await this.Error.Resolve(providerContext)).GetValueOrThrowIfFailed();
                ((RaiseErrorActionData)actionData).RaisedError = error.ToError();
                return Result.Failure<Void, Domain.Error>(error.ToError());
            }
        }

        public override bool IsReadOnly() => this.AreAllOnErrorActionsReadOnly();
    }
}
