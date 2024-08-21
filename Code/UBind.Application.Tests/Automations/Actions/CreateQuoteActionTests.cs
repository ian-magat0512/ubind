// <copyright file="CreateQuoteActionTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Actions
{
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Tests.Fakes;
    using UBind.Domain.ValueTypes;
    using Xunit;

    public class CreateQuoteActionTests
    {
        private readonly Domain.Product.Product product;
        private Guid organisationId = Guid.NewGuid();
        private Guid tenantId = Guid.NewGuid();

        public CreateQuoteActionTests()
        {
            this.product = ProductFactory.Create(this.tenantId, Guid.NewGuid());
        }

        [Fact]
        public void CreateQuoteAction_ShouldBuildFromAutomationJson()
        {
            // Arrange
            var json = @"{
                            ""name"": ""Create Quote Action"",
                            ""alias"": ""createQuoteAction"",
                            ""description"": ""Create quote action."",
                            ""asynchronous"": false,
                            ""policyTransactionType"": ""newBusiness""
                        }";
            var organisationId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var mockServiceProvider = MockAutomationData.GetServiceProviderForCreateQuote(tenantId, this.product.Id, organisationId, null);
            var builder
                = JsonConvert.DeserializeObject<CreateQuoteActionConfigModel>(
                    json, AutomationDeserializationConfiguration.ModelSettings);

            // Act
            var action = builder!.Build(mockServiceProvider);

            // Assert
            action.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateQuoteAction_ShouldCreate_NewBusinessQuote()
        {
            // Arrange
            var json = @"{
                            ""name"": ""Create Quote Action"",
                            ""alias"": ""createQuoteAction"",
                            ""description"": ""Create quote action."",
                            ""asynchronous"": false,
                            ""policyTransactionType"": ""newBusiness""
                        }";
            var quote = QuoteFactory.CreateNewBusinessQuote(
                this.tenantId,
                this.product.Id);
            var mockServiceProvider = MockAutomationData.GetServiceProviderForCreateQuote(this.tenantId, this.product.Id, this.organisationId, quote);
            var builder
                = JsonConvert.DeserializeObject<CreateQuoteActionConfigModel>(
                    json, AutomationDeserializationConfiguration.ModelSettings);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, this.tenantId, this.organisationId, this.product.Id, quote.Aggregate.Environment);
            IProviderContext providerContext = new ProviderContext(automationData);
            var action = builder!.Build(mockServiceProvider);
            var actionData = action.CreateActionData();

            // Act
            await action.Execute(providerContext, actionData);

            // Assert
            ((CreateQuoteActionData)actionData).QuoteId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CreateQuoteAction_ShouldCreate_NewBusinessQuote_WithNonNascentState()
        {
            // Arrange
            var json = @"{
                            ""name"": ""Create Quote Action"",
                            ""alias"": ""createQuoteAction"",
                            ""description"": ""Create quote action."",
                            ""asynchronous"": false,
                            ""policyTransactionType"": ""newBusiness"",
                            ""initialQuoteState"": ""incomplete""
                        }";
            var quote = QuoteFactory.CreateNewBusinessQuote(
               this.tenantId,
               this.product.Id,
               initialQuoteState: StandardQuoteStates.Incomplete);
            var mockServiceProvider = MockAutomationData.GetServiceProviderForCreateQuote(this.tenantId, this.product.Id, this.organisationId, quote);
            var builder
                = JsonConvert.DeserializeObject<CreateQuoteActionConfigModel>(
                    json, AutomationDeserializationConfiguration.ModelSettings);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, this.tenantId, this.organisationId, this.product.Id, quote.Aggregate.Environment);
            IProviderContext providerContext = new ProviderContext(automationData);
            var action = builder!.Build(mockServiceProvider);
            var actionData = action.CreateActionData();

            // Act
            await action.Execute(providerContext, actionData);

            // Assert
            ((CreateQuoteActionData)actionData).QuoteId.Should().Be(quote.Id);
            quote.QuoteStatus.Should().Be("Incomplete");
        }

        [Fact]
        public async Task CreateQuoteAction_ShouldCreate_AdjustmentQuote()
        {
            // Arrange
            var quote = QuoteFactory.CreateNewPolicyWithSubmittedQuote(this.tenantId, this.product.Id, organisationId: this.organisationId);
            var policyId = quote.Policy!.PolicyId;

            var json = @"{
                            ""name"": ""Create Quote Action"",
                            ""alias"": ""createQuoteAction"",
                            ""description"": ""Create quote action."",
                            ""asynchronous"": false,
                            ""policyTransactionType"": ""adjustment"",
                            ""policy"": """ + policyId.ToString() + @""",
                        }";
            var mockServiceProvider = MockAutomationData
                .GetServiceProviderForCreateQuote(this.tenantId, quote.ProductId, quote.OrganisationId, quote.GetLatestQuote(), Domain.TransactionType.Adjustment);
            var builder
                = JsonConvert.DeserializeObject<CreateQuoteActionConfigModel>(
                    json, AutomationDeserializationConfiguration.ModelSettings);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, this.tenantId, this.organisationId, this.product.Id, quote.Environment);
            IProviderContext providerContext = new ProviderContext(automationData);
            var action = builder!.Build(mockServiceProvider);
            var actionData = action.CreateActionData();

            // Act
            await action.Execute(providerContext, actionData);

            // Assert
            ((CreateQuoteActionData)actionData).QuoteId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CreateQuoteAction_ShouldCreate_RenewalQuote()
        {
            // Arrange
            var quote = QuoteFactory.CreateNewPolicyWithSubmittedQuote(this.tenantId, this.product.Id);
            var policyId = quote.Policy!.PolicyId;

            var json = @"{
                            ""name"": ""Create Quote Action"",
                            ""alias"": ""createQuoteAction"",
                            ""description"": ""Create quote action."",
                            ""asynchronous"": false,
                            ""policyTransactionType"": ""renewal"",
                            ""policy"": """ + policyId.ToString() + @""",
                            ""product"": """ + quote.ProductId.ToString() + @""",
                            ""organisation"": """ + quote.OrganisationId.ToString() + @""",

                        }";
            var mockServiceProvider = MockAutomationData
                .GetServiceProviderForCreateQuote(this.tenantId, quote.ProductId, quote.OrganisationId, quote.GetLatestQuote(), Domain.TransactionType.Renewal);
            var builder
                = JsonConvert.DeserializeObject<CreateQuoteActionConfigModel>(
                    json, AutomationDeserializationConfiguration.ModelSettings);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, this.tenantId, quote.OrganisationId, this.product.Id, quote.Environment);
            IProviderContext providerContext = new ProviderContext(automationData);
            var action = builder!.Build(mockServiceProvider);
            var actionData = action.CreateActionData();

            // Act
            await action.Execute(providerContext, actionData);

            // Assert
            ((CreateQuoteActionData)actionData).QuoteId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CreateQuoteAction_ShouldCreate_CancellationQuote()
        {
            // Arrange
            var quote = QuoteFactory.CreateNewPolicyWithSubmittedQuote(this.tenantId, this.product.Id, organisationId: this.organisationId);
            var policyId = quote.Policy!.PolicyId;

            var json = @"{
                            ""name"": ""Create Quote Action"",
                            ""alias"": ""createQuoteAction"",
                            ""description"": ""Create quote action."",
                            ""asynchronous"": false,
                            ""policyTransactionType"": ""cancellation"",
                            ""policy"": """ + policyId.ToString() + @""",
                        }";
            var mockServiceProvider = MockAutomationData
                .GetServiceProviderForCreateQuote(this.tenantId, quote.ProductId, quote.OrganisationId, quote.GetLatestQuote(), Domain.TransactionType.Cancellation);
            var builder
                = JsonConvert.DeserializeObject<CreateQuoteActionConfigModel>(
                    json, AutomationDeserializationConfiguration.ModelSettings);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, this.tenantId, quote.OrganisationId, quote.ProductId, quote.Environment);
            IProviderContext providerContext = new ProviderContext(automationData);
            var action = builder!.Build(mockServiceProvider);
            var actionData = action.CreateActionData();

            // Act
            await action.Execute(providerContext, actionData);

            // Assert
            ((CreateQuoteActionData)actionData).QuoteId.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CreateQuoteAction_ShouldThrow_WhenCustomerMismatchWithPolicy()
        {
            // Arrange
            var quote = QuoteFactory.CreateNewPolicyWithSubmittedQuote(this.tenantId, this.product.Id, organisationId: this.organisationId);
            var policyId = quote.Policy!.PolicyId;

            var json = @"{
                            ""name"": ""Create Quote Action"",
                            ""alias"": ""createQuoteAction"",
                            ""description"": ""Create quote action."",
                            ""asynchronous"": false,
                            ""policyTransactionType"": ""adjustment"",
                            ""policy"": """ + policyId.ToString() + @""",
                            ""product"": """ + quote.ProductId.ToString() + @""",
                            ""organisation"": """ + quote.OrganisationId.ToString() + @""",
                            ""customer"": """ + Guid.NewGuid().ToString() + @""",   
                        }";
            var mockServiceProvider = MockAutomationData
                .GetServiceProviderForCreateQuote(this.tenantId, quote.ProductId, quote.OrganisationId, quote.GetLatestQuote(), Domain.TransactionType.Adjustment);
            var builder
                = JsonConvert.DeserializeObject<CreateQuoteActionConfigModel>(
                    json, AutomationDeserializationConfiguration.ModelSettings);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, this.tenantId, quote.OrganisationId);
            IProviderContext providerContext = new ProviderContext(automationData);
            var action = builder!.Build(mockServiceProvider);
            var actionData = action.CreateActionData();

            // Act
            Func<Task> act = async () => await action.Execute(providerContext, actionData);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>())
                .Which.Error.Code.Should().Be("quote.creation.customer.mismatch.with.policy");
        }

        [Fact]
        public async Task CreateQuoteAction_ShouldThrow_WhenProductMismatchWithPolicy()
        {
            // Arrange
            var quote = QuoteFactory.CreateNewPolicyWithSubmittedQuote(this.tenantId, this.product.Id);
            var policyId = quote.Policy!.PolicyId;

            var json = @"{
                            ""name"": ""Create Quote Action"",
                            ""alias"": ""createQuoteAction"",
                            ""description"": ""Create quote action."",
                            ""asynchronous"": false,
                            ""policyTransactionType"": ""adjustment"",
                            ""policy"": """ + policyId.ToString() + @""",
                            ""product"": """ + Guid.NewGuid().ToString() + @""",
                        }";
            var mockServiceProvider = MockAutomationData
                .GetServiceProviderForCreateQuote(this.tenantId, quote.ProductId, quote.OrganisationId, quote.GetLatestQuote(), Domain.TransactionType.Adjustment);
            var builder
                = JsonConvert.DeserializeObject<CreateQuoteActionConfigModel>(
                    json, AutomationDeserializationConfiguration.ModelSettings);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, this.tenantId, quote.OrganisationId, quote.ProductId, quote.Environment);
            IProviderContext providerContext = new ProviderContext(automationData);
            var action = builder!.Build(mockServiceProvider);
            var actionData = action.CreateActionData();

            // Act
            Func<Task> act = async () => await action.Execute(providerContext, actionData);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>())
                .Which.Error.Code.Should().Be("quote.creation.product.mismatch.with.policy");
        }

        [Fact]
        public async Task CreateQuoteAction_ShouldThrow_WhenOrganisationMismatchWithPolicy()
        {
            // Arrange
            var quote = QuoteFactory.CreateNewPolicyWithSubmittedQuote(this.tenantId, this.product.Id, organisationId: this.organisationId);
            var policyId = quote.Policy!.PolicyId;

            var json = @"{
                            ""name"": ""Create Quote Action"",
                            ""alias"": ""createQuoteAction"",
                            ""description"": ""Create quote action."",
                            ""asynchronous"": false,
                            ""policyTransactionType"": ""adjustment"",
                            ""policy"": """ + policyId.ToString() + @""",
                            ""organisation"": """ + Guid.NewGuid().ToString() + @"""
                        }";
            var mockServiceProvider = MockAutomationData
                .GetServiceProviderForCreateQuote(this.tenantId, quote.ProductId, quote.OrganisationId, quote.GetLatestQuote(), Domain.TransactionType.Adjustment);
            var builder
                = JsonConvert.DeserializeObject<CreateQuoteActionConfigModel>(
                    json, AutomationDeserializationConfiguration.ModelSettings);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(null, null, this.tenantId, quote.OrganisationId, quote.ProductId, quote.Environment);
            IProviderContext providerContext = new ProviderContext(automationData);
            var action = builder!.Build(mockServiceProvider);
            var actionData = action.CreateActionData();

            // Act
            Func<Task> act = async () => await action.Execute(providerContext, actionData);

            // Assert
            (await act.Should().ThrowAsync<ErrorException>())
                .Which.Error.Code.Should().Be("quote.creation.organisation.mismatch.with.policy");
        }
    }
}
