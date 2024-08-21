// <copyright file="DateFilterProviderBase.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Persistence.Extensions;

    public abstract class DateFilterProviderBase : IFilterProvider
    {
        private readonly IExpressionProvider firstProvider;
        private readonly IProvider<Data<Instant>> secondProvider;

        public DateFilterProviderBase(IExpressionProvider first, IProvider<Data<Instant>> second, string schemaReferenceKey)
        {
            this.firstProvider = first;
            this.secondProvider = second;
            this.SchemaReferenceKey = schemaReferenceKey;
        }

        public virtual string SchemaReferenceKey { get; }

        public async Task<Expression> Resolve(IProviderContext providerContext, ExpressionScope scope = null)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            var firstExpression = await this.firstProvider.Invoke(providerContext, scope);
            if (!this.IsValidDateExpression(firstExpression))
            {
                throw new ErrorException(
                    Errors.Automation.ParameterValueTypeInvalid(
                        this.SchemaReferenceKey,
                        "date"));
            }

            var instantExpression = firstExpression.Type != typeof(long)
                ? firstExpression.ToTicksAtMidnight()
                : firstExpression;
            var secondValue = (await this.secondProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            var comparisonExpression = this.GetComparisonExpression(instantExpression, secondValue);
            return Expression.Lambda(comparisonExpression, scope?.CurrentParameter);
        }

        protected abstract bool IsValidDateExpression(Expression dateExpression);

        protected abstract Expression GetComparisonExpression(Expression instantExpression, Instant comparerValue);
    }
}
