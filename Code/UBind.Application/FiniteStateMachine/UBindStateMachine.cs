// <copyright file="UBindStateMachine.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FiniteStateMachine
{
    using System;
    using System.Threading.Tasks;
    using Stateless;

    /// <summary>
    /// Provides abstraction for UBind generic state machine to be used by third party data set job and of similar use case.
    /// </summary>
    /// <typeparam name="TState">The state machine state type.</typeparam>
    /// <typeparam name="TTrigger">The state machine trigger type.</typeparam>
    public abstract class UBindStateMachine<TState, TTrigger>
    {
        private StateMachine<TState, TTrigger> stateMachine;

        /// <summary>
        /// Get the state configuration of a given sate.
        /// </summary>
        /// <param name="configureState">The state machine state.</param>
        /// <returns>Return the State Configuration of the given state machine state.</returns>
        public StateMachine<TState, TTrigger>.StateConfiguration Configure(TState configureState)
        {
            return this.stateMachine.Configure(configureState);
        }

        /// <summary>
        /// Transition from the current state via the specified trigger.
        /// </summary>
        /// <param name="trigger">The trigger to fire..</param>
        public void Fire(TTrigger trigger)
        {
            this.stateMachine.Fire(trigger);
        }

        /// <summary>
        /// Transition from the current state via the specified trigger in async fashion.
        /// </summary>
        /// <param name="trigger">The trigger to fire..</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task FireAsync(TTrigger trigger)
        {
            await this.stateMachine.FireAsync(trigger);
        }

        /// <summary>
        /// Transition from the current state via the specified trigger in async fashion.
        /// </summary>
        /// <param name="trigger">The trigger to fire..</param>
        /// <param name="parameter">parameter.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task FireAsync(TTrigger trigger, string parameter)
        {
            var triggerWithParam = this.stateMachine.SetTriggerParameters<string>(trigger);

            await this.stateMachine.FireAsync(triggerWithParam, parameter);
        }

        /// <summary>
        /// Set the trigger to be fired with a parameter.
        /// </summary>
        /// <typeparam name="TArg0">.TArg0.</typeparam>
        /// <param name="trigger">trigger.</param>
        /// <returns>TriggerWithParameters.</returns>
        public StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0> SetTriggerParameters<TArg0>(TTrigger trigger)
        {
            return this.stateMachine.SetTriggerParameters<TArg0>(trigger);
        }

        /// <summary>
        /// Get the current state of the state machine.
        /// </summary>
        /// <returns>Returns the current state machine state.</returns>
        public TState CurrentState()
        {
            return this.stateMachine.State;
        }

        /// <summary>
        /// Activates current state. The activation is idempotent and subsequent activation of the same current state
        /// will not lead to re-execution of activation callbacks.
        /// </summary>
        public void Activate()
        {
            this.stateMachine.Activate();
        }

        /// <summary>
        /// Activates current state in async fashion. The activation is idempotent and subsequent activation of the same current state
        /// will not lead to re-execution of activation callbacks.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ActivateAsync()
        {
            await this.stateMachine.ActivateAsync();
        }

        /// <summary>
        /// Construct a state machine with external state storage.
        /// </summary>
        /// <param name="stateAccessor">A function that will be called to read the current state value.</param>
        /// <param name="stateMutator">An action that will be called to write new state values.</param>
        public void SetupStateMachine(Func<TState> stateAccessor, Action<TState> stateMutator)
        {
            this.stateMachine = new StateMachine<TState, TTrigger>(stateAccessor, stateMutator);
        }
    }
}
