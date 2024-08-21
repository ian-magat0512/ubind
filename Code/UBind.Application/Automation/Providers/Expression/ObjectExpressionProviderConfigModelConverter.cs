// <copyright file="ObjectExpressionProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.Export;

    public class ObjectExpressionProviderConfigModelConverter : PropertyDiscriminatorConverter<IDataExpressionProviderConfigModel<object>>
    {
        public ObjectExpressionProviderConfigModelConverter(TypeMap typeMap)
            : base(typeMap)
        {
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IDataExpressionProviderConfigModel<object>) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var builder = base.ReadJson(reader, objectType, existingValue, serializer);
            if (builder.GetType() == typeof(IBuilder<IObjectProvider>))
            {
                return new ConstantExpressionProviderConfigModel(builder as IBuilder<IProvider<IData>>);
            }

            return builder;
        }
    }
}
