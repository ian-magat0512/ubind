// <copyright file="ProductConfigurationJsonFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Fakes
{
    /// <summary>
    /// Factory for creating product configuration json for use in tests.
    /// </summary>
    public static class ProductConfigurationJsonFactory
    {
        /// <summary>
        /// Creates sample product configuration json for the quote app.
        /// </summary>
        /// <returns>Sample json.</returns>
        public static string CreateForQuoteApp() => @"{
            ""quoteNumberSource"": 2,
            ""quoteDataLocator"": {
                    ""inceptionDate"": {
                      ""object"": ""CalculationResult"",
                      ""path"": ""questions.ratingSecondary.policyStartDateFormatted""
                    },
                    ""expiryDate"": {
                      ""object"": ""CalculationResult"",
                      ""path"": ""questions.ratingSecondary.policyEndDateFormatted""
                    }
            },
            ""quoteWorkflow"": {
                            ""isSettlementRequired"": true,
                            ""bindOptions"": 3,
		                    ""transitions"": [
			                    {
			                      ""action"": ""Quote"",
			                      ""requiredStates"": [""Nascent""], 
			                      ""resultingState"": ""Incomplete""
			                    },
                                {
			                      ""action"": ""Review"",
			                      ""requiredStates"": [""Incomplete"", ""Approved""], 
			                      ""resultingState"": ""Review""
			                    }
		                    ]
                    }
}";

        /// <summary>
        /// Creates sample product configuration json for the claim app.
        /// </summary>
        /// <returns>Sample json.</returns>
        public static string CreateForClaimApp() => @"{
            ""quoteNumberSource"": 2,
            ""quoteDataLocator"": {
                    ""inceptionDate"": {
                      ""object"": ""CalculationResult"",
                      ""path"": ""questions.ratingSecondary.policyStartDateFormatted""
                    },
                    ""expiryDate"": {
                      ""object"": ""CalculationResult"",
                      ""path"": ""questions.ratingSecondary.policyEndDateFormatted""
                    }
            },
            ""claimWorkflow"": {
                            ""isSettlementRequired"": true,
                            ""bindOptions"": 3,
		                    ""transitions"": [
			                    {
			                      ""action"": ""Claim"",
			                      ""requiredStates"": [""Nascent""], 
			                      ""resultingState"": ""Incomplete""
			                    },
                                {
			                      ""action"": ""AutoApproval"",
			                      ""requiredStates"": [""Incomplete""], 
			                      ""resultingState"": ""Approved""
			                    }
		                    ]
                    }
}";
    }
}
