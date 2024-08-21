// <copyright file="NodaLocalDateTimeConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System.Globalization;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Extensions;
    using NodaTime.Text;

    /// <summary>
    /// This class is used to convert local value from previous serialized json nodatime version.
    /// previous nodatime version used format like an object and the current version used string format.
    /// </summary>
    public class NodaLocalDateTimeConverter : JsonConverter
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
                    var result = this.ParseToDateTime(value);
                    return result.Success ? result.Value : value;
                case JsonToken.StartObject:
                    return this.ReadObject(reader);
                case JsonToken.Date:
                    var dateStringValue = (DateTime)reader.Value;
                    return dateStringValue.ToLocalDateTime();
                default:
                    throw new JsonSerializationException("Could not deserialize local date time property. "
                        + $"Unexpected JsonToken {reader.TokenType}.");
            }
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LocalDateTime)
                || objectType == typeof(LocalDateTime?);
        }

        private ParseResult<LocalDateTime> ParseToDateTime(string dateTimeInString)
        {
            var cultureInfo2 = CultureInfo.GetCultureInfo(Locales.en_AU);
            var result = LocalDateTimePattern.Create("MM/dd/yyyy HH:mm:ss", cultureInfo2).Parse(dateTimeInString);
            if (result.Success)
            {
                return result;
            }
            else
            {
                result = LocalDateTimePattern.Create("yyyy-MM-dd'T'HH:mm:ss", cultureInfo2).Parse(dateTimeInString);
                return result;
            }
        }

        private object ReadObject(JsonReader reader)
        {
            var dateObject = new JsonSerializer().Deserialize<Dictionary<string, object>>(reader);
            int year = Convert.ToInt32(dateObject["year"]);
            int month = Convert.ToInt32(dateObject["month"]);
            int day = Convert.ToInt32(dateObject["day"]);
            int hours = 0;
            int minutes = 0;
            int seconds = 0;
            int nanoseconds = 0;
            if (dateObject.ContainsKey("nanoOfDay"))
            {
                long nanoOfDay = Convert.ToInt64(dateObject["nanoOfDay"]);
                hours = (int)(nanoOfDay / 3_600_000_000_000L);
                minutes = (int)((nanoOfDay % 3_600_000_000_000L) / 60_000_000_000L);
                seconds = (int)((nanoOfDay % 60_000_000_000L) / 1_000_000_000L);
                nanoseconds = (int)(nanoOfDay % 1_000_000_000L);
            }

            var convertedLocalDate = new LocalDateTime(year, month, day, hours, minutes, seconds).PlusNanoseconds(nanoseconds);
            return convertedLocalDate;
        }
    }
}
