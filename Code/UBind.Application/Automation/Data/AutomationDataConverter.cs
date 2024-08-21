// <copyright file="AutomationDataConverter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Data
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Export;

    /// <summary>
    /// Converts <see cref="AutomationData"/> to/from json.
    /// </summary>
    public class AutomationDataConverter : JsonConverter<AutomationData>
    {
        private readonly TypeMap typeMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationDataConverter"/> class.
        /// </summary>
        /// <param name="typeMap">The map of keys to concrete types.</param>
        public AutomationDataConverter(TypeMap typeMap)
        {
            this.typeMap = typeMap;
        }

        /// <inheritdoc/>
        public override AutomationData ReadJson(
            JsonReader reader,
            Type entityType,
            AutomationData existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            AutomationData automationData = (AutomationData)Activator.CreateInstance(typeof(AutomationData), true);
            serializer.Populate(reader, automationData);
            var keys = automationData.Context.Keys.ToList();
            foreach (var key in keys)
            {
                if (this.typeMap.TryGetValue(key, out Type itemEntityType))
                {
                    if (automationData.Context[key] is JToken jsonContext)
                    {
                        automationData.Context[key] = jsonContext.ToObject(itemEntityType);
                    }
                }
                else
                {
                    throw new NotSupportedException(
                        "When trying to deserialise the AutomationData context entities, we came across a key that doesn't exist in our map of keys to types: " + key);
                }
            }

            return automationData;
        }

        public override void WriteJson(JsonWriter writer, AutomationData value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
