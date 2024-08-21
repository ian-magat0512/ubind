// <copyright file="NotFilterProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System.Linq.Expressions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    public class NotFilterProvider : IFilterProvider
    {
        private readonly IFilterProvider filterProvider;

        public NotFilterProvider(IFilterProvider filterProvider)
        {
            this.filterProvider = filterProvider;
        }

        public string SchemaReferenceKey => "notCondition";

        public async Task<Expression> Resolve(IProviderContext providerContext, ExpressionScope scope)
        {
            var originalResult = await this.filterProvider.Resolve(providerContext, scope);
            if (originalResult is LambdaExpression lambdaExpression)
            {
                var notExpression = Expression.Not(lambdaExpression.Body);
                var lambda = Expression.Lambda(notExpression, scope.CurrentParameter);
                return (Expression)lambda;
            }

            throw new ErrorException(Errors.Automation.ParameterValueTypeInvalid(
                this.SchemaReferenceKey,
                "#condition",
                reasonWhyValueIsInvalidIfAvailable: $"The {this.SchemaReferenceKey} requires the \"condition\" to resolve to a valid condition. "));
        }
    }
}
