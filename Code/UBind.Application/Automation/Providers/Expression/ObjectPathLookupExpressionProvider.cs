// <copyright file="ObjectPathLookupExpressionProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.Json.Pointer;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Entities;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.PathLookup;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Provider for obtaining an expression for data from an object path.
    /// </summary>
    /// <typeparam name="TData">The type of the data.</typeparam>
    public class ObjectPathLookupExpressionProvider<TData> : IExpressionProvider
    {
        private readonly IProvider<Data<string>> pathProvider;
        private readonly IProvider<IData> defaultValueProvider;
        private readonly IObjectProvider objectProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectPathLookupExpressionProvider{TData}"/> class.
        /// </summary>
        /// <param name="pathProvider">A provider for obtaining the path.</param>
        /// <param name="objectProvider">A provider for the object to look in. If null, the automation data context will be used.</param>
        public ObjectPathLookupExpressionProvider(
            IProvider<Data<string>> pathProvider,
            IProvider<IData> defaultValueProvider = null,
            IObjectProvider objectProvider = null)
        {
            this.pathProvider = pathProvider;
            this.defaultValueProvider = defaultValueProvider;
            this.objectProvider = objectProvider;
        }

        public virtual string SchemaReferenceKey => "objectPathLookupExpression";

        /// <inheritdoc/>
        public async Task<Expression> Invoke(IProviderContext providerContext, ExpressionScope scope)
        {
            string path = (await this.pathProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            providerContext.CancellationToken.ThrowIfCancellationRequested();

            string[] parts;
            if (PathHelper.IsJsonPointer(path))
            {
                var jPointer = new JsonPointer(path.TrimStart('#'));
                parts = jPointer.ReferenceTokens.ToArray();
            }
            else
            {
                parts = path.TrimStart('#').Split(".".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            }

            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            if (path.Length < 1)
            {
                errorData.Add("path", path);
                throw new ErrorException(
                    Errors.Automation.PathSyntaxError(
                        "Path should not be empty", this.SchemaReferenceKey, errorData));
            }

            object value = null;
            try
            {
                if (this.objectProvider != null)
                {
                    if (path.StartsWith("#"))
                    {
                        throw new ErrorException(
                        Errors.Automation.PathSyntaxError(
                                "Path segment should not use a scope if a dataObject is present", this.SchemaReferenceKey, errorData));
                    }

                    var dataObject = (await this.objectProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                    IProviderResult<IData> result = PocoPathLookupResolver.Resolve(
                        dataObject,
                        path,
                        "objectPathLookupExpression",
                        errorData);
                    value = result.GetValueOrThrowIfFailed().GetValueFromGeneric();
                }
                else if (path.StartsWith("#"))
                {
                    var expression = this.ResolveFromScope(parts, scope, path, errorData);
                    var genericType = this.GetType().GetGenericArguments()[0];
                    if (genericType.IsEquivalentTo(typeof(string)) && expression.Type != typeof(string))
                    {
                        expression = Expression.Call(
                            Expression.Convert(expression, typeof(object)), typeof(object).GetMethod("ToString"));
                    }

                    // include expression that will use default value when returned value is null.
                    if (this.defaultValueProvider != null)
                    {
                        var defaultValue = (await this.defaultValueProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
                        return this.WrapExpressionToUseDefaultValueIfExpressionResolvesToNull(expression, defaultValue);
                    }

                    return expression;
                }
                else
                {
                    var result = PocoPathLookupResolver.Resolve(providerContext.AutomationData, path, this.SchemaReferenceKey, errorData)
                        .GetValueOrThrowIfFailed();
                    value = result.GetValueFromGeneric();
                }
            }
            catch (Exception e) when (e is NullReferenceException || e is ErrorException)
            {
                if (this.defaultValueProvider != null)
                {
                    value = (await this.defaultValueProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().GetValueFromGeneric();
                }

                if (value == null)
                {
                    throw;
                }
            }

            return Expression.Constant(value?.ToString());
        }

        private Expression ResolveFromScope(IEnumerable<string> pathSegments, ExpressionScope scope, string path, JObject debugContext)
        {
            var itemAlias = pathSegments.FirstOrDefault();
            var itemParameter = itemAlias == "item" || string.IsNullOrWhiteSpace(itemAlias)
                ? scope.CurrentParameter
                : scope.GetParameterExpression(itemAlias, this.SchemaReferenceKey, debugContext);

            if (!typeof(IEntityReadModelWithRelatedEntities).IsAssignableFrom(itemParameter.Type))
            {
                pathSegments = pathSegments.Skip(1);
            }
            else
            {
                var entityName = pathSegments.FirstOrDefault();
                var propertyName = pathSegments.Last();
                var equivalentColumn = ColumnMapping.GetEquivalentDatabaseColumn(entityName, propertyName);
                pathSegments = string.IsNullOrWhiteSpace(equivalentColumn) ? pathSegments : equivalentColumn.Split('.', StringSplitOptions.RemoveEmptyEntries);
            }

            Expression expression = itemParameter;
            bool expressionIsObject = expression.Type == typeof(object);
            var paths = new List<string>();
            foreach (var part in pathSegments)
            {
                var pascalizedPart = part.Pascalize();
                if (part == pascalizedPart)
                {
                    bool isInt = int.TryParse(part, out int arrayIndex);
                    if (!isInt)
                    {
                        debugContext.Add("path", path);
                        debugContext.Add("segment", part);
                        throw new ErrorException(
                            Errors.Automation.PathSyntaxError(
                                "Path segment should use camel case", this.SchemaReferenceKey, debugContext));
                    }
                }

                paths.Add(expressionIsObject ? part : pascalizedPart);
            }

            var fullPath = expressionIsObject ? string.Join("/", paths) : string.Join(".", paths);
            try
            {
                expression = ExpressionWrapper.GetProperty(expression, fullPath, this.SchemaReferenceKey, debugContext);
            }
            catch (ArgumentException ex)
            {
                debugContext.Add("path", path);
                debugContext.Add("segment", fullPath);
                throw new ErrorException(
                    Errors.Automation.PathResolutionError(this.SchemaReferenceKey, debugContext), ex);
            }

            return DatabasePropertyTranslator.Translate(expression);
        }

        private Expression WrapExpressionToUseDefaultValueIfExpressionResolvesToNull(Expression expression, IData defaultValue)
        {
            var methodCallExpression = expression as MethodCallExpression;
            if (defaultValue.GetInnerType() == typeof(Instant)
                && (expression.Type == typeof(long) || expression.Type == typeof(long?)))
            {
                // we need to convert this here as Instant values are stored as long in db
                // and defaultValue will be used instead of entity property
                var instant = defaultValue.GetValueFromGeneric();
                defaultValue = new Data<long>(instant.ToUnixTimeTicks());
            }

            var defaultValueExpression = Expression.Constant(defaultValue.GetValueFromGeneric());
            var expressionType = defaultValueExpression.Type;
            if (expressionType.IsValueType && Nullable.GetUnderlyingType(expressionType) == null)
            {
                // convert primitive typed expression to its nullable format prior to coalesce
                expression = Expression.Convert(expression, typeof(Nullable<>).MakeGenericType(expressionType));
            }
            else if (expression.Type == typeof(object) && defaultValueExpression.Type != expression.Type)
            {
                expression = Expression.Convert(expression, defaultValueExpression.Type);
            }

            var useDefaultWhenNullExpr = expressionType.IsValueType
                ? Expression.Coalesce(expression, defaultValueExpression)
                : Expression.Condition(
                    Expression.Equal(
                        expression,
                        Expression.Constant(null)),
                    defaultValueExpression,
                    expression);

            if (methodCallExpression != null && this.UsesPocoResolver(methodCallExpression))
            {
                return Expression.TryCatch(
                    useDefaultWhenNullExpr,
                    Expression.Catch(
                        typeof(ErrorException),
                        defaultValueExpression));
            }
            else
            {
                return useDefaultWhenNullExpr;
            }
        }

        /// <summary>
        /// Returns true if the expression-tree includes an Expression.Call to PocoPathLookupResolver.Resolve.
        /// </summary>
        /// <param name="expression">The expression to be evaluated.</param>
        /// <returns>True if the expression-tree satisfies the condition, otherwise false.</returns>
        private bool UsesPocoResolver(MethodCallExpression expression)
        {
            var resolverMethodInfo = typeof(PocoPathLookupResolver).GetMethod(nameof(PocoPathLookupResolver.Resolve));
            Expression currentExpression = expression;
            while (true)
            {
                if (currentExpression is MethodCallExpression methodCallExpr)
                {
                    if (methodCallExpr.Method == resolverMethodInfo)
                    {
                        return true;
                    }
                    else if (methodCallExpr.Arguments.Any())
                    {
                        currentExpression = methodCallExpr.Arguments[0];
                    }
                    else if (methodCallExpr.Object is MethodCallExpression)
                    {
                        currentExpression = methodCallExpr.Object;
                    }
                    else if (methodCallExpr.Object is UnaryExpression unary
                        && unary.NodeType == ExpressionType.Convert
                        && unary.Operand is MethodCallExpression)
                    {
                        currentExpression = unary.Operand;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (currentExpression is MemberExpression memberExpr)
                {
                    var member = memberExpr.Member;
                    if (member is MethodInfo methodInfo)
                    {
                        if (methodInfo == resolverMethodInfo)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        currentExpression = memberExpr.Expression;
                    }
                }
                else if (currentExpression is UnaryExpression unary
                        && unary.NodeType == ExpressionType.Convert
                        && unary.Operand is MethodCallExpression)
                {
                    currentExpression = unary.Operand;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
