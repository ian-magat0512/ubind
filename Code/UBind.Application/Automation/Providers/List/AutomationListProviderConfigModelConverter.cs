// <copyright file="AutomationListProviderConfigModelConverter.cs" company="uBind">
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
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Export;

    /// <summary>
    /// Model converter for list-providers.
    /// </summary>
    public class AutomationListProviderConfigModelConverter : PropertyDiscriminatorConverter<IBuilder<IDataListProvider<object>>>
    {
        public AutomationListProviderConfigModelConverter(TypeMap typeMap)
            : base(typeMap)
        {
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IBuilder<IDataListProvider<object>>) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                var arrayToken = JArray.Load(reader);
                var list = new List<object>();
                Type valueType = null;
                foreach (var token in arrayToken)
                {
                    var value = this.GetValueBasedOnType(token, objectType, existingValue, serializer);
                    valueType = value?.GetType();
                    list.Add(value);
                }

                // check list item contains dynamic object provider do something else
                if (valueType == typeof(DynamicObjectProviderConfigModel))
                {
                    return new DynamicListBuilder() { Value = new GenericDataList<object>(list) };
                }

                return new StaticListBuilder<object>() { Value = new GenericDataList<object>(list) };
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }

        private object GetValueBasedOnType(JToken token, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var reader = token.CreateReader();
            reader.Read();
            switch (token.Type)
            {
                case JTokenType.Array:
                    if (token.Children<JObject>().Properties().Any(x => x.Name == "propertyName"))
                    {
                        return serializer.Deserialize<IBuilder<IObjectProvider>>(reader);
                    }
                    else
                    {
                        return this.ReadJson(reader, objectType, existingValue, serializer);
                    }

                case JTokenType.Object:
                    return serializer.Deserialize<IBuilder<IProvider<IData>>>(reader);
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Null:
                default:
                    return reader.Value;
            }
        }
    }
}
