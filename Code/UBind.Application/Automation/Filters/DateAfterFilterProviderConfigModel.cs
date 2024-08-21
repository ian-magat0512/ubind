// <copyright file="DateAfterFilterProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Providers.DateTime;
    using UBind.Application.Automation.Providers.Expression;

    public class DateAfterFilterProviderConfigModel : IBuilder<IFilterProvider>
    {
        [JsonProperty]
        public IDataExpressionProviderConfigModel<LocalDate> Date { get; private set; }

        [JsonProperty]
        public DateToDateTimeProviderConfigModel IsAfter { get; private set; }

        public IFilterProvider Build(IServiceProvider dependencyProvider)
        {
            return new DateAfterFilterProvider(
                this.Date.Build(dependencyProvider),
                this.IsAfter.Build(dependencyProvider));
        }
    }
}
