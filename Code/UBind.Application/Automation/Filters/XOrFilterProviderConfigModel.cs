// <copyright file="XOrFilterProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters;

using Newtonsoft.Json;

public class XOrFilterProviderConfigModel : IBuilder<IFilterProvider>
{
    [JsonProperty("firstCondition")]
    public IBuilder<IFilterProvider> FirstCondition { get; set; }

    [JsonProperty("secondCondition")]
    public IBuilder<IFilterProvider> SecondCondition { get; set; }

    public IFilterProvider Build(IServiceProvider dependencyProvider)
    {
        var filterProviders = new List<IFilterProvider>()
        {
            this.FirstCondition.Build(dependencyProvider),
            this.SecondCondition.Build(dependencyProvider),
        };

        return new XOrFilterProvider(filterProviders);
    }
}
