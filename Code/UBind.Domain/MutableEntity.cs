// <copyright file="MutableEntity.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using Newtonsoft.Json;
    using NodaTime;

    /// <inheritdoc/>
    public class MutableEntity<TKey> : Entity<TKey>, IMutableEntity<TKey>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MutableEntity{TKey}"/> class.
        /// </summary>
        /// <param name="id">A unique identifier for the entity.</param>
        /// <param name="createdTimestamp">The current time.</param>
        public MutableEntity(TKey id, Instant createdTimestamp)
            : base(id, createdTimestamp)
        {
            this.LastModifiedTimestamp = createdTimestamp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MutableEntity{TKey}"/> class.
        /// </summary>
        protected MutableEntity()
            : base()
        {
        }

        public Instant LastModifiedTimestamp
        {
            get
            {
                return Instant.FromUnixTimeTicks(this.LastModifiedTicksSinceEpoch);
            }

            set
            {
                this.LastModifiedTicksSinceEpoch = value.ToUnixTimeTicks();
            }
        }

        [JsonProperty]
        public long LastModifiedTicksSinceEpoch { get; set; }
    }
}
