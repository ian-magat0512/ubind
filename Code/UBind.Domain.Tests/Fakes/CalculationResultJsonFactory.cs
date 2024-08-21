// <copyright file="CalculationResultJsonFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1600
#pragma warning disable SA1201

namespace UBind.Domain.Tests.Fakes
{
    using NodaTime;
    using UBind.Domain.Extensions;

    public static class CalculationResultJsonFactory
    {
        public static string Create(
            bool bindable = true,
            LocalDate? startDate = null,
            int durationInDays = 365,
            bool includeEffectiveDate = false)
        {
            var state = bindable ? "bindingQuote" : "premiumComplete";
            var startDateValue = startDate ?? SystemClock.Instance.Now().InZone(Timezones.AET).Date;
            var startDateString = startDateValue.ToIso8601();
            var endDateString = startDateValue.PlusDays(durationInDays).ToIso8601();
            var effectiveDateString = startDateValue.PlusDays(durationInDays / 2).ToIso8601();
            var effectiveDateLine = $@",
            ""policyEffectiveDate"": ""{effectiveDateString}"",";

            return $@"{{
    ""questions"": {{
        ""dates"": {{
            ""inceptionDate"": ""{startDateString}"",
            ""policyInceptionDate"": ""{startDateString}"",
            ""policyStartDate"": ""{startDateString}"",
            ""expiryDate"": ""{endDateString}"",
            ""policyEndDate"": ""{endDateString}""{(includeEffectiveDate ? effectiveDateLine : string.Empty)}
        }},
    }},
    ""state"": ""{state}"",
    ""payment"" : {{
        ""currencyCode"": ""AUD"",
        ""total"" : {{
            ""premium"" : ""100.00"",
            ""esl"" : ""0.00"",
            ""gst"" : ""10.00"",
            ""stampDuty"" : ""11.00"",
            ""serviceFees"" : ""0.00"",
            ""interest"" : ""0.00"",
            ""merchantFees"" : ""4.84"",
            ""transactionCosts"" : ""0.00"",
            ""payable"" : ""121.00""
        }},
        ""instalments"" : {{
            ""instalmentsPerYear"" : 12,
            ""instalmentAmount"" : ""10.08""
        }}
    }},
    ""risk1"": {{
        ""ratingFactors"": {{
            ""policyStartDate"": ""{startDateString}"",
            ""policyEndDate"": ""{endDateString}""{(includeEffectiveDate ? effectiveDateLine : string.Empty)}
        }}
    }}
}}";
        }

        public static string CreateWithNewBreakdown(
            bool bindable = true,
            string totalPayable = default)
        {
            var state = bindable ? "bindingQuote" : "premiumComplete";
            var payable = totalPayable ?? "100.00";
            return $@"{{
    ""state"": ""{state}"",
    ""payment"" : {{
        ""currencyCode"": ""AUD"",
        ""total"" : {{
            ""premium"" : ""{payable}"",
            ""esl"" : ""0.00"",
            ""gst"" : ""0.00"",
            ""stampDuty"" : ""0.00"",
            ""serviceFees"" : ""0.00"",
            ""interest"" : ""0.00"",
            ""merchantFees"" : ""0.00"",
            ""transactionCosts"" : ""0.00"",
            ""payable"" : ""{payable}""
        }},
        ""priceComponents"": {{
            ""basePremium"": ""{payable}"",
            ""esl"": ""0.00"",
            ""premiumGst"": ""20.00"",
            ""stampDutyAct"": ""0.00"",
            ""stampDutyNsw"": ""0.00"",
            ""stampDutyNt"": ""0.00"",
            ""stampDutyQld"": ""0.00"",
            ""stampDutySa"": ""0.00"",
            ""stampDutyTas"": ""0.00"",
            ""stampDutyWa"": ""0.00"",
            ""stampDutyVic"": ""22.00"",
            ""commission"": ""0.00"",
            ""commissionGst"": ""0.00"",
            ""brokerFee"": ""0.00"",
            ""brokerFeeGst"": ""0.00"",
            ""underwriterFee"": ""0.00"",
            ""underwriterFeeGst"": ""0.00"",
            ""interest"": ""0.00"",
            ""merchantFees"": ""0.00"",
            ""transactionCosts"": ""0.00"",
            ""stampDutyTotal"": ""0.00"",
            ""totalPremium"": ""0.00"",
            ""totalGst"": ""0.00"",
            ""totalPayable"": ""{payable}""
        }},
        ""instalments"" : {{
            ""instalmentsPerYear"" : 12,
            ""instalmentAmount"" : ""10.08""
        }}
    }}
}}";
        }

        public static string Sample => @"{
    ""state"": ""premiumComplete"",
    ""payment"" : {
        ""currencyCode"": ""AUD"",
        ""total"" : {
            ""premium"" : ""100.00"",
            ""esl"" : ""0.00"",
            ""gst"" : ""10.00"",
            ""stampDuty"" : ""11.00"",
            ""serviceFees"" : ""0.00"",
            ""interest"" : ""0.00"",
            ""merchantFees"" : ""0.00"",
            ""transactionCosts"" : ""0.00"",
            ""payable"" : ""121.00""
        },
        ""instalments"" : {
            ""instalmentsPerYear"" : 12,
            ""instalmentAmount"" : ""10.08""
        }
    }
}";

        public static string SampleWithOldBreakdown => @"{
    ""state"": ""premiumComplete"",
    ""payment"" : {
        ""currencyCode"": ""AUD"",
        ""total"" : {
            ""premium"" : ""100.00"",
            ""esl"" : ""0.00"",
            ""gst"" : ""10.00"",
            ""stampDuty"" : ""11.00"",
            ""serviceFees"" : ""0.00"",
            ""interest"" : ""0.00"",
            ""merchantFees"" : ""0.00"",
            ""transactionCosts"" : ""0.00"",
            ""payable"" : ""121.00""
        },
        ""instalments"" : {
            ""instalmentsPerYear"" : 12,
            ""instalmentAmount"" : ""10.08""
        }
    }
}";

        public static string SampleWithNewBreakdown => @"{
    ""state"": ""premiumComplete"",
    ""payment"" : {
        ""currencyCode"": ""AUD"",
        ""total"" : {
            ""premium"" : ""100.00"",
            ""esl"" : ""0.00"",
            ""gst"" : ""10.00"",
            ""stampDuty"" : ""11.00"",
            ""serviceFees"" : ""0.00"",
            ""interest"" : ""0.00"",
            ""merchantFees"" : ""0.00"",
            ""transactionCosts"" : ""0.00"",
            ""payable"" : ""121.00""
        },
        ""priceComponents"": {
            ""basePremium"": ""200.00"",
            ""esl"": ""0.00"",
            ""premiumGst"": ""20.00"",
            ""stampDutyAct"": ""0.00"",
            ""stampDutyNsw"": ""0.00"",
            ""stampDutyNt"": ""0.00"",
            ""stampDutyQld"": ""0.00"",
            ""stampDutySa"": ""0.00"",
            ""stampDutyTas"": ""0.00"",
            ""stampDutyWa"": ""0.00"",
            ""stampDutyVic"": ""22.00"",
            ""commission"": ""0.00"",
            ""commissionGst"": ""0.00"",
            ""brokerFee"": ""50.00"",
            ""brokerFeeGst"": ""5.00"",
            ""underwriterFee"": ""0.00"",
            ""underwriterFeeGst"": ""0.00"",
            ""interest"": ""0.00"",
            ""merchantFees"": ""0.00"",
            ""transactionCosts"": ""0.00"",
            ""stampDutyTotal"": ""22.00"",
            ""totalPremium"": ""0.00"",
            ""totalGst"": ""0.00"",
            ""totalPayable"": ""0.00""
        },
        ""instalments"" : {
            ""instalmentsPerYear"" : 12,
            ""instalmentAmount"" : ""10.08""
        }
    }
}";

        public static string DeftSample => @"{
    ""state"": ""premiumComplete"",
    ""payment"" : {
        ""currencyCode"": ""AUD"",
        ""total"" : {
            ""premium"" : ""100.00"",
            ""esl"" : ""0.00"",
            ""gst"" : ""0.00"",
            ""stampDuty"" : ""0.00"",
            ""serviceFees"" : ""0.00"",
            ""interest"" : ""0.00"",
            ""merchantFees"" : ""1.50"",
            ""transactionCosts"" : ""0.00"",
            ""payable"" : ""101.50""
        },
        ""instalments"" : {
            ""instalmentsPerYear"" : 1,
            ""instalmentAmount"" : ""101.50""
        }
    }
}";

        public static string SampleWithSoftReferral => @"{
    ""state"": ""premiumComplete"",
    ""payment"" : {
        ""currencyCode"": ""AUD"",
        ""total"" : {
            ""premium"" : ""100.00"",
            ""esl"" : ""0.00"",
            ""gst"" : ""10.00"",
            ""stampDuty"" : ""11.00"",
            ""serviceFees"" : ""0.00"",
            ""interest"" : ""0.00"",
            ""merchantFees"" : ""0.00"",
            ""transactionCosts"" : ""0.00"",
            ""payable"" : ""121.00""
        },
        ""instalments"" : {
            ""instalmentsPerYear"" : 12,
            ""instalmentAmount"" : ""10.08""
        }
    },
    ""triggers"":{  
      ""softReferral"":{  
         ""liabilityReferral"":true,
         ""generalReferral"":false,
         ""toolsover10000"":false
      },
      ""hardReferral"":{  

      },
      ""decline"":{  
         ""employees"":false
      },
      ""error"":{  
         ""noCoverSelected"":false
      }
    }
}";

        public static string SampleWithHardReferral => @"{
    ""state"": ""premiumComplete"",
    ""payment"" : {
        ""currencyCode"": ""AUD"",
        ""total"" : {
            ""premium"" : ""100.00"",
            ""esl"" : ""0.00"",
            ""gst"" : ""10.00"",
            ""stampDuty"" : ""11.00"",
            ""serviceFees"" : ""0.00"",
            ""interest"" : ""0.00"",
            ""merchantFees"" : ""0.00"",
            ""transactionCosts"" : ""0.00"",
            ""payable"" : ""121.00""
        },
        ""instalments"" : {
            ""instalmentsPerYear"" : 12,
            ""instalmentAmount"" : ""10.08""
        }
    },
    ""triggers"":{  
      ""softReferral"":{  
         ""liabilityReferral"":false,
         ""generalReferral"":false,
         ""toolsover10000"":false
      },
      ""hardReferral"":{  
         ""liabilityReferral"":true,
      },
      ""decline"":{  
         ""employees"":false
      },
      ""error"":{  
         ""noCoverSelected"":false
      }
    }
}";

        public static string SampleWithDeclined => @"{
    ""state"": ""premiumComplete"",
    ""payment"" : {
        ""currencyCode"": ""AUD"",
        ""total"" : {
            ""premium"" : ""100.00"",
            ""esl"" : ""0.00"",
            ""gst"" : ""10.00"",
            ""stampDuty"" : ""11.00"",
            ""serviceFees"" : ""0.00"",
            ""interest"" : ""0.00"",
            ""merchantFees"" : ""0.00"",
            ""transactionCosts"" : ""0.00"",
            ""payable"" : ""121.00""
        },
        ""instalments"" : {
            ""instalmentsPerYear"" : 12,
            ""instalmentAmount"" : ""10.08""
        }
    },
    ""triggers"":{  
      ""softReferral"":{  
         ""liabilityReferral"":false,
         ""generalReferral"":false,
         ""toolsover10000"":false
      },
      ""hardReferral"":{  

      },
      ""decline"":{  
         ""employees"":true
      },
      ""error"":{  
         ""noCoverSelected"":false
      }
    }
}";

        public static string SampleWithError => @"{
    ""state"": ""premiumComplete"",
    ""payment"" : {
        ""currencyCode"": ""AUD"",
        ""total"" : {
            ""premium"" : ""100.00"",
            ""esl"" : ""0.00"",
            ""gst"" : ""10.00"",
            ""stampDuty"" : ""11.00"",
            ""serviceFees"" : ""0.00"",
            ""interest"" : ""0.00"",
            ""merchantFees"" : ""0.00"",
            ""transactionCosts"" : ""0.00"",
            ""payable"" : ""121.00""
        },
        ""instalments"" : {
            ""instalmentsPerYear"" : 12,
            ""instalmentAmount"" : ""10.08""
        }
    },
    ""triggers"":{  
      ""softReferral"":{  
         ""liabilityReferral"":false,
         ""generalReferral"":false,
         ""toolsover10000"":false
      },
      ""hardReferral"":{  

      },
      ""decline"":{  
         ""employees"":false
      },
      ""error"":{  
         ""noCoverSelected"":true
      }
    }
}";

        /// <summary>
        /// Gets sample form data with variety of calculation result json for patch testing.
        /// </summary>
        /// <param name="nestedPropertyValue">The nested property value.</param>
        /// <returns>A string with the calculation result data.</returns>
        public static string GetSampleCalculationResultForPatching(
            string nestedPropertyValue = @"""foo""") => $@"{{
    ""state"": ""premiumComplete"",
    ""questions"": {{
        ""ratingPrimary"": {{
            ""emptyStringProperty"": """",
            ""nullProperty"": null,
            ""emptyArrayProperty"": [],
            ""emptyObjectProperty"": {{}},
            ""objectProperty"": {{
                ""nestedProperty"": {nestedPropertyValue}
            }}
        }}
    }},
    ""payment"" : {{
        ""currencyCode"": ""AUD"",
        ""total"" : {{
            ""premium"" : ""100.00"",
            ""esl"" : ""0.00"",
            ""gst"" : ""10.00"",
            ""stampDuty"" : ""11.00"",
            ""serviceFees"" : ""0.00"",
            ""interest"" : ""0.00"",
            ""merchantFees"" : ""0.00"",
            ""transactionCosts"" : ""0.00"",
            ""payable"" : ""121.00""
        }},
        ""priceComponents"": {{
            ""basePremium"": ""200.00"",
            ""esl"": ""0.00"",
            ""premiumGst"": ""20.00"",
            ""stampDutyAct"": ""0.00"",
            ""stampDutyNsw"": ""0.00"",
            ""stampDutyNt"": ""0.00"",
            ""stampDutyQld"": ""0.00"",
            ""stampDutySa"": ""0.00"",
            ""stampDutyTas"": ""0.00"",
            ""stampDutyWa"": ""0.00"",
            ""stampDutyVic"": ""22.00"",
            ""commission"": ""0.00"",
            ""commissionGst"": ""0.00"",
            ""brokerFee"": ""50.00"",
            ""brokerFeeGst"": ""5.00"",
            ""underwriterFee"": ""0.00"",
            ""underwriterFeeGst"": ""0.00"",
            ""interest"": ""0.00"",
            ""merchantFees"": ""0.00"",
            ""transactionCosts"": ""0.00"",
            ""stampDutyTotal"": ""22.00"",
            ""totalPremium"": ""0.00"",
            ""totalGst"": ""0.00"",
            ""totalPayable"": ""0.00""
        }},
        ""instalments"" : {{
            ""instalmentsPerYear"" : 12,
            ""instalmentAmount"" : ""10.08""
        }}
    }}
}}";

        /// <summary>
        /// Gets a string containing sample form data with questionset json for tests.
        /// </summary>
        /// <param name="questionKey">The question key.</param>
        /// <param name="value">The value of question.</param>
        /// <returns>A string containg sample form data json with policy start and end dates.</returns>
        public static string GetSampleWithCalculationValue(string questionKey, string value)
        {
            return $@"{{
    ""questions"": {{
        ""ratingPrimary"": {{
            ""{questionKey}"": ""{value}""
        }}
    }}
}}";
        }
    }
}
