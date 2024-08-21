﻿// <copyright file="ProductEntityProviderConfigModelConverter.cs" company="uBind">
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
    /// Converter for product entity provider expecting an object of type data object.
    /// </summary>
    public class ProductEntityProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(ProductEntityProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null)
            {
                var id = new StaticBuilder<Data<string>>() { Value = reader.Value.ToString() };
                return new ProductEntityProviderConfigModel() { ProductId = id };
            }
            else
            {
                var productId = default(IBuilder<IProvider<Data<string>>>);
                var productAlias = default(IBuilder<IProvider<Data<string>>>);

                var obj = JObject.Load(reader);
                var productIdProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("productId"));
                if (productIdProperty != null)
                {
                    productId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(productIdProperty.Value.CreateReader());
                }

                var productAliasProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("productAlias"));
                if (productAliasProperty != null)
                {
                    productAlias = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(productAliasProperty.Value.CreateReader());
                }

                if (productId == default && productAlias == default)
                {
                    productId = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(obj.CreateReader());
                }

                return new ProductEntityProviderConfigModel() { ProductId = productId, ProductAlias = productAlias };
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
