// <copyright file="PerformQuoteCalculationActionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Actions
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Application.Commands.QuoteCalculation;
    using UBind.Application.Services.Imports;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Helpers;
    using UBind.Domain.Json;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using Action = UBind.Application.Automation.Actions.Action;

    public class PerformQuoteCalculationActionTests
    {
        private IServiceProvider dependencyProvider;

        public PerformQuoteCalculationActionTests()
        {
            var entityId = new Guid("dcbbf092-9ef5-4303-a565-c3576a379e6e");
            var organisationService = new Mock<IOrganisationService>();
            var serviceCollection = new ServiceCollection();
            organisationService.Setup(o => o.GetDefaultOrganisationForTenant(It.IsAny<Guid>()))
                .Returns(new OrganisationReadModelSummary()
                {
                    Id = Guid.NewGuid(),
                });
            serviceCollection.AddScoped<IOrganisationService>(c => organisationService.Object);

            var mockCachingResolver = new Mock<ICachingResolver>();
            mockCachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>()))
                .Returns(Task.FromResult(TenantFactory.Create(TenantFactory.DefaultId)));
            serviceCollection.AddScoped<ICachingResolver>(c => mockCachingResolver.Object);

            var calculationResultJson = CalculationResultJsonFactory.SampleWithSoftReferral;
            var calculationData = new CachingJObjectWrapper(calculationResultJson);
            var formData = new Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates());
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationData);
            var calculationResult = CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver);
            var calculationResponseModel = new CalculationResponseModel(
                calculationResult, null, null);
            var mockMediator = new Mock<ICqrsMediator>();
            mockMediator.Setup(s => s.Send(It.IsAny<QuoteCalculationCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(calculationResponseModel));
            serviceCollection.AddSingleton<ICqrsMediator>(c => mockMediator.Object);

            serviceCollection.AddSingleton<IClock>(c => SystemClock.Instance);

            var mockProductConfig = new DefaultProductConfiguration();
            var mockProductConfigProvider = new Mock<IProductConfigurationProvider>();
            var mockFormDataPrettifier = new Mock<IFormDataPrettifier>();
            var mockUrlConfiguration = new Mock<IInternalUrlConfiguration>();
            mockUrlConfiguration.Setup(c => c.BaseApi).Returns("https://localhost:4366/api");
            serviceCollection.AddScoped<ISerialisedEntityFactory>(c => new SerialisedEntityFactory(
                mockUrlConfiguration.Object,
                mockProductConfigProvider.Object,
                mockFormDataPrettifier.Object,
                mockCachingResolver.Object,
                mockMediator.Object,
                new DefaultPolicyTransactionTimeOfDayScheme()));
            this.dependencyProvider = serviceCollection.BuildServiceProvider();
        }

        [Fact(Skip = "This test needs to be rewritten to properly invoke the command pipline handlers so at least "
            + "ValidateQuoteCalculationCommandHandler runs https://jira.aptiture.com/browse/UB-8019")]
        public async Task PerformQuoteCalculationAction_ShouldExecute_WhenNoQuoteAndNotPersistingResults()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger(
                TestJson.InputData, "application/json");
            var action = this.CreateActionUsingJson(TestJson.ActionConfig);
            var actionData = action.CreateActionData();
            automationData.AddActionData(actionData);

            // Act
            await action.Execute(new ProviderContext(automationData), actionData);

            // Assert
            var calcActionData = actionData as PerformQuoteCalculationActionData;
            calcActionData.CalculationResult.Should().NotBeNull();
        }

        private PerformQuoteCalculationAction CreateActionUsingJson(string actionConfig)
        {
            var actionModel = JsonConvert.DeserializeObject<IBuilder<Action>>(
                actionConfig, AutomationDeserializationConfiguration.ModelSettings);
            return actionModel.Build(this.dependencyProvider) as PerformQuoteCalculationAction;
        }

        private class TestJson
        {
            public static string InputData => @"
                {
                    ""productAlias"": ""ub-7578"",
                    ""formModel"": {
                        ""liabilityLimit"": ""5000000"",
                        ""occupation"": ""Arborist/Tree Surgeon Trade"",
                        ""turnover"": 123123,
                        ""stateACT"": ""100"",
                        ""stateNSW"": """",
                        ""stateNT"": """",
                        ""stateQLD"": """",
                        ""stateSA"": """",
                        ""stateTAS"": """",
                        ""stateVIC"": """",
                        ""stateWA"": """",
                        ""stateOverseas"": """",
                        ""totalState"": 100,
                        ""toolsLimit"": ""1000"",
                        ""toolsState"": ""VIC"",
                        ""NSWStampDutyExemption"": ""Not Applicable"",
                        ""manualEndorsement"": false,
                        ""paymentOption"": ""Yearly"",
                        ""paymentMethod"": ""VISA"",
                        ""liabilityCustomPremium"": """",
                        ""policyStartDate"": ""11/07/2022"",
                        ""policyEndDate"": ""11/08/2022"",
                        ""contactName"": """",
                        ""contactEmail"": """",
                        ""contactPhone"": """",
                        ""contactMobile"": """"
                    }
                }";

            public static string ActionConfig => @"
                {
                    ""performQuoteCalculationAction"": {
                        ""name"": ""Perform Quote Calculation Action Test"",
                        ""alias"": ""performQuoteCalculationActionTest"",
                        ""description"": ""A test of the perform quote calculation action"",
                        ""inputData"": {
                            ""jsonObject"": {
                                ""objectPathLookupText"": ""/trigger/httpRequest/content/formModel""
                            }
                        },
                        ""calculationResultVersion"": 1
                    }
                }";
        }
    }
}
