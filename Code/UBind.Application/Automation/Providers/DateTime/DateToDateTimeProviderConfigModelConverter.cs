// <copyright file="DateToDateTimeProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.DateTime
{
    using Newtonsoft.Json;

    public class DateToDateTimeProviderConfigModelConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(DateToDateTimeProviderConfigModel) == objectType;
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var readerValue = reader.Value;
            if (readerValue != null)
            {
                var dateOrDateTimeStringProvider = new StaticBuilder<Data<string>>() { Value = readerValue.ToString() };
                return new DateToDateTimeProviderConfigModel() { InputInString = dateOrDateTimeStringProvider };
            }

            var dateOrDateTimeInProvider = serializer.Deserialize<IBuilder<IProvider<IData>>>(reader);
            return new DateToDateTimeProviderConfigModel() { InputInProvider = dateOrDateTimeInProvider };
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
