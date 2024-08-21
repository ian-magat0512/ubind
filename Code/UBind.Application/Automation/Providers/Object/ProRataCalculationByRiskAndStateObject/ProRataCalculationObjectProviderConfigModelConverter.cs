// <copyright file="ProRataCalculationObjectProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class is needed because ProRataCalculationObjectProvider has custom schema for entity and its related entities.
    /// </summary>
    public class ProRataCalculationObjectProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(EntityObjectProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var policyTransactionId = default(IBuilder<IProvider<Data<string>>>);
            var optionalProperties = new List<string>();

            var obj = JObject.Load(reader);
            var policyTransactionIdProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("policyTransactionId"));
            if (policyTransactionIdProperty != null)
            {
                policyTransactionId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(
                    policyTransactionIdProperty.Value.CreateReader());
            }

            return new ProRataCalculationObjectProviderConfigModel() { PolicyTransactionId = policyTransactionId };
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
