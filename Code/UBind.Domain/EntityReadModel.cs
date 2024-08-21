// <copyright file="EntityReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using NodaTime;

    /// <summary>
    /// Base class for read model entities.
    /// </summary>
    public abstract class EntityReadModel<TKey> : IEntityReadModel<TKey>
    {
        /// <summary>
        /// Initialized static properties.
        /// </summary>
        static EntityReadModel()
        {
            SupportsAdditionalProperties = false;
        }

        protected EntityReadModel(Guid tenantId, TKey id, Instant createdTimestamp)
        {
            this.Id = id;
            this.TenantId = tenantId;
            this.CreatedTimestamp = createdTimestamp;
            this.LastModifiedTimestamp = createdTimestamp;
        }

        protected EntityReadModel()
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether the entity supports additional properties.
        /// </summary>
        public static bool SupportsAdditionalProperties { get; protected set; }

        /// <summary>
        /// Gets or sets the unique identifier of entity.
        /// </summary>
        [Key]
        public TKey Id { get; set; }

        /// <summary>
        /// Gets or sets the tenant Id.
        /// </summary>
        public Guid TenantId { get; set; }

        public Instant CreatedTimestamp
        {
            get
            {
                return Instant.FromUnixTimeTicks(this.CreatedTicksSinceEpoch);
            }

            set
            {
                this.CreatedTicksSinceEpoch = value.ToUnixTimeTicks();
            }
        }

        public long CreatedTicksSinceEpoch { get; set; }

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

        public long LastModifiedTicksSinceEpoch { get; set; }
    }
}
