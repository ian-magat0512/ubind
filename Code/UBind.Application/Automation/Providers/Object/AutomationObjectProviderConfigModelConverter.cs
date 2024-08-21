// <copyright file="AutomationObjectProviderConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Object
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Object;
    using UBind.Application.Export;

    /// <summary>
    /// Converter for deserializing object provider models from automation configuration json.
    /// </summary>
    /// <remarks>
    /// Objects with an anonymous key-value pattern are interpreted as dynamic automation object providers.
    /// </remarks>
    public class AutomationObjectProviderConfigModelConverter : PropertyDiscriminatorConverter<IBuilder<IObjectProvider>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationObjectProviderConfigModelConverter"/> class.
        /// </summary>
        /// <param name="typeMap">A map for the property field values to concrete object types.</param>
        public AutomationObjectProviderConfigModelConverter(TypeMap typeMap)
            : base(typeMap)
        {
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var arrayToken = JToken.Load(reader);
            if (arrayToken.Type == JTokenType.Array
                && (arrayToken.Children<JObject>().Properties().Any(x => x.Name == "propertyName")
                || !arrayToken.Children().Any()))
            {
                IList<ObjectPropertyConfigModel> propertyList = new List<ObjectPropertyConfigModel>();
                propertyList = arrayToken.Children<JObject>().Select(item => serializer.Deserialize<ObjectPropertyConfigModel>(item.CreateReader())).ToList();

                DynamicObjectProviderConfigModel objectProviderModel = new DynamicObjectProviderConfigModel
                {
                    Properties = propertyList,
                };
                return objectProviderModel;
            }

            return base.ReadJson(arrayToken.CreateReader(), objectType, existingValue, serializer);
        }
    }
}
