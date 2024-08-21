// <copyright file="JsonViewModelTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Export.ViewModels
{
    using UBind.Application.Export.ViewModels;
    using Xunit;

    public class JsonViewModelTests
    {
        [Fact]
        public void BracketsOperator_SupportsJsonPath()
        {
            // Arrange
            var json = this.GetSampleJson();
            var sut = new JsonViewModel(json);

            // Act
            var last4 = sut["$.payment_method_details.card.last4"];

            // Assert
            Assert.Equal("4242", last4);
        }

        [Fact]
        public void BracketsOperator_SupportsSimplePropertyNames()
        {
            // Arrange
            var json = this.GetSampleJson();
            var sut = new JsonViewModel(json);

            // Act
            var amount = sut["amount"];

            // Assert
            Assert.Equal("100", amount);
        }

        private string GetSampleJson()
        {
            return @"{
""id"": ""ch_1EICwOJywDWzpIRLNGPtEytL"",
  ""object"": ""charge"",
  ""amount"": 100,
  ""amount_refunded"": 0,
  ""application"": null,
  ""application_fee"": null,
  ""application_fee_amount"": null,
  ""balance_transaction"": ""txn_1EICwOJywDWzpIRLzfeWaaiy"",
  ""billing_details"": {
                ""address"": {
                    ""city"": null,
      ""country"": null,
      ""line1"": null,
      ""line2"": null,
      ""postal_code"": null,
      ""state"": null
                
},
    ""email"": null,
    ""name"": null,
    ""phone"": null

},
  ""captured"": true,
  ""created"": 1553597736,
  ""currency"": ""aud"",
  ""customer"": null,
  ""description"": ""b30791de-faf7-4921-b834-d272eaa5f05f"",
  ""destination"": null,
  ""dispute"": null,
  ""failure_code"": null,
  ""failure_message"": null,
  ""fraud_details"": {
            },
  ""invoice"": null,
  ""livemode"": false,
  ""metadata"": {
            },
  ""on_behalf_of"": null,
  ""order"": null,
  ""level3"": null,
  ""outcome"": {
                ""network_status"": ""approved_by_network"",
    ""reason"": null,
    ""risk_level"": ""normal"",
    ""risk_score"": 15,
    ""rule"": null,
    ""seller_message"": ""Payment complete."",
    ""type"": ""authorized""

},
  ""paid"": true,
  ""payment_intent"": null,
  ""payment_method"": null,
  ""payment_method_details"": {
                ""ach_credit_transfer"": null,
    ""ach_debit"": null,
    ""alipay"": null,
    ""bancontact"": null,
    ""bitcoin"": null,
    ""card"": {
                    ""brand"": ""visa"",
      ""checks"": {
                        ""address_line1_check"": null,
        ""address_postal_code_check"": null,
        ""cvc_check"": ""pass""

},
      ""country"": ""US"",
      ""exp_month"": 12,
      ""exp_year"": 2022,
      ""fingerprint"": ""earo9o7SsHGH4A5C"",
      ""funding"": ""credit"",
      ""last4"": ""4242"",
      ""three_d_secure"": null,
      ""wallet"": null

},
    ""card_present"": null,
    ""eps"": null,
    ""giropay"": null,
    ""ideal"": null,
    ""multibanco"": null,
    ""p24"": null,
    ""sepa_debit"": null,
    ""stripe_account"": null,
    ""type"": ""card"",
    ""wechat"": null

},
  ""receipt_email"": null,
  ""receipt_number"": null,
  ""receipt_url"": ""https://pay.stripe.com/receipts/acct_1EBF3jJywDWzpIRL/ch_1EICwOJywDWzpIRLNGPtEytL/rcpt_ElkR9F3ihhqGs3hxOQZw0sl8CW1TrHS"",
  ""refunded"": false,
  ""refunds"": {
                ""object"": ""list"",
    ""data"": [],
    ""has_more"": false,
    ""url"": ""/v1/charges/ch_1EICwOJywDWzpIRLNGPtEytL/refunds""
  },
  ""review"": null,
  ""shipping"": null,
  ""source"": {
    ""id"": ""card_1EICwMJywDWzpIRLIUiCdG4c"",
    ""object"": ""card"",
    ""account"": null,
    ""address_city"": null,
    ""address_country"": null,
    ""address_line1"": null,
    ""address_line1_check"": null,
    ""address_line2"": null,
    ""address_state"": null,
    ""address_zip"": null,
    ""address_zip_check"": null,
    ""available_payout_methods"": null,
    ""brand"": ""Visa"",
    ""country"": ""US"",
    ""currency"": null,
    ""customer"": null,
    ""cvc_check"": ""pass"",
    ""default_for_currency"": false,
    ""dynamic_last4"": null,
    ""exp_month"": 12,
    ""exp_year"": 2022,
    ""fingerprint"": ""earo9o7SsHGH4A5C"",
    ""funding"": ""credit"",
    ""last4"": ""4242"",
    ""metadata"": {},
    ""name"": null,
    ""recipient"": null,
    ""three_d_secure"": null,
    ""tokenization_method"": null,
    ""description"": null,
    ""iin"": null,
    ""issuer"": null
  },
  ""source_transfer"": null,
  ""statement_descriptor"": null,
  ""status"": ""succeeded"",
  ""transfer"": null,
  ""transfer_data"": null,
  ""transfer_group"": null,
  ""authorization_code"": null
}";
        }
    }
}
