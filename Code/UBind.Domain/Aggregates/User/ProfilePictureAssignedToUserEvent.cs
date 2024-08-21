// <copyright file="ProfilePictureAssignedToUserEvent.cs" company="uBind">
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
        /// Event raised when the profile picture for the user is updated.
        /// </summary>
        public class ProfilePictureAssignedToUserEvent : Event<UserAggregate, Guid>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ProfilePictureAssignedToUserEvent"/> class.
            /// </summary>
            /// <param name="userId">A unique ID for the user.</param>
            /// <param name="performingUserId">The performing userId.</param>
            /// <param name="profilePictureId">The profile picture id to be assigned to the user.</param>
            /// <param name="createdTimestamp">The time the event was created.</param>
            public ProfilePictureAssignedToUserEvent(Guid tenantId, Guid userId, Guid? performingUserId, Guid profilePictureId, Instant createdTimestamp)
                : base(tenantId, userId, performingUserId, createdTimestamp)
            {
                this.ProfilePictureId = profilePictureId;
            }

            [JsonConstructor]
            private ProfilePictureAssignedToUserEvent()
                : base(default, default(Guid), default(Guid), default(Instant))
            {
            }

            /// <summary>
            /// Gets the user's profile picture id.
            /// </summary>
            [JsonProperty]
            public Guid ProfilePictureId { get; private set; }
        }
    }
}
