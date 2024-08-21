// <copyright file="DeftPaymentGateway.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Payment.Deft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Flurl.Http;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Exceptions;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Payment;
    using UBind.Domain.Product;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// For making payments via the eWAY payment gateway.
    /// </summary>
    public class DeftPaymentGateway : IPaymentGateway
    {
        private const string DefaultVisaCreditCardBin = "405221";
        private const string PaymentMethodCard = "CARD";
        private readonly IDeftConfiguration configuration;
        private readonly DeftAccessTokenProvider accessTokenProvider;
        private readonly IDeftCustomerReferenceNumberGenerator crnGenerator;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeftPaymentGateway"/> class.
        /// </summary>
        /// <param name="configuration">Configuration settings.</param>
        /// <param name="accessTokenProvider">Access token provider.</param>
        /// <param name="crnGenerator">service for generating unique CRNs.</param>
        /// <param name="clock">A clock for getting the current time.</param>
        public DeftPaymentGateway(
            IDeftConfiguration configuration,
            DeftAccessTokenProvider accessTokenProvider,
            IDeftCustomerReferenceNumberGenerator crnGenerator,
            IClock clock)
        {
            this.configuration = configuration;
            this.accessTokenProvider = accessTokenProvider;
            this.crnGenerator = crnGenerator;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public bool CanCalculateMerchantFees()
        {
            return true;
        }

        /// <inheritdoc/>
        public async Task<MerchantFees> CalculateMerchantFees(
            decimal payableAmount,
            string currencyCode,
            PaymentData paymentData)
        {
            var accessToken = await this.accessTokenProvider.GetAccessToken();

            var payload = new
            {
                dbc = this.configuration.BillerCode,
                currencyCode = currencyCode,
                amount = payableAmount,
                paymentFrequency = paymentData.SinglePayment ? "ONE_OFF" : "RECURRENT",
                bin = paymentData.CardBin ?? DefaultVisaCreditCardBin,
                paymentMethod = paymentData.PaymentMethod ?? PaymentMethodCard,
            };

            try
            {
                var deftSurchargeResponse = await this.configuration.SurchargeUrl
                    .WithOAuthBearerToken(accessToken)
                    .PostJsonAsync(payload)
                    .ReceiveJson<DeftSurchargeResponse>();

                return new MerchantFees
                {
                    MerchantRequest = payload?.ToString() ?? string.Empty,
                    SchemeName = deftSurchargeResponse.SchemeName,
                    Fee = deftSurchargeResponse.Fee,
                    SurchargeAmount = deftSurchargeResponse.SurchargeAmount,
                };
            }
            catch (FlurlHttpException ex)
            {
                var errorMessages = new List<string>();
                var errorReponseToken = await ex.GetResponseJsonAsync<JToken>();

                if (errorReponseToken is JArray responseArray)
                {
                    foreach (JToken item in responseArray)
                    {
                        var message = item is JObject ? item.SelectToken("message") : null;
                        if (message != null)
                        {
                            errorMessages.Add(message.ToString());
                        }
                    }
                }
                else if (errorReponseToken is JObject responseObject)
                {
                    var message = responseObject.SelectToken("message");
                    if (message != null)
                    {
                        errorMessages.Add(message.ToString());
                    }
                }
                else
                {
                    errorMessages.Add(errorReponseToken.ToString());
                }

                throw new ExternalServiceException(Errors.Payment.SurchargeRequestFailed(errorMessages, payload.ToString() ?? string.Empty, errorReponseToken.ToString()));
            }
        }

        /// <inheritdoc />
        public async Task<PaymentGatewayResult> MakePayment(
            Quote quote, PriceBreakdown priceBreakdown, CreditCardDetails cardDetails, string reference)
        {
            bool retry = true;
            int retryCount = 0;
            DeftPaymentRequest? request = null;
            PaymentGatewayResult? paymentGatewayResult = null;
            var quoteAggregate = quote.Aggregate;
            this.ValidateConfiguration(this.configuration, quote.ProductContext);

            while (retry)
            {
                retryCount++;
                retry = false;
                try
                {
                    var calculatedMerchantFee = priceBreakdown.MerchantFees + priceBreakdown.MerchantFeesGst;
                    decimal amount = priceBreakdown.TotalPayable - calculatedMerchantFee;
                    var paymentData = new PaymentData(
                       cardDetails.Number,
                       cardDetails.Number.Length,
                       PaymentMethodCard,
                       true);

                    MerchantFees merchantFees = await this.CalculateMerchantFees(amount, priceBreakdown.CurrencyCode, paymentData);
                    if (merchantFees.Total != calculatedMerchantFee)
                    {
                        var merchantRequestJson = merchantFees.MerchantRequest != null ? JsonConvert.SerializeObject(merchantFees.MerchantRequest) : null;
                        var merchantPaymentDetails = new PaymentDetails(
                            PaymentGatewayName.Deft, priceBreakdown.TotalPayable, reference, merchantRequestJson, string.Empty);
                        var errorMessage = $"Incorrect surcharge, expected merchant fee: {calculatedMerchantFee}, "
                            + $"got total merchant fee from the gateway: {merchantFees.Total}. "
                            + "Please ensure you recalculate the price after the payment method is selected, so that we can "
                            + "apply the correct merchant fees and surcharges.";
                        return PaymentGatewayResult.CreateErrorResponse(
                            merchantPaymentDetails,
                            errorMessage);
                    }

                    var crn = this.crnGenerator.GenerateDeftCrnNumber(
                        quoteAggregate.TenantId,
                        quoteAggregate.ProductId,
                        quoteAggregate.Environment,
                        this.configuration.CrnGeneration);

                    var accessToken = await this.accessTokenProvider.GetAccessToken();

                    var drnRequest = new DeftDrnRequest(this.configuration.BillerCode, crn);
                    var drnResponse = await this.configuration.DrnUrl
                        .WithOAuthBearerToken(accessToken)
                        .PostJsonAsync(drnRequest)
                        .ReceiveJson<DeftDrnResponse>();

                    request = new DeftPaymentRequest(
                        this.configuration,
                        priceBreakdown,
                        cardDetails,
                        crn,
                        reference,
                        drnResponse.Drn,
                        this.configuration.BillerCode,
                        merchantFees.SchemeName,
                        this.clock.Now());
                    var response = await this.configuration.PaymentUrl
                        .WithOAuthBearerToken(accessToken)
                        .PostJsonAsync(request)
                        .ReceiveJson<DeftPaymentResponse>();
                    response.Crn = crn;

                    // Remove sensitive data as we do not want to persist or expose it.
                    request.ClearSensitiveData();
                    var requestJson = JsonConvert.SerializeObject(request);
                    var responseJson = JsonConvert.SerializeObject(response);
                    var paymentDetails = new UBind.Domain.Aggregates.Quote.PaymentDetails(PaymentGatewayName.Deft, response.TotalAmount, reference, requestJson, responseJson);
                    paymentGatewayResult = PaymentGatewayResult.CreateSuccessResponse(paymentDetails);
                }
                catch (FlurlHttpException ex)
                {
                    List<string> errorMessages = new List<string>();
                    var rawErrorResponse = await ex.GetResponseStringAsync();
                    IEnumerable<DeftErrorResponse>? deftStructuredErrors = null;
                    try
                    {
                        errorMessages.Add("Payment gateway returned the following error:");
                        deftStructuredErrors = ex.Call.Request.Settings.JsonSerializer.Deserialize<IEnumerable<DeftErrorResponse>>(rawErrorResponse);
                    }
                    catch (JsonException)
                    {
                        // check if it really is Json
                        try
                        {
                            var jObject = JObject.Parse(rawErrorResponse);
                            if (jObject != null)
                            {
                                var errorMessage = jObject["Error"]?.ToString();
                                if (errorMessage != null)
                                {
                                    errorMessages.Add(errorMessage);
                                }
                            }
                            else
                            {
                                errorMessages.Add(rawErrorResponse);
                            }
                        }
                        catch (JsonReaderException)
                        {
                            errorMessages.Add(rawErrorResponse);
                        }
                    }

                    if (deftStructuredErrors != null)
                    {
                        errorMessages.AddRange(deftStructuredErrors.Select(e => e.ToString()));
                    }

                    if (errorMessages.None())
                    {
                        errorMessages.Add("There was a general failure communicating with the payment gateway. Please contact customer support.");
                    }

                    var requestJson = request != null ? JsonConvert.SerializeObject(request) : null;
                    var paymentDetails = new PaymentDetails(PaymentGatewayName.Deft, priceBreakdown.TotalPayable, reference, requestJson, rawErrorResponse);
                    if (deftStructuredErrors != null && deftStructuredErrors.Any(e => e.Type == "PAYMENT_METHOD_ERROR"))
                    {
                        return PaymentGatewayResult.CreateFailureResponse(paymentDetails, errorMessages.ToArray());
                    }

                    // if has this error . try one more time
                    if (deftStructuredErrors != null && deftStructuredErrors.Any(e => e.Type == "LOGIC_ERROR" && e.Message == "Field Validation Error") && retryCount == 1)
                    {
                        retry = true;
                    }
                    else
                    {
                        paymentGatewayResult = PaymentGatewayResult.CreateErrorResponse(paymentDetails, errorMessages.ToArray());
                    }
                }
                catch (ExternalServiceException ex)
                {
                    var requestJson = ex.Error.Data != null && ex.Error.Data.TryGetValue("RequestPayload", out var requestPayload)
                        ? JsonConvert.SerializeObject(requestPayload.ToString()) : null;
                    var responseJson = ex.Error.Data != null && ex.Error.Data.TryGetValue("ResponsePayload", out var responsePayload)
                        ? JsonConvert.SerializeObject(responsePayload.ToString()) : null;
                    var paymentDetails = new PaymentDetails(PaymentGatewayName.Deft, priceBreakdown.TotalPayable, reference, requestJson, responseJson);
                    return PaymentGatewayResult.CreateFailureResponse(paymentDetails, ex.Error.AdditionalDetails?.ToArray() ?? Array.Empty<string>());
                }
            }

            paymentGatewayResult = EntityHelper.ThrowIfNotFound(paymentGatewayResult, "PaymentGatewayResult");
            return paymentGatewayResult;
        }

        /// <inheritdoc />
        public Task<PaymentGatewayResult> MakePayment(PriceBreakdown priceBreakdown, string tokenId, string reference)
        {
            throw new NotImplementedException("The DEFT payment gateway does not support using tokenized credit card details.");
        }

        /// <inheritdoc />
        public Task<PaymentGatewayResult> MakePayment(Quote quote, PriceBreakdown priceBreakdown, SavedPaymentMethod savedPaymentMethod)
        {
            throw new NotImplementedException("The DEFT payment gateway does not support using saved payment methods yet.");
        }

        private void ValidateConfiguration(IDeftConfiguration configuration, IProductContext productContext)
        {
            this.ThrowExceptionIfMissing(
                configuration.AuthorizationUrl,
                nameof(configuration.AuthorizationUrl),
                productContext);
            this.ThrowExceptionIfMissing(
                configuration.ClientId,
                nameof(configuration.ClientId),
                productContext);
            this.ThrowExceptionIfMissing(
                configuration.ClientSecret,
                nameof(configuration.ClientSecret),
                productContext);
            this.ThrowExceptionIfMissing(
                configuration.PaymentUrl,
                nameof(configuration.PaymentUrl),
                productContext);
            this.ThrowExceptionIfMissing(
                configuration.SurchargeUrl,
                nameof(configuration.SurchargeUrl),
                productContext);
            this.ThrowExceptionIfMissing(
                configuration.DrnUrl,
                nameof(configuration.DrnUrl),
                productContext);
            this.ThrowExceptionIfMissing(
                configuration.BillerCode,
                nameof(configuration.BillerCode),
                productContext);
            this.ThrowExceptionIfMissing(
                configuration.SecurityKey,
                nameof(configuration.SecurityKey),
                productContext);
            if (configuration.CrnGeneration == null)
            {
                throw new ErrorException(Errors.Payment.InvalidConfiguration(
                    productContext,
                    $"The CrnGeneration object is missing"));
            }
        }

        private void ThrowExceptionIfMissing(string value, string name, IProductContext productContext)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ErrorException(Errors.Payment.InvalidConfiguration(
                    productContext,
                    $"The {name} is missing"));
            }
        }
    }
}
