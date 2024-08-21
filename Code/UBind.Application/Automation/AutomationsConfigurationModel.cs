// <copyright file="AutomationsConfigurationModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Configuration for automations.
    /// </summary>
    public class AutomationsConfigurationModel : IBuilder<AutomationsConfiguration>
    {
        /// <summary>
        /// Gets or sets the schema version control number for the configuration.
        /// </summary>
        [JsonProperty(PropertyName = "schemaVersion")]
        public string SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the configuration for automations.
        /// </summary>
        [JsonProperty(PropertyName = "automations")]
        public IEnumerable<AutomationConfigModel> AutomationModels { get; set; } = Enumerable.Empty<AutomationConfigModel>();

        /// <summary>
        /// Instantiates an instance of the <see cref="AutomationsConfiguration"/> class from the model.
        /// </summary>
        /// <param name="dependencyProvider">Container of dependency providers required for defining the configuration objects.</param>
        /// <returns>An instance of <see cref="AutomationsConfiguration"/>.</returns>
        public AutomationsConfiguration Build(IServiceProvider dependencyProvider)
        {
            var config = new AutomationsConfiguration(
                this.AutomationModels.Select(model => model.Build(dependencyProvider)));
            return config;
        }
    }
}
