// <copyright file="TextMatchesRegexPatternFilterProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Filters
{
    using System;
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    public class TextMatchesRegexPatternFilterProvider : IFilterProvider
    {
        private readonly IExpressionProvider textProvider;
        private readonly IExpressionProvider regexPatternProvider;

        public TextMatchesRegexPatternFilterProvider(
            IExpressionProvider text,
            IExpressionProvider regexPattern)
        {
            this.textProvider = text;
            this.regexPatternProvider = regexPattern;
        }

        public string SchemaReferenceKey => "textMatchesRegexCondition";

        public async Task<Expression> Resolve(IProviderContext providerContext, ExpressionScope scope = null)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            var text = await this.textProvider.Invoke(providerContext, scope);
            if (text is MemberExpression)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                throw new ErrorException(Errors.Automation.Provider.ExpressionMethodNotSupportedForEntityQueries(this.SchemaReferenceKey, errorData));
            }

            if (text.Type != typeof(string))
            {
                throw new ErrorException(
                    Errors.Automation.ParameterValueTypeInvalid(
                        this.SchemaReferenceKey,
                        "text",
                        reasonWhyValueIsInvalidIfAvailable: $"The {this.SchemaReferenceKey} requires the \"text\" parameter to resolve to a valid text value. "));
            }

            var regexPattern = await this.regexPatternProvider.Invoke(providerContext, scope);
            var regexIsMatch = typeof(Regex).GetMethod(
                nameof(Regex.IsMatch), new Type[] { typeof(string), typeof(string) });
            var exceptionExpression = Expression.New(
                typeof(ErrorException).GetConstructor(new[] { typeof(Error) }),
                Expression.Constant(Errors.Automation.ParameterValueTypeInvalid(this.SchemaReferenceKey, "pattern")));
            var tryCatchExpression = Expression.TryCatch(
                Expression.Call(regexIsMatch, text, regexPattern),
                Expression.Catch(typeof(Exception), Expression.Throw(exceptionExpression, typeof(bool))));
            var lambdaExpression = Expression.Lambda(tryCatchExpression, scope?.CurrentParameter);
            return lambdaExpression;
        }
    }
}
