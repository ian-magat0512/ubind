// <copyright file="Entity.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using Newtonsoft.Json;
    using NodaTime;

    public class Entity<TKey> : IEntity<TKey>
    {
        /// <summary>
        /// Initializes the static properties.
        /// </summary>
        static Entity()
        {
            SupportsAdditionalProperties = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity{TKey}"/> class.
        /// </summary>
        /// <param name="id">A unique identifier for the entity.</param>
        /// <param name="createdTimestamp">The current time.</param>
        public Entity(TKey id, Instant createdTimestamp)
        {
            this.Id = id;
            this.CreatedTimestamp = createdTimestamp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity{TKey}"/> class.
        /// </summary>
        protected Entity()
        {
        }

        public static bool SupportsAdditionalProperties { get; protected set; }

        /// <inheritdoc/>
        [JsonProperty]
        public TKey Id { get; protected set; }

        /// <inheritdoc/>
        public Instant CreatedTimestamp
        {
            get { return Instant.FromUnixTimeTicks(this.CreatedTicksSinceEpoch); }
            protected set { this.CreatedTicksSinceEpoch = value.ToUnixTimeTicks(); }
        }

        [JsonProperty]
        public long CreatedTicksSinceEpoch { get; protected set; }
    }
}
