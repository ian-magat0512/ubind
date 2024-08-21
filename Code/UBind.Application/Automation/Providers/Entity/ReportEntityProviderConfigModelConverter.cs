// <copyright file="ReportEntityProviderConfigModelConverter.cs" company="uBind">
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
    /// Converter for report entity provider expecting an object of type data object.
    /// </summary>
    public class ReportEntityProviderConfigModelConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(ReportEntityProviderConfigModel) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null)
            {
                var id = new StaticBuilder<Data<string>>() { Value = reader.Value.ToString() };
                return new ReportEntityProviderConfigModel() { ReportId = id };
            }
            else
            {
                var obj = JObject.Load(reader);
                var reportIdProperty = obj.Properties().FirstOrDefault(c => c.Name.Equals("reportId"));

                var reportId = reportIdProperty != null ?
                    serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(reportIdProperty.Value.CreateReader()) :
                    default(IBuilder<IProvider<Data<string>>>);

                return new ReportEntityProviderConfigModel() { ReportId = reportId };
            }
        }

        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
