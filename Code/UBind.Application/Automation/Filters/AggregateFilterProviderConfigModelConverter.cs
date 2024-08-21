// <copyright file="AggregateFilterProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Converter for <see cref="AndFilterProviderConfigModel"/>.
    /// </summary>
    public class AggregateFilterProviderConfigModelConverter : DeserializationConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType) =>
            typeof(AggregateFilterProviderConfigModel).IsAssignableFrom(objectType);

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            IList<IBuilder<IFilterProvider>> filterProviderModels = new List<IBuilder<IFilterProvider>>();
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndArray:
                        break;
                    default:
                        var filterProviderModel = serializer.Deserialize<IBuilder<IFilterProvider>>(reader);
                        filterProviderModels.Add(filterProviderModel);
                        break;
                }
            }

            if (objectType == typeof(AndFilterProviderConfigModel))
            {
                return new AndFilterProviderConfigModel() { Conditions = filterProviderModels };
            }
            else if (objectType == typeof(OrFilterProviderConfigModel))
            {
                return new OrFilterProviderConfigModel() { Conditions = filterProviderModels };
            }

            throw new NotSupportedException($"Unsupported object type: {objectType}.");
        }
    }
}
