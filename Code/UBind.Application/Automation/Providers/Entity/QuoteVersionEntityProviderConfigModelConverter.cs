// <copyright file="QuoteVersionEntityProviderConfigModelConverter.cs" company="uBind">
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
    /// Converter for quote version entity provider expecting an object of type data object.
    /// </summary>
    public class QuoteVersionEntityProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(QuoteVersionEntityProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            var quoteVersionId = default(IBuilder<IProvider<Data<string>>>);
            if (value != null)
            {
                quoteVersionId = new StaticBuilder<Data<string>>() { Value = reader.Value.ToString() };
                return new QuoteVersionEntityProviderConfigModel() { QuoteVersionId = quoteVersionId };
            }
            else
            {
                var quoteId = default(IBuilder<IProvider<Data<string>>>);
                var quoteReference = default(IBuilder<IProvider<Data<string>>>);
                var quoteVersionNumber = default(IBuilder<IProvider<Data<long>>>);
                var environment = default(IBuilder<IProvider<Data<string>>>);

                var obj = JObject.Load(reader);
                var quoteVersionIdProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("quoteVersionId"));
                if (quoteVersionIdProperty != null)
                {
                    quoteVersionId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(quoteVersionIdProperty.Value.CreateReader());
                }

                var quoteIdProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("quoteId"));
                if (quoteIdProperty != null)
                {
                    quoteId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(quoteIdProperty.Value.CreateReader());
                }

                var quoteReferenceProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("quoteReference"));
                if (quoteReferenceProperty != null)
                {
                    quoteReference = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(quoteReferenceProperty.Value.CreateReader());
                }

                var quoteVersionProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("versionNumber"));
                if (quoteVersionProperty != null)
                {
                    if (quoteVersionProperty.Value.Type == JTokenType.Integer)
                    {
                        quoteVersionNumber = new StaticBuilder<Data<long>>() { Value = (long)quoteVersionProperty.Value };
                    }
                    else
                    {
                        quoteVersionNumber = serializer.Deserialize<IBuilder<IProvider<Data<long>>>>(quoteVersionProperty.Value.CreateReader());
                    }
                }

                var environmentProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("environment"));
                if (environmentProperty != null)
                {
                    environment = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(environmentProperty.Value.CreateReader());
                }

                if (quoteVersionId == default && quoteId == default && quoteReference == default)
                {
                    quoteId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(obj.CreateReader());
                }

                return new QuoteVersionEntityProviderConfigModel()
                {
                    QuoteVersionId = quoteVersionId,
                    QuoteId = quoteId,
                    QuoteReference = quoteReference,
                    VersionNumber = quoteVersionNumber,
                    Environment = environment,
                };
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
