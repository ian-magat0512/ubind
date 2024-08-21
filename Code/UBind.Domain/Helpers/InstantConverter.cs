// <copyright file="InstantConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System.Globalization;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Text;

    /// <summary>
    /// This class is used to convert value from previous serialized json nodatime version.
    /// previous nodatime version used format like an object and the current version used string format.
    /// </summary>
    public class InstantConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override bool CanRead => true;

        public static Instant ParseToInstantWithLocale(string timestamp, DateTimeZone timeZone, CultureInfo cultureInfo)
        {
            string pattern = "MM/dd/yyyy HH:mm:ss";
            var localDateTimePattern = LocalDateTimePattern.Create(pattern, cultureInfo);
            LocalDateTime localDateTime = localDateTimePattern.Parse(timestamp).Value;
            ZonedDateTime zonedDateTime = localDateTime.InZoneLeniently(timeZone);
            Instant instant = zonedDateTime.ToInstant();
            return instant;
        }

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
                    var cultureInfo = CultureInfo.GetCultureInfo(Locales.en_AU);
                    var parseResult = InstantPattern.Create("yyyy-MM-ddTHH:mm:ss'Z'", cultureInfo).Parse(value);
                    return parseResult.Success ? parseResult.Value : value;
                case JsonToken.StartObject:
                    return this.ReadObject(reader);
                case JsonToken.Date:
                    var datetime = (DateTime)reader.Value;

                    // The reason why dates are changing is because the timestamps that were stored are the set local date time
                    // that was converted to UTC. So when we convert it back to instant, we shouldn't convert it into certain
                    // timezone, but rather just convert it back to UTC.
                    var instant = Instant.FromDateTimeUtc(datetime);
                    return instant;

                default:
                    throw new JsonSerializationException("Could not deserialize value property. "
                        + $"Unexpected JsonToken {reader.TokenType}.");
            }
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Instant)
                || objectType == typeof(Instant?);
        }

        protected object ReadObject(JsonReader reader)
        {
            var dateObject = new JsonSerializer().Deserialize<Dictionary<string, object>>(reader);
            long days = Convert.ToInt64(dateObject["days"]?.ToString());
            long nanoOfDay = Convert.ToInt64(dateObject["nanoOfDay"]?.ToString());
            Instant fromDays = Instant.FromUnixTimeTicks(days * NodaConstants.TicksPerDay);
            Duration fromNanoseconds = Duration.FromNanoseconds(nanoOfDay);
            Instant result = fromDays.Plus(fromNanoseconds);
            return result;
        }
    }
}
