// <copyright file="PolicyEntityProviderConfigModelConverter.cs" company="uBind">
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
    /// Converter for policy entity provider expecting an object of type data object.
    /// </summary>
    public class PolicyEntityProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(PolicyEntityProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null)
            {
                var id = new StaticBuilder<Data<string>>() { Value = reader.Value.ToString() };
                return new PolicyEntityProviderConfigModel() { PolicyId = id };
            }
            else
            {
                var policyId = default(IBuilder<IProvider<Data<string>>>);
                var policyNumber = default(IBuilder<IProvider<Data<string>>>);
                var environment = default(IBuilder<IProvider<Data<string>>>);

                var obj = JObject.Load(reader);
                var policyIdProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("policyId"));
                if (policyIdProperty != null)
                {
                    policyId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(policyIdProperty.Value.CreateReader());
                }

                var policyNumberProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("policyNumber"));
                if (policyNumberProperty != null)
                {
                    policyNumber = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(policyNumberProperty.Value.CreateReader());
                }

                var environmentProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("environment"));
                if (environmentProperty != null)
                {
                    environment = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(environmentProperty.Value.CreateReader());
                }

                if (policyId == default && policyNumber == default)
                {
                    policyId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(obj.CreateReader());
                }

                return new PolicyEntityProviderConfigModel() { PolicyId = policyId, PolicyNumber = policyNumber, Environment = environment };
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
