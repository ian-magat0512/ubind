// <copyright file="ExpressionScope.cs" company="uBind">
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
    using System.Text.RegularExpressions;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Scope containing iteration parameter expressions for use in filters including nested filters.
    /// </summary>
    public class ExpressionScope
    {
        private readonly Stack<ScopeParameter> scopeParameters = new Stack<ScopeParameter>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionScope"/> class with an initial iteration parameter.
        /// </summary>
        /// <param name="alias">The alias by which the iteration parameter can be referenced.</param>
        /// <param name="expression">The expression for the iteration parameter.</param>
        public ExpressionScope(string alias, ParameterExpression expression) =>
            this.scopeParameters.Push(new ScopeParameter(alias, expression));

        /// <summary>
        /// Interface to allow nested scopes to used inside using statements for automatic clean up.
        /// </summary>
        public interface INestedScope : IDisposable
        {
        }

        /// <summary>
        /// Gets the current iteration parameter (the innermost nested parameter).
        /// </summary>
        public ParameterExpression CurrentParameter => this.scopeParameters.Peek()?.Expression;

        /// <summary>
        /// Create a new nested scope with a new iteration parameter with a specific alias.
        /// </summary>
        /// <param name="alias">The alias by which the iteration parameter can be referenced (must not already exist in scope).</param>
        /// <param name="expression">The expression for the iteration parameter.</param>
        /// <param name="providerName">The name of the provider this is beingn used by, for error reporting.</param>
        /// <returns>An instance of a nested scope that can be disposed to remove the scope.</returns>
        public INestedScope Push(string alias, ParameterExpression expression, string providerName)
        {
            if (this.scopeParameters.Any(p => p.Alias == alias))
            {
                var errorData = new JObject { { "alias", alias } };
                throw new ErrorException(Errors.Automation.DuplicateExpressionAlias(providerName, errorData));
            }

            this.scopeParameters.Push(new ScopeParameter(alias, expression));
            return new NestedScope(this);
        }

        /// <summary>
        /// Create a new nested scope with a new iteration parameter with an alias to be generated.
        /// </summary>
        /// <param name="aliasRoot">A root that can be used to generate an alias by optionally appending an integer to avoid name clashes.</param>
        /// <param name="expression">The expression for the iteration parameter.</param>
        /// <param name="providerName">The name of the provider this is beingn used by, for error reporting.</param>
        /// <returns>An instance of a nested scope that can be disposed to remove the scope.</returns>
        public INestedScope PushWithGeneratedAlias(string aliasRoot, ParameterExpression expression, string providerName)
        {
            var alias = this.GenerateUniqueAlias(aliasRoot);
            return this.Push(alias, expression, providerName);
        }

        /// <summary>
        /// Gets the expression for an iteration parameter by its alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="providerName">The name of the requestingn provider (for error reporting).</param>
        /// <param name="debugContext">The error debug context.</param>
        /// <returns>An instance of <see cref="ParameterExpression"/> for the iteration parameter.</returns>
        public ParameterExpression GetParameterExpression(string alias, string providerName, JObject debugContext)
        {
            var scopeParameter = this.scopeParameters.SingleOrDefault(sp => sp.Alias == alias);
            if (scopeParameter == null)
            {
                debugContext.Add("alias", alias);
                throw new ErrorException(Errors.Automation.ExpressionAliasNotFound(providerName, debugContext));
            }

            return scopeParameter.Expression;
        }

        private string GenerateUniqueAlias(string aliasRoot)
        {
            if (!this.scopeParameters.Any(p => p.Alias == aliasRoot))
            {
                return aliasRoot;
            }

            var existingAliasCount = this.scopeParameters.Where(p => Regex.IsMatch(p.Alias, $"{aliasRoot}\\d+")).Count();
            return $"{aliasRoot}{existingAliasCount + 1}";
        }

        private class ScopeParameter
        {
            public ScopeParameter(string alias, ParameterExpression expression)
            {
                this.Alias = alias;
                this.Expression = expression;
            }

            public string Alias { get; }

            public ParameterExpression Expression { get; }
        }

        private class NestedScope : INestedScope
        {
            private ExpressionScope scope;

            public NestedScope(ExpressionScope scope) => this.scope = scope;

            public void Dispose()
            {
                if (this.scope != null)
                {
                    this.scope.scopeParameters.Pop();
                    this.scope = null;
                }
            }
        }
    }
}
