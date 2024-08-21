// <copyright file="DateExpressionProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Providers.Date;
    using UBind.Application.Export;

    public class DateExpressionProviderConfigModelConverter : PropertyDiscriminatorConverter<IDataExpressionProviderConfigModel<LocalDate>>
    {
        public DateExpressionProviderConfigModelConverter(TypeMap typeMap)
            : base(typeMap)
        {
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IDataExpressionProviderConfigModel<LocalDate>) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null)
            {
                var textToDateBuilder = new TextToDateProviderConfigModel() { TextToDate = new StaticBuilder<Data<string>> { Value = value.ToString() } };
                return new DateExpressionProviderConfigModel { Date = textToDateBuilder };
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
