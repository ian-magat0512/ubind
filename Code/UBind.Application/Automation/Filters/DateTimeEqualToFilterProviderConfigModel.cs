// <copyright file="DateTimeEqualToFilterProviderConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System.Linq.Expressions;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers.Expression;

    public class DateTimeEqualToFilterProviderConfigModel : IBuilder<IFilterProvider>
    {
        [JsonProperty]
        public IDataExpressionProviderConfigModel<long> DateTime { get; set; }

        [JsonProperty]
        public IDataExpressionProviderConfigModel<long> IsEqualTo { get; set; }

        public IFilterProvider Build(IServiceProvider dependencyProvider)
        {
            // datetime comparison precision set to minute. Equality check ensures that dateTime >= comparand and dateTime < comparand  + 1 minute.
            return new DateTimeComparisonFilterProvider(
                (first, second) => Expression.And(
                    Expression.GreaterThanOrEqual(first, second),
                    Expression.LessThan(first, Expression.Add(second, Expression.Constant(TimeSpan.TicksPerMinute)))),
                this.DateTime.Build(dependencyProvider),
                this.IsEqualTo.Build(dependencyProvider),
                "dateTimeIsEqualToCondition");
        }
    }
}
