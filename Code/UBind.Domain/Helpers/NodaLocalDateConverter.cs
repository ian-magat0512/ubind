// <copyright file="NodaLocalDateConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Text;

    /// <summary>
    /// This class is used to convert local value from previous serialized json nodatime version.
    /// previous nodatime version used format like an object and the current version used string format.
    /// </summary>
    public class NodaLocalDateConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override bool CanRead => true;

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.None:
                case JsonToken.Null:
                    return null;
                case JsonToken.String:
                    var value = (string)reader.Value;
                    var result = LocalDatePattern.Iso.Parse(value);
                    return result.Success ? result.Value : value;
                case JsonToken.StartObject:
                    return this.ReadObject(reader);
                default:
                    throw new JsonSerializationException("Could not deserialize local value property. "
                        + $"Unexpected JsonToken {reader.TokenType}.");
            }
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LocalDate)
                || objectType == typeof(LocalDate?);
        }

        private object ReadObject(JsonReader reader)
        {
            var dateObject = new JsonSerializer().Deserialize<Dictionary<string, object>>(reader);
            int year = Convert.ToInt32(dateObject["year"]);
            int month = Convert.ToInt32(dateObject["month"]);
            int day = Convert.ToInt32(dateObject["day"]);
            var convertedLocalDate = new LocalDate(year, month, day);
            return convertedLocalDate;
        }
    }
}
