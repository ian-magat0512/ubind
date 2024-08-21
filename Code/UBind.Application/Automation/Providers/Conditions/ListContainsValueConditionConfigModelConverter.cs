// <copyright file="ListContainsValueConditionConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Providers.List;

    /// <summary>
    ///  Converter for deserializing list of object for condition provider objects from json.
    /// </summary>
    /// <remarks>
    /// List of object.
    /// </remarks>
    public class ListContainsValueConditionConfigModelConverter : DeserializationConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(ListContainsValueConditionConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var list = jObject.SelectToken("list");
            var value = jObject.SelectToken("value");
            IBuilder<IDataListProvider<object>> objectList = serializer.Deserialize<IBuilder<IDataListProvider<object>>>(list.CreateReader());
            IBuilder<IProvider<IData>> valueProvider = serializer.Deserialize<IBuilder<IProvider<IData>>>(value.CreateReader());

            return new ListContainsValueConditionConfigModel
            {
                List = objectList,
                Value = valueProvider,
            };
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
