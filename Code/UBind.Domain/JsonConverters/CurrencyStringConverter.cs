// <copyright file="CurrencyStringConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.JsonConverters
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Json converter for converting a currency string to a decimal.
    /// </summary>
    public class CurrencyStringConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var currencyString = reader.Value.ToString();
            var result = CurrencyParser.TryParseToDecimalWithResult(currencyString);
            if (result.IsFailure)
            {
                throw new ErrorException(result.Error);
            }

            return result.Value;
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // We no longer want to output the dollar sign as we need to be able to support
            // multiple currencies. We can't assume this is dollars and cents.
            var jToken = JToken.FromObject(value);
            jToken.WriteTo(writer);
        }
    }
}
