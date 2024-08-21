// <copyright file="AutomationConditionConfigModelConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Conditions
{
    using System;
    using Newtonsoft.Json;
    using UBind.Application.Export;

    /// <summary>
    /// Converter for deserializing automation condition models from json.
    /// </summary>
    /// <remarks>
    /// Value fields are interpreted as fixed automation conditions.
    /// </remarks>
    public class AutomationConditionConfigModelConverter : PropertyDiscriminatorConverter<IBuilder<IProvider<Data<bool>>>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationConditionConfigModelConverter"/> class.
        /// </summary>
        /// <param name="typeMap">A map for the property field values to concrete child types.</param>
        public AutomationConditionConfigModelConverter(TypeMap typeMap)
            : base(typeMap)
        {
        }

        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;
            if (value != null && bool.TryParse(value.ToString(), out bool result))
            {
                return new StaticBuilder<Data<bool>> { Value = result };
            }

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}
