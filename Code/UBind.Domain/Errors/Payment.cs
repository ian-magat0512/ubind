// <copyright file="Payment.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

#pragma warning disable SA1600
#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace UBind.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Extensions;
    using UBind.Domain.Funding;
    using UBind.Domain.Helpers;
    using UBind.Domain.Product;

    /// <summary>
    /// Allows enumeration of all application errors as outlined here: https://enterprisecraftsmanship.com/posts/advanced-error-handling-techniques/.
    /// </summary>
    public static partial class Errors
    {
        /// <summary>
        /// Payment related errors.
        /// </summary>
        public static class Payment
        {
            public static Error InvalidCardNumber() =>
                new Error(
                    "payment.invalid.card.number",
                    "Invalid card number",
                    "The payment card number you entered is not valid. Please check the number and try again.",
                    HttpStatusCode.BadRequest);

            public static Error CardPaymentFailed(IEnumerable<string> providerErrors, PaymentDetails? paymentDetails = null)
            {
                var data = new JObject();
                if (paymentDetails != null)
                {
                    data = new JObject()
                    {
                         { "Payload Request", paymentDetails.Request },
                         { "Payload Response", paymentDetails.Response },
                    };
                }

                return new Error(
                    "payment.card.payment.failed",
                    "Card payment failed",
                    "There was a problem processing your credit card. Please review the failure details provided and "
                    + "please feel free to try again if your credit card details have been entered incorrectly. "
                    + "If you need help resolving this issue, kindly report the details to our customer support team as "
                    + "this will assist us in diagnosing the issue. ",
                    HttpStatusCode.PaymentRequired,
                    providerErrors,
                    data);
            }

            public static Error NotConfigured(IProductContext productContext) =>
                new Error(
                    "payment.not.configured",
                    "Payment options not configured",
                    $"Payment options have not been configured for the environment '{productContext.Environment}' for the product '{productContext.ProductId}'.",
                    HttpStatusCode.NotFound);

            public static Error InvalidConfiguration(IProductContext productContext, string reason) =>
                new Error(
                    "payment.invalid.configuration",
                    "The payment configuration is invalid or incomplete",
                    $"The payment configuration for the environment '{productContext.Environment}' "
                    + $"and product '{productContext.ProductId}' is invalid or incomplete: "
                    + reason.WithDot().WithSpace(),
                    HttpStatusCode.PreconditionFailed);

            public static Error PaymentAlreadyMade() =>
                new Error(
                    "payment.already.made",
                    "Payment already made",
                    "It looks like you've already made payment, so to make sure we don't charge you twice we're stopping this payment attempt.",
                    HttpStatusCode.Conflict);

            public static Error MethodNotSupported() =>
                new Error(
                    "payment.method.not.supported",
                    "Payment method not supported",
                    "You're trying to pay using an unsupported payment method. "
                    + "Please try another payment method, or get in touch with customer support.",
                    HttpStatusCode.MethodNotAllowed);

            public static Error CouldNotObtainAccessToken(string errorMessage) =>
                new Error(
                    "payment.could.not.obtain.access.token",
                    "Could not obtain an access token from the payment provider",
                    $"When trying to process a payment, we ran into a problem obtaining an access token "
                    + "from the payment gateway provider. This is a likely configuration issue which a product "
                    + "developer would need to fix. Please check that the credentials are correct and up to date.",
                    HttpStatusCode.BadRequest,
                    new List<string>
                    {
                        { errorMessage },
                    });

            public static Error SurchargeRequestFailed(IEnumerable<string> errorMessages, string requestPayload = "", string responsePayload = "") =>
                new Error(
                    "payment.surcharge.request.failed",
                    "Could not obtain surcharge amount from payment provider",
                    "There was a problem processing the surcharge amount from the payment gateway provider. "
                    + "This is likely a configuration issue which a product developer would need to fix. ",
                    HttpStatusCode.BadRequest,
                    errorMessages,
                    new JObject()
                    {
                        { "RequestPayload", requestPayload },
                        { "ResponsePayload", responsePayload },
                    });

            public static Error TokenisationFailure(IEnumerable<string> errorMessages) =>
                new Error(
                    "payment.request.failed",
                    "Payment request blocked due to tokenisation error.",
                    $"When trying to process a payment, we ran into a problem when processing the card "
                    + "details. There was an issue with the payment request. This is likely an issue with the system "
                    + "instead of your credit card/bank account. Please get in touch with customer support for assistance.",
                    HttpStatusCode.BadRequest,
                    errorMessages);

            public static Error CardCaptureFailure() =>
                new Error(
                    "payment.card.capture.failed",
                    "Payment request failed",
                    $"When trying to process a payment, we ran into a problem when processing the card "
                    + "details. There was an issue with the card capture request. This is likely an issue with the system "
                    + "instead of your credit card/bank account. Please get in touch with customer support for assistance",
                    HttpStatusCode.PreconditionFailed);

            public static Error PaymentDetailsNotFound(string paymentType = "payment") =>
                new Error(
                    "payment.details.not.found",
                    "Payment request failed",
                    $"When trying to process a payment, we ran into a problem when processing the {paymentType} "
                    + $"details. No {paymentType} details provided. Please get in touch with customer support for assistance",
                    HttpStatusCode.PreconditionFailed);

            public static Error PaymentGatewayResponseNotFound(string paymentType = "payment") =>
                new Error(
                    "payment.gateway.response.not.found",
                    "Payment gateway response not found",
                    $"When trying to process a payment, we ran into a problem when persisting the {paymentType} "
                    + $"details. The payment gateway response is missing. Please get in touch with customer support for assistance",
                    HttpStatusCode.BadRequest);

            // internal error only.
            public static Error PaymentPending(string transactionId) =>
                new Error(
                    "payment.request.pending",
                    "Credit card payment still being processed",
                    $"Payment transaction for item {transactionId} is still being processed.");

            public static class Funding
            {
                public static Error CouldNotObtainAccessToken(JObject? data = null) =>
                new Error(
                    "payment.could.not.obtain.access.token",
                    "Could not obtain an access token from the funding premium provider",
                    $"When trying to process a payment, we ran into a problem obtaining an access token "
                    + "from the funding premium provider. This is a likely configuration issue which a product "
                    + "developer would need to fix. Please check that the credentials are correct and up to date.",
                    HttpStatusCode.BadRequest,
                    data: data);

                public static Error NotConfigured(DeploymentEnvironment environment, Guid productId, bool isMutual)
                {
                    var title = "Funding not configured";
                    var message = $"Premium funding has not been configured for the environment '{environment}' for the product '{productId}'.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        "payment.funding.not.configured",
                        title,
                        message,
                        HttpStatusCode.NotFound);
                }

                public static Error InvalidConfiguration(DeploymentEnvironment environment, Guid productId)
                {
                    var title = "Funding Configuration is Invalid";
                    var message = "A premium funding payment configuration was included in the product configuration, "
                        + "however the premium funding provider type was not recognised, or the configuration is invalid. "
                        + "Please review the premium funding configuration to ensure it is correct, and try again";
                    return new Error(
                        "payment.funding.configuration.invalid",
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        data: new JObject()
                        {
                            { "environment", environment.ToString() },
                            { "productId", productId },
                        });
                }

                public static Error ProviderError(IEnumerable<string> errors, bool isMutual)
                {
                    var title = "Funding Provider Error";
                    var message = "The premium funding provider has returned one of more errors";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        "payment.funding.provider.error",
                        title,
                        message,
                        HttpStatusCode.Conflict,
                        errors);
                }

                public static Error UnsupportedTransactionType(string transactionTypeName, bool isMutual)
                {
                    var title = "Funding not supported for transaction";
                    var message = $"Premium funding operations for {transactionTypeName} transactions are not supported.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        "payment.funding.unsupported.transaction.type",
                        title,
                        message);
                }

                public static Error AmountDoesNotEqualTotalPayable(decimal? totalPayable, decimal? fundedAmount, bool isMutual)
                {
                    var title = "Premium funding calculations are out";
                    var message = $"When calculating the premium funding amount, the amount to be funded came to {fundedAmount}, "
                        + $"yet the total payable is {totalPayable}. The amounts need to match.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        "payment.funding.amount.not.equals.total.payable",
                        title,
                        message,
                        HttpStatusCode.ExpectationFailed,
                        null,
                        new JObject()
                        {
                            { "totalPayable", totalPayable.ToString() },
                            { "fundedAmount", fundedAmount.ToString() },
                        });
                }

                public static Error FurtherInformationRequired(string informationName, bool isMutual)
                {
                    var title = "Information required";
                    var message = $"We could not calculate premium funding pricing, because the {informationName} has not been set.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        "payment.funding.information.required",
                        title,
                        message,
                        HttpStatusCode.PreconditionFailed);
                }

                public static Error NoCustomerFound(bool isMutual)
                {
                    var title = "Customer record needed for premium funding";
                    var message = "Funding cannot be requested when there is no customer record for the quote.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        "payment.funding.no.customer.record",
                        title,
                        message,
                        HttpStatusCode.NotFound);
                }

                public static Error TotalPayableAmountMissing(bool isMutual)
                {
                    var title = "Total payable amount missing";
                    var message = "Premium funding contracts require a total payable amount, however it's missing from the price breakdown.";
                    TenantHelper.CheckAndChangeTextToMutualForErrorObject(ref title, ref message, isMutual);
                    return new Error(
                        "payment.funding.total.payable.amount.missing",
                        title,
                        message,
                        HttpStatusCode.NotFound);
                }

                public static Error AcceptFundingProposalNotSupported(Guid quoteId, string fundingService) =>
                    new Error(
                        "funding.proposal.not.supported",
                        "We were unable to complete the funding proposal for your quote",
                        "While trying to complete your quote, we encountered an issue that prevents us from completing the transaction. "
                        + $"{fundingService} only supports accepting funding proposals externally. "
                        + "We apologise for the inconvenience. "
                        + "This is likely a configuration issue which a product developer would need to fix. "
                        + "Please get in touch with customer support.",
                        HttpStatusCode.BadRequest,
                        new List<string>()
                        {
                            $"Quote Id: {quoteId}",
                        });

                public static Error FundingProposalMismatch(Guid requestFundingProposalId, Guid latestFundingProposalId) => new Error(
                   "payment.funding.proposal.mismatch",
                   "Funding proposal mismatch",
                   $"A request was made to retrieve funding proposal with ID \"{requestFundingProposalId}\" however "
                   + $"the latest recorded funding proposal has ID \"{latestFundingProposalId}\". There may have "
                   + "been a delay in storing the funding proposal, so please retry.",
                   HttpStatusCode.Conflict);

                public static class RedPlanetPremiumFunding
                {
                    public static Error ApiRequestFailed(
                        string requestUrl,
                        string rawResponse,
                        HttpStatusCode statusCode,
                        RedPlanetFundingType fundingType,
                        JObject data)
                    {
                        var additionalDetails = new List<string>
                        {
                            $"Reason: {rawResponse}",
                            $"Request URL: {requestUrl}",
                        };

                        return new Error(
                            "red.planet.api.request.failed",
                            "A request to a third party premium funding provider failed",
                            $"When trying to obtain a premium funding proposal using the \"{fundingType}\" "
                            + "premium funding provider, the attempt failed because an associated request "
                            + "to the \"Red Planet\" API returned an error. "
                            + "To resolve this,  we would appreciate if you could report the details of this situation "
                            + "to our customer support team, as this will assist us in diagnosing the issue. "
                            + "We apologise for the inconvenience.",
                            statusCode,
                            additionalDetails,
                            data);
                    }

                    public static Error ApiRequestTimedOut(
                        string requestUrl,
                        string? externalQuoteNumber,
                        RedPlanetFundingType fundingType,
                        JObject data)
                    {
                        var additionalDetails = new List<string>
                        {
                            $"Request URL: {requestUrl}",
                        };

                        if (externalQuoteNumber != null)
                        {
                            additionalDetails.Add($"External Quote Number: {externalQuoteNumber}");
                        }

                        return new Error(
                            "red.planet.api.request.timed.out",
                            "A request to a third party premium funding provider timed out",
                            $"When trying to obtain a premium funding proposal using the \"{fundingType}\" "
                            + "premium funding provider, the attempt failed because an associated request "
                            + "to the \"Red Planet\" API timed out. "
                            + "To resolve this, we would appreciate if you could report the details of this situation "
                            + "to our customer support team, as this will assist us in diagnosing the issue. "
                            + "We apologise for the inconvenience.",
                            HttpStatusCode.GatewayTimeout,
                            additionalDetails,
                            data);
                    }

                    public static Error UnableToRetrieveFundingProposal(JObject? data = null)
                    {
                        return new Error(
                            "red.planet.funding.proposal.was.not.retrieved",
                            "Unable to retrieve Funding Proposal",
                            "When trying to accept the funding proposal, we were unable to retrieve the latest funding proposal. "
                            + "To resolve this, we would appreciate if you could report the details of this situation "
                            + "to our customer support team, as this will assist us in diagnosing the issue. "
                            + "We apologise for the inconvenience.",
                            HttpStatusCode.BadRequest,
                            data: data);
                    }

                    public static Error CreditCardTypeIsNotSupported(RedPlanetFundingType fundingType, IEnumerable<string> supportedCreditCardTypes, JObject? data = null)
                    {
                        var creditCardTypes = string.Join(", ", supportedCreditCardTypes.Select(x => "\"" + x + "\""));
                        return new Error(
                            "red.planet.credit.card.type.not.supported",
                            "Credit card type is not supported",
                            "When trying to determine the credit card type, "
                            + "the specified credit card number did not match the supported credit card types "
                            + $"from \"{fundingType}\" Premium Funding provider ({creditCardTypes}). "
                            + $"To resolve this issue, please ensure that you provide a valid credit card number from the supported types listed. "
                            + "We apologise for the inconvenience. If you need further assistance please contact customer support.",
                            HttpStatusCode.BadRequest,
                            data: data);
                    }

                    public static Error ProposalExternalQuoteNumberWasNotCreated(RedPlanetFundingType fundingType, JObject? data)
                    {
                        return new Error(
                            "red.planet.external.quote.number.was.not.created",
                            "External Quote Number was not created",
                            $"We were unable to retrieve an external quote number from \"{fundingType}\" Premium Funding provider. "
                            + "We would appreciate if you could report the details of this situation "
                            + "to our customer support team, as this will assist us in diagnosing the issue. "
                            + "We apologise for the inconvenience.",
                            HttpStatusCode.BadRequest,
                            data: data);
                    }

                    public static Error UnableToRetrieveContractDocumentUrl(RedPlanetFundingType fundingType, JObject? data = null)
                    {
                        return new Error(
                            "red.planet.funding.proposal.contract.url.was.not.retrieved",
                            "Unable to retrieve Funding Proposal's Contract",
                            "Contract document from the quote proposal was not retrieved."
                            + $"This is an error by \"{fundingType}\" Premium Funding provider. "
                            + "We would appreciate if you could report the details of this situation "
                            + "to our customer support team, as this will assist us in diagnosing the issue. "
                            + "We apologise for the inconvenience.",
                            HttpStatusCode.BadRequest,
                            data: data);
                    }
                }
            }

            public static class SavedPayments
            {
                public static Error CustomerHasNoSavedPayments() =>
                    new Error(
                        "payment.customer.has.no.saved.payment.methods",
                        "There are no saved payment details for the customer",
                        "When trying to process a payment, you've elected to use saved payment method/s but you do not have any "
                        + "currently persisted. Please enter your preferred payment details and try again. If this issue persists, "
                        + "please get in touch with our customer support for assistance",
                        HttpStatusCode.NotFound);

                public static Error SavedPaymentMethodNotFound(Guid savedPaymentMethodId) =>
                    new Error(
                        "payment.saved.payment.method.not.found",
                        "The saved payment method selected does not exist",
                        $"When trying to process a payment, the saved payment method with id ({savedPaymentMethodId}) "
                        + "cannot be found. Please check the selected payment method, or opt to enter different payment details",
                        HttpStatusCode.NotFound);

                public static Error SavedPaymentOwnerAndQuoteCustomerDoNotMatch(Guid quoteId) =>
                    new Error(
                        "payment.saved.payment.forbidden",
                        "The saved payment method selected is inaccessible",
                        "When trying to process a payment, the customer owning the saved payment method to be used does not match "
                        + $"the customer for the given quote (id: {quoteId}). Please contact customer support for assistance.",
                        HttpStatusCode.PreconditionFailed);
            }
        }
    }
}
