// <copyright file="TimeConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Model converter for objects of type <see cref="TimeConfigModelConverter"/>.
    /// </summary>
    public class TimeConfigModelConverter : DeserializationConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(TimeConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var obj = JObject.Load(reader);
                var listProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("list"));
                if (listProperty != null)
                {
                    JArray listValue = JArray.Load(listProperty.Value.CreateReader());
                    return TimeConfigModel.FromList(listValue.ToObject<List<string>>());
                }

                var rangeProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("range"));
                if (rangeProperty != null)
                {
                    var range = JObject.Load(rangeProperty.Value.CreateReader());
                    var from = range.Properties().FirstOrDefault(c => c.Name.Equals("from")).Value.ToString();
                    var to = range.Properties().FirstOrDefault(c => c.Name.Equals("to")).Value.ToString();
                    return TimeConfigModel.FromRange(from, to);
                }

                var everyProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("every"));
                if (everyProperty != null)
                {
                    return TimeConfigModel.FromEvery(everyProperty.ToObject<int>());
                }
            }

            return null;
        }
    }
}
