// <copyright file="BasePeriodicSummaryQueryOptionsModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Application;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Web.Validation;

    /// <summary>
    /// Model used for the parameters of the GET request to the Quote Periodic Summary endpoint.
    /// </summary>
    public abstract class BasePeriodicSummaryQueryOptionsModel : BaseQueryOptionsModel, IPeriodicSummaryQueryOptionsModel
    {
        protected const int NumberOfMaximumExpectedSummary = 1000;

        /// <inheritdoc/>
        public SamplePeriodLength? SamplePeriodLength
        {
            get
            {
                if (string.IsNullOrEmpty(this.SamplePeriodLengthString))
                {
                    return Domain.Enums.SamplePeriodLength.None;
                }

                if (!Enum.TryParse<SamplePeriodLength>(this.SamplePeriodLengthString, true, out var samplePeriodLength))
                {
                    return Domain.Enums.SamplePeriodLength.None;
                }

                return samplePeriodLength;
            }
        }

        /// <inheritdoc/>
        public int? CustomSamplePeriodMinutes
        {
            get
            {
                if (string.IsNullOrEmpty(this.CustomSamplePeriodMinutesString))
                {
                    return null;
                }

                if (!int.TryParse(this.CustomSamplePeriodMinutesString, out int customSamplePeriodMinutes))
                {
                    return null;
                }

                return customSamplePeriodMinutes;
            }
        }

        /// <inheritdoc/>
        public DateTimeZone Timezone
        {
            get
            {
                return Timezones.GetTimeZoneByIdOrNull(this.TimeZoneId);
            }
        }

        [FromQuery(Name = "samplePeriodLength")]
        public string? SamplePeriodLengthString { get; set; }

        /// <inheritdoc/>
        [FromQuery(Name = "fromDateTime")]
        public string? FromDateTime { get; set; }

        /// <inheritdoc/>
        [FromQuery(Name = "toDateTime")]
        public string? ToDateTime { get; set; }

        /// <inheritdoc/>
        [FromQuery(Name = "includeProperty")]
        public IEnumerable<string>? IncludeProperties { get; set; }

        /// <inheritdoc/>
        [FromQuery(Name = "product")]
        public IEnumerable<string>? Products { get; set; }

        /// <inheritdoc/>
        [FromQuery(Name = "customSamplePeriodMinutes")]
        public string? CustomSamplePeriodMinutesString { get; set; }

        /// <inheritdoc/>
        [FromQuery(Name = "timeZone")]
        public string? TimeZoneId { get; set; }

        protected abstract HashSet<string> ValidIncludeProperties { get; }

        /// <summary>
        /// Check and validates the options included in the query.
        /// </summary>
        /// <exception cref="ErrorException">Throws an exception if any of the options are invalid.</exception>
        public virtual void ValidateQueryOptions()
        {
            this.ValidateEnvironment();
            this.ValidateParsedDates();
            this.ValidateTimeZoneOrSetDefault();
            this.ValidateIncludeProperties();
            this.ValidateSamplePeriodLength();
            this.ValidateProducts();
        }

        public virtual void SetFromDateTime(Instant? dateTime)
        {
            if (dateTime == null)
            {
                return;
            }

            var timeZone = Timezones.GetTimeZoneByIdOrNull(this.TimeZoneId);
            var fromDateTime = dateTime.Value.ToStartOfYearInZone(timeZone).ToYYYYMMDD();
            this.FromDateTime = this.GetValidDateTime("fromDateTime", fromDateTime, true);
        }

        protected abstract long GetNumberOfExpectedPeriods();

        protected async Task<List<Guid>> GetValidProducts(Guid tenantId, ICachingResolver cachingResolver)
        {
            var productIds = new List<Guid>();
            var invalidProducts = new List<string>();

            foreach (var productAlias in this.Products)
            {
                var product = await cachingResolver.GetProductByAliasOrNull(tenantId, productAlias);
                if (product != null)
                {
                    productIds.Add(product.Id);
                }
                else
                {
                    invalidProducts.Add(productAlias);
                }
            }

            if (invalidProducts.Any())
            {
                var productsNotFound = this.FormatMultipleValuesString(invalidProducts, false, "and");
                throw new ErrorException(this.GetErrorMultipleValueParameterInvalid(
                    "product",
                    invalidProducts,
                    $"Each value for the \"product\" parameter must contain a valid product alias," +
                    $" however {productsNotFound} could not be resolved to an active product in this tenancy."));
            }

            return productIds;
        }

        protected Error GetErrorParameterValueNotProvided(string parameterName, string reason = "")
        {
            reason = string.IsNullOrEmpty(reason) ? string.Empty : $" {reason}";
            string details = "When trying to process your request, the attempt failed because the required parameter" +
                $" \"{parameterName}\" was missing.{reason} To resolve the issue," +
                " please ensure that all required parameters are included and have valid values." +
                " If you require further assistance please contact technical support.";
            return new Error(
                $"required.request.parameter.missing",
                $"A required request parameter was missing",
                details,
                HttpStatusCode.BadRequest);
        }

        protected Error GetErrorMultipleValueParameterInvalid(string parameterName, IEnumerable<string> invalidValues, string reason)
        {
            if (invalidValues.Count() == 1)
            {
                return this.GetErrorParameterValueInvalid(parameterName, $"an invalid value (\"{invalidValues.First()}\")", reason);
            }

            var result = this.FormatMultipleValuesString(invalidValues, false, "and");
            return this.GetErrorParameterValueInvalid(parameterName, $"invalid values ({result})", reason);
        }

        protected Error GetErrorSingleValueParameterInvalid(string parameterName, string value, string reason)
        {
            return this.GetErrorParameterValueInvalid(parameterName, $"an invalid value (\"{value}\")", reason);
        }

        protected Error GetErrorNumericValueParameterInvalid(string parameterName, int value, string reason)
        {
            return this.GetErrorParameterValueInvalid(parameterName, $"an invalid value ({value})", reason);
        }

        protected string FormatMultipleValuesString(IEnumerable<string> validValues, bool camelCase = true, string andOr = "or")
        {
            if (validValues.Count() == 1)
            {
                return $"\"{(camelCase ? validValues.First().ToCamelCase() : validValues.First())}\"";
            }

            validValues = camelCase ? validValues.Select(x => x.ToCamelCase()) : validValues;
            var result = string.Join(", ", validValues.SkipLast(1).Select(x => $"\"{x}\""));
            result += $" {andOr} \"{validValues.Last()}\"";
            return result;
        }

        private Error GetErrorParameterValueInvalid(string parameterName, string value, string reason)
        {
            string details = "When trying to process your request, the attempt failed because the parameter" +
                $" \"{parameterName}\" had {value}. {reason} To resolve the issue," +
                " please ensure that all request parameters have valid values." +
                " If you require further assistance please contact technical support.";
            return new Error(
                $"request.parameter.invalid",
                $"A request parameter had an invalid value",
                details,
                HttpStatusCode.BadRequest);
        }

        private void ValidateEnvironment()
        {
            this.Environment = string.IsNullOrEmpty(this.Environment) ? "production" : this.Environment;
            var isSuccess = Enum.TryParse(this.Environment, true, out DeploymentEnvironment env);
            if (!isSuccess)
            {
                var validValues = this.FormatMultipleValuesString(
                    Enum.GetValues<DeploymentEnvironment>()
                    .Where(p => p != DeploymentEnvironment.None)
                    .Select(p => p.ToString()));
                throw new ErrorException(
                    this.GetErrorSingleValueParameterInvalid(
                        "environment",
                        this.Environment,
                        $"The \"environment\" parameter value must be one of {validValues}."));
            }
        }

        private void ValidateParsedDates()
        {
            var originalToDateTime = this.ToDateTime;

            // if the SamplePeriodLength is Year, it's possible that fromDateTime is null
            // since it can mean to get summary for all years, otherwise we require it
            bool isSamplePeriodLengthYear = this.SamplePeriodLength == Domain.Enums.SamplePeriodLength.Year;
            this.FromDateTime = this.GetValidDateTime(
                nameof(this.FromDateTime),
                this.FromDateTime,
                true,
                !isSamplePeriodLengthYear);
            this.ToDateTime = this.GetValidDateTime(nameof(this.ToDateTime), this.ToDateTime, false);
            LocalDateTime? fromDateTime = this.FromDateTime != null ? this.FromDateTime.ToLocalDateTimeFromExtendedIso8601() : null;
            LocalDateTime toDateTime = this.ToDateTime.ToLocalDateTimeFromExtendedIso8601();

            if (fromDateTime != null && fromDateTime >= toDateTime)
            {
                throw new ErrorException(
                    this.GetErrorSingleValueParameterInvalid(
                        "toDateTime",
                        originalToDateTime,
                        "The \"toDateTime\" parameter value should be later than the \"fromDateTime\" parameter value."));
            }
        }

        private string GetValidDateTime(string datePropertyName, string? dateOrDateTime, bool appendStartOfDay, bool required = true)
        {
            if (string.IsNullOrEmpty(dateOrDateTime) && !required)
            {
                return null;
            }

            var validDateTime = dateOrDateTime;
            if (string.IsNullOrEmpty(validDateTime))
            {
                throw new ErrorException(
                    this.GetErrorParameterValueNotProvided(
                        datePropertyName.ToCamelCase()));
            }

            var resultDateTime = LocalDateTimePattern.ExtendedIso.Parse(validDateTime);
            if (!resultDateTime.Success)
            {
                var resultDate = LocalDatePattern.Iso.Parse(validDateTime);
                if (resultDate.Success)
                {
                    var date = resultDate.Value;
                    LocalDateTime localDateTime = date.At(LocalTime.Midnight);
                    if (!appendStartOfDay)
                    {
                        localDateTime = localDateTime.Plus(Period.FromDays(1)).PlusTicks(-1);
                    }

                    validDateTime = localDateTime.ToExtendedIso8601();
                }

                resultDateTime = LocalDateTimePattern.ExtendedIso.Parse(validDateTime);
            }

            if (!resultDateTime.Success)
            {
                throw new ErrorException(
                    this.GetErrorSingleValueParameterInvalid(
                        datePropertyName.ToCamelCase(),
                        dateOrDateTime,
                        $"The \"{datePropertyName.ToCamelCase()}\" parameter value must be" +
                        $" in the ISO format yyyy-MM-dd or yyyy-MM-ddTHH:mm:ss.fffffff."));
            }

            return validDateTime;
        }

        private void ValidateIncludeProperties()
        {
            var validValues = this.FormatMultipleValuesString(this.ValidIncludeProperties);
            var reason = $"Each \"includeProperty\" parameter value must be one of {validValues}.";
            this.IncludeProperties = this.IncludeProperties ?? Enumerable.Empty<string>();
            if (!this.IncludeProperties.Any())
            {
                throw new ErrorException(
                    this.GetErrorParameterValueNotProvided(
                        "includeProperty",
                        reason));
            }

            List<string> invalidIncludeProperties = this.GetInvalidIncludeProperties();
            if (invalidIncludeProperties.Any())
            {
                throw new ErrorException(
                    this.GetErrorMultipleValueParameterInvalid(
                        "includeProperty",
                        invalidIncludeProperties,
                        reason));
            }
        }

        private List<string> GetInvalidIncludeProperties()
        {
            List<string> invalidIncludeProperties = new List<string>();
            foreach (var item in this.IncludeProperties)
            {
                if (!this.ValidIncludeProperties.Any(p => p.Equals(item, StringComparison.InvariantCultureIgnoreCase)))
                {
                    invalidIncludeProperties.Add(item);
                }
            }

            return invalidIncludeProperties;
        }

        private void ValidateTimeZoneOrSetDefault()
        {
            if (this.TimeZoneId == null)
            {
                this.TimeZoneId = Timezones.AET.Id;
            }

            if (this.Timezone == null)
            {
                throw new ErrorException(
                    this.GetErrorSingleValueParameterInvalid(
                        "timezone",
                        this.TimeZoneId,
                        $"{this.TimeZoneId} is not a valid or known time zone ID within the TZDB database."));
            }
        }

        private void ValidateSamplePeriodLength()
        {
            var samplePeriodLengthValues =
                Enum.GetValues<SamplePeriodLength>()
                .Where(p => p != Domain.Enums.SamplePeriodLength.None);
            var validValues = this.FormatMultipleValuesString(samplePeriodLengthValues.Select(x => x.ToString()));
            var reason = $"The \"samplePeriodLength\" parameter value must be one of {validValues}.";
            if (this.SamplePeriodLengthString == null)
            {
                throw new ErrorException(
                    this.GetErrorParameterValueNotProvided(
                        "samplePeriodLength",
                        reason));
            }

            if (this.SamplePeriodLength == null || this.SamplePeriodLength == Domain.Enums.SamplePeriodLength.None)
            {
                throw new ErrorException(
                    this.GetErrorSingleValueParameterInvalid(
                        "samplePeriodLength",
                        this.SamplePeriodLengthString,
                        reason));
            }

            if (this.SamplePeriodLength != Domain.Enums.SamplePeriodLength.Custom
                && this.CustomSamplePeriodMinutes.HasValue)
            {
                throw new ErrorException(
                    this.GetErrorNumericValueParameterInvalid(
                        "customSamplePeriodMinutes",
                        this.CustomSamplePeriodMinutes.Value,
                        "This is invalid because the \"samplePeriodLength\" parameter is" +
                        $" set to \"{this.SamplePeriodLengthString}\". The \"customSamplePeriodMinutes\" parameter" +
                        " must not be included in the request unless the value of the \"samplePeriodLength\" parameter" +
                        " is set to \"custom\"."));
            }

            this.ValidateCustomSamplePeriodMinutes();
        }

        private void ValidateProducts()
        {
            this.Products = this.Products ?? Enumerable.Empty<string>();
            var invalidProductAliases = new List<string>();
            foreach (var productAlias in this.Products)
            {
                var aliasAttribute = new AliasAttribute();
                var valid = aliasAttribute.IsValid(productAlias);
                if (!valid)
                {
                    invalidProductAliases.Add(productAlias);
                }
            }

            if (invalidProductAliases.Any())
            {
                throw new ErrorException(
                   this.GetErrorMultipleValueParameterInvalid(
                       "product",
                       invalidProductAliases,
                       "Each value for the \"product\" parameter must contain a valid product alias," +
                       " and as such must contain only lowercase alphabetic characters," +
                       " numeric characters, hyphens characters, and must not begin or end with a hyphen."));
            }
        }

        private void ValidateCustomSamplePeriodMinutes()
        {
            if (this.SamplePeriodLength != Domain.Enums.SamplePeriodLength.Custom)
            {
                return;
            }

            var reportingPeriodLengthMinutes = this.GetReportingPeriodLengthMinutes();
            if (!int.TryParse(this.CustomSamplePeriodMinutesString, out int customSamplePeriodMinutes))
            {
                throw new ErrorException(
                    this.GetErrorSingleValueParameterInvalid(
                        "customSamplePeriodMinutes",
                        this.CustomSamplePeriodMinutesString,
                        $"The \"customSamplePeriodMinutes\" parameter must have value that is greater than zero and less than the" +
                        $" length of the reporting period in minutes ({reportingPeriodLengthMinutes})."));
            }

            if (this.CustomSamplePeriodMinutes == null)
            {
                throw new ErrorException(
                    this.GetErrorParameterValueNotProvided(
                        "customSamplePeriodMinutes",
                        "The \"customSamplePeriodMinutes\" must be specified" +
                        " when the \"samplePeriodLength\" parameter value is \"custom\"." +
                        " The value must be greater than zero and less than the" +
                        $" length of the reporting period in minutes ({reportingPeriodLengthMinutes})."));
            }

            if (this.CustomSamplePeriodMinutes < 1 || this.CustomSamplePeriodMinutes >= reportingPeriodLengthMinutes)
            {
                throw new ErrorException(
                    this.GetErrorNumericValueParameterInvalid(
                        "customSamplePeriodMinutes",
                        this.CustomSamplePeriodMinutes.Value,
                        $"The \"customSamplePeriodMinutes\" parameter must have value that is greater than zero and less than the" +
                        $" length of the reporting period in minutes ({reportingPeriodLengthMinutes})."));
            }

            var numberOfExpectedSummary = this.GetNumberOfExpectedPeriods();
            if (numberOfExpectedSummary > BasePeriodicSummaryQueryOptionsModel.NumberOfMaximumExpectedSummary)
            {
                var minimumPeriodLengthInMins = Convert.ToInt32(Math.Ceiling(
                    Convert.ToDouble(reportingPeriodLengthMinutes) / BasePeriodicSummaryQueryOptionsModel.NumberOfMaximumExpectedSummary));
                string details = "While trying to generate a list of sample periods," +
                    $" the attempt failed because the value of the \"customSamplePeriodMinutes\"" +
                    $" parameter ({this.CustomSamplePeriodMinutes}) resulted in a list of {numberOfExpectedSummary} sample periods," +
                    $" which exceeds the maximum allowed of {BasePeriodicSummaryQueryOptionsModel.NumberOfMaximumExpectedSummary}." +
                    $" To resolve the issue, please ensure that the \"customSamplePeriodMinutes\"" +
                    $" parameter has a value of {minimumPeriodLengthInMins} or greater." +
                    " If you require further assistance please contact technical support.";
                throw new ErrorException(
                    new Error(
                        $"periodic.summary.maximum.sample.period.count.exceeded",
                        $"The resulting sample period count exceeded the maximum allowed",
                        details,
                        HttpStatusCode.BadRequest));
            }
        }

        private int GetReportingPeriodLengthMinutes()
        {
            var fromInstant = this.FromDateTime
                .ToLocalDateTimeFromExtendedIso8601()
                .InZoneLeniently(this.Timezone)
                .ToInstant();
            var toInstant = this.ToDateTime
                .ToLocalDateTimeFromExtendedIso8601()
                .InZoneLeniently(this.Timezone)
                .ToInstant();
            return (int)Math.Ceiling((toInstant - fromInstant).ToTimeSpan().TotalMinutes);
        }
    }
}