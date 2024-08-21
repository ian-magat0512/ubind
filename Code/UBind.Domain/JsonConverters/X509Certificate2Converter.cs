// <copyright file="X509Certificate2Converter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.JsonConverters
{
    using System.Security.Cryptography.X509Certificates;
    using Newtonsoft.Json;

    /// <summary>
    /// Ensures that X509Certificate2 is serialized as a base64 string.
    /// To use this converter, decorate your X509Certificate2 property with the following attribute:
    ///     [JsonConverter(typeof(X509Certificate2Converter))]
    /// .
    /// </summary>
    public class X509Certificate2Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(X509Certificate2);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var certificate = (X509Certificate2)value;
            var rawData = certificate.Export(X509ContentType.Cert);
            var base64String = Convert.ToBase64String(rawData);

            serializer.Serialize(writer, base64String);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var base64String = serializer.Deserialize<string>(reader);
            var rawData = Convert.FromBase64String(base64String);
            var certificate = new X509Certificate2(rawData);

            return certificate;
        }
    }
}
