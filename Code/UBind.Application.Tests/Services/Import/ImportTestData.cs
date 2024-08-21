// <copyright file="ImportTestData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services.Import
{
    /// <summary>
    /// Generates dummy JSON for import testing.
    /// </summary>
    public class ImportTestData
    {
        /// <summary>
        /// Request to have a sample JSON payload string.
        /// </summary>
        /// <param name="policyNumber">The sample policy number.</param>
        /// <returns>A string containing a sample JSON payload.</returns>
        public static string GenerateCustomerPolicyClaimCompleteImportJson(string policyNumber = "P003")
        {
            return @"{
                ""data"":[ {
                        ""name"":""Brayden Matters"",
                        ""pref_name"":""Brayden"",
                        ""email"":""brayden.matters@uol.com.br"",
                        ""alt_email"":""brayden.matters@nature.com"",
                        ""mobile_phone"":""897-680-2374"",
                        ""home_phone"":""647-108-1776"",
                        ""work_phone"":""565-702-6032"",
                        ""policy_no"":""" + policyNumber + @""",
                        ""inception_date"":""20/08/19"",
                        ""cancellation_date"":""20/08/19"",
                        ""expiry_date"":""20/09/19"",
                        ""total_premium"":""1.0"",
                        ""total_esl"":""2.0"",
                        ""total_gst"":""3.0"",
                        ""total_stamp_duty"":""4.0"",
                        ""total_service_fees"":""5.0"",
                        ""total_interest"":""6.0"",
                        ""total_merchant_fees"":""7.0"",
                        ""total_transaction_costs"":""8.0"",
                        ""total_payable"":""9.0"",
                        ""installment_per_year"":""5"",
                        ""installment_amount"":""5.0"",
                        ""incident_date"":""09/09/19"",
                        ""descr"":""Sample description for P0003"",
                        ""partner_id"":""123"",
                        ""occupation"":""CEO"",
                        ""pet_name"":""b"",
                        ""rating_state"":""VIC"",
                        ""question_travel_cover"":""No"",
                        ""notified_date"":""09/09/19"",
                        ""amount"":""1000.00""
                    }
                ],
                ""customerMapping"":{ 
                    ""fullName"":""name"",
                    ""preferredName"":""pref_name"",
                    ""email"":""email"",
                    ""alternativeEmail"":""alt_email"",
                    ""mobilePhone"":""mobile_phone"",
                    ""homePhone"":""home_phone"",
                    ""workPhone"":""work_phone""
                },
                ""policyMapping"":{         

                    ""customerEmail"":""email"",
                    ""customerName"":""pref_name"",
                    ""policyNumber"":""policy_no"",
                    ""inceptionDate"":""inception_date"",
                    ""cancellationDate"":""cancellation_date"",
                    ""expiryDate"":""expiry_date"",
                    ""formData"":{             

                        ""formModel"":{ 
                            ""policyStartDate"":""inception_date"",
                            ""policyInceptionDate"":""inception_date"",
                            ""CancellationEffectiveDate"":""cancellation_effective_date"",
                            ""policyEndDate"":""expiry_date"",
                            ""policyExpiryDate"":""expiry_date""
                        }
                    },
                    ""calculationResult"":{
                        ""state"":""rating_state"",
                        ""payment"":{
                            ""total"":{ 
                                ""premium"":""total_premium"",
                                ""esl"":""total_esl"",
                                ""gst"":""total_gst"",
                                ""stampDuty"":""total_stamp_duty"",
                                ""serviceFees"":""total_service_fees"",
                                ""interest"":""total_interest"",
                                ""merchantFees"":""total_merchant_fees"",
                                ""transactionCosts"":""total_transaction_costs"",
                                ""payable"":""total_payable""
                            },
                            ""instalments"":{ 
                                ""instalmentsPerYear"":""installment_per_year"",
                                ""instalmentAmount"":""installment_amount""
                            }
                        },
                        ""questions"": { }
                    }
                },
                ""claimMapping"":{ 
                    ""policyNumber"":""policy_no"",
                    ""claimNumber"":""policy_no"",
                    ""incidentDate"":""incident_date"",
                    ""description"":""descr"",
                    ""amount"":""amount"",
                    ""notifiedDate"":""notified_date""
                }
            }";
        }

        /// <summary>
        /// Request to have a simple JSON payload string.
        /// </summary>
        /// <param name="policyNumber">The sample policy number.</param>
        /// <returns>A string containing a sample JSON payload.</returns>
        public static string GenerateCustomerPolicyCompleteImportJson(string policyNumber = "P003")
        {
            return @"{
                ""data"":[ {
                        ""name"":""Brayden Matters"",
                        ""pref_name"":""Brayden"",
                        ""email"":""brayden.matters@uol.com.br"",
                        ""alt_email"":""brayden.matters@nature.com"",
                        ""mobile_phone"":""897-680-2374"",
                        ""home_phone"":""647-108-1776"",
                        ""work_phone"":""565-702-6032"",
                        ""policy_no"":""" + policyNumber + @""",
                        ""inception_date"":""20/08/19"",
                        ""cancellation_effective_date"":""20/08/19"",
                        ""expiry_date"":""20/09/19"",
                        ""total_premium"":""1.0"",
                        ""total_esl"":""2.0"",
                        ""total_gst"":""3.0"",
                        ""total_stamp_duty"":""4.0"",
                        ""total_service_fees"":""5.0"",
                        ""total_interest"":""6.0"",
                        ""total_merchant_fees"":""7.0"",
                        ""total_transaction_costs"":""8.0"",
                        ""total_payable"":""9.0"",
                        ""installment_per_year"":""5"",
                        ""installment_amount"":""5.0"",
                        ""incident_date"":""09/09/19"",
                        ""descr"":""Sample description for P0003"",
                        ""partner_id"":""123"",
                        ""occupation"":""CEO"",
                        ""pet_name"":""b"",
                        ""rating_state"":""VIC"",
                        ""question_travel_cover"":""No""
                    }
                ],
                ""customerMapping"":{ 
                    ""fullName"":""name"",
                    ""preferredName"":""pref_name"",
                    ""email"":""email"",
                    ""alternativeEmail"":""alt_email"",
                    ""mobilePhone"":""mobile_phone"",
                    ""homePhone"":""home_phone"",
                    ""workPhone"":""work_phone""
                },
                ""policyMapping"":{         

                    ""customerEmail"":""email"",
                    ""customerName"":""pref_name"",
                    ""policyNumber"":""policy_no"",
                    ""inceptionDate"":""inception_date"",
                    ""cancellationDate"":""cancellation_date"",
                    ""expiryDate"":""expiry_date"",
                    ""formData"":{             

                        ""formModel"":{ 
                            ""policyStartDate"":""inception_date"",
                            ""policyInceptionDate"":""inception_date"",
                            ""CancellationEffectiveDate"":""cancellation_date"",
                            ""policyEndDate"":""expiry_date"",
                            ""policyExpiryDate"":""expiry_date""
                        }
                    },
                    ""calculationResult"":{
                        ""state"":""rating_state"",
                        ""payment"":{
                            ""total"":{ 
                                ""premium"":""total_premium"",
                                ""esl"":""total_esl"",
                                ""gst"":""total_gst"",
                                ""stampDuty"":""total_stamp_duty"",
                                ""serviceFees"":""total_service_fees"",
                                ""interest"":""total_interest"",
                                ""merchantFees"":""total_merchant_fees"",
                                ""transactionCosts"":""total_transaction_costs"",
                                ""payable"":""total_payable""
                            },
                            ""instalments"":{ 
                                ""instalmentsPerYear"":""installment_per_year"",
                                ""instalmentAmount"":""installment_amount""
                            }
                        },
                        ""questions"": { }
                    }
                }
            }";
        }

        public static string GeneratePolicyJson(string policyNumber = "P004", string? agentEmail = null)
        {
            string agentEmailJson = string.IsNullOrEmpty(agentEmail) ? string.Empty : $@",""agentEmail"":""{agentEmail}""";

            return $@"{{
        ""data"":[ {{
            ""name"":""Brayden Matters"",
            ""pref_name"":""Brayden"",
            ""email"":""brayden.matters@uol.com.br"",
            ""alt_email"":""brayden.matters@nature.com"",
            ""mobile_phone"":""897-680-2374"",
            ""home_phone"":""647-108-1776"",
            ""work_phone"":""565-702-6032"",
            ""policy_no"":""{policyNumber}"",
            ""inception_date"":""20/08/19"",
            ""cancellation_effective_date"":""20/08/19"",
            ""expiry_date"":""20/09/19"",
            ""total_premium"":""1.0"",
            ""total_esl"":""2.0"",
            ""total_gst"":""3.0"",
            ""total_stamp_duty"":""4.0"",
            ""total_service_fees"":""5.0"",
            ""total_interest"":""6.0"",
            ""total_merchant_fees"":""7.0"",
            ""total_transaction_costs"":""8.0"",
            ""total_payable"":""9.0"",
            ""installment_per_year"":""5"",
            ""installment_amount"":""5.0"",
            ""incident_date"":""09/09/19"",
            ""descr"":""Sample description for P0003"",
            ""partner_id"":""123"",
            ""occupation"":""CEO"",
            ""pet_name"":""b"",
            ""rating_state"":""VIC"",
            ""question_travel_cover"":""No""{agentEmailJson}
        }}],
        ""customerMapping"":{{ 
            ""fullName"":""name"",
            ""preferredName"":""pref_name"",
            ""email"":""email"",
            ""alternativeEmail"":""alt_email"",
            ""mobilePhone"":""mobile_phone"",
            ""homePhone"":""home_phone"",
            ""workPhone"":""work_phone""
        }},
        ""policyMapping"":{{         
            ""customerEmail"":""email"",
            ""customerName"":""pref_name"",
            ""policyNumber"":""policy_no"",
            ""inceptionDate"":""inception_date"",
            ""cancellationDate"":""cancellation_date"",
            ""expiryDate"":""expiry_date"",
            ""agentEmail"":""agentEmail"",
            ""formData"":{{             
                ""formModel"":{{ 
                    ""policyStartDate"":""inception_date"",
                    ""policyInceptionDate"":""inception_date"",
                    ""CancellationEffectiveDate"":""cancellation_date"",
                    ""policyEndDate"":""expiry_date"",
                    ""policyExpiryDate"":""expiry_date""
                }}
            }},
            ""calculationResult"":{{
                ""state"":""rating_state"",
                ""payment"":{{
                    ""total"":{{ 
                        ""premium"":""total_premium"",
                        ""esl"":""total_esl"",
                        ""gst"":""total_gst"",
                        ""stampDuty"":""total_stamp_duty"",
                        ""serviceFees"":""total_service_fees"",
                        ""interest"":""total_interest"",
                        ""merchantFees"":""total_merchant_fees"",
                        ""transactionCosts"":""total_transaction_costs"",
                        ""payable"":""total_payable""
                    }},
                    ""instalments"":{{
                        ""instalmentsPerYear"":""installment_per_year"",
                        ""instalmentAmount"":""installment_amount""
                    }}
                }},
                ""questions"": {{ }}
            }}
        }}
    }}";
        }

        /// <summary>
        /// Request to have a simple JSON payload string.
        /// </summary>
        /// <param name="emailAddress">The sample email address.</param>
        /// <returns>A string containing a sample JSON payload.</returns>
        public static string GenerateCustomerCompleteImportJson(string emailAddress = "brayden.matters@uol.com.br")
        {
            return @"{ 
              ""data"":[ 
                { 
                  ""name"":""Brayden Matters"",
                  ""pref_name"":""Brayden"",
                  ""email"":""" + emailAddress + @""",
                  ""alt_email"":""brayden.matters@nature.com"",
                  ""mobile_phone"":""897-680-2374"",
                  ""home_phone"":""647-108-1776"",
                  ""work_phone"":""565-702-6032""
                }
              ],
              ""customerMapping"":{
                ""fullName"":""name"",
                ""preferredName"":""pref_name"",
                ""email"":""email"",
                ""alternativeEmail"":""alt_email"",
                ""mobilePhone"":""mobile_phone"",
                ""homePhone"":""home_phone"",
                ""workPhone"":""work_phone""
              }
            }";
        }

        /// <summary>
        /// Request to have a simple JSON payload string.
        /// </summary>
        /// <param name="emailAddress">The sample email address.</param>
        /// <returns>A string containing a sample JSON payload.</returns>
        public static string GenerateCustomerIncompleteImportJson(string emailAddress = "brayden.matters@uol.com.br")
        {
            return @"{ 
              ""data"":[ 
                { 
                  ""pref_name"":""Brayden"",
                  ""email"":""" + emailAddress + @""",
                  ""alt_email"":""brayden.matters@nature.com"",
                  ""mobile_phone"":""897-680-2374"",
                  ""home_phone"":""647-108-1776"",
                  ""work_phone"":""565-702-6032""
                }
              ],
              ""customerMapping"":{
                ""preferredName"":""pref_name"",
                ""email"":""email"",
                ""alternativeEmail"":""alt_email"",
                ""mobilePhone"":""mobile_phone"",
                ""homePhone"":""home_phone"",
                ""workPhone"":""work_phone""
              }
            }";
        }

        /// <summary>
        /// Request to have a simple JSON payload string.
        /// </summary>
        /// <param name="policyNumber">The policy number for the sample data.</param>
        /// <returns>A string containing a sample JSON payload.</returns>
        public static string GeneratePolicyCompleteImportJson(string policyNumber = "P0003")
        {
            return @"{ 
              ""data"":[ 
                { 
                  ""name"":""Brayden Matters"",
                  ""pref_name"":""Brayden"",
                  ""email"":""brayden.matters@uol.com.br"",
                  ""alt_email"":""brayden.matters@nature.com"",
                  ""mobile_phone"":""897-680-2374"",
                  ""home_phone"":""647-108-1776"",
                  ""work_phone"":""565-702-6032"",
                  ""policy_no"":""" + policyNumber + @""",
                  ""inception_date"":""20/08/19"",
                  ""cancellation_date"":""20/08/19"",
                  ""expiry_date"":""20/09/19"",
                  ""total_premium"":""1.0"",
                  ""total_esl"":""2.0"",
                  ""total_gst"":""3.0"",
                  ""total_stamp_duty"":""4.0"",
                  ""total_service_fees"":""5.0"",
                  ""total_interest"":""6.0"",
                  ""total_merchant_fees"":""7.0"",
                  ""total_transaction_costs"":""8.0"",
                  ""total_payable"":""9.0"",
                  ""installment_per_year"":""5"",
                  ""installment_amount"":""5.0"",
                  ""incident_date"":""09/09/19"",
                  ""descr"":""Sample description for P0003"",
                  ""partner_id"":""123"",
                  ""occupation"":""CEO"",
                  ""pet_name"":""b"",
                  ""rating_state"":""VIC"",
                  ""question_travel_cover"":""No""
                }
              ],
              ""policyMapping"":{ 
                ""customerEmail"":""email"",
                ""customerName"":""pref_name"",
                ""policyNumber"":""policy_no"",
                ""inceptionDate"":""inception_date"",
                ""cancellationDate"":""cancellation_date"",
                ""expiryDate"":""expiry_date"",
                ""formData"":{ 
                  ""formModel"":{ 
                    ""partnerId"":""partner_id"",
                    ""occupation"":""occupation"",
                    ""pets"":{ 
                      ""name"":""pet_name""
                    },
                    ""policyStartDate"":""inception_date"",
                    ""policyInceptionDate"":""inception_date"",
                    ""CancellationEffectiveDate"":""inception_date"",
                    ""CancellationEffectiveDate"":""cancellation_date"",
                    ""policyEndDate"":""expiry_date"",
                    ""policyExpiryDate"":""expiry_date""
                  }
                },
                ""calculationResult"":{ 
                  ""state"":""rating_state"",
                  ""payment"":{ 
                    ""total"":{ 
                      ""premium"":""total_premium"",
                      ""esl"":""total_esl"",
                      ""gst"":""total_gst"",
                      ""stampDuty"":""total_stamp_duty"",
                      ""serviceFees"":""total_service_fees"",
                      ""interest"":""total_interest"",
                      ""merchantFees"":""total_merchant_fees"",
                      ""transactionCosts"":""total_transaction_costs"",
                      ""payable"":""total_payable""
                    },
                    ""instalments"":{ 
                      ""instalmentsPerYear"":""installment_per_year"",
                      ""instalmentAmount"":""installment_amount""
                    }
                  },
                  ""questions"":{ 
                    ""ratingPrimary"":{ 
                      ""ratingState"":""rating_state""
                    },
                    ""ratingSecondary"":{ 
                      ""travelCover"":""question_travel_cover"",
                      ""policyStartDate"":""inception_date""
                    }
                  }
                }
              }
            }";
        }

        /// <summary>
        /// Request to have a simple JSON payload string.
        /// </summary>
        /// <param name="policyNumber">The sample policy number.</param>
        /// <returns>A string containing a sample JSON payload.</returns>
        public static string GeneratePolicyIncompleteImportJson(string policyNumber = "P0003")
        {
            return @"{ 
              ""data"":[ 
                { 
                  ""name"":""Brayden Matters"",
                  ""pref_name"":""Brayden"",
                  ""email"":""brayden.matters@uol.com.br"",
                  ""alt_email"":""brayden.matters@nature.com"",
                  ""mobile_phone"":""897-680-2374"",
                  ""home_phone"":""647-108-1776"",
                  ""work_phone"":""565-702-6032"",
                  ""policy_no"":""" + policyNumber + @""",
                  ""inception_date"":""20/08/2019"",
                  ""cancellation_date"":""20/08/2019"",
                  ""expiry_date"":""20/09/2019"",
                  ""total_premium"":""1.0"",
                  ""total_esl"":""2.0"",
                  ""total_gst"":""3.0"",
                  ""total_stamp_duty"":""4.0"",
                  ""total_service_fees"":""5.0"",
                  ""total_interest"":""6.0"",
                  ""total_merchant_fees"":""7.0"",
                  ""total_transaction_costs"":""8.0"",
                  ""total_payable"":""9.0"",
                  ""installment_per_year"":""5"",
                  ""installment_amount"":""5.0"",
                  ""incident_date"":""09/09/19"",
                  ""descr"":""Sample description for P0003"",
                  ""partner_id"":""123"",
                  ""occupation"":""CEO"",
                  ""pet_name"":""b"",
                  ""rating_state"":""VIC"",
                  ""question_travel_cover"":""No""
                }
              ],
              ""policyMapping"":{ 
                ""policyNumber"":""policy_no"",
                ""inceptionDate"":""inception_date"",
                ""cancellationDate"":""cancellation_date"",
                ""expiryDate"":""expiry_date"",
                ""formData"":{ 
                  ""formModel"":{ 
                    ""partnerId"":""partner_id"",
                    ""occupation"":""occupation"",
                    ""pets"":{ 
                      ""name"":""pet_name""
                    },
                    ""policyStartDate"":""inception_date"",
                    ""policyInceptionDate"":""inception_date"",
                    ""CancellationEffectiveDate"":""cancellation_date"",
                    ""policyEndDate"":""expiry_date"",
                    ""policyExpiryDate"":""expiry_date""
                  }
                },
                ""calculationResult"":{ 
                  ""state"":""rating_state"",
                  ""payment"":{ 
                    ""total"":{ 
                      ""premium"":""total_premium"",
                      ""esl"":""total_esl"",
                      ""gst"":""total_gst"",
                      ""stampDuty"":""total_stamp_duty"",
                      ""serviceFees"":""total_service_fees"",
                      ""interest"":""total_interest"",
                      ""merchantFees"":""total_merchant_fees"",
                      ""transactionCosts"":""total_transaction_costs"",
                      ""payable"":""total_payable""
                    },
                    ""instalments"":{ 
                      ""instalmentsPerYear"":""installment_per_year"",
                      ""instalmentAmount"":""installment_amount""
                    }
                  },
                  ""questions"":{ 
                    ""ratingPrimary"":{ 
                      ""ratingState"":""rating_state""
                    },
                    ""ratingSecondary"":{ 
                      ""travelCover"":""question_travel_cover"",
                      ""policyStartDate"":""inception_date""
                    }
                  }
                }
              }
            }";
        }

        /// <summary>
        /// Request to have a simple JSON payload string.
        /// </summary>
        /// <param name="policyNumber">The sample policy number.</param>
        /// <returns>A string containing a sample JSON payload.</returns>
        public static string GenerateClaimCompleteImportJson(string policyNumber = "P0003")
        {
            return @"{ 
              ""data"":[ 
                { 
                  ""policy_no"":""" + policyNumber + @""",
                  ""incident_date"":""09/09/19"",
                  ""descr"":""Sample description for P0003"",
                  ""notified_date"":""09/09/19"",
                  ""amount"":""1000.00""
                }
              ],
              ""claimMapping"":{ 
                ""policyNumber"":""policy_no"",
                ""claimNumber"":""policy_no"",
                ""incidentDate"":""incident_date"",
                ""notifiedDate"":""notified_date"",
                ""description"":""descr"",
                ""amount"":""amount"",
                ""notifiedDate"":""notified_date""
              }
            }";
        }

        /// <summary>
        /// Request to have a simple JSON payload string.
        /// </summary>
        /// <param name="policyNumber">The sample policy number.</param>
        /// <returns>A string containing a sample JSON payload.</returns>
        public static string GenerateClaimCompleteImportJsonV2(string policyNumber = "P0003")
        {
            return @"{ 
              ""data"":[ 
                { 
                  ""policy_no"":""" + policyNumber + @""",
                  ""incident_date"":""10/10/19"",
                  ""descr"":""New description"",
                  ""notified_date"":""09/09/19"",
                  ""amount"":""1000.00""
                }
              ],
              ""claimMapping"":{ 
                ""policyNumber"":""policy_no"",
                ""claimNumber"":""policy_no"",
                ""incidentDate"":""incident_date"",
                ""notifiedDate"":""notified_date"",
                ""description"":""descr"",
                ""amount"":""amount"",
                ""notifiedDate"":""notified_date""
              }
            }";
        }

        /// <summary>
        /// Request to have a simple JSON payload string.
        /// </summary>
        /// <param name="policyNumber">The sample policy number.</param>
        /// <returns>A string containing a sample JSON payload.</returns>
        public static string GenerateClaimIncompleteImportJson(string policyNumber = "P0003")
        {
            return @"{ 
              ""data"":[ 
                { 
                  ""policy_no"":""" + policyNumber + @""",
                  ""incident_date"":""09/09/19"",
                  ""descr"":""Sample description for P0003""
                }
              ],
              ""claimMapping"":{ 
                ""referenceNumber"":""policy_no"",
                ""incidentDate"":""incident_date"",
                ""notifiedDate"":""notified_date"",
                ""description"":""descr"",
                ""amount"":""amount""
              }
            }";
        }
    }
}
