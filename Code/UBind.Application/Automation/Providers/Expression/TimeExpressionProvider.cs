// <copyright file="TimeExpressionProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System.Linq.Expressions;
    using NodaTime;
    using UBind.Application.Automation.Extensions;

    public class TimeExpressionProvider : IExpressionProvider
    {
        private readonly IProvider<Data<LocalTime>> timeProvider;

        public TimeExpressionProvider(IProvider<Data<LocalTime>> timeProvider)
        {
            this.timeProvider = timeProvider;
        }

        public string SchemaReferenceKey => "expressionTime";

        public async Task<Expression> Invoke(IProviderContext providerContext, ExpressionScope scope)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            var localTime = (await this.timeProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
            return Expression.Constant(localTime.GetValueFromGeneric());
        }
    }
}
