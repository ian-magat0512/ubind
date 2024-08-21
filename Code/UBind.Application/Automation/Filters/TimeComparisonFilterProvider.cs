// <copyright file="TimeComparisonFilterProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    public class TimeComparisonFilterProvider : IFilterProvider
    {
        private readonly Func<Expression, Expression, Expression> comparisonExpressionFactory;
        private readonly IExpressionProvider firstProvider;
        private readonly IExpressionProvider secondProvider;

        public TimeComparisonFilterProvider(
            Func<Expression, Expression, Expression> comparisonExpressionFactory,
            IExpressionProvider firstProvider,
            IExpressionProvider secondProvider,
            string schemaReferenceKey)
        {
            this.firstProvider = firstProvider;
            this.secondProvider = secondProvider;
            this.comparisonExpressionFactory = comparisonExpressionFactory;
            this.SchemaReferenceKey = schemaReferenceKey;
        }

        public string SchemaReferenceKey { get; }

        public async Task<Expression> Resolve(IProviderContext providerContext, ExpressionScope scope = null)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            var firstExpression = await this.firstProvider.Invoke(providerContext, scope);
            if (firstExpression.Type != typeof(object) && firstExpression.Type != typeof(LocalTime))
            {
                throw new ErrorException(Errors.Automation.ParameterValueTypeInvalid(this.SchemaReferenceKey, "time"));
            }

            var secondExpression = await this.secondProvider.Invoke(providerContext, scope);
            var comparisonExpression = this.comparisonExpressionFactory(firstExpression, secondExpression);
            return Expression.Lambda(comparisonExpression, scope?.CurrentParameter);
        }
    }
}
