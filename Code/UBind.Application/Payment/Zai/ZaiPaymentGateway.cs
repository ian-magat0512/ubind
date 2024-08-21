// <copyright file="ZaiPaymentGateway.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Payment.Zai
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using Flurl.Http;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Payment.Zai.ZaiEntities;
    using UBind.Application.Queries.Person;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Payment;
    using UBind.Domain.ReadWriteModel;

    public class ZaiPaymentGateway : IPaymentGateway
    {
        private readonly IZaiConfiguration configuration;
        private readonly ZaiAccessTokenProvider accessTokenProvider;
        private readonly ICachingResolver cacheResolver;
        private readonly ICqrsMediator mediator;
        private readonly IHttpContextPropertiesResolver httpContextResolver;
        private readonly string placeholder = "{id}";
        private string? accessToken;

        /// <summary>
        /// Initializes a new instace of the <see cref="ZaiPaymentGateway"/> class.
        /// </summary>
        /// <param name="zaiConfiguration">Zai configuration.</param>
        /// <param name="accessTokenProvider">The token provider.</param>
        public ZaiPaymentGateway(
            IZaiConfiguration zaiConfiguration,
            ZaiAccessTokenProvider accessTokenProvider,
            ICachingResolver cachingResolver,
            IHttpContextPropertiesResolver httpContextResolver,
            ICqrsMediator mediator)
        {
            this.configuration = zaiConfiguration;
            this.accessTokenProvider = accessTokenProvider;
            this.cacheResolver = cachingResolver;
            this.httpContextResolver = httpContextResolver;
            this.mediator = mediator;
        }

        public bool CanCalculateMerchantFees()
        {
            return false;
        }

        public Task<MerchantFees> CalculateMerchantFees(decimal payableAmount, string currencyCode, PaymentData paymentData)
        {
            throw new NotImplementedException();
        }

        public Task<PaymentGatewayResult> MakePayment(PriceBreakdown priceBreakdown, string tokenId, string reference)
        {
            throw new NotImplementedException("The Zai payment gateway does not support tokenized payments.");
        }

        public async Task<PaymentGatewayResult> MakePayment(Quote quote, PriceBreakdown priceBreakdown, CreditCardDetails cardDetails, string reference)
        {
            if (!quote.HasCustomer || !quote.CustomerId.HasValue)
            {
                throw new ErrorException(Errors.Policy.Issuance.RequiresCustomerData(false));
            }

            var quoteAggregate = quote.Aggregate;
            this.accessToken = await this.accessTokenProvider.GetAccessTokenAsync();
            ZaiItemOperationResponse? response = null;
            Item? quoteItem = null;

            try
            {
                var organisation = await this.cacheResolver.GetOrganisationOrThrow(quoteAggregate.TenantId, quoteAggregate.OrganisationId);

                var customerDetails = quote.LatestCustomerDetails?.Data != null
                    ? quote.LatestCustomerDetails.Data
                    : quoteAggregate.CustomerId.HasValue
                        ? await this.RetrievePrimaryPersonForCustomer(quoteAggregate.TenantId, quoteAggregate.CustomerId.Value)
                        : null;
                if (customerDetails == null)
                {
                    throw new ErrorException(Errors.Policy.Issuance.RequiresCustomerData(false));
                }

                var userAccountId = quote.CustomerId.Value;
                var userAccount = await this.GetOrCreateUserAccount(userAccountId, customerDetails.FirstName, customerDetails.LastName, customerDetails.Email);
                var cardAccount = await this.CreateCardAccount(userAccountId, cardDetails);
                string sellerAccountId = this.GetSellerAccountId(organisation.Id);
                quoteItem = await this.GetOrCreateQuoteItem(quote, priceBreakdown, userAccountId.ToString(), sellerAccountId);
                var paymentRequestPayload = new
                {
                    account_id = cardAccount.Id,
                    device_id = "not applicable",
                    ip_address = this.httpContextResolver.ClientIpAddress.ToString(),
                    cvv = cardDetails.Cvv,
                };

                // pay quote item.
                var url = this.configuration.PaymentUrl.Replace(this.placeholder, quoteItem.Id);
                response = await url
                    .WithOAuthBearerToken(this.accessToken)
                    .PatchJsonAsync(paymentRequestPayload)
                    .ReceiveJson<ZaiItemOperationResponse>();
                quoteItem = response.Item;
                var requestDetails = new ZaiRequestDetails(
                    userAccount.User.Id,
                    cardAccount.Id,
                    cardAccount.Card);
                var requestDetailJsonString = JsonConvert.SerializeObject(requestDetails);
                var itemStatusResponseJson = quoteItem != null ? JsonConvert.SerializeObject(quoteItem) : "{}";
                var paymentDetails = new PaymentDetails(PaymentGatewayName.Zai, priceBreakdown.TotalPayable, quoteItem?.ReferenceId, requestDetailJsonString, itemStatusResponseJson);
                return PaymentGatewayResult.CreateSuccessResponse(paymentDetails);
            }
            catch (FlurlHttpException ex)
            {
                return await this.ProcessException(ex, response, priceBreakdown.TotalPayable, quoteItem?.ReferenceId);
            }
            catch (ErrorException ex)
            {
                var responseJson = JsonConvert.SerializeObject(response);
                var paymentDetails = new PaymentDetails(PaymentGatewayName.Zai, priceBreakdown.TotalPayable, quoteItem?.ReferenceId, string.Empty, responseJson);
                return PaymentGatewayResult.CreateErrorResponse(paymentDetails, ex.Error.AdditionalDetails?.ToArray());
            }
        }

        public async Task<PaymentGatewayResult> MakePayment(Quote quote, PriceBreakdown priceBreakdown, SavedPaymentMethod savedPaymentMethod)
        {
            if (!quote.CustomerId.HasValue)
            {
                throw new ErrorException(Errors.Policy.Issuance.RequiresCustomerData(false));
            }
            if (quote.CustomerId.Value != savedPaymentMethod.CustomerId)
            {
                throw new ErrorException(Errors.Payment.SavedPayments.SavedPaymentOwnerAndQuoteCustomerDoNotMatch(quote.Id));
            }

            var quoteAggregate = quote.Aggregate;
            this.accessToken = await this.accessTokenProvider.GetAccessTokenAsync();
            ZaiItemOperationResponse? response = null;
            Item? quoteItem = null;
            try
            {
                var userAuthenticationInfo = JsonConvert.DeserializeObject<ZaiRequestDetails>(savedPaymentMethod.AuthenticationDataJson);
                userAuthenticationInfo = EntityHelper.ThrowIfNotFound(userAuthenticationInfo, "userAccountId");
                var organisation = await this.cacheResolver.GetOrganisationOrThrow(quoteAggregate.TenantId, quoteAggregate.OrganisationId);
                var cardAccountId = userAuthenticationInfo.CardAccountId;
                string sellerAccountId = this.GetSellerAccountId(organisation.Id);
                quoteItem = await this.GetOrCreateQuoteItem(quote, priceBreakdown, userAuthenticationInfo.UserAccountId, sellerAccountId);
                var paymentUrl = this.configuration.PaymentUrl.Replace(this.placeholder, quoteItem.Id);
                var paymentRequestPayload = new
                {
                    account_id = cardAccountId,
                    device_id = "not applicable",
                    ip_address = this.httpContextResolver.ClientIpAddress.ToString(),
                };

                response = await paymentUrl
                    .WithOAuthBearerToken(this.accessToken)
                    .PatchJsonAsync(paymentRequestPayload)
                    .ReceiveJson<ZaiItemOperationResponse>();
                quoteItem = response.Item;
                var cardDetails = JsonConvert.DeserializeObject<MaskedCreditCardDetails>(savedPaymentMethod.IdentificationDataJson);
                cardDetails = EntityHelper.ThrowIfNotFound(cardDetails, "userAccountId");
                var requestDetails = new ZaiRequestDetails(
                    userAuthenticationInfo.UserAccountId, cardAccountId, cardDetails);
                var requestDetailJsonString = JsonConvert.SerializeObject(requestDetails);

                var itemStatusResponseJson = quoteItem != null ? JsonConvert.SerializeObject(quoteItem) : "{}";
                var paymentDetails = new PaymentDetails(PaymentGatewayName.Zai, priceBreakdown.TotalPayable, quoteItem?.ReferenceId, requestDetailJsonString, itemStatusResponseJson);
                return PaymentGatewayResult.CreateSuccessResponse(paymentDetails);
            }
            catch (FlurlHttpException ex)
            {
                return await this.ProcessException(ex, response, priceBreakdown.TotalPayable, quoteItem?.ReferenceId);
            }
            catch (ErrorException ex)
            {
                var responseJson = JsonConvert.SerializeObject(response);
                var paymentDetails = new PaymentDetails(PaymentGatewayName.Zai, priceBreakdown.TotalPayable, quoteItem?.ReferenceId, string.Empty, responseJson);
                return PaymentGatewayResult.CreateErrorResponse(paymentDetails, ex.Error.AdditionalDetails?.ToArray());
            }
        }

        private async Task<CardAccount> CreateCardAccount(Guid userAccountId, CreditCardDetails cardDetails)
        {
            var createCardAccountPayload = new
            {
                full_name = cardDetails.Name,
                number = cardDetails.Number,
                expiry_month = cardDetails.ExpiryMonth,
                expiry_year = cardDetails.ExpiryYear,
                cvv = cardDetails.Cvv,
                user_id = userAccountId,
            };

            var cardCreationResponse = await this.configuration.CardCaptureUrl
                .WithOAuthBearerToken(this.accessToken)
                .PostJsonAsync(createCardAccountPayload)
                .ReceiveJson<ZaiCardOperationResponse>();

            var retrieveCardAccountUrl = this.configuration.CardRetrievalUrl.Replace(this.placeholder, cardCreationResponse.CardAccount.Id);
            var cardOwnerRetrievalResponse = await retrieveCardAccountUrl
                .WithOAuthBearerToken(this.accessToken)
                .GetJsonAsync<ZaiUserOperationResponse>();
            if (cardOwnerRetrievalResponse.User.Id == userAccountId.ToString())
            {
                // have to query one time again to retrieve details of Card from Zai, as card retrieval response only returns user;
                var cardDataRetrievalUrl = retrieveCardAccountUrl.Substring(0, retrieveCardAccountUrl.LastIndexOf('/') + 1);
                var cardDataRetrievalResponse = await cardDataRetrievalUrl
                    .WithOAuthBearerToken(this.accessToken)
                    .GetJsonAsync<ZaiCardOperationResponse>();
                return cardDataRetrievalResponse.CardAccount;
            }

            throw new ErrorException(Errors.Payment.CardCaptureFailure());
        }

        /// <summary>
        /// Used to create fees based on the given fee entries on the price breakdown.
        /// </summary>
        /// <param name="priceBreakdown">The price breakdown where the relevant values for fee creation is taken.</param>
        /// <returns>The list of <see cref="Fee"/> created for the given price breakdown.</returns>
        /// <remarks>Currently unused.</remarks>
        private async Task<List<Fee>> CreateFees(PriceBreakdown priceBreakdown)
        {
            List<Fee> feesToPay = new List<Fee>();
            ZaiFeeCreationResponse? feeCreationResponse = null;

            if (priceBreakdown.TransactionCosts != 0)
            {
                // transaction cost
                var transactionCostTotal = priceBreakdown.TransactionCosts + priceBreakdown.TransactionCostsGst;
                feeCreationResponse = await this.configuration.FeeCreationUrl
                    .WithOAuthBearerToken(this.accessToken)
                    .SetQueryParam("name", "Processing Fee")
                    .SetQueryParam("fee_type_id", 1)
                    .SetQueryParam("amount", transactionCostTotal * 100)
                    .SetQueryParam("to", "cc")
                    .PostAsync(null)
                    .ReceiveJson<ZaiFeeCreationResponse>();
                feesToPay.Add(feeCreationResponse.Fee);
            }

            if (priceBreakdown.MerchantFees != 0)
            {
                // merchant fees
                var merchantFeesTotal = priceBreakdown.MerchantFees + priceBreakdown.MerchantFeesGst;
                feeCreationResponse = await this.configuration.FeeCreationUrl
                    .WithOAuthBearerToken(this.accessToken)
                    .SetQueryParam("name", "Merchant Fee")
                    .SetQueryParam("fee_type_id", 1)
                    .SetQueryParam("amount", merchantFeesTotal * 100)
                    .SetQueryParam("to", "cc")
                    .PostAsync(null)
                    .ReceiveJson<ZaiFeeCreationResponse>();
                feesToPay.Add(feeCreationResponse.Fee);
            }

            return feesToPay;
        }

        private async Task<Item> GetOrCreateQuoteItem(Quote quote, PriceBreakdown priceBreakdown, string accountId, string sellerAccountId)
        {
            // Retrieve list of items under buyer
            var itemId = quote.Id.ToString();
            Item? item = await this.GetQuoteItemByAccountId(itemId, accountId);
            var policyAmount = priceBreakdown.TotalPayable * 100;

            if (item == null)
            {
                return await this.CreateQuoteItem(quote, priceBreakdown, accountId, sellerAccountId);
            }
            if (item.State == ZaiItemState.Pending && item.Amount != policyAmount)
            {
                return await this.UpdateQuoteItemAmount(item, policyAmount);
            }

            return item;
        }

        private async Task<Item> CreateQuoteItem(Quote quote, PriceBreakdown priceBreakdown, string accountId, string sellerAccountId)
        {
            var quoteAggregate = quote.Aggregate;
            var product = await this.cacheResolver.GetProductOrNull(quoteAggregate.TenantId, quoteAggregate.ProductId);
            var productName = product?.Details?.Name;
            var policyAmount = priceBreakdown.TotalPayable * 100;
            var itemDescription = $"{productName}-{quote.QuoteNumber ?? string.Empty}";
            var itemCreationPayload = new
            {
                id = quote.Id,
                name = itemDescription,
                amount = policyAmount,
                currency = "AUD",
                payment_type = 2,
                buyer_id = accountId,
                seller_id = sellerAccountId,
                descriptor = itemDescription,
                custom_descriptor = itemDescription,
            };

            ZaiItemOperationResponse itemCreationResponse = await this.configuration.ItemCreationUrl
                .WithOAuthBearerToken(this.accessToken)
                .PostJsonAsync(itemCreationPayload)
                .ReceiveJson<ZaiItemOperationResponse>();
            return itemCreationResponse.Item;
        }

        private async Task<Item> UpdateQuoteItemAmount(Item item, decimal policyAmount)
        {
            item.Amount = policyAmount;
            var itemUpdateUrl = this.configuration.ItemUpdateUrl?.Replace(this.placeholder, item.Id)
                    ?? $"{this.configuration.ItemCreationUrl}/{item.Id}";
            ZaiItemOperationResponse itemUpdateResponse = await itemUpdateUrl
                .WithOAuthBearerToken(this.accessToken)
                .PatchJsonAsync(item)
                .ReceiveJson<ZaiItemOperationResponse>();
            return itemUpdateResponse.Item;
        }

        private string GetSellerAccountId(Guid transactingOrgansationId)
        {
            string? sellerAccountId = this.configuration.OrganisationSellerAccounts
                .FirstOrDefault(x => x.OrganisationId.Equals(transactingOrgansationId))?.OrganisationSellerAccountId;
            if (string.IsNullOrEmpty(sellerAccountId))
            {
                sellerAccountId = this.configuration.OrganisationSellerAccounts.First().OrganisationSellerAccountId;
            }

            return sellerAccountId;
        }

        private async Task<Item?> GetQuoteItemByAccountId(string itemId, string accountId)
        {
            ZaiUserOperationResponse? userItems = null;
            try
            {
                var itemRetrievalUrl = this.configuration.UserItemRetrievalUrl.Replace(this.placeholder, accountId);
                userItems = await itemRetrievalUrl
                    .WithOAuthBearerToken(this.accessToken)
                    .GetJsonAsync<ZaiUserOperationResponse>();
            }
            catch (FlurlHttpException ex)
            {
                var errorMessages = this.GetErrorMessages(await ex.GetResponseStringAsync());
                if (!errorMessages.Any(x => x.Contains("id invalid")))
                {
                    throw ex;
                }
            }

            return userItems?.Items?.FirstOrDefault(x => x.Id == itemId);
        }

        private async Task<ZaiUserOperationResponse> GetOrCreateUserAccount(Guid id, string firstName, string lastName, string email)
        {
            try
            {
                var userRetrievalUrl = this.configuration.UserRetrievalUrl.Replace(this.placeholder, id.ToString());
                var retrievedUserAccount = await userRetrievalUrl
                    .WithOAuthBearerToken(this.accessToken)
                    .GetJsonAsync<ZaiUserOperationResponse>();
                if (retrievedUserAccount.User != null)
                {
                    return retrievedUserAccount;
                }
            }
            catch (FlurlHttpException ex)
            {
                var errorMessages = this.GetErrorMessages(await ex.GetResponseStringAsync());
                if (!errorMessages.Any(x => x.Contains("id invalid")))
                {
                    throw ex;
                }
            }

            var createUserAccountPayload = new
            {
                id,
                first_name = firstName,
                last_name = lastName,
                email,
                country = "AUS",
            };
            return await this.configuration.UserCreationUrl
                .WithOAuthBearerToken(this.accessToken)
                .PostJsonAsync(createUserAccountPayload)
                .ReceiveJson<ZaiUserOperationResponse>();
        }

        private async Task<IPersonalDetails?> RetrievePrimaryPersonForCustomer(Guid tenantId, Guid customerId)
        {
            var query = new GetPrimaryPersonForCustomerQuery(tenantId, customerId);
            var personDetails = await this.mediator.Send(query);
            return personDetails;
        }

        private async Task<PaymentGatewayResult> ProcessException(FlurlHttpException ex, ZaiItemOperationResponse? response, decimal totalPayable, string? itemReference)
        {
            var request = new JObject()
                {
                    { "url", ex.Call?.Request?.Url?.ToString() },
                    { "method", ex.Call?.Request?.Verb?.ToString() },
                };

            if (ex.Call?.RequestBody != null)
            {
                var contentString = JsonConvert.SerializeObject(ex.Call?.RequestBody);
                request.Add("body", contentString);
            }

            List<string> errorMessages = this.GetErrorMessages(await ex.GetResponseStringAsync());
            var requestJson = JsonConvert.SerializeObject(request);
            var responseJson = JsonConvert.SerializeObject(response);
            var paymentDetails = new PaymentDetails(PaymentGatewayName.Zai, totalPayable, itemReference, requestJson, responseJson);
            return PaymentGatewayResult.CreateErrorResponse(paymentDetails, errorMessages.ToArray());
        }

        private List<string> GetErrorMessages(string rawErrorResponse)
        {
            List<string> errorMessages = new List<string>();
            try
            {
                var jObject = JObject.Parse(rawErrorResponse);
                if (jObject.ContainsKey("errors"))
                {
                    var errors = jObject.SelectToken("errors.base")?.ToObject<string[]>();
                    if (errors != null)
                    {
                        errorMessages.AddRange(errors);
                    }
                    else
                    {
                        var errorTokens = jObject.Properties().Select(x => this.ParseErrorTokens(x)).ToList();
                        if (errorTokens != null)
                        {
                            var errorTokenValues = errorTokens
                                .Where(error => error != null)
                                .Select(error => error!);
                            errorMessages.AddRange(errorTokenValues);
                        }
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

            if (!errorMessages.Any())
            {
                errorMessages.Add("There was a general failure communicating with the payment gateway. Please contact customer support.");
            }

            return errorMessages;
        }

        private string? ParseErrorTokens(JProperty token)
        {
            switch (token.Value.Type)
            {
                case JTokenType.None:
                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Null:
                case JTokenType.Undefined:
                case JTokenType.Comment:
                    return null;
                case JTokenType.Object:
                    var errorMessages = new List<string>();
                    foreach (JProperty property in token.Value)
                    {
                        var propertyValue = this.ParseErrorTokens(property);
                        if (propertyValue != null)
                        {
                            errorMessages.Add(propertyValue);
                        }
                    }

                    return errorMessages.Count > 1
                        ? string.Join(", ", errorMessages)
                        : errorMessages.FirstOrDefault();
                case JTokenType.Array:
                    // Note, value is always expected to be string here.
                    return $"{token.Name} {string.Join(", ", token.Value)}";
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.Boolean:
                case JTokenType.Date:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                case JTokenType.String:
                    return $"{token.Name} {token}";
                default:
                    return null;
            }
        }

        internal class ZaiRequestDetails
        {
            public ZaiRequestDetails(string userAccountId, string cardAccountId, Card cardDetails)
            {
                this.UserAccountId = userAccountId;
                this.CardAccountId = cardAccountId;
                var maskedCardNumber = $"**** **** ****{cardDetails.Number.Substring(cardDetails.Number.Length - 4)}";
                this.CreditCardInfo = new MaskedCreditCardDetails(
                    maskedCardNumber,
                    cardDetails.CardholderName,
                    int.Parse(cardDetails.ExpiryMonth),
                    int.Parse(cardDetails.ExpiryYear),
                    cardDetails.Type);
            }

            [JsonConstructor]
            public ZaiRequestDetails(string userAccountId, string cardAccountId, MaskedCreditCardDetails cardDetails)
            {
                this.UserAccountId = userAccountId;
                this.CardAccountId = cardAccountId;
                this.CreditCardInfo = cardDetails;
            }

            [JsonProperty("userAccountId")]
            public string UserAccountId { get; private set; }

            [JsonProperty("cardAccountId")]
            public string CardAccountId { get; private set; }

            [JsonProperty("cardAccount")]
            public MaskedCreditCardDetails CreditCardInfo { get; private set; }
        }
    }
}
