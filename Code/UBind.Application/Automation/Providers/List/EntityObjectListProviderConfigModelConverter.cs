// <copyright file="EntityObjectListProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class is needed because EntityObjectListEntityProvider has custom schema for entity list and its related entities.
    /// </summary>
    public class EntityObjectListProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(EntityObjectListProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var includeRelatedEntities = new List<string>();
            var entityListProvider = default(IBuilder<IDataListProvider<object>>);

            var obj = JObject.Load(reader);
            var entityListProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("entityList"));
            if (entityListProperty != null)
            {
                entityListProvider = serializer.Deserialize<IBuilder<IDataListProvider<object>>>(entityListProperty.Value.CreateReader());
            }

            var includeRelatedEntitiesProperty = obj.Properties()
                .FirstOrDefault(c => c.Name.Equals("includeOptionalProperties"));
            if (includeRelatedEntitiesProperty != null)
            {
                includeRelatedEntities = serializer.Deserialize<List<string>>(includeRelatedEntitiesProperty.Value.CreateReader());
            }

            if (entityListProvider == default)
            {
                entityListProvider = serializer.Deserialize<IBuilder<IDataListProvider<object>>>(obj.CreateReader());
            }

            var entityObjectConfigModel = new EntityObjectListProviderConfigModel()
            {
                EntityListProvider = entityListProvider,
                IncludeOptionalProperties = includeRelatedEntities,
            };
            return entityObjectConfigModel;
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
