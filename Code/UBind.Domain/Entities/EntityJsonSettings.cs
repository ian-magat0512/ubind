// <copyright file="EntityJsonSettings.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Entities
{
    using System;
    using System.Linq.Expressions;
    using CSharpFunctionalExtensions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain.Models;

    /// <summary>
    /// Entity JSON settings.
    /// </summary>
    public class EntityJsonSettings : UBind.Domain.Entity<Guid>
    {
        /// <summary>
        /// Gets an expression mapping private property <see cref="EntityJsonSettings.SerializedSettings"/> requiring persistence for EF.
        /// </summary>
        public static readonly Expression<Func<EntityJsonSettings, string>> SettingsExpression =
            entitysetting => entitysetting.SerializedSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityJsonSettings"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant Id that the product belongs to.</param>
        /// <param name="entityType">The entity type.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="creationTime">The time this setting has been created.</param>
        private EntityJsonSettings(
            Guid tenantId, EntityType entityType, Guid entityId, JObject configuration, Instant creationTime)
            : base(Guid.NewGuid(), creationTime)
        {
            this.TenantId = tenantId;
            this.EntityType = entityType;
            this.EntityId = entityId;
            this.Settings = configuration;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityJsonSettings"/> class.
        /// </summary>
        /// <remarks>Parameterless constructor for EF.</remarks>
        private EntityJsonSettings()
            : base(default, default)
        {
        }

        /// <summary>
        /// Gets the tenant Id.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the entity type.
        /// </summary>
        public EntityType EntityType { get; private set; }

        /// <summary>
        /// Gets the entity Id.
        /// </summary>
        public Guid EntityId { get; private set; }

        /// <summary>
        /// Gets or sets the entity settings.
        /// </summary>
        private JObject Settings
        {
            get
            {
                return JsonConvert.DeserializeObject<JObject>(string.IsNullOrEmpty(this.SerializedSettings) ? "{}" : this.SerializedSettings);
            }

            set
            {
                this.SerializedSettings = JsonConvert.SerializeObject(value, Formatting.None);
            }
        }

        /// <summary>
        /// Gets or sets the serialized settings.
        /// </summary>
        private string SerializedSettings { get; set; }

        /// <summary>
        /// Create <see cref="EntityJsonSettings"/> object.
        /// </summary>
        /// <param name="tenantId">The tenant Id that the product belongs to.</param>
        /// <param name="entityType">The entity type.</param>
        /// <param name="entityId">The entity id.</param>
        /// <param name="configuration">A configuration object.</param>
        /// <param name="creationTime">The time this setting has been created.</param>
        /// <returns><see cref="EntityJsonSettings"/> object.</returns>
        public static Result<EntityJsonSettings> Create(
            Guid tenantId, EntityType entityType, Guid entityId, IEntitySettings configuration, Instant creationTime)
        {
            var result = new EntityJsonSettings(
                   tenantId, entityType, entityId, JObject.FromObject(configuration), creationTime);

            return Result.Success(result);
        }

        /// <summary>
        /// Update settings.
        /// </summary>
        /// <param name="setting">The setting object.</param>
        public void UpdateSettings(IEntitySettings setting)
        {
            this.Settings = JObject.FromObject(setting);
        }

        /// <summary>
        /// Gets the typed configuration.
        /// </summary>
        public T GetSettings<T>()
            where T : IEntitySettings
        {
            return this.Settings.ToObject<T>();
        }
    }
}
