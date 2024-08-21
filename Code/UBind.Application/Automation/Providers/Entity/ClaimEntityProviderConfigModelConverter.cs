// <copyright file="ClaimEntityProviderConfigModelConverter.cs" company="uBind">
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
    /// Converter for claim entity provider expecting an object of type data object.
    /// </summary>
    public class ClaimEntityProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(ClaimEntityProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null)
            {
                var id = new StaticBuilder<Data<string>>() { Value = reader.Value.ToString() };
                return new ClaimEntityProviderConfigModel() { ClaimId = id };
            }
            else
            {
                var claimId = default(IBuilder<IProvider<Data<string>>>);
                var claimReference = default(IBuilder<IProvider<Data<string>>>);
                var claimNumber = default(IBuilder<IProvider<Data<string>>>);
                var environment = default(IBuilder<IProvider<Data<string>>>);

                var obj = JObject.Load(reader);
                var claimIdProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("claimId"));
                if (claimIdProperty != null)
                {
                    claimId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(claimIdProperty.Value.CreateReader());
                }

                var claimReferenceProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("claimReference"));
                if (claimReferenceProperty != null)
                {
                    claimReference = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(claimReferenceProperty.Value.CreateReader());
                }

                var claimNumberProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("claimNumber"));
                if (claimNumberProperty != null)
                {
                    claimNumber = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(claimNumberProperty.Value.CreateReader());
                }

                var environmentProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("environment"));
                if (environmentProperty != null)
                {
                    environment = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(environmentProperty.Value.CreateReader());
                }

                if (claimId == default && claimReference == default && claimNumber == default)
                {
                    claimId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(obj.CreateReader());
                }

                return new ClaimEntityProviderConfigModel() { ClaimId = claimId, ClaimReference = claimReference, ClaimNumber = claimNumber, Environment = environment };
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
