// <copyright file="ProductConfigurationJson.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Fakes
{
    public static class ProductConfigurationJson
    {
        public static string Default()
        {
            return @"
{
  ""quoteNumberSource"": 0,
  ""quoteDataLocator"": {
    ""insuredName"": {
      ""object"": ""FormData"",
      ""path"": ""insuredName""
    },
    ""totalPremium"": {
      ""object"": ""CalculationResult"",
      ""path"": ""payment.total.premium""
    },
    ""currencyCode"": {
      ""object"": ""CalculationResult"",
      ""path"": ""payment.currencyCode""
    },
    ""contactAddressLine1"": {
      ""object"": ""FormData"",
      ""path"": ""contactAddressLine1""
    },
    ""contactAddressSuburb"": {
      ""object"": ""FormData"",
      ""path"": ""contactAddressSuburb""
    },
    ""contactAddressState"": {
      ""object"": ""FormData"",
      ""path"": ""contactAddressState""
    },
    ""contactAddressPostcode"": {
      ""object"": ""FormData"",
      ""path"": ""contactAddressPostcode""
    },
    ""tradingName"": {
      ""object"": ""FormData"",
      ""path"": ""tradingName""
    },
    ""abn"": {
      ""object"": ""FormData"",
      ""path"": ""abn""
    },
    ""numberOfInstallments"": {
      ""object"": ""FormData"",
      ""path"": ""numberOfInstallments""
    },
    ""runOffQuestion"": {
      ""object"": ""FormData"",
      ""path"": ""runoffQuestion""
    },
    ""inceptionDate"": {
      ""object"": ""FormData"",
      ""path"": ""policyStartDate""
    },
    ""expiryDate"": {
      ""object"": ""FormData"",
      ""path"": ""policyEndDate""
    },
    ""cancellationDate"": {
      ""object"": ""FormData"",
      ""path"": ""cancellationDate""
    },
    ""effectiveDate"": {
      ""object"": ""FormData"",
      ""path"": ""effectiveDate""
    }
  },
  ""quoteWorkflow"": {
    ""isSettlementRequired"": false,
    ""bindOptions"": 3,
    ""transitions"": [
      {
        ""action"": ""quote"",
        ""requiredStates"": [],
        ""resultingState"": ""incomplete""
      },
      {
        ""action"": ""approve"",
        ""requiredStates"": [],
        ""resultingState"": ""approved""
      },
      {
        ""action"": ""refer"",
        ""requiredStates"": [],
        ""resultingState"": ""referred""
      },
      {
        ""action"": ""autoApproval"",
        ""requiredStates"": [],
        ""resultingState"": ""approved""
      },
      {
        ""action"": ""submit"",
        ""requiredStates"": [],
        ""resultingState"": ""complete""
      },
      {
        ""action"": ""bind"",
        ""requiredStates"": [],
        ""resultingState"": ""complete""
      },
      {
        ""action"": ""policy"",
        ""requiredStates"": [],
        ""resultingState"": ""complete""
      },
      {
        ""action"": ""invoice"",
        ""requiredStates"": [],
        ""resultingState"": ""complete""
      },
      {
        ""action"": ""creditNote"",
        ""requiredStates"": [],
        ""resultingState"": ""complete""
      }
    ]
  }
}";
        }

        public static string DatesFromCalculationResult()
        {
            return @"
{
  ""quoteNumberSource"": 0,
  ""quoteDataLocator"": {
    ""insuredName"": {
      ""object"": ""FormData"",
      ""path"": ""insuredName""
    },
    ""inceptionDate"": {
      ""object"": ""CalculationResult"",
      ""path"": ""questions.dates.policyStartDate""
    },
    ""expiryDate"": {
      ""object"": ""CalculationResult"",
      ""path"": ""questions.dates.policyEndDate""
    },
    ""effectiveDate"": {
      ""object"": ""CalculationResult"",
      ""path"": ""questions.dates.policyAdjustmentDate""
    },
    ""cancellationDate"": {
      ""object"": ""CalculationResult"",
      ""path"": ""questions.dates.policyCancellationDate""
    },
    ""totalPremium"": {
      ""object"": ""CalculationResult"",
      ""path"": ""payment.total.premium""
    },
    ""contactAddressLine1"": {
      ""object"": ""FormData"",
      ""path"": ""contactAddressLine1""
    },
    ""contactAddressSuburb"": {
      ""object"": ""FormData"",
      ""path"": ""contactAddressSuburb""
    },
    ""contactAddressState"": {
      ""object"": ""FormData"",
      ""path"": ""contactAddressState""
    },
    ""contactAddressPostcode"": {
      ""object"": ""FormData"",
      ""path"": ""contactAddressPostcode""
    }
  },
  ""quoteWorkflow"": {
    ""isSettlementRequired"": false,
    ""bindOptions"": 3,
    ""transitions"": [
      {
        ""action"": ""quote"",
        ""requiredStates"": [ ""nascent"" ],
        ""resultingState"": ""incomplete""
      },
      {
        ""action"": ""reviewReferral"",
        ""requiredStates"": [ ""incomplete"" ],
        ""resultingState"": ""review""
      },
      {
        ""action"": ""reviewApproval"",
        ""requiredStates"": [ ""review"" ],
        ""resultingState"": ""approved""
      },
      {
        ""action"": ""autoApproval"",
        ""requiredStates"": [ ""incomplete"" ],
        ""resultingState"": ""approved""
      },
      {
        ""action"": ""return"",
        ""requiredStates"": [ ""review"", ""endorsement"", ""approved"" ],
        ""resultingState"": ""incomplete""
      },
      {
        ""action"": ""endorsementReferral"",
        ""requiredStates"": [ ""incomplete"", ""review"" ],
        ""resultingState"": ""endorsement""
      },
      {
        ""action"": ""endorsementApproval"",
        ""requiredStates"": [ ""endorsement"" ],
        ""resultingState"": ""approved""
      },
      {
        ""action"": ""decline"",
        ""requiredStates"": [],
        ""resultingState"": ""declined""
      },
      {
        ""action"": ""bind"",
        ""requiredStates"": [ ""approved"" ],
        ""resultingState"": ""complete""
      },
      {
        ""action"": ""submit"",
        ""requiredStates"": [ ""approved"" ],
        ""resultingState"": ""complete""
      },
      {
        ""action"": ""policy"",
        ""requiredStates"": [ ""approved"" ],
        ""resultingState"": ""complete""
      },
      {
        ""action"": ""invoice"",
        ""requiredStates"": [ ""approved"" ],
        ""resultingState"": """"
      },
      {
        ""action"": ""payment"",
        ""requiredStates"": [ ""approved"" ],
        ""resultingState"": """"
      },
      {
        ""action"": ""fund"",
        ""requiredStates"": [ ""approved"" ],
        ""resultingState"": """"
      },
      {
        ""action"": ""creditNote"",
        ""requiredStates"": [ ""approved"" ],
        ""resultingState"": """"
      }
    ]
  }
}";
        }
    }
}
