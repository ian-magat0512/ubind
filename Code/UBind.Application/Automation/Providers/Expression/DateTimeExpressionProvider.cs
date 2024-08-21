// <copyright file="DateTimeExpressionProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Providers.Expression
{
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Humanizer;
    using NodaTime.Text;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// For providing an expression specifying a datetime value, for use in entity collection filters.
    /// </summary>
    public class DateTimeExpressionProvider : IExpressionProvider
    {
        private readonly IProvider<Data<string>> dateTimeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeExpressionProvider"/> class.
        /// </summary>
        /// <param name="dateTimeProvider">A provider for the string specifying the date time.</param>
        /// <remarks>
        /// Currently supported date time strings:
        /// "now" - the current time.
        /// </remarks>
        public DateTimeExpressionProvider(IProvider<Data<string>> dateTimeProvider)
        {
            this.dateTimeProvider = dateTimeProvider;
        }

        public string SchemaReferenceKey => "expressionDateTime";

        /// <inheritdoc/>
        public async Task<Expression> Invoke(IProviderContext providerContext, ExpressionScope scope = null)
        {
            providerContext.CancellationToken.ThrowIfCancellationRequested();
            string dateTimeString = (await this.dateTimeProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            if (dateTimeString == "now")
            {
                return Expression.Constant(NodaTime.SystemClock.Instance.Now().ToUnixTimeTicks());
            }

            var parseResult = InstantPattern.ExtendedIso.Parse(dateTimeString);
            if (parseResult.Success)
            {
                return Expression.Constant(parseResult.Value.ToUnixTimeTicks());
            }

            var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
            errorData.Add(ErrorDataKey.ValueToParse, dateTimeString.Truncate(80, "..."));
            errorData.Add(ErrorDataKey.ErrorMessage, parseResult.Exception.Message);
            var error = Errors.Automation.ValueResolutionError(this.SchemaReferenceKey, errorData);
            throw new ErrorException(error);
        }
    }
}
