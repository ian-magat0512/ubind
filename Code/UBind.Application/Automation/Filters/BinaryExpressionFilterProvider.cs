// <copyright file="BinaryExpressionFilterProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// For providing filters based on binary expressions.
    /// </summary>
    public class BinaryExpressionFilterProvider : IFilterProvider
    {
        private readonly Func<Expression, Expression, Expression> binaryExpressionFactory;
        private readonly IExpressionProvider firstOperandExpressionProvider;
        private readonly IExpressionProvider secondOperandExpressionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryExpressionFilterProvider"/> class.
        /// </summary>
        /// <param name="binaryExpressionFactory">A factory for creating a binary expression.</param>
        /// <param name="firstOperandExpressionProvider">A provider for the first operand.</param>
        /// <param name="secondOperandExpressionProvider">A provider for the second operand.</param>
        public BinaryExpressionFilterProvider(
            Func<Expression, Expression, Expression> binaryExpressionFactory,
            IExpressionProvider firstOperandExpressionProvider,
            IExpressionProvider secondOperandExpressionProvider,
            string schemaReferenceKey)
        {
            this.binaryExpressionFactory = binaryExpressionFactory;
            this.firstOperandExpressionProvider = firstOperandExpressionProvider;
            this.secondOperandExpressionProvider = secondOperandExpressionProvider;
            this.SchemaReferenceKey = schemaReferenceKey;
        }

        public virtual string SchemaReferenceKey { get; }

        /// <inheritdoc/>
        public async Task<Expression> Resolve(IProviderContext providerContext, ExpressionScope scope)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            var firstOperandExpression = await this.firstOperandExpressionProvider.Invoke(providerContext, scope);
            var secondOperandExpression = await this.secondOperandExpressionProvider.Invoke(providerContext, scope);
            if (secondOperandExpression.Type != firstOperandExpression.Type)
            {
                firstOperandExpression = this.ConvertFirstOperandToAppropriateType(
                    firstOperandExpression,
                    secondOperandExpression.Type);
            }

            var binaryExpression = this.binaryExpressionFactory(firstOperandExpression, secondOperandExpression);
            var lambdaExpression = Expression.Lambda(binaryExpression, scope.CurrentParameter);
            return lambdaExpression;
        }

        private Expression ConvertFirstOperandToAppropriateType(Expression inputExpression, Type typeToConvert)
        {
            if (this.IsNullableOf(inputExpression, typeToConvert))
            {
                return Expression.Convert(inputExpression, typeToConvert);
            }

            var toStringMethod = typeof(object).GetMethod("ToString");
            var toStringExpression = Expression.Call(inputExpression, toStringMethod);
            if (typeToConvert == typeof(string))
            {
                return toStringExpression;
            }
            else if (typeToConvert.IsEquivalentTo(typeof(long)) || typeToConvert.IsEquivalentTo(typeof(decimal)))
            {
                var schemaEquivalentType = typeToConvert.IsEquivalentTo(typeof(decimal))
                    ? "number"
                    : "integer";
                var errorToThrow = Errors.Automation.ParameterValueTypeInvalid(
                        this.SchemaReferenceKey,
                        schemaEquivalentType,
                        reasonWhyValueIsInvalidIfAvailable: $"The {this.SchemaReferenceKey} provider requires the \"{schemaEquivalentType}\" parameter to resolve to a valid {schemaEquivalentType} value. ");

                if (inputExpression.NodeType == ExpressionType.MemberAccess)
                {
                    throw new ErrorException(errorToThrow);
                }

                var parseMethod = typeToConvert.GetMethod("Parse", new[] { typeof(string) });
                var parseCallExpression = Expression.Call(parseMethod, toStringExpression);
                return this.CreateTryCatchExpressionForParsingError(parseCallExpression, typeToConvert, errorToThrow);
            }

            // unreachable code.
            throw new NotSupportedException($"Expressions of type {typeToConvert} are not yet supported by binary expression conditions.");
        }

        private bool IsNullableOf(Expression expression, Type underlyingType)
        {
            return Nullable.GetUnderlyingType(expression.Type) == underlyingType;
        }

        private Expression CreateTryCatchExpressionForParsingError(MethodCallExpression parseCallExpression, Type expectedType, Error error)
        {
            var errorDataExpression = Expression.Constant(error);
            var errorException = typeof(ErrorException).GetConstructor(new[] { typeof(Error) });
            var throwErrorException = Expression.Throw(Expression.New(errorException, errorDataExpression), expectedType);
            var catchBlock = Expression.Catch(typeof(Exception), throwErrorException);
            return Expression.TryCatch(parseCallExpression, catchBlock);
        }
    }
}
