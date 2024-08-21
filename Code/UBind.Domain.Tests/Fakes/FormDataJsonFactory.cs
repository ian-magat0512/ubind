// <copyright file="FormDataJsonFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Fakes
{
    using System;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Fake form data json for tests.
    /// </summary>
    public static class FormDataJsonFactory
    {
        /// <summary>
        /// Gets a string containing empty form data json for tests.
        /// </summary>
        public static string Empty => @"{
    ""formModel"": {}
}";

        /// <summary>
        /// Gets a string containing sample form data json for tests.
        /// </summary>
        public static string Sample => @"{
    ""formModel"": {}
}";

        /// <summary>
        /// Gets a string containing sample form data json with unique data for tests.
        /// </summary>
        /// <returns>A string containing sample form data with a unique value.</returns>
        public static string GetUniqueSample() => $@"{{
    ""formModel"": {{
        ""foo"": ""{Guid.NewGuid()}""
    }}
}}";

        /// <summary>
        /// Gets a string containing sample form data json with a custom value for tests.
        /// </summary>
        /// <param name="value">a value to insert into the json.</param>
        /// <returns>A string containing sample form data with a custom value.</returns>
        public static string GetCustomSample(object value) => $@"{{
    ""formModel"": {{
        ""foo"": ""{value}""
    }}
}}";

        /// <summary>
        /// Gets a string containing sample form data json with a custom value for tests.
        /// </summary>
        /// <param name="property">The property name to insert into the json.</param>
        /// <param name="value">The property value to insert into the json.</param>
        /// <param name="inceptionDate">The inception date to use.</param>
        /// <param name="durationInMonths">The duration in months used to calculate end date.</param>
        /// <returns>A string containing sample form data with a custom value.</returns>
        public static string GetCustomSampleWithSpecifiedProperty(object property, object value, LocalDate inceptionDate, int durationInMonths = 12)
        {
            var expiryDate = inceptionDate.PlusMonths(durationInMonths);

            return $@"{{
                    ""formModel"": {{
                        ""{property}"": ""{value}"",
                        ""inceptionDate"": ""{LocalDatePattern.Iso.Format(inceptionDate)}"",
                        ""policyStartDate"": ""{LocalDatePattern.Iso.Format(inceptionDate)}"",
                        ""expiryDate"": ""{LocalDatePattern.Iso.Format(expiryDate)}"",
                        ""policyEndDate"": ""{LocalDatePattern.Iso.Format(expiryDate)}""
                    }}
                }}";
        }

        /// <summary>
        /// Gets a string containing sample form data json for tests.
        /// </summary>
        /// <param name="inceptionDate">The inception date to use.</param>
        /// <param name="durationInMonths">The duration in months used to calculate end date.</param>
        /// <returns>A string containg sample form data json with policy start and end dates.</returns>
        public static string GetSampleWithStartAndEndDates(LocalDate inceptionDate, int durationInMonths = 12)
        {
            var expiryDate = inceptionDate.PlusMonths(durationInMonths);
            return $@"{{
            ""formModel"": {{
                ""inceptionDate"": ""{LocalDatePattern.Iso.Format(inceptionDate)}"",
                ""policyStartDate"": ""{LocalDatePattern.Iso.Format(inceptionDate)}"",
                ""expiryDate"": ""{LocalDatePattern.Iso.Format(expiryDate)}"",
                ""policyEndDate"": ""{LocalDatePattern.Iso.Format(expiryDate)}""
                }}
            }}";
        }

        /// <summary>
        /// Gets a string containing sample form data json for tests.
        /// </summary>
        /// <param name="inceptionDate">The inception date to use.</param>
        /// <param name="expiryDate">The expiry date to use.</param>
        /// <returns>A string containg sample form data json with policy start and end dates.</returns>
        public static string GetSampleWithStartAndEndDates(LocalDate inceptionDate, LocalDate expiryDate)
        {
            return $@"{{
    ""formModel"": {{
        ""policyStartDate"": ""{LocalDatePattern.Iso.Format(inceptionDate)}"",
        ""policyEndDate"": ""{LocalDatePattern.Iso.Format(expiryDate)}""
    }}
}}";
        }

        /// <summary>
        /// Gets a string containing sample form data json for tests with 12 month policy starting today.
        /// </summary>
        /// <returns>A string containg sample form data json with policy start and end dates.</returns>
        public static string GetSampleWithStartAndEndDates()
        {
            return GetSampleWithStartAndEndDates(SystemClock.Instance.GetCurrentInstant().InZone(Timezones.AET).Date);
        }

        /// <summary>
        /// Gets a string containing sample form data json for tests.
        /// </summary>
        /// <returns>A string containing sample form data json with policy start, end and effective dates.</returns>
        public static string GetSampleWithStartEndAndEffectiveDates()
        {
            return GetSampleWithStartEndAndEffectiveDates(SystemClock.Instance.GetCurrentInstant().InZone(Timezones.AET).Date);
        }

        /// <summary>
        /// Gets a string containing sample form data json for tests.
        /// </summary>
        /// <param name="inceptionDate">The inception date.</param>
        /// <param name="durationInMonths">The number of months to offset expiry by.</param>
        /// <param name="effectivedateOffsetInMonths">The number of months to offset effective date by.</param>
        /// <returns>A string containing sample form data json with policy start, end and effective dates.</returns>
        public static string GetSampleWithStartEndAndEffectiveDates(LocalDate inceptionDate, int durationInMonths = 12, int effectivedateOffsetInMonths = 6)
        {
            var expiryDate = inceptionDate.PlusMonths(durationInMonths);
            var effectiveDate = inceptionDate.PlusMonths(effectivedateOffsetInMonths);
            return $@"{{
    ""formModel"": {{
        ""inceptionDate"": ""{LocalDatePattern.Iso.Format(inceptionDate)}"",
        ""policyStartDate"": ""{LocalDatePattern.Iso.Format(inceptionDate)}"",
        ""expiryDate"": ""{LocalDatePattern.Iso.Format(expiryDate)}"",
        ""policyEndDate"": ""{LocalDatePattern.Iso.Format(expiryDate)}"",
        ""effectiveDate"": ""{LocalDatePattern.Iso.Format(effectiveDate)}""
    }}
}}";
        }

        /// <summary>
        /// Gets a string containing sample form data json for tests.
        /// </summary>
        /// <param name="inceptionDate">The inception date.</param>
        /// <param name="durationInDays">The number of days to offset expiry by.</param>
        /// <param name="effectivedateOffsetInDays">The number of days to offset effective date by.</param>
        /// <returns>A string containg sample form data json with policy start, end and effective dates.</returns>
        public static string GetSampleWithStartEndAndEffectiveDatesInDays(LocalDate inceptionDate, int durationInDays = 20, int effectivedateOffsetInDays = 10)
        {
            var expiryDate = inceptionDate.PlusDays(durationInDays);
            var effectiveDate = inceptionDate.PlusDays(effectivedateOffsetInDays);
            return $@"{{
    ""formModel"": {{
        ""inceptionDate"": ""{LocalDatePattern.Iso.Format(inceptionDate)}"",
        ""policyStartDate"": ""{LocalDatePattern.Iso.Format(inceptionDate)}"",
        ""expiryDate"": ""{LocalDatePattern.Iso.Format(expiryDate)}"",
        ""policyEndDate"": ""{LocalDatePattern.Iso.Format(expiryDate)}"",
        ""effectiveDate"": ""{LocalDatePattern.Iso.Format(effectiveDate)}""
    }}
}}";
        }

        public static string GetSampleWithStartEndAndEffectiveAndCancellationDatesInDays()
        {
            return GetSampleWithStartEndAndEffectiveAndCancelllationDatesInDays(SystemClock.Instance.GetCurrentInstant().InZone(Timezones.AET).Date);
        }

        /// <summary>
        /// Gets a string containing sample form data json for tests.
        /// </summary>
        /// <param name="inceptionDate">The inception date.</param>
        /// <param name="durationInDays">The number of days to offset expiry by.</param>
        /// <param name="numberOfDaysToExpire">The number of days to expire.</param>
        /// <param name="effectivedateOffsetInDays">The number of days to offset effective date by.</param>
        /// <returns>A string containg sample form data json with policy start, end and effective dates.</returns>
        public static string GetSampleWithStartEndAndEffectiveAndCancelllationDatesInDays(LocalDate inceptionDate, int durationInDays = 20, int numberOfDaysToExpire = 20, int effectivedateOffsetInDays = 0)
        {
            var expiryDate = inceptionDate.PlusDays(durationInDays);
            var cancellationDate = inceptionDate.PlusDays(durationInDays - numberOfDaysToExpire);
            var effectiveDate = inceptionDate.PlusDays(effectivedateOffsetInDays);
            return $@"{{
    ""formModel"": {{
        ""inceptionDate"": ""{LocalDatePattern.Iso.Format(inceptionDate)}"",
        ""policyStartDate"": ""{LocalDatePattern.Iso.Format(inceptionDate)}"",
        ""expiryDate"": ""{LocalDatePattern.Iso.Format(expiryDate)}"",
        ""policyEndDate"": ""{LocalDatePattern.Iso.Format(expiryDate)}"",
        ""effectiveDate"": ""{LocalDatePattern.Iso.Format(effectiveDate)}"",
        ""cancellationDate"": ""{LocalDatePattern.Iso.Format(cancellationDate)}""
    }}
}}";
        }

        /// <summary>
        /// Gets sample form data with variety of form data for patch testing.
        /// </summary>
        /// <param name="nestedPropertyValue">The value to use for the nested property in the sample form data.</param>
        /// <returns>A string with the form data.</returns>
        public static string GetSampleFormDataJsonForPatching(string nestedPropertyValue = @"""foo""") => $@"{{
    ""formModel"": {{
        ""policyStartDate"": ""{LocalDatePattern.Iso.Format(SystemClock.Instance.Now().ToLocalDateInAet())}"",
        ""policyEndDate"": ""{LocalDatePattern.Iso.Format(SystemClock.Instance.Now().ToLocalDateInAet().PlusYears(1))}"",
        ""emptyStringProperty"": """",
        ""nullProperty"": null,
        ""emptyArrayProperty"": [],
        ""emptyObjectProperty"": {{}},
        ""objectProperty"": {{
            ""nestedProperty"": {nestedPropertyValue}
        }}
    }}
}}";

        /// <summary>
        /// Get sample form data for a claim.
        /// </summary>
        /// <returns>FOrm data json for a claim.</returns>
        public static string GetSampleWithClaimData() => $@"{{
    ""formModel"": {{
        ""claimAmount"": 100,
        ""description"": ""Test claim"",
        ""incidentDate"": ""2020-01-01""
    }}
}}";

        /// <summary>
        /// Get sample form data for a claim.
        /// </summary>
        /// <returns>FOrm data json for a claim.</returns>
        public static string GetSampleWithEmptyStringsForClaimData() => $@"{{
    ""formModel"": {{
        ""claimAmount"": """",
        ""description"": """",
        ""incidentDate"": """"
    }}
}}";
    }
}
