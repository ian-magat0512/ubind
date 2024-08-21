// <copyright file="ObjectPathLookupConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.PathLookup
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Converter for deserializing object path lookup objects from json.
    /// </summary>
    /// <remarks>
    /// Objects with only text path are interpreted as FixedObjectPathLookup.
    /// </remarks>
    public class ObjectPathLookupConfigModelConverter : DeserializationConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPathLookupConfigModelConverter"/> class.
        /// </summary>
        public ObjectPathLookupConfigModelConverter()
        {
        }

        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return typeof(IBuilder<IObjectPathLookupProvider>) == objectType;
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null)
            {
                var textProviderModelConfig = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(reader);
                var fixedPathLookupModel = new FixedObjectPathLookupConfigModel { Path = textProviderModelConfig };
                return fixedPathLookupModel;
            }
            else
            {
                JObject obj = JObject.Load(reader);
                var properties = obj.Properties().Select(na => na.Name).ToList();
                if (!obj.Properties().Any())
                {
                    var erringConfig = JsonConvert.SerializeObject(obj);
                    JObject errorData = new JObject
                    {
                        { "failingAutomationConfiguration", erringConfig },
                        { ErrorDataKey.ErrorMessage, "objectPathLookup are expected to have at least the path property when configured as an object" },
                    };
                    throw new ErrorException(Errors.Automation.InvalidAutomationConfiguration(errorData));
                }
                else if (obj.Properties().Count() > 1)
                {
                    var objectPathLookupModel = new ObjectPathLookupProviderConfigModel();
                    serializer.Populate(obj.CreateReader(), objectPathLookupModel);
                    return objectPathLookupModel;
                }
                else
                {
                    var property = obj.Properties().First().Name.Equals("path") ? obj.Properties().First().Value : obj;
                    var fixedPathModel = serializer.Deserialize<IBuilder<IProvider<Data<string>>>>(property.CreateReader());
                    return new FixedObjectPathLookupConfigModel { Path = fixedPathModel };
                }
            }
        }
    }
}
