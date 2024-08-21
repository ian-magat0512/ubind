// <copyright file="DynamicEntityProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Entity
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Providers;

    /// <summary>
    /// This class is needed because DynamicEntityProvider has custom schema for type of entity and the Id of the entity.
    /// </summary>
    public class DynamicEntityProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(DynamicEntityProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var entityId = default(IBuilder<IProvider<Data<string>>>);
            var entityType = default(IBuilder<IProvider<Data<string>>>);

            var obj = JObject.Load(reader);
            var entityTypeProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("entityType"));
            if (entityTypeProperty != null)
            {
                entityType = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(entityTypeProperty.Value.CreateReader());
            }

            var entityIdProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("entityId"));
            if (entityIdProperty != null)
            {
                entityId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(entityIdProperty.Value.CreateReader());
            }

            return new DynamicEntityProviderConfigModel() { EntityType = entityType, EntityId = entityId };
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
