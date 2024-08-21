// <copyright file="CreditCardConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.JsonConverters
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Json converter for hiding all but last 4 digits of a credit card number.
    /// </summary>
    public class CreditCardConverter : JsonConverter
    {
        private const int NumberOfDigitsToExpose = 4;
        private const char MaskCharacter = '*';

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(string).IsAssignableFrom(objectType);
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return (string)reader.Value;
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var cardNumber = (string)value;
            var length = cardNumber.Length;
            var obfuscatedCardNumber = length >= NumberOfDigitsToExpose
                ? string.Concat(new string(MaskCharacter, length - NumberOfDigitsToExpose), cardNumber.AsSpan(length - NumberOfDigitsToExpose))
                : cardNumber;
            var jToken = JToken.FromObject(obfuscatedCardNumber);
            jToken.WriteTo(writer);
        }
    }
}
