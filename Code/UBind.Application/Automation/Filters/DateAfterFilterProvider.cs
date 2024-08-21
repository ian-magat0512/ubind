// <copyright file="DateAfterFilterProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System.Linq.Expressions;
    using NodaTime;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Domain.Extensions;

    public class DateAfterFilterProvider : DateFilterProviderBase, IFilterProvider
    {
        public DateAfterFilterProvider(
            IExpressionProvider dateProvider,
            IProvider<Data<Instant>> isAfterProvider)
            : base(dateProvider, isAfterProvider, "dateIsAfterCondition")
        {
        }

        public async Task<Expression> Resolve(IProviderContext providerContext, ExpressionScope scope = null)
        {
            return await base.Resolve(providerContext, scope);
        }

        protected override Expression GetComparisonExpression(Expression instantExpression, Instant comparerValue)
        {
            var isAfterDateExpression = Expression.Constant(comparerValue.ToUtcAtEndOfDay().ToUnixTimeTicks());
            return Expression.GreaterThan(instantExpression, isAfterDateExpression);
        }

        protected override bool IsValidDateExpression(Expression dateExpression)
        {
            return dateExpression.Type == typeof(LocalDate)
                   || dateExpression.Type == typeof(long);
        }
    }
}
