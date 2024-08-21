// <copyright file="DateTimeAfterFilterProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System.Linq.Expressions;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers.Expression;

    public class DateTimeAfterFilterProviderConfigModel : IBuilder<IFilterProvider>
    {
        [JsonProperty]
        public IDataExpressionProviderConfigModel<long> DateTime { get; set; }

        [JsonProperty]
        public IDataExpressionProviderConfigModel<long> IsAfter { get; set; }

        public IFilterProvider Build(IServiceProvider dependencyProvider)
        {
            return new DateTimeComparisonFilterProvider(
                (first, second) => Expression.GreaterThan(first, second),
                this.DateTime.Build(dependencyProvider),
                this.IsAfter.Build(dependencyProvider),
                "dateTimeIsAfterCondition");
        }
    }
}
