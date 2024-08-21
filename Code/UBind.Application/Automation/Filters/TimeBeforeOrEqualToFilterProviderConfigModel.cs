// <copyright file="TimeBeforeOrEqualToFilterProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System.Linq.Expressions;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Providers.Expression;

    public class TimeBeforeOrEqualToFilterProviderConfigModel : IBuilder<IFilterProvider>
    {
        [JsonProperty]
        public IDataExpressionProviderConfigModel<LocalTime> Time { get; set; }

        [JsonProperty]
        public IDataExpressionProviderConfigModel<LocalTime> IsBeforeOrEqualTo { get; set; }

        public IFilterProvider Build(IServiceProvider dependencyProvider)
        {
            return new TimeComparisonFilterProvider(
                (first, second) => Expression.LessThanOrEqual(first, second),
                this.Time.Build(dependencyProvider),
                this.IsBeforeOrEqualTo.Build(dependencyProvider),
                "timeIsBeforeOrEqualToCondition");
        }
    }
}
