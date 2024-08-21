// <copyright file="RaiseEventAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.SystemEvents;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services;
    using Void = UBind.Domain.Helpers.Void;

    /// <summary>
    /// Represents an action of type RaiseEventAction.
    /// </summary>
    public class RaiseEventAction : Action
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ISystemEventService systemEventService;
        private readonly IOrganisationService organisationService;
        private readonly IClock clock;
        private Instant timestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="RaiseEventAction"/> class.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <param name="alias">The alias of the action.</param>
        /// <param name="customEventAlias">The custom event alias of the action.</param>
        /// <param name="description">The action description.</param>
        /// <param name="asynchronous">The asynchronous to be used.</param>
        /// <param name="runCondition">An optional condition.</param>
        /// <param name="beforeRunErrorConditions">The validation rules before the action.</param>
        /// <param name="afterRunErrorConditions">The validation rules after the action.</param>
        /// <param name="onErrorActions">The list of non successful actions.</param>
        /// <param name="eventDataProvider">The eventData provider.</param>
        /// <param name="tagProviders">The event tags provider.</param>
        /// <param name="eventDurationPersistenceProvider">The duration provider.</param>
        /// <param name="systemEventService">The system event service.</param>
        /// <param name="organisationService">The service for fetching default organisation from tenancy.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="clock">The clock for telling time.</param>
        public RaiseEventAction(
            string name,
            string alias,
            string customEventAlias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>> runCondition,
            IEnumerable<ErrorCondition> beforeRunErrorConditions,
            IEnumerable<ErrorCondition> afterRunErrorConditions,
            IEnumerable<Action> onErrorActions,
            IObjectProvider eventDataProvider,
            IEnumerable<IProvider<Data<string>>> tagProviders,
            IProvider<Data<Period>>? eventDurationPersistenceProvider,
            ISystemEventService systemEventService,
            IOrganisationService organisationService,
            ICachingResolver cachingResolver,
            IClock clock)
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
            this.cachingResolver = cachingResolver;
            this.CustomEventAlias = customEventAlias;
            this.EventDataProvider = eventDataProvider;
            this.TagProviders = tagProviders ?? Enumerable.Empty<IProvider<Data<string>>>();
            this.EventPersistenceDurationProvider = eventDurationPersistenceProvider;
            this.systemEventService = systemEventService;
            this.organisationService = organisationService;
            this.timestamp = clock.GetCurrentInstant();
            this.clock = clock;
        }

        /// <summary>
        /// Gets the type of action this represents.
        /// </summary>
        public ActionType Type => ActionType.RaiseEventAction;

        /// <summary>
        /// Gets the custom event alias.
        /// </summary>
        public string CustomEventAlias { get; }

        /// <summary>
        /// Gets the event data that the action will result in.
        /// </summary>
        public IObjectProvider EventDataProvider { get; }

        /// <summary>
        /// Gets the event tags.
        /// </summary>
        public IEnumerable<IProvider<Data<string>>> TagProviders { get; }

        /// <summary>
        /// Gets the provider which resolves how long the event should be persisted for.
        /// </summary>
        public IProvider<Data<Period>>? EventPersistenceDurationProvider { get; }

        /// <inheritdoc/>
        public override ActionData CreateActionData() => new RaiseEventActionData(this.Name, this.Alias, this.clock);

        /// <summary>
        /// Fires an event request.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <param name="actionData">The action data to be updated.</param>
        /// <returns>An awaitable task.</returns>
        public override async Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal = false)
        {
            using (MiniProfiler.Current.Step(nameof(RaiseEventAction) + "." + nameof(this.Execute)))
            {
                actionData.UpdateState(ActionState.Running);
                var resolveEventTags = await this.TagProviders
                    .SelectAsync(async x => (await x.Resolve(providerContext)).GetValueOrThrowIfFailed());
                var tags = resolveEventTags.Where(x => !string.IsNullOrEmpty(x)).Select(x => x.DataValue)
                    .ToList();
                RaiseEventActionData raiseEventActionData = (RaiseEventActionData)actionData;
                raiseEventActionData.Tags = tags;
#pragma warning disable CS0618 // Type or member is obsolete
                raiseEventActionData.EventTags = tags;
#pragma warning restore CS0618 // Type or member is obsolete
                SystemEvent systemEvent
                    = await this.GetSystemEvent(providerContext, actionData);
                systemEvent.SetTags(this.CreateUserDefinedTags(tags, systemEvent));

                // if no expiry timestamp is given for this custom event, we won't persist it.
                // It is still raised and so it can be used in other automation event triggers.
                var persist = systemEvent.ExpiryTimestamp != null;
                if (persist)
                {
                    await this.systemEventService.PersistAndEmit(new List<SystemEvent> { systemEvent });
                }
                else
                {
                    await this.systemEventService.Emit(systemEvent);
                }

                return Result.Success<Void, Domain.Error>(default);
            }
        }

        public override bool IsReadOnly() => false;

        /// <summary>
        /// Retrieves the system event.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <param name="actionData">The action data.</param>
        /// <returns>The system event.</returns>
        private async Task<SystemEvent> GetSystemEvent(IProviderContext providerContext, ActionData actionData)
        {
            var data = providerContext.AutomationData;
            Guid tenantId = data.ContextManager.Tenant.Id;
            Guid productId = data.ContextManager.Product.Id;
            DeploymentEnvironment environment = data.System.Environment;
            Guid organisationId = data.ContextManager.Organisation.Id;
            if (organisationId == default)
            {
                organisationId = this.organisationService.GetDefaultOrganisationForTenant(tenantId).Id;
            }
            RaiseEventActionData raiseEventActionData = (RaiseEventActionData)actionData;
            var performingUserId = data.ContextManager.PerformingUser?.Id ?? default;
            Period? period = (await this.EventPersistenceDurationProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (period != null)
            {
                raiseEventActionData.Period = period;
            }

            var eventData = (await this.EventDataProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
            string? eventDataStr = null;
            if (eventData != null)
            {
                eventDataStr = JsonConvert.SerializeObject(eventData);
                raiseEventActionData.EventData = eventData;
            }

            Instant? expiryTimestamp = (period != null) ? this.timestamp.AddPeriodInAet(period) : (Instant?)null;
            SystemEvent systemEvent = SystemEvent.CreateCustom(
                tenantId,
                organisationId,
                productId,
                environment,
                this.CustomEventAlias,
                eventDataStr,
                this.timestamp,
                expiryTimestamp);
            if (performingUserId != default)
            {
                systemEvent.AddRelationshipToEntity(
                   RelationshipType.EventPerformingUser, EntityType.User, performingUserId);
            }

            return systemEvent;
        }

        private List<Tag> CreateUserDefinedTags(IEnumerable<string> tags, SystemEvent systemEvent)
        {
            return tags.Select(t => new Tag(
                EntityType.Event, systemEvent.Id, TagType.UserDefined, t, systemEvent.CreatedTimestamp)).ToList();
        }
    }
}
