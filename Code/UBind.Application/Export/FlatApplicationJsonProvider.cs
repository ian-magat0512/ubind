// <copyright file="FlatApplicationJsonProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using UBind.Application.FileHandling.Template_Provider;
    using UBind.Domain;

    /// <summary>
    /// For converting a JSON string from a text provider to a URL encoded string.
    /// </summary>
    public class FlatApplicationJsonProvider : ITextProvider
    {
        private readonly IEnumerable<IJObjectProvider> jsonProviders;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlatApplicationJsonProvider"/> class.
        /// </summary>
        /// <param name="jsonProviders">Providers for creating json from application.</param>
        public FlatApplicationJsonProvider(IEnumerable<IJObjectProvider> jsonProviders)
        {
            this.jsonProviders = jsonProviders;
        }

        /// <inheritdoc/>
        public async Task<string> Invoke(ApplicationEvent applicationEvent)
        {
            var json = new JObject();
            foreach (var provider in this.jsonProviders)
            {
                await provider.CreateJsonObject(applicationEvent);
                json.Merge(provider.JsonObject);
            }

            return json.ToString();
        }
    }
}
