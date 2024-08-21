// <copyright file="InvitationEvent.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.User
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates;

    /// <summary>
    /// Aggregate for users.
    /// </summary>
    public partial class UserAggregate
    {
        /// <summary>
        /// Base class for invitation events.
        /// </summary>
        /// <typeparam name="TEvent">The type of the derived event.</typeparam>
        /// <typeparam name="TEventObserver">The type of observer that can handle these user events via double dispatch.</typeparam>
        public abstract class InvitationEvent<TEvent, TEventObserver> : Event<UserAggregate, Guid>
            where TEvent : InvitationEvent<TEvent, TEventObserver>
            where TEventObserver : Domain.IAggregateEventObserver<UserAggregate, TEvent>, Domain.IAggregateEventObserver<UserAggregate, IEvent<UserAggregate, Guid>>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="InvitationEvent{TEvent, TEventObserver}"/> class.
            /// </summary>
            /// <param name="userId">The ID of the user aggregate the event belongs to.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public InvitationEvent(Guid tenantId, Guid userId, Guid? performingUserId, Instant createdTimestamp)
                : base(tenantId, userId, performingUserId, createdTimestamp)
            {
                this.InvitationId = Guid.NewGuid();
            }

            [JsonConstructor]
            private InvitationEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the ID of the invitation.
            /// </summary>
            [JsonProperty]
            public Guid InvitationId { get; private set; }
        }
    }
}
