// <copyright file="EwayPaymentGateway.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Payment.Eway
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using eWAY.Rapid;
    using eWAY.Rapid.Enums;
    using eWAY.Rapid.Models;
    using Flurl.Http;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Payment;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Payment;
    using UBind.Domain.ReadWriteModel;
    using PaymentMethod = eWAY.Rapid.Enums.PaymentMethod;

    /// <summary>
    /// For making payments via the eWAY payment gateway.
    /// </summary>
    public class EwayPaymentGateway : IPaymentGateway
    {
        private readonly IEwayConfiguration configuration;
        private readonly IClock clock;
        private readonly ILogger logger;
        private HttpClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="EwayPaymentGateway"/> class.
        /// </summary>
        /// <param name="configuration">Configuration settings.</param>
        /// <param name="clock">A clock for getting the current time.</param>
        /// <param name="logger">A logger for outputting diagnostic information.</param>
        public EwayPaymentGateway(IEwayConfiguration configuration, IClock clock, ILogger logger)
        {
            this.configuration = configuration;
            this.clock = clock;
            this.logger = logger;
            this.client = new HttpClient();
        }

        /// <inheritdoc/>
        public bool CanCalculateMerchantFees()
        {
            return false;
        }

        /// <inheritdoc/>
        public Task<MerchantFees> CalculateMerchantFees(
            decimal payableAmount,
            string currencyCode,
            PaymentData paymentData)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<PaymentGatewayResult> MakePayment(
            Quote quote, PriceBreakdown priceBreakdown, CreditCardDetails cardDetails, string reference)
        {
            this.logger.LogInformation("Making payment via Eway:");
            var model = await this.EncryptData(cardDetails);
            Transaction transaction = CreateTransaction(model, priceBreakdown.TotalPayable, priceBreakdown.CurrencyCode, reference);
            IRapidClient ewayClient = RapidClientFactory.NewRapidClient(
                this.configuration.ApiKey, this.configuration.Password, this.configuration.Endpoint.ToString());
            var response = ewayClient.Create(PaymentMethod.Direct, transaction);
            var result = this.InterpretResponse(transaction, response);
            return result;
        }

        /// <inheritdoc />
        public Task<PaymentGatewayResult> MakePayment(PriceBreakdown priceBreakdown, string tokenId, string reference)
        {
            throw new NotImplementedException("The eWay payment gateway does not support using tokenized credit card details.");
        }

        /// <inheritdoc />
        public Task<PaymentGatewayResult> MakePayment(Quote quote, PriceBreakdown priceBreakdown, SavedPaymentMethod savedPaymentMethod)
        {
            throw new NotImplementedException("The eWay payment gateway does not support using saved payment methods yet.");
        }

        private static Transaction CreateTransaction(CreditCardDetails cardDetails, decimal price, string currencyCode, string reference)
        {
            var priceInCents = (int)Math.Round(price * 100, 0, MidpointRounding.ToEven);

            return new Transaction
            {
                Customer = new eWAY.Rapid.Models.Customer
                {
                    CardDetails = new CardDetails
                    {
                        Name = cardDetails.Name,
                        Number = cardDetails.Number,
                        ExpiryMonth = cardDetails.ExpiryMonth.ToString(),
                        ExpiryYear = cardDetails.ExpiryYear.ToString(),
                        CVN = cardDetails.Cvv,
                    },
                },
                PaymentDetails = new eWAY.Rapid.Models.PaymentDetails
                {
                    InvoiceReference = reference,
                    TotalAmount = priceInCents,
                    CurrencyCode = currencyCode,
                },
                TransactionType = TransactionTypes.Purchase,
            };
        }

        private PaymentGatewayResult InterpretResponse(Transaction transaction, CreateTransactionResponse response)
        {
            var details = new Domain.Aggregates.Quote.PaymentDetails(
                PaymentGatewayName.EWay,
                response?.Transaction?.PaymentDetails?.TotalAmount ?? default,
                response?.Transaction?.PaymentDetails?.InvoiceReference ?? default,
                JsonConvert.SerializeObject(transaction),
                JsonConvert.SerializeObject(response));
            if (response.TransactionStatus != null && response.TransactionStatus.Status == true)
            {
                return PaymentGatewayResult.CreateSuccessResponse(details);
            }

            // Note that there are two ways Eway returns errors, depending on if they were validation errors trapped by Eway, or errors from the bank.
            // This is not explictly documented for direct connections in the Eway API documentation, but has been verified with Eway tech support, and
            // you can see examples of the different ways in the API docs under different methods (e.g. responsive shared page, etc.)
            // Eway API docs: https://eway.io/api-v3/
            var errorMessages = response.Errors != null
                ? response
                    .Errors // Validation errors returned in Errors property
                    .Select(errorCode => Eway.ErrorMessagesByCode[errorCode]) // Customer mapping as RapidClientFactory.UserDisplayMessage does not always give nice messages.
                : response
                    .TransactionStatus
                    .ProcessingDetails
                    .getResponseMessages() // Bank errors returned in ResponseMessage
                    .Select(errorCode => RapidClientFactory.UserDisplayMessage(errorCode, "EN"));

            return PaymentGatewayResult.CreateErrorResponse(details, errorMessages.ToArray());
        }

        private async Task<CreditCardDetails> EncryptData(CreditCardDetails cardDetails)
        {
            var payload = new EwayEncryptionDetails(cardDetails);
            var response = await this.configuration.EncryptionUrl
                .WithBasicAuth(this.configuration.ServerSideEncryptionKey, string.Empty)
                .PostJsonAsync(payload)
                .ReceiveJson<EwayEncryptionResponse>();
            var model = new EwayEncryptionDetails(response.Method, response.Items);

            return model.Map(cardDetails.ExpiryMonth, int.Parse(cardDetails.ExpiryYear), cardDetails.Name);
        }
    }
}
