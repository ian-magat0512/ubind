// <copyright file="AutomationConfigurationParser.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using Newtonsoft.Json;

    /// <summary>
    /// Service for parsing automation configuration json into a model that can be built.
    /// </summary>
    public static class AutomationConfigurationParser
    {
        /// <summary>
        /// Parses a config string and creates an instance of <see cref="AutomationsConfigurationModel"/>.
        /// </summary>
        /// <param name="automationConfigJson">The JSON config.</param>
        /// <returns>The automation config model.</returns>
        public static AutomationsConfigurationModel Parse(string automationConfigJson)
        {
            var converters = AutomationDeserializationConfiguration.ModelSettings;
            var configuration = JsonConvert.DeserializeObject<AutomationsConfigurationModel>(
                automationConfigJson, converters);
            return configuration;
        }
    }
}
