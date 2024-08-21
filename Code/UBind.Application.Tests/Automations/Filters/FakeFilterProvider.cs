// <copyright file="FakeFilterProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Filters
{
    using System;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using UBind.Application.Automation.Filters;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Expression;

    public class FakeFilterProvider<TData> : IFilterProvider
    {
        private readonly Expression<Func<TData, bool>> expression;

        public FakeFilterProvider(Expression<Func<TData, bool>> expression) =>
            this.expression = expression;

        public string SchemaReferenceKey => throw new NotImplementedException();

        Task<Expression> IFilterProvider.Resolve(IProviderContext providerContext, ExpressionScope scope)
        {
            return Task.FromResult(this.expression as Expression);
        }
    }
}
