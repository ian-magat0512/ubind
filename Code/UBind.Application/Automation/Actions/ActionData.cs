// <copyright file="ActionData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Application.Automation.Enums;
    using UBind.Application.JsonConverters;
    using UBind.Domain;

    /// <summary>
    /// A class containing the data output of an action in an automation.
    /// </summary>
    public abstract class ActionData
    {
        private IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionData"/> class.
        /// </summary>
        /// <param name="name">The name for the action, for display purposes.</param>
        /// <param name="alias">The alias of the action, for identification purpooses.</param>
        /// <param name="type">The type of action this data is for.</param>
        /// <param name="clock">The clock for telling time.</param>
        public ActionData(string name, string alias, ActionType type, IClock clock)
        {
            this.Name = name;
            this.Alias = alias;
            this.Type = type;
            this.clock = clock;
        }

        [JsonConstructor]
        protected ActionData(ActionType type)
        {
            this.Type = type;
        }

        /// <summary>
        /// Gets the type of action this data is for.
        /// </summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter), converterParameters: typeof(CamelCaseNamingStrategy))]
        public ActionType Type { get; }

        /// <summary>
        /// Gets or sets the name of the action this data is for.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; protected set; }

        /// <summary>
        /// Gets or sets the alias of the action this data is for.
        /// </summary>
        [JsonProperty("alias")]
        public string Alias { get; protected set; }

        /// <summary>
        /// Gets or sets the value indicating whether the action is asynchronous.
        /// </summary>
        [JsonProperty("asynchronous", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Asynchronous { get; protected set; }

        /// <summary>
        /// Gets the current state of this action.
        /// </summary>
        [JsonProperty("state")]
        [JsonConverter(typeof(StringEnumConverter), converterParameters: typeof(CamelCaseNamingStrategy))]
        public ActionState State { get; private set; } = ActionState.NotStarted;

        /// <summary>
        /// Gets a value indicating whether this action has started.
        /// </summary>
        [JsonProperty("started")]
        public bool Started { get; private set; } = false;

        /// <summary>
        /// Gets a value indicating whether this action ran, i.e. the run condition was true and
        /// there were no errors before the action had a chance to run.
        /// </summary>
        [JsonProperty("ran")]
        public bool? Ran { get; private set; } = false;

        /// <summary>
        /// Gets a value indicating whether this action has finished,
        /// i.e. it has completed running and after error checking has completed.
        /// </summary>
        [JsonProperty("finished")]
        public bool? Finished { get; private set; } = false;

        /// <summary>
        /// Gets a value indicating whether this action has finished without errors.
        /// If this is false, an error object is required.
        /// </summary>
        [JsonProperty("succeeded")]
        public bool? Succeeded { get; private set; } = false;

        /// <summary>
        /// Gets the error object that was raised and unhandled during action execution.
        /// Only included when finished is true and suceeded is false.
        /// </summary>
        [JsonProperty(PropertyName = "error", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ErrorConverter))]
        public Domain.Error Error { get; private set; }

        /// <summary>
        /// Gets the data of the on-error actions raised and ran for this action.
        /// This contains a property named after the aliases of each action within this group of error handling actions.
        /// Each property records the data output of the corresponding action.
        /// </summary>
        [JsonProperty(PropertyName = "onErrorActions", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, ActionData> OnErrorActions { get; private set; }

        /// <summary>
        /// Gets or sets the timestamp of the time when the action did started, represented as a ticks since epoch integer value.
        /// </summary>
        [JsonProperty("startedTicksSinceEpoch")]
        public long? StartedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets the the timestamp of the time when the action did start,
        /// represented as an ISO-8601 formated datetime string with the configured offset.
        /// </summary>
        [JsonProperty(PropertyName = "startedDateTime")]
        public string StartedDateTime { get; private set; }

        /// <summary>
        /// Gets or sets the timestamp of the time when the action did run, represented as a ticks since epoch integer value.
        /// </summary>
        [JsonProperty("ranTicksSinceEpoch")]
        public long? RanTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets the timestamp of the time when the action did run,
        /// represented as an ISO-8601 formated datetime string with the configured offset.
        /// </summary>
        [JsonProperty("ranDateTime")]
        public string RanDateTime { get; private set; }

        /// <summary>
        /// Gets or sets the timestamp of the time when the action did finish, represented as a ticks since epoch integer value.
        /// </summary>
        [JsonProperty("finishedTicksSinceEpoch")]
        public long? FinishedTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets the timestamp of the time when the action did finish,
        /// represented as an ISO-8601 formated datetime string with the configured offset.
        /// </summary>
        [JsonProperty("finishedDateTime")]
        public string FinishedDateTime { get; private set; }

        /// <summary>
        /// Updates the current state of this action data based on the current running action's state.
        /// </summary>
        /// <param name="state">The current action state.</param>
        public void UpdateState(ActionState state)
        {
            this.State = state;
            if (state is ActionState.Started or ActionState.Running or ActionState.Completed)
            {
                var now = this.clock != null
                    ? this.clock.GetCurrentInstant()
                    : SystemClock.Instance.GetCurrentInstant();
                var timeZone = Timezones.AET;
                var offsetDateTime = now.InZone(timeZone).ToOffsetDateTime().WithOffset(timeZone.GetUtcOffset(now));
                string isoDateTimeString = OffsetDateTimePattern.ExtendedIso.Format(offsetDateTime);
                if (state == ActionState.Started)
                {
                    this.Started = true;
                    this.StartedTicksSinceEpoch = now.ToUnixTimeTicks();
                    this.StartedDateTime = isoDateTimeString;
                }

                if (state == ActionState.Running)
                {
                    this.Ran = true;
                    this.RanTicksSinceEpoch = now.ToUnixTimeTicks();
                    this.RanDateTime = isoDateTimeString;
                }

                if (state == ActionState.Completed)
                {
                    this.Finished = true;
                    this.FinishedTicksSinceEpoch = now.ToUnixTimeTicks();
                    this.FinishedDateTime = isoDateTimeString;
                }
            }

            if (this.Finished.GetValueOrDefault() && this.Error == null)
            {
                this.Succeeded = true;
            }
        }

        /// <summary>
        /// Updates this action data with the data of the on error actions raised for this action when
        /// an error was encountered during execution.
        /// </summary>
        /// <param name="onErrorActionData">The action data of the OnErrorAction executed.</param>
        public void AddRaisedOnErrorAction(ActionData onErrorActionData)
        {
            if (this.OnErrorActions == null)
            {
                this.OnErrorActions = new Dictionary<string, ActionData>();
            }

            // If the onErrorAction is already added in the dictionary, we dont add it anymore.
            // This can happen if the onErrorAction has already been executed previously,
            // such as if the a parent or child action threw an error resulting an execution of
            // a set of OnErrorActions (of the parent), and one of the OnErrorActions executed threw an error itself.
            // The parent action will handle the error thrown by reexecuting its set of onErrorActions again.
            // TryAdd (this.OnErrorActions.TryAdd(onErrorActionData.Alias, onErrorActionData)) is used
            // in order to let the original exception to excalate instead of a dictionary exception
            // if Add is used (this.OnErrorActions.Add(onErrorActionData.Alias, onErrorActionData)).
            this.OnErrorActions.TryAdd(onErrorActionData.Alias, onErrorActionData);
        }

        /// <summary>
        /// Updates this action data with the unhandled error encountered during action execution.
        /// This also marks the consequent action as failed.
        /// </summary>
        /// <param name="error">The error raised.</param>
        public void AppendRaisedError(Domain.Error error)
        {
            this.Error = error;
            this.Succeeded = false;
        }

        public void ToggleStatusValuesForAsyncActions()
        {
            this.Asynchronous = true;
            this.Started = true;
            this.Ran = null;
            this.Finished = null;
            this.Succeeded = null;
        }

        public void SetProviders(IServiceProvider service)
        {
            this.clock = service.GetRequiredService<IClock>();
        }
    }
}
