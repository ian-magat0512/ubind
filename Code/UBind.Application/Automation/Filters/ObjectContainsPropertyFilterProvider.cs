// <copyright file="ObjectContainsPropertyFilterProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System.Linq.Expressions;
    using System.Reflection;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    public class ObjectContainsPropertyFilterProvider : IFilterProvider
    {
        private readonly IExpressionProvider objectExpressionProvider;
        private readonly IExpressionProvider propertyNameExpressionProvider;

        public ObjectContainsPropertyFilterProvider(
            IExpressionProvider objectExpression,
            IExpressionProvider propertyNameExpression)
        {
            this.objectExpressionProvider = objectExpression;
            this.propertyNameExpressionProvider = propertyNameExpression;
        }

        public string SchemaReferenceKey => "objectContainsPropertyCondition";

        public async Task<Expression> Resolve(IProviderContext providerContext, ExpressionScope scope = null)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            var dataObject = await this.objectExpressionProvider.Invoke(providerContext, scope);
            var propertyName = await this.propertyNameExpressionProvider.Invoke(providerContext, scope);

            // check if dataObject is a primitive and if true - throw an exception
            MethodInfo isPrimitiveMethod = typeof(DataObjectHelper)
                .GetMethod(nameof(DataObjectHelper.IsPrimitive));
            var isPrimitiveExpression = Expression.Call(isPrimitiveMethod, dataObject);
            var exceptionExpression = Expression.New(
                typeof(ErrorException)
                .GetConstructor(new[] { typeof(Error) }), Expression.Constant(Errors.Automation.ParameterValueTypeInvalid(this.SchemaReferenceKey, "object")));

            // if dataObject is not primitive, execute the method
            var tryGetValueMethodInfo = typeof(ObjectContainsPropertyFilterProvider)
                .GetMethod(nameof(this.TryGetWrapper), BindingFlags.NonPublic | BindingFlags.Instance);
            var methodCallExpression = Expression.Call(
                Expression.Constant(this),
                tryGetValueMethodInfo,
                dataObject,
                propertyName);

            // define the return values for the if-statement
            Expression trueBranch = Expression.Block(Expression.Throw(exceptionExpression), Expression.Constant(false));

            var ifThenElse = Expression.Condition(
                isPrimitiveExpression,
                trueBranch,
                methodCallExpression);
            return Expression.Lambda(ifThenElse, scope?.CurrentParameter);
        }

        private bool TryGetWrapper(object dataObject, string name)
        {
            object _;
            return DataObjectHelper.TryGetPropertyValue(dataObject, name, out _);
        }
    }
}
