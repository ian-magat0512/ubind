// <copyright file="AutomationGenericObjectProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Export;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Model converter for generic-typed providers.
    /// </summary>
    public class AutomationGenericObjectProviderConfigModelConverter : PropertyDiscriminatorConverter<IBuilder<IProvider<IData>>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationGenericObjectProviderConfigModelConverter"/> class.
        /// </summary>
        /// <param name="typeMap">The typemap to use.</param>
        public AutomationGenericObjectProviderConfigModelConverter(TypeMap typeMap)
            : base(typeMap)
        {
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(IBuilder<IProvider<IData>>) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Boolean:
                    return serializer.Deserialize<IBuilder<IProvider<Data<bool>>>>(reader);
                case JsonToken.Integer:
                    return serializer.Deserialize<IBuilder<IProvider<Data<long>>>>(reader);
                case JsonToken.Float:
                    return serializer.Deserialize<IBuilder<IProvider<Data<decimal>>>>(reader);
                case JsonToken.String:
                    return serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(reader);
                case JsonToken.StartArray:
                    var token = JToken.Load(reader);
                    if (token.Children<JObject>().Properties().Any(x => x.Name == "propertyName"))
                    {
                        return serializer.Deserialize<IBuilder<IObjectProvider>>(token.CreateReader());
                    }
                    else
                    {
                        return serializer.Deserialize<IBuilder<IDataListProvider<object>>>(token.CreateReader());
                    }

                case JsonToken.StartObject:
                    return base.ReadJson(reader, objectType, existingValue, serializer);
                case JsonToken.Null:
                    return reader.Value;
            }

            var errorData = new JObject()
            {
                { ErrorDataKey.ErrorMessage,  $"[#value] cannot be converted from type {reader.TokenType}." },
            };
            throw new ErrorException(Errors.Automation.InvalidAutomationConfiguration(errorData));
        }
    }
}
