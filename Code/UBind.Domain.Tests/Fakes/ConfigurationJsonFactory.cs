// <copyright file="ConfigurationJsonFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Fakes
{
    /// <summary>
    /// Factory for generating the configuration JSON normally obtained fron the workbook for use in tests.
    /// </summary>
    public static class ConfigurationJsonFactory
    {
        /// <summary>
        /// Gets a string containing sample form data with questionset json for tests.
        /// </summary>
        /// <param name="questionKey">The question key.</param>
        /// <param name="canChangeWhenApprove">Indicator whether question can change when approve.</param>
        /// <returns>A string containg sample form data json with policy start and end dates.</returns>
        public static string GetSampleWithQuestionSet(string questionKey, string canChangeWhenApprove)
        {
            return $@"{{
	""questionMetaData"": {{
		""questionSets"": {{
			""ratingPrimary"": {{
				""{questionKey}"": {{
                    ""dataType"": ""text"",
					""canChangeWhenApproved"": {canChangeWhenApprove}
				}}
			}}
		}}
	}}
}}";
        }

        public static string GetSampleWithQuestionSetForReset(
            string questionKey,
            string resetForNewQuotes = "false",
            string resetForNewRenewalQuotes = "false",
            string resetForNewAdjustmentQuotes = "false",
            string resetForNewCancellationQuotes = "false",
            string resetForNewPurchaseQuotes = "false")
        {
            return $@"{{
	""questionMetaData"": {{
		""questionSets"": {{
			""ratingPrimary"": {{
				""{questionKey}"": {{
                    ""dataType"": ""text"",
					""resetForNewQuotes"": {resetForNewQuotes},
                    ""resetForNewRenewalQuotes"": {resetForNewRenewalQuotes},
                    ""resetForNewAdjustmentQuotes"": {resetForNewAdjustmentQuotes},
                    ""resetForNewCancellationQuotes"": {resetForNewCancellationQuotes},
                    ""resetForNewPurchaseQuotes"": {resetForNewPurchaseQuotes},
                    ""canChangeWhenApproved"": ""true""
				}}
			}}
		}}
	}}
}}";
        }

        /// <summary>
        /// Gets a string containing sample form data with base configuration json for tests.
        /// </summary>
        /// <returns>A string containg sample form data json with policy start and end dates.</returns>
        public static string GetSampleBaseConfiguration()
        {
            return $@"{{
	""baseConfiguration"": {{
	}}
}}";
        }

        /// <summary>
        /// Creates a string containing sample configuration json with quesion metadata.
        /// </summary>
        /// <returns>The configuration json string.</returns>
        public static string GetSampleBaseConfigurationWithQuestionMetadata()
        {
            return $@"{{
	""baseConfiguration"": {{
		""questionMetaData"": {{
			""questionSets"": {{
				""ratingPrimary"": {{
					""price"": {{
						""canChangeWhenApproved"": false,
                        ""dataType"" : ""currency""
					}}
				}}
			}}
		}}
	}}
}}";
        }
    }
}
