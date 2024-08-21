// <copyright file="PremiumFundingClientTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Funding.PremiumFunding
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NodaTime;
    using UBind.Application.Funding.PremiumFunding;
    using UBind.Domain.Extensions;
    using UBind.Domain.Tests;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="PremiumFundingClientTests" />.
    /// </summary>
    public class PremiumFundingClientTests
    {
        /// <summary>
        /// Defines the Username.
        /// </summary>
        private const string Username = "aptiture-dev";

        /// <summary>
        /// Defines the Password.
        /// </summary>
        private const string Password = "hwyRrL4hU1nU";

        /// <summary>
        /// Defines the ApiVersion.
        /// </summary>
        private const string ApiVersion = "1.4.9";

        /// <summary>
        /// The PostContractAsync_Succeeds_WhenRequireFieldsArePopulated.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task PostContractAsync_Succeeds_WhenRequiredFieldsArePopulated()
        {
            // Arrange
            var client = await this.CreatePremiumFundingClient();
            var contract = this.CreateValidContract();

            // Act
            var response = await client.PostContractAsync(contract);

            // Assert
            response.Should().NotBeNull();
        }

        /// <summary>
        /// The PostContractAsync_ReturnsSameAmount_WhenPaymentMethodChangesFromDebitToCredit.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task PostContractAsync_ReturnsSameAmount_WhenPaymentMethodChangesFromDebitToCredit()
        {
            // Arrange
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = await this.CreatePremiumFundingClient();

            var contract = this.CreateValidContract();  // Default is ContractPaymentType.DirectDebit
            var debitResponse = await client.PostContractAsync(contract);
            var debitPremium = debitResponse.Data.Attributes.ContractSummary.TotalPremiumAmount;

            contract.PaymentType = ContractPaymentType.CreditCard;

            // Act
            var response = await client.PostContractAsync(contract);

            // Assert
            response.Data.Attributes.ContractSummary.TotalPremiumAmount.Should().Be(debitPremium);
        }

        /// <summary>
        /// The PostContractAsync_ReturnsSameAmount_WhenPaymentMethodChangesFromCreditToDebit.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "This will be fixed in UB-10155")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task PostContractAsync_ReturnsSameAmount_WhenPaymentMethodChangesFromCreditToDebit()
        {
            // Arrange
            var client = await this.CreatePremiumFundingClient();

            var contract = this.CreateValidContract();  // Default is ContractPaymentType.DirectDebit
            this.AddCreditCardDetails(contract);        // This sets payment method to CreditCard
            var creditResponse = await client.PostContractAsync(contract);
            var creditPremium = creditResponse.Data.Attributes.ContractSummary.TotalPremiumAmount;

            contract.PaymentType = ContractPaymentType.DirectDebit;

            // Act
            var response = await client.PostContractAsync(contract);

            // Assert
            response.Data.Attributes.ContractSummary.TotalPremiumAmount.Should().Be(creditPremium);
        }

        /// <summary>
        /// The PutContractAsync_AllowsAddingOfPaymentDetails.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task PutContractAsync_Succeeds_WhenPaymentDetailsAdded()
        {
            // Arrange
            var client = await this.CreatePremiumFundingClient();

            var contract = this.CreateValidContract();
            var response = await client.PostContractAsync(contract);

            // Act
            this.AddCreditCardDetails(contract);
            var updateResponse = await client.PutContractAsync(response.Data.Attributes.ContractSummary.ID, contract);

            // Assert
            updateResponse.Should().NotBeNull();
        }

        /// <summary>
        /// The PutSubmissionAsync_Succeeds_ForValidContract.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task PutSubmissionAsync_Succeeds_ForValidContract()
        {
            // Arrange
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = await this.CreatePremiumFundingClient();
            var contract = this.CreateValidContract();

            // No valid credit card number currently acceptable for testing. We use direct debit instead.
            this.AddBankAccountDetails(contract);

            var contractResponse = await client.PostContractAsync(contract);
            var contractId = contractResponse.Data.Attributes.ContractSummary.ID;

            // Act
            var submissionResponse = await client.PutSubmissionAsync(contractId);

            // Assert
            submissionResponse.Should().NotBeNull();
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task PutSubmissionAsync_Fails_ForInvalidCreditCardDetails()
        {
            // Arrange
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = await this.CreatePremiumFundingClient();
            var contract = this.CreateValidContract();
            this.AddInvalidCreditCard(contract);

            var contractResponse = await client.PostContractAsync(contract);
            var contractId = contractResponse.Data.Attributes.ContractSummary.ID;

            // Act
            Func<Task> action = async () => await client.PutSubmissionAsync(contractId);

            // Assert
            await action.Should().ThrowAsync<PremiumFundingException<ErrorResponseModel>>()
                .Where(e => e.Response.Contains("INVALID CARD"));
        }

        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task PutSubmissionAsync_Fails_ForExpiredCreditCard()
        {
            // Arrange
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var client = await this.CreatePremiumFundingClient();
            var contract = this.CreateValidContract();
            this.AddExpiredCreditCard(contract);

            var contractResponse = await client.PostContractAsync(contract);
            var contractId = contractResponse.Data.Attributes.ContractSummary.ID;

            // Act
            Func<Task> action = async () => await client.PutSubmissionAsync(contractId);

            // Assert
            await action.Should().ThrowAsync<PremiumFundingException<ErrorResponseModel>>()
                .Where(e => e.Response.Contains("Invalid expiry date"));
        }

        /// <summary>
        /// The PutSubmissionAsync_Throws_ForInvalidContractId.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "Failing due to 401. To be fixed in UB-9584")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task PutSubmissionAsync_ThrowsException_ForInvalidContractId()
        {
            // Arrange
            var client = await this.CreatePremiumFundingClient();

            // Act
            Func<Task> action = async () => await client.PutSubmissionAsync(98765);

            // Assert
            await action.Should().ThrowAsync<PremiumFundingException<ErrorResponseModel>>();
        }

        /// <summary>
        /// The PutContractAsync_Succeeds_WhenCachedTokenUsedForAtLeast10Minutes.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact(Skip = "Slow test only to be run occasionally. It will sleep for 15 minutes.")]
        public async Task PutContractAsync_Succeeds_WhenCachedTokenUsedForAtLeast10Minutes()
        {
            // Arrange
            var tokenProvider = new AccessTokenProvider();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var accessToken = await tokenProvider.GetAccessToken(Username, Password, ApiVersion);
            var sut = PremiumFundingClient.CreateAuthenticatingClient(accessToken);
            var contract = this.CreateValidContract();
            var response = await sut.PostContractAsync(contract);

            // Act
            while (stopwatch.Elapsed < TimeSpan.FromMinutes(60))
            {
                sut = PremiumFundingClient.CreateAuthenticatingClient(accessToken);
                try
                {
                    var contractId = response.Data.Attributes.ContractSummary.ID;
                    var updateResponse = await sut.PutContractAsync(contractId, contract);
                    Debug.WriteLine($"Succeeded when access token was {stopwatch.Elapsed:g} old");
                    Thread.Sleep(15 * 1000);
                }
                catch (PremiumFundingException)
                {
                    Debug.WriteLine($"Failed when access token was {stopwatch.Elapsed:g} old");
                    break;
                }
            }

            // Assert
            stopwatch.Elapsed.Should().BeGreaterThan(TimeSpan.FromMinutes(10));
        }

        /// <summary>
        /// The CreateValidContract.
        /// </summary>
        /// <returns>The <see cref="Contract"/>.</returns>
        private Contract CreateValidContract()
        {
            var clock = SystemClock.Instance;
            var inceptionDate = clock.Now().ToUnixTimeSeconds();
            var random = new Random();
            var clientName = $"John Smith{random.Next()}";
            var premiumAmount = 1.1;
            var contract = new Contract
            {
                ClientName = clientName,
                TotalPremiumAmount = premiumAmount,
                TypeOfContract = ContractTypeOfContract.Domestic,
                PaymentFrequency = ContractPaymentFrequency.Monthly,
                InceptionDate = (int)inceptionDate,
                SettlementDays = ContractSettlementDays._30,
                Commission = 0.7,
                NumberOfMonths = 12,
                Address = $"{random.Next()} Foo Street",
                Suburb = "Fooville",
                PostCode = 2600,
                State = ContractState.ACT,
                PhoneNumber = "0412 345 678",
                MobileNumber = "0412 345 678",
                PaymentType = ContractPaymentType.DirectDebit,
                ClientEmailAddress = "leon@ubind.io",
                Insurers = new ObservableCollection<Insurer>
                {
                          new Insurer
                          {
                              Amount = premiumAmount,
                              InceptionDate = inceptionDate.ToString(),
                              Name = "Foo Corp",
                              Term = 12,
                          },
                },
                SettlementTo = new ObservableCollection<SettlementTo>
                {
                    new SettlementTo
                    {
                        Name = "Bar Corp",
                        Amount = premiumAmount,
                    },
                },
            };

            return contract;
        }

        private void AddExpiredCreditCard(Contract contract)
        {
            this.AddCreditCardDetails(
                contract,
                creditCardExpiry: DateTime.Now.AddYears(-1).ToString("MMyyyy"));
        }

        private void AddInvalidCreditCard(Contract contract)
        {
            this.AddCreditCardDetails(
                contract,
                cardName: "Test Payment Failure");
        }

        /// <summary>
        /// The AddCreditCardDetails.
        /// </summary>
        /// <param name="contract">The contract<see cref="Contract"/>.</param>
        private void AddCreditCardDetails(
            Contract contract,
            string cardName = "John Smith",
            string cardNumber = "4111111111111111",
            string creditCardExpiry = "")
        {
            contract.PaymentType = ContractPaymentType.CreditCard;
            contract.CreditCardType = ContractCreditCardType.Visa;
            contract.CreditCardHolder = cardName;
            contract.CreditCardNumber = cardNumber;
            contract.CreditCardExpiry = string.IsNullOrEmpty(creditCardExpiry)
                ? DateTime.Now.AddYears(1).ToString("MMyyyy") : creditCardExpiry;
            contract.CVN = "123";
        }

        /// <summary>
        /// Add direct debit details.
        /// </summary>
        /// <param name="contract">The contract<see cref="Contract"/>.</param>
        private void AddBankAccountDetails(
            Contract contract,
            string accountName = "John Smith",
            string accountNumber = "123456",
            string bsb = "123456")
        {
            contract.PaymentType = ContractPaymentType.DirectDebit;
            contract.AccountName = accountName;
            contract.AccountNumber = accountNumber;
            contract.BSB = bsb;
        }

        private async Task<PremiumFundingClient> CreatePremiumFundingClient()
        {
            var tokenProvider = new AccessTokenProvider();
            var accessToken = await tokenProvider.GetAccessToken(Username, Password, ApiVersion);
            return PremiumFundingClient.CreateAuthenticatingClient(accessToken);
        }
    }
}
