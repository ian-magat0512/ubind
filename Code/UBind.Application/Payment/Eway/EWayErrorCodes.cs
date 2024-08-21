// <copyright file="EWayErrorCodes.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Eway
{
    using System.Collections.Generic;

    /// <summary>
    /// Mapping error codes to descriptions.
    /// </summary>
    public class EwayErrorCodes
    {
        private Dictionary<string, string> dictionary = new Dictionary<string, string>
        {
            { "V6000", "Validation error" },
            { "V6001", "Invalid CustomerIP" },
            { "V6002", "Invalid DeviceID" },
            { "V6003", "Invalid Request PartnerID" },
            { "V6004", "Invalid Request Method" },
            { "V6010", "Invalid TransactionType, account not certified for eCome only MOTO or Recurring available" },
            { "V6011", "Invalid Payment TotalAmount" },
            { "V6012", "Invalid Payment InvoiceDescription" },
            { "V6013", "Invalid Payment InvoiceNumber" },
            { "V6014", "Invalid Payment InvoiceReference" },
            { "V6015", "Invalid Payment CurrencyCode" },
            { "V6016", "Payment Required" },
            { "V6017", "Payment CurrencyCode Required" },
            { "V6018", "Unknown Payment CurrencyCode" },
            { "V6021", "Cardholder Name Required" }, // EWAY_CARDHOLDERNAME Required
            { "V6022", "Card Number Required" }, // EWAY_CARDNUMBER Required
            { "V6023", "Card CVN Required" }, // EWAY_CARDCVN Required
            { "V6033", "Invalid Expiry Date" },
            { "V6034", "Invalid Issue Number" },
            { "V6035", "Invalid Valid From Date" },
            { "V6040", "Invalid TokenCustomerID" },
            { "V6041", "Customer Required" },
            { "V6042", "Customer FirstName Required" },
            { "V6043", "Customer LastName Required" },
            { "V6044", "Customer CountryCode Required" },
            { "V6045", "Customer Title Required" },
            { "V6046", "TokenCustomerID Required" },
            { "V6047", "RedirectURL Required" },
            { "V6048", "CheckoutURL Required when CheckoutPayment specified" },
            { "V6049", "Invalid Checkout URL" },
            { "V6051", "Invalid Customer FirstName" },
            { "V6052", "Invalid Customer LastName" },
            { "V6053", "Invalid Customer CountryCode" },
            { "V6058", "Invalid Customer Title" },
            { "V6059", "Invalid RedirectURL" },
            { "V6060", "Invalid TokenCustomerID" },
            { "V6061", "Invalid Customer Reference" },
            { "V6062", "Invalid Customer CompanyName" },
            { "V6063", "Invalid Customer JobDescription" },
            { "V6064", "Invalid Customer Street1" },
            { "V6065", "Invalid Customer Street2" },
            { "V6066", "Invalid Customer City" },
            { "V6067", "Invalid Customer State" },
            { "V6068", "Invalid Customer PostalCode" },
            { "V6069", "Invalid Customer Email" },
            { "V6070", "Invalid Customer Phone" },
            { "V6071", "Invalid Customer Mobile" },
            { "V6072", "Invalid Customer Comments" },
            { "V6073", "Invalid Customer Fax" },
            { "V6074", "Invalid Customer URL" },
            { "V6075", "Invalid Shipping Address First Name" },
            { "V6076", "Invalid Shipping Address Last Name" },
            { "V6077", "Invalid Shipping Address Street 1" },
            { "V6078", "Invalid Shipping Address Street 2" },
            { "V6079", "Invalid Shipping Address City" },
            { "V6080", "Invalid Shipping Address State" },
            { "V6081", "Invalid Shipping Address Postal Code" },
            { "V6082", "Invalid Shipping Address Email" },
            { "V6083", "Invalid Shipping Address Phone" },
            { "V6084", "Invalid Shipping Address Country" },
            { "V6085", "Invalid Shipping Address Shipping Method" },
            { "V6086", "Invalid Shipping Address Fax" },
            { "V6091", "Unknown Customer Country Code" },
            { "V6092", "Unknown Shipping Address Country Code" },
            { "V6100", "Invalid Card Name" }, // Invalid EWAY_CARDNAME
            { "V6101", "Invalid Card Expiry Month" }, // Invalid EWAY_CARDEXPIRYMONTH
            { "V6102", "Invalid Card Expiry Year" }, // Invalid EWAY_CARDEXPIRYYEAR
            { "V6103", "Invalid Card Start Month" }, // Invalid EWAY_CARDSTARTMONTH
            { "V6104", "Invalid Card Start Year" }, // Invalid EWAY_CARDSTARTYEAR
            { "V6105", "Invalid Card Issue number" }, // Invalid EWAY_CARDISSUENUMBER
            { "V6106", "Invalid Card CVN" }, // EWAY_CARDCVN
            { "V6107", "Invalid Access Code" }, // EWAY_ACCESSCODE
            { "V6108", "Invalid Customer Host Address" },
            { "V6109", "Invalid User Agent" },
            { "V6110", "Invalid Card Number" }, // EWAY_CARDNUMBER
            { "V6111", "Unauthorised API Access, Account Not PCI Certified" },
            { "V6112", "Redundant card details other than expiry year and month" },
            { "V6113", "Invalid transaction for refund" },
            { "V6114", "Gateway validation error" },
            { "V6115", "Invalid Direct Refund Request, Transaction ID" },
            { "V6116", "Invalid card data on original TransactionID" },
            ////{ "V6117", "Invalid CreateAccessCodeSharedRequest, FooterText" },
            ////{ "V6118", "Invalid CreateAccessCodeSharedRequest, HeaderText" },
            ////{ "V6119", "Invalid CreateAccessCodeSharedRequest, Language" },
            ////{ "V6120", "Invalid CreateAccessCodeSharedRequest, LogoUrl" },
            ////{ "V6121", "Invalid TransactionSearch, Filter Match Type" },
            ////{ "V6122", "Invalid TransactionSearch, Non numeric Transaction ID" },
            ////{ "V6123", "Invalid TransactionSearch,no TransactionID or AccessCode specified" },
            { "V6124", "Invalid Line Items. The line items have been provided however the totals do not match the TotalAmount field" },
            { "V6125", "Selected Payment Type not enabled" },
            { "V6126", "Invalid encrypted card number, decryption failed" },
            { "V6127", "Invalid encrypted cvn, decryption failed" },
            { "V6128", "Invalid Method for Payment Type" },
            { "V6129", "Transaction has not been authorised for Capture/Cancellation" },
            { "V6130", "Generic customer information error" },
            { "V6131", "Generic shipping information error" },
            { "V6132", "Transaction has already been completed or voided, operation not permitted" },
            { "V6133", "Checkout not available for Payment Type" },
            { "V6134", "Invalid Auth Transaction ID for Capture/Void" },
            { "V6135", "PayPal Error Processing Refund" },
            { "V6140", "Merchant account is suspended" },
            { "V6141", "Invalid PayPal account details or API signature" },
            { "V6142", "Authorise not available for Bank/Branch" },
            { "V6143", "Invalid Public Key" },
            { "V6146", "Client Side Encryption Key Missing or Invalid" },
            { "V6147", "Unable to Create One Time Code for Secure Field" },
            { "V6148", "Secure Field has Expired" },
            { "V6149", "Invalid Secure Field One Time Code" },
            { "V6150", "Invalid Refund Amount" },
            { "V6151", "Refund amount greater than original transaction" },
            { "V6152", "Original transaction already refunded for total amount" },
            { "V6153", "Card type not support by merchant" },
            { "V6154", "Insufficent Funds Available For Refund" },
            { "V6160", "Encryption Method Not Supported" },
            { "V6161", "Encryption failed, missing or invalid key" },
            { "V6165", "Invalid Visa Checkout data or decryption failed" },
            { "V6170", "Invalid TransactionSearch, Invoice Number is not unique" },
            { "V6171", "Invalid TransactionSearch, Invoice Number not found" },
            { "V6501", "Invalid Amex InstallementPlan" },
            { "V6502", "Invalid Number Of Installements for Amex. Valid values are from 0 to 99 inclusive" },
            { "V6503", "Merchant Amex ID required" },
            { "V6504", "Invalid Merchant Amex ID" },
            { "V6505", "Merchant Terminal ID required" },
            { "V6506", "Merchant category code required" },
            { "V6507", "Invalid merchant category code" },
            { "V6508", "Amex 3D ECI required" },
            { "V6509", "Invalid Amex 3D ECI" },
            { "V6510", "Invalid Amex 3D verification value" },
            { "V6511", "Invalid merchant location data" },
            { "V6512", "Invalid merchant street address" },
            { "V6513", "Invalid merchant city" },
            { "V6514", "Invalid merchant country" },
            { "V6515", "Invalid merchant phone" },
            { "V6516", "Invalid merchant postcode" },
            { "V6517", "Amex connection error" },
            { "V6518", "Amex EC Card Details API returned invalid data" },
            { "V6519", "Invalid Amex Express Checkout Encryption Request" },
            { "V6520", "Invalid or missing Amex Point Of Sale Data" },
            { "V6521", "Invalid or missing Amex transaction date time" },
            { "V6522", "Invalid or missing Amex Original transaction date time" },
            { "V6530", "Credit Card Number in non Credit Card Field" },
            { "S9990", "Rapid endpoint not set or invalid" },
            { "S9901", "Response is not JSON" },
            { "S9902", "Empty response" },
            { "S9991", "Rapid API key or password not set" },
            { "S9992", "Error connecting to Rapid gateway" },
            { "S9993", "Authentication error" },
            { "S9995", "Error converting to or from JSON, invalid parameter" },
            { "S9996", "Rapid gateway server error" },
        };

        /// <summary>
        /// Gets the description for a give error code.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <returns>The description.</returns>
        public string this[string code]
        {
            get
            {
                return this.dictionary.TryGetValue(code, out string message)
                    ? message
                    : $"Payment Error ({code})";
            }
        }
    }
}
