// <copyright file="PropertyExpressionProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Humanizer;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// For providing an expression specifying a property on an entity, for use in entity collection filters.
    /// </summary>
    public class PropertyExpressionProvider : IExpressionProvider
    {
        private readonly IProvider<Data<string>> pathProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyExpressionProvider"/> class.
        /// </summary>
        /// <param name="pathProvider">A provider for the path specifying the property.</param>
        /// <remarks>
        /// The path can be a subset of JSON path:
        /// $ - the entity itself
        /// $.foo or foo - the property foo of the entity
        /// $.foo.bar or foo.bar - the property bar of the sub-entity foo.
        /// </remarks>
        public PropertyExpressionProvider(IProvider<Data<string>> pathProvider)
        {
            this.pathProvider = pathProvider;
        }

        public string SchemaReferenceKey => "expression";

        /// <inheritdoc/>
        public async Task<Expression> Invoke(IProviderContext providerContext, ExpressionScope scope = null)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            string path = (await this.pathProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            if (path == "$")
            {
                return scope.CurrentParameter;
            }

            if (path.StartsWith("$."))
            {
                path = path.Substring(2);
            }

            Expression expression = scope.CurrentParameter;
            var paths = new List<string>();
            foreach (var part in path.Split('.'))
            {
                var pascalizedPart = part.Pascalize();
                if (part == pascalizedPart)
                {
                    var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                    errorData.Add("path", path);
                    errorData.Add("segment", part);
                    throw new ErrorException(
                        Errors.Automation.PathSyntaxError(
                            "Path segment should use camel case", this.SchemaReferenceKey, errorData));
                }

                if (expression.Type == typeof(object))
                {
                    paths.Add(part);
                }
                else
                {
                    paths.Add(pascalizedPart);
                }
            }

            var fullPath = expression.Type == typeof(object)
                ? string.Join("/", paths)
                : string.Join(".", paths);
            try
            {
                expression = ExpressionWrapper.GetProperty(
                    expression,
                    fullPath,
                    this.SchemaReferenceKey,
                    await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey));
            }
            catch (ArgumentException ex)
            {
                var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                errorData.Add("path", path);
                errorData.Add("segment", fullPath);
                throw new ErrorException(
                    Errors.Automation.PathResolutionError(this.SchemaReferenceKey, errorData), ex);
            }

            return DatabasePropertyTranslator.Translate(expression);
        }
    }
}
