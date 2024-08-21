// <copyright file="ExpressionProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Export;

    /// <summary>
    /// Converter for parsing expression provider config models.
    /// </summary>
    public class ExpressionProviderConfigModelConverter : PropertyDiscriminatorConverter<IBuilder<IExpressionProvider>>
    {
        private TypeMap fallbackTypeMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionProviderConfigModelConverter"/> class.
        /// </summary>
        /// <param name="typeMap">A map of keys to config model types for expression providers.</param>
        /// <param name="fallbackTypeMap">A map of keys to regular providers to use when expression provider not found.</param>
        public ExpressionProviderConfigModelConverter(TypeMap typeMap, TypeMap fallbackTypeMap)
            : base(typeMap)
        {
            this.fallbackTypeMap = fallbackTypeMap;
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(IBuilder<IExpressionProvider>) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartArray:
                    var array = JArray.Load(reader);
                    return new LiteralConstantExpressionProviderConfigModel(this.CreateListExpression(array));
                case JsonToken.Integer:
                    return new LiteralConstantExpressionProviderConfigModel(Expression.Constant((long)reader.Value));
                case JsonToken.Float:
                    return new LiteralConstantExpressionProviderConfigModel(Expression.Constant(decimal.Parse(reader.Value.ToString())));
                case JsonToken.String:
                    return new LiteralConstantExpressionProviderConfigModel(Expression.Constant(reader.Value.ToString()));
                case JsonToken.Boolean:
                    return new LiteralConstantExpressionProviderConfigModel(Expression.Constant((bool)reader.Value));
                default:
                    break;
            }

            var jObject = JObject.Load(reader);
            var key = jObject?.Properties()?.FirstOrDefault()?.Name.ToString();
            Type targetType;
            if (this.TypeMap.TryGetValue(key, out targetType))
            {
                var token = jObject.SelectToken(key);
                return serializer.Deserialize(token.CreateReader(), targetType);
            }

            if (this.fallbackTypeMap.TryGetValue(key, out targetType))
            {
                var token = jObject.SelectToken(key);
                var providerConfigModel = serializer.Deserialize(token.CreateReader(), targetType) as IBuilder<IProvider<IData>>;
                return new ConstantExpressionProviderConfigModel(providerConfigModel);
            }

            throw new NotSupportedException($"Cannot use provider {key} in expression-based provider.");
        }

        private ConstantExpression CreateListExpression(JArray array)
        {
            var firstItem = array.Children().FirstOrDefault();
            if (firstItem == null)
            {
                throw new NotSupportedException("Empty literal lists are not supported.");
            }

            switch (firstItem.Type)
            {
                case JTokenType.Integer:
                    return Expression.Constant(array.ToObject<List<int>>());
                case JTokenType.Float:
                    return Expression.Constant(array.ToObject<List<decimal>>());
                case JTokenType.String:
                    return Expression.Constant(array.ToObject<List<string>>());
                case JTokenType.Boolean:
                    return Expression.Constant(array.ToObject<List<bool>>());
                default:
                    break;
            }

            throw new NotSupportedException($"Literal lists of type {firstItem.Type} are not supported.");
        }
    }
}
