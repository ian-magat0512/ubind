// <copyright file="NotConditionConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    ///  Converter for deserializing the not condition for condition provider objects from json.
    /// </summary>
    /// <remarks>
    /// Return the Not condition configuration model.
    /// </remarks>
    public class NotConditionConfigModelConverter : DeserializationConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NotConditionConfigModel);
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            IBuilder<IProvider<Data<bool>>> conditionBuilder = null;
            if (reader != null)
            {
                conditionBuilder = serializer.Deserialize<IBuilder<IProvider<Data<bool>>>>(reader);
            }

            return new NotConditionConfigModel() { Condition = conditionBuilder };
        }
    }
}
