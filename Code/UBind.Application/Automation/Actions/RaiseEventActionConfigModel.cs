// <copyright file="RaiseEventActionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.SystemEvents;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Services;

    /// <summary>
    /// Model for raise event action configuration model.
    /// </summary>
    public class RaiseEventActionConfigModel : BaseActionConfigurationModel, IBuilder<Action>
    {
        [JsonConstructor]
        public RaiseEventActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            string customEventAlias,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IEnumerable<ErrorConditionConfigModel> beforeRunErrorConditions,
            IEnumerable<ErrorConditionConfigModel> afterRunErrorConditions,
            IEnumerable<IBuilder<Action>> onErrorActions,
            IBuilder<IObjectProvider> eventData,
            IEnumerable<IBuilder<IProvider<Data<string>>>> tags,
            IEnumerable<IBuilder<IProvider<Data<string>>>> eventTags,
            IBuilder<IProvider<Data<Period>>> eventPersistanceDuration)
            : base(
              name,
              alias,
              description,
              asynchronous,
              runCondition,
              beforeRunErrorConditions,
              afterRunErrorConditions,
              onErrorActions)
        {
            customEventAlias.ThrowIfArgumentNullOrEmpty(nameof(customEventAlias));
            this.CustomEventAlias = customEventAlias;
            this.EventData = eventData;
            this.Tags = eventTags ?? tags ?? Enumerable.Empty<IBuilder<IProvider<Data<string>>>>();
#pragma warning disable CS0618 // Type or member is obsolete
            this.EventTags = eventTags ?? tags ?? Enumerable.Empty<IBuilder<IProvider<Data<string>>>>();
#pragma warning restore CS0618 // Type or member is obsolete
            this.EventPersistanceDuration = eventPersistanceDuration;
        }

        /// <summary>
        /// Gets the alias to be used when creating the event to be raised.
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public string CustomEventAlias { get; }

        /// <summary>
        /// Gets the data object that will be passed to the event. If not configured, everything available to this automation is available in the resulting automations also.
        /// </summary>
        public IBuilder<IObjectProvider> EventData { get; }

        /// <summary>
        /// Gets an optional array of tags that will be attached to the event. These are indexed so that they can be used for querying or counting events retrospectively.
        /// </summary>
        public IEnumerable<IBuilder<IProvider<Data<string>>>> Tags { get; } = Enumerable.Empty<IBuilder<IProvider<Data<string>>>>();

        /// <summary>
        /// Gets an optional array of tags that will be attached to the event. These are indexed so that they can be used for querying or counting events retrospectively.
        /// </summary>
        [Obsolete("Please use Tags instead", false)]
        public IEnumerable<IBuilder<IProvider<Data<string>>>> EventTags { get; } = Enumerable.Empty<IBuilder<IProvider<Data<string>>>>();

        /// <summary>
        /// Gets the duration for which this event will be persisted before it is purged, otherwise this event is not persisted.
        /// </summary>
        public IBuilder<IProvider<Data<Period>>> EventPersistanceDuration { get; }

        /// <inheritdoc/>
        public override Action Build(IServiceProvider dependencyProvider)
        {
            var beforeRunConditions = this.BeforeRunErrorConditions.Select(br => br.Build(dependencyProvider));
            var afterRunConditions = this.AfterRunErrorConditions.Select(ar => ar.Build(dependencyProvider));
            var errorActions = this.OnErrorActions.Select(oa => oa.Build(dependencyProvider));
            var tags = this.Tags.Select(x => x.Build(dependencyProvider));
#pragma warning disable CS0618 // Type or member is obsolete
            var eventTags = this.EventTags.Select(x => x.Build(dependencyProvider));
#pragma warning restore CS0618 // Type or member is obsolete
            if (eventTags.Any())
            {
                tags = tags.Concat(eventTags);
            }

            var clock = dependencyProvider.GetService<IClock>();

            return new RaiseEventAction(
               this.Name,
               this.Alias,
               this.CustomEventAlias,
               this.Description,
               this.Asynchronous,
               this.RunCondition?.Build(dependencyProvider),
               beforeRunConditions,
               afterRunConditions,
               errorActions,
               this.EventData?.Build(dependencyProvider),
               tags,
               this.EventPersistanceDuration?.Build(dependencyProvider),
               dependencyProvider.GetService<ISystemEventService>(),
               dependencyProvider.GetService<IOrganisationService>(),
               dependencyProvider.GetService<ICachingResolver>(),
               clock);
        }
    }
}
