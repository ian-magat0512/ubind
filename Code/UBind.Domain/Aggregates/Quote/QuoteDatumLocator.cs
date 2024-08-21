// <copyright file="QuoteDatumLocator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote
{
    using System;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;

    /// <summary>
    /// Specifies the location of an item of quote data in form data or calculation result json.
    /// </summary>
    public class QuoteDatumLocator : IResolvable
    {
        private QuoteDatumLocation location;
        private CachingJObjectWrapper formData;
        private CachingJObjectWrapper calculationResultData;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteDatumLocator"/> class.
        /// </summary>
        /// <param name="location">The location of the datum.</param>
        /// <param name="formData">The form data to find it in.</param>
        /// <param name="calculationResultData">The calculation result to find it in.</param>
        public QuoteDatumLocator(
            QuoteDatumLocation location,
            CachingJObjectWrapper formData,
            CachingJObjectWrapper calculationResultData)
        {
            this.location = location;
            this.formData = formData;
            this.calculationResultData = calculationResultData;
        }

        /// <inheritdoc/>
        public TDatum Resolve<TDatum>()
        {
            this.formData.ThrowIfArgumentNull(nameof(this.formData));
            this.calculationResultData.ThrowIfArgumentNull(nameof(this.calculationResultData));

            JToken token = this.ResolveJToken();
            if (token == null)
            {
                return default;
            }

            Type returnType = typeof(TDatum);
            if (returnType == typeof(decimal) || returnType == typeof(decimal?))
            {
                string value = token.Value<string>();
                var result = value.TryParseAsDecimal();
                if (result.IsFailure)
                {
                    throw new ErrorException(Errors.DataLocators.ParseFailure(this.location, returnType, result.Error));
                }

                return (TDatum)(object)result.Value;
            }
            else if (returnType == typeof(LocalDate) || returnType == typeof(LocalDate?))
            {
                string value = token.Value<string>();
                var result = value.TryParseAsLocalDate();
                if (result.IsFailure)
                {
                    throw new ErrorException(Errors.DataLocators.ParseFailure(this.location, returnType, result.Error));
                }

                return (TDatum)(object)result.Value;
            }
            else
            {
                return token.Value<TDatum>();
            }
        }

        /// <summary>
        /// Resolves the JToken from the specified json object, or null if not found.
        /// </summary>
        /// <returns>The JToken or null if not found.</returns>
        private JToken ResolveJToken()
        {
            JToken token = null;
            if (this.location.Object == QuoteDataLocationObject.FormData)
            {
                if (this.formData != null)
                {
                    var formModelToken = this.formData.JObject.SelectToken("formModel");
                    if (formModelToken != null)
                    {
                        token = this.formData.JObject.SelectToken("formModel").SelectToken(this.location.Path);
                    }
                }
            }
            else if (this.location.Object == QuoteDataLocationObject.CalculationResult)
            {
                this.calculationResultData.ThrowIfArgumentNull(nameof(this.calculationResultData));
                if (this.calculationResultData != null)
                {
                    token = this.calculationResultData.JObject.SelectToken(this.location.Path);
                }
            }
            else
            {
                throw new ArgumentException(
                    "In QuoteDataLocator was expecting the object to look in to be specified as either "
                    + $"\"FormData\" or \"CalculationResult\", but instead got {nameof(this.location.Object)}.");
            }

            return token;
        }
    }
}
