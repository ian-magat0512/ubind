// <copyright file="LogicOperationConditionConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Converter for <see cref="AndConditionConfigModel"/> and <see cref="OrConditionConfigModel"/>.
    /// </summary>
    public class LogicOperationConditionConfigModelConverter : DeserializationConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(AndConditionConfigModel) || objectType == typeof(OrConditionConfigModel))
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            IList<IBuilder<IProvider<Data<bool>>>> conditions = new List<IBuilder<IProvider<Data<bool>>>>();
            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
            {
                if (reader.TokenType != JsonToken.Comment)
                {
                    var v = serializer.Deserialize<IBuilder<IProvider<Data<bool>>>>(reader);
                    conditions.Add(v);
                }
            }

            if (objectType == typeof(AndConditionConfigModel))
            {
                return new AndConditionConfigModel() { Conditions = conditions };
            }
            else
            {
                return new OrConditionConfigModel() { Conditions = conditions };
            }
        }
    }
}
