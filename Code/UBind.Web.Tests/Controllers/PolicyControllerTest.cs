// <copyright file="PolicyControllerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application;
    using UBind.Application.Authorisation;
    using UBind.Application.Dashboard.Model;
    using UBind.Application.Queries.PolicyTransaction;
    using UBind.Application.Services;
    using UBind.Application.Tests.Automations.Providers.Fakes;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Commands;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests.Fakes;
    using UBind.Web.Controllers;
    using UBind.Web.ResourceModels;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="PolicyControllerTest" />.
    /// </summary>
    public class PolicyControllerTest
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private readonly Guid tenantId = TenantFactory.DefaultId;
        private readonly Guid organisationId = Guid.NewGuid();

        /// <summary>
        /// Defines the clock.
        /// </summary>
        private readonly IClock clock = SystemClock.Instance;

        /// <summary>
        /// Defines the policy service.
        /// </summary>
        private readonly Mock<IPolicyService> policyService = new Mock<IPolicyService>();

        /// <summary>
        /// Defines the customerService.
        /// </summary>
        private readonly Mock<ICustomerService> customerService = new Mock<ICustomerService>();

        /// <summary>
        /// Defines the configurationService.
        /// </summary>
        private readonly Mock<IConfigurationService> configurationService = new Mock<IConfigurationService>();

        /// <summary>
        /// Defines the mediator service.
        /// </summary>
        private readonly Mock<ICqrsMediator> cqrsMediator = new Mock<ICqrsMediator>();

        /// <summary>
        /// Defines the authorisation service.
        /// </summary>
        private readonly Mock<IAuthorisationService> authorisationService = new Mock<IAuthorisationService>();

        /// <summary>
        /// Defines the userId.
        /// </summary>
        private readonly Guid userId = new Guid("A1B2A3B4-3333-2222-1111-000000000000");

        /// <summary>
        /// Defines the testPolicyNumber.
        /// </summary>
        private readonly string testPolicyNumber = "P0001";

        /// <summary>
        /// Defines the policyDetails.
        /// </summary>
        private FakePolicyReadModelDetails policyDetails = new FakePolicyReadModelDetails();

        /// <summary>
        /// Defines the quoteAggregate.
        /// </summary>
        private QuoteAggregate quoteAggregate;

        /// <summary>
        /// Defines the quoteAggregate.
        /// </summary>
        private Quote quote;

        /// <summary>
        /// Defines the fakePolicyHistory.
        /// </summary>
        private IEnumerable<FakePolicyReadModelDetails> fakePolicyHistory = new List<FakePolicyReadModelDetails>();

        /// <summary>
        /// Defines the policyController.
        /// </summary>
        private PolicyController policyController;

        // Paths for patch tests.
        private JsonPath validFormDataPath = new JsonPath("objectProperty.nestedProperty");
        private JsonPath validCalculationResultPath = new JsonPath("questions.ratingPrimary.objectProperty.nestedProperty");
        private string sampleExpectedJson =
            @"{
                ""Value"": {
                    ""Customer"": {
                        ""Id"": ""fca04c35-74b5-4f58-a93c-db3e7b8b3bf3"",
                        ""FullName"": ""Test Policy"",
                        ""IsTestData"": false
                    },
                    ""ProductName"": null,
                    ""Status"": ""Issued"",
                    ""QuoteId"": ""00000000-0000-0000-0000-000000000000"",
                    ""TenantId"": ""ccae2079-2ebc-4200-879d-866fc82e6afa"",
                    ""QuoteNumber"": null,
                    ""PolicyNumber"": ""P0001"",
                    ""CreatedDateTime"": ""2019-12-05T16:00:00Z"",
                    ""InceptionDateTime"": ""2020-01-05T13:00:00Z"",
                    ""ExpiryDateTime"": ""2021-01-05T13:00:00Z"",
                    ""Owner"": {
                        ""Id"": ""00000000-0000-0000-0000-000000000000"",
                        ""FullName"": null
                    }
                },
                ""Formatters"": [],
                ""ContentTypes"": [],
                ""DeclaredType"": null,
                ""StatusCode"": 200
            }";

        public PolicyControllerTest()
        {
            var organisationId = Guid.NewGuid();
            var tenant = TenantFactory.Create(TenantFactory.DefaultId);
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", tenant.Id.ToString()),
                new Claim(UBind.Application.Infrastructure.ClaimNames.OrganisationId, organisationId.ToString()),
            }));

            this.CreatePolicyHistory(this.testPolicyNumber);
            var options = new QueryOptionsModel
            {
                PolicyNumber = this.testPolicyNumber,
            };
            tenant.SetDefaultOrganisation(organisationId, SystemClock.Instance.GetCurrentInstant());
            var person = PersonAggregate.CreatePerson(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                this.performingUserId,
                SystemClock.Instance.GetCurrentInstant());
            var personCommonProperties = new PersonCommonProperties()
            {
                FullName = "Test Policy",
                PreferredName = "TestPolicy",
            };

            var personDetails = new PersonalDetails(tenant.Id, personCommonProperties);
            person.Update(personDetails, this.performingUserId, SystemClock.Instance.GetCurrentInstant());
            var customer = CustomerAggregate.CreateNewCustomer(
                tenant.Id,
                person,
                QuoteFactory.DefaultEnvironment,
                this.performingUserId,
                null,
                SystemClock.Instance.GetCurrentInstant(),
                false);

            var patchCommand = new GivenValuePolicyDataPatchCommand(
               this.validFormDataPath,
               this.validCalculationResultPath,
               "hello",
               PolicyDataPatchScope.CreateGlobalPatchScope(),
               PatchRules.None);

            var formData = FormDataJsonFactory.GetSampleFormDataJsonForPatching();
            var calculationResultJson = CalculationResultJsonFactory.GetSampleCalculationResultForPatching();
            this.quote = QuoteFactory.CreateNewBusinessQuote(tenant.Id, organisationId: organisationId);
            this.quoteAggregate = this.quote
                .Aggregate
                .WithCustomerDetails(this.quote.Id)
                .WithCustomer()
                .WithQuoteNumber(this.quote.Id)
                .WithCalculationResult(this.quote.Id, formData, calculationResultJson)
                .WithSubmission(this.quote.Id)
                .WithQuoteVersion(this.quote.Id)
                .WithPolicy(this.quote.Id);

            this.quoteAggregate.PatchFormData(patchCommand, this.performingUserId, SystemClock.Instance.Now());
            this.quoteAggregate.RecordAssociationWithCustomer(customer, person, this.performingUserId, SystemClock.Instance.GetCurrentInstant());
            this.quote.UpdateFormData(new Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates()), this.performingUserId, SystemClock.Instance.GetCurrentInstant());
            this.SetPolicyDetails(person);

            var customerRepository = new Mock<ICustomerAggregateRepository>();
            var policyReadModelRepository = new Mock<IPolicyReadModelRepository>();
            var quoteAggregateRepository = new Mock<IQuoteAggregateRepository>();
            var userRepository = new Mock<IUserAggregateRepository>();
            var quoteReadModelRepository = new Mock<IQuoteReadModelRepository>();
            var applicationQuoteService = new Mock<IApplicationQuoteService>();
            var productFeatureService = new Mock<IProductFeatureSettingService>();
            var tenantRepository = new Mock<ITenantRepository>();
            var policyNumberRepository = new Mock<IPolicyNumberRepository>();
            var productConfigurationProvider = new Mock<IProductConfigurationProvider>();
            this.policyService
                 .Setup(x => x.GetPolicy(this.quoteAggregate.TenantId, this.quoteAggregate.Id))
                 .Returns(Task.FromResult((IPolicyReadModelDetails)this.policyDetails));
            var systemAlertService = new Mock<ISystemAlertService>();
            var organisationService = new Mock<IOrganisationService>();
            var formDataPrettifier = new Mock<IFormDataPrettifier>();
            var quoteDocumentReadModelRepository = new Mock<IQuoteDocumentReadModelRepository>();
            var cachingResolver = new Mock<ICachingResolver>();
            var additionalPropertyValueService = new Mock<IAdditionalPropertyValueService>();
            policyReadModelRepository.Setup(policy => policy.GetPolicyDetails(tenant.Id, this.quoteAggregate.Id)).Returns(this.policyDetails);
            policyReadModelRepository
                .Setup(policy => policy.ListPolicies(
                    It.IsAny<Guid>(), It.IsAny<PolicyReadModelFilters>()))
                .Returns(this.fakePolicyHistory);

            cachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            cachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            this.policyController =
                new PolicyController(
                    this.policyService.Object,
                    this.clock,
                    this.configurationService.Object,
                    productConfigurationProvider.Object,
                    formDataPrettifier.Object,
                    this.authorisationService.Object,
                    this.cqrsMediator.Object,
                    cachingResolver.Object,
                    additionalPropertyValueService.Object,
                    this.customerService.Object,
                    new Mock<IRenewalInvitationService>().Object)
                {
                    ControllerContext = new ControllerContext()
                    {
                        HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
                    },
                };
        }

        /// <summary>
        /// The GetPolicyDetails_NoDetailsType_ReturnsBaseDetails.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        public async Task GetPolicyDetails_NoDetailsType_ReturnsBaseDetails()
        {
            // act
            dynamic expectedResult = JsonConvert.DeserializeObject(this.sampleExpectedJson);
            IActionResult result = await this.policyController.GetPolicyDetails(this.quoteAggregate.Id, string.Empty, "none");
            var resultJsonString = JsonConvert.SerializeObject(result);
            dynamic obj = JsonConvert.DeserializeObject(resultJsonString);

            // assert
            Assert.Equal(expectedResult.Value["Customer"]["Id"], obj.Value["Customer"]["Id"]);
            Assert.Equal(expectedResult.Value["ProductName"], obj.Value["ProductName"]);
            Assert.Equal(expectedResult.Value["TenantId"], obj.Value["TenantId"]);
            Assert.Equal(expectedResult.Value["PolicyNumber"], obj.Value["PolicyNumber"]);
            Assert.Equal(expectedResult.Value["CreatedDate"], obj.Value["CreatedDate"]);
            Assert.Equal(expectedResult.Value["PolicyInceptionDate"], obj.Value["PolicyInceptionDate"]);
            Assert.Equal(expectedResult.Value["PolicyExpiryDate"], obj.Value["PolicyExpiryDate"]);

            // should not include all other tabs
            ((string)obj.Value["Claims"]).Should().BeNull();
            ((string)obj.Value["PremiumData"]).Should().BeNull();
            ((string)obj.Value["Questions"]).Should().BeNull();
            ((string)obj.Value["Documents"]).Should().BeNull();
        }

        /// <summary>
        /// The GetPolicyDetails_PageNotFound_NotFoundQuoteId.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        public async Task GetPolicyDetails_PageNotFound_NotFoundQuoteId()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Customer.Humanize()),
                new Claim("Tenant", "foo-tenant"),
                new Claim("OrganisationId", Guid.NewGuid().ToString()),
                new Claim("CustomerId", default(Guid).ToString()),
            }));

            // act
            this.policyController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };

            IActionResult result = await this.policyController.GetPolicyDetails(Guid.NewGuid(), "base", "none");
            var resultJsonString = JsonConvert.SerializeObject(result);
            dynamic obj = JsonConvert.DeserializeObject(resultJsonString);
            dynamic statusCode = obj["StatusCode"];

            // assert
            Assert.True(statusCode == 404);
        }

        /// <summary>
        /// The GetPolicyDetails_Base.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        public async Task GetPolicyDetails_Base()
        {
            // act
            var inceptionDate = new LocalDate(2020, 1, 6);
            var inceptionTimestamp = inceptionDate.AtStartOfDayInZone(Timezones.AET).ToInstant();
            var createdDate = new LocalDate(2019, 12, 6);
            var issuedTimestamp = createdDate.AtStartOfDayInZone(Timezones.AET).ToInstant();
            var expectedJson = @"{
                ""Value"": {
                    ""Customer"": {
                        ""Id"": ""fca04c35-74b5-4f58-a93c-db3e7b8b3bf3"",
                        ""FullName"": ""Test Policy"",
                        ""IsTestData"": false
                    },
                    ""ProductName"": null,
                    ""Status"": ""Issued"",
                    ""QuoteId"": ""00000000-0000-0000-0000-000000000000"",
                    ""TenantId"": ""ccae2079-2ebc-4200-879d-866fc82e6afa"",
                    ""QuoteNumber"": null,
                    ""PolicyNumber"": ""P0001"",
                    ""CreatedDateTime"": """ + issuedTimestamp + @""",
                    ""InceptionDateTime"": """ + inceptionTimestamp + @""",
                    ""ExpiryDateTime"": ""2021-01-05T13:00:00Z"",
                    ""Owner"": {
                        ""Id"": ""00000000-0000-0000-0000-000000000000"",
                        ""FullName"": null
                    }
                },
                ""Formatters"": [],
                ""ContentTypes"": [],
                ""DeclaredType"": null,
                ""StatusCode"": 200
            }";

            dynamic expectedResult = JsonConvert.DeserializeObject(expectedJson);
            IActionResult result = await this.policyController.GetPolicyDetails(this.quoteAggregate.Id, "base", "none");
            var resultJsonString = JsonConvert.SerializeObject(result);
            dynamic obj = JsonConvert.DeserializeObject(resultJsonString);

            // assert
            ((string)expectedResult.Value["InceptionDateTime"]).Should().NotBeNull();
            ((string)expectedResult.Value["ExpiryDateTime"]).Should().NotBeNull();

            ((string)expectedResult.Value["PolicyInceptionDateTime"]).Should().BeNull();
            ((string)expectedResult.Value["PolicyExpiryDateTime"]).Should().BeNull();

            ((string)expectedResult.Value["Customer"]["Id"]).Should().Be((string)obj.Value["Customer"]["Id"]);
            ((string)expectedResult.Value["ProductName"]).Should().Be((string)obj.Value["ProductName"]);
            ((string)expectedResult.Value["TenantId"]).Should().Be((string)obj.Value["TenantId"]);
            ((string)expectedResult.Value["PolicyNumber"]).Should().Be((string)obj.Value["PolicyNumber"]);
            ((string)expectedResult.Value["CreatedDateTime"]).Should().Be((string)obj.Value["CreatedDateTime"]);
            ((string)expectedResult.Value["InceptionDateTime"]).Should().Be((string)obj.Value["InceptionDateTime"]);
            ((string)expectedResult.Value["ExpiryDateTime"]).Should().Be((string)obj.Value["ExpiryDateTime"]);

            // should not include all other tabs
            ((string)obj.Value["Claims"]).Should().BeNull();
            ((string)obj.Value["PremiumData"]).Should().BeNull();
            ((string)obj.Value["Questions"]).Should().BeNull();
            ((string)obj.Value["Documents"]).Should().BeNull();
        }

        /// <summary>
        /// The GetPolicyDetails_Premium.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        public async Task GetPolicyDetails_Premium()
        {
            // act
            IActionResult result = await this.policyController.GetPolicyDetails(this.quoteAggregate.Id, "premium", "none");
            var resultJsonString = JsonConvert.SerializeObject(result);
            dynamic obj = JsonConvert.DeserializeObject(resultJsonString);

            // assert
            Assert.NotNull(obj.Value["PremiumData"]);
            dynamic premium = obj.Value["PremiumData"];
            Assert.True(premium["BasePremium"] == "100.0");
            Assert.True(premium["Interest"] == "0.0");
            Assert.True(premium["MerchantFees"] == "4.84");
            Assert.True(premium["TransactionCosts"] == "0.0");
            Assert.True(premium["PremiumGst"] == "10.0");
            Assert.True(premium["StampDutyAct"] == "0.0");
            Assert.True(premium["StampDutyNsw"] == "0.0");
            Assert.True(premium["StampDutyNt"] == "0.0");
            Assert.True(premium["StampDutyQld"] == "0.0");
            Assert.True(premium["StampDutySa"] == "0.00");
            Assert.True(premium["StampDutyTas"] == "0.00");
            Assert.True(premium["StampDutyWa"] == "0.00");
            Assert.True(premium["StampDutyVic"] == "0.00");
            Assert.True(premium["Commission"] == "0.0");
            Assert.True(premium["CommissionGst"] == "0.0");
            Assert.True(premium["BrokerFee"] == "0.0");
            Assert.True(premium["BrokerFeeGst"] == "0.0");
            Assert.True(premium["UnderwriterFee"] == "0.0");
            Assert.True(premium["UnderwriterFeeGst"] == "0.0");
            Assert.True(premium["StampDutyTotal"] == "11.0");
            Assert.True(premium["TotalPremium"] == "121.0");
            Assert.True(premium["TotalGst"] == "10.0");
            Assert.True(premium["TotalPayable"] == "125.84");
            Assert.True(premium["Esl"] == "0.0");

            // should not include all other tabs
            ((string)obj.Value["Claims"]).Should().BeNull();
            ((string)obj.Value["Questions"]).Should().BeNull();
            ((string)obj.Value["Documents"]).Should().BeNull();
        }

        /// <summary>
        /// The GetPolicyDetails_Questions.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>PolicyTransactionPeriodicSummaryQueryOptionsModel
        [Fact]
        public async Task GetPolicyDetails_Questions()
        {
            // act
            IActionResult result = await this.policyController.GetPolicyDetails(this.quoteAggregate.Id, "questions", "none");
            var resultJsonString = JsonConvert.SerializeObject(result);
            dynamic obj = JsonConvert.DeserializeObject(resultJsonString);

            // assert
            Assert.NotNull(obj.Value["Questions"]);

            // should not include all other tabs
            Assert.Null(obj.Value["PremiumData"]);
            Assert.Null(obj.Value["Claims"]);
            Assert.Null(obj.Value["Documents"]);
        }

        /// <summary>
        /// The GetPolicyDetails_Documents.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        public async Task GetPolicyDetails_Documents()
        {
            // act
            IActionResult result = await this.policyController.GetPolicyDetails(this.quoteAggregate.Id, "documents", "none");
            var resultJsonString = JsonConvert.SerializeObject(result);
            dynamic obj = JsonConvert.DeserializeObject(resultJsonString);

            // assert
            Assert.NotNull(obj.Value["Documents"]);

            // should not include all other tabs
            Assert.Null(obj.Value["PremiumData"]);
            Assert.Null(obj.Value["Claims"]);
            Assert.Null(obj.Value["Questions"]);
        }

        /// <summary>
        /// The GetPolicyHistory.
        /// </summary>
        [Fact]
        public async void GetPolicyHistory()
        {
            IActionResult history = await this.policyController.GetHistory(this.quoteAggregate.Id, "none");
            var result = JsonConvert.SerializeObject(history);
            dynamic obj = JsonConvert.DeserializeObject(result);

            // assert
            Assert.True(obj.Value.Count == 1);

            obj = obj.Value[0];
            Guid? policyId = Guid.Parse(obj.PolicyId.ToString());

            // assert
            policyId.Should().NotBeNull();
            ((string)obj.EventTypeSummary).Should().Be("Sample");
        }

        [Fact]
        public async Task GetPeriodicSummary_ReturnsForbiddenResult_WhenAgentUserIsNotAuthorizedAsync()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", this.tenantId.ToString()),
                new Claim("OrganisationId", this.organisationId.ToString()),
            }));
            this.policyController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };
            var options = new PolicyTransactionPeriodicSummaryQueryOptionsModel();
            options.FromDateTime = Instant.FromDateTimeUtc(DateTime.Now.AddMonths(-1).ToUniversalTime()).ToLocalDateInAet().ToIso8601();
            options.ToDateTime = Instant.FromDateTimeUtc(DateTime.Now.ToUniversalTime()).ToLocalDateInAet().ToIso8601();
            options.SamplePeriodLengthString = "day";
            options.IncludeProperties = new List<string>()
            {
                nameof(PolicyTransactionPeriodicSummaryModel.CreatedCount),
            };

            this.authorisationService
                .Setup(c => c.ApplyViewPolicyRestrictionsToFilters(It.IsAny<ClaimsPrincipal>(), It.IsAny<PolicyReadModelFilters>()))
                .Throws(new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    Permission.ViewPolicies, "policy", null)));

            // Act
            Func<Task> act = () => this.policyController.GetPeriodicSummary(options);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("permission.required.to.access.resource");
            exception.Which.Error.HttpStatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetPeriodicSummary_ReturnsError_WhenMissingRequireParameters()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", this.tenantId.ToString()),
                new Claim("OrganisationId", this.organisationId.ToString()),
            }));
            this.policyController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };

            // required includeProperties is missing
            var options = new PolicyTransactionPeriodicSummaryQueryOptionsModel();
            options.FromDateTime = Instant.FromDateTimeUtc(DateTime.Now.AddMonths(-1).ToUniversalTime()).ToLocalDateInAet().ToIso8601();
            options.ToDateTime = Instant.FromDateTimeUtc(DateTime.Now.ToUniversalTime()).ToLocalDateInAet().ToIso8601();
            options.SamplePeriodLengthString = "all";
            this.authorisationService
              .Setup(c => c.ApplyViewPolicyRestrictionsToFilters(It.IsAny<ClaimsPrincipal>(), It.IsAny<PolicyReadModelFilters>()))
              .Returns(Task.FromResult(true));

            // Act
            Func<Task> act = () => this.policyController.GetPeriodicSummary(options);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("required.request.parameter.missing");
            exception.Which.Error.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-05T00:00:00.0000000", "day", "CreatedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("20-01-2020", "2020-01-05T00:00:00.0000000", "day", "CreatedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "20-01-2050", "day", "CreatedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2023-03-05T00:00:00.0000000", "2020-01-05T00:00:00.0000000", "day", "CreatedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "", "day", "CreatedCount", "newBusiness", "dev", "required.request.parameter.missing")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-10T00:00:00.0000000", "", "CreatedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-10T00:00:00.0000000", "days", "CreatedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-10T00:00:00.0000000", "months", "CreatedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-10T00:00:00.0000000", "day", null, "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-10T00:00:00.0000000", "day", "TotalPremium", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-10T00:00:00.0000000", "day", "Total,Premium", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-10T00:00:00.0000000", "day", "Total*Premium", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-10T00:00:00.0000000", "day", "Total|Premium", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-10T00:00:00.0000000", "*", "CreatedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-06T00:00:00.0000000", "day", "CreatedCount", "oldBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "day", "CreatedCount", "newBusiness, oldBusiness", null, "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "month", "CreatedCount", "newBusiness", "dev*", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "quarter", "CreatedCount", "newBusiness", "dev, deborah-", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "quarter", "CreatedCount", "newBusiness", ",,", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "month", "CreatedCount, CreatedTotalPremium,CreatedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "month", "CreatedCount", "newBusinessrenewal", null, "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "month", "CreatedCount", "newBusiness", "dev,devtwo", "request.parameter.invalid")]

        public async Task GetPeriodicSummary_ReturnsError_WhenParametersAreInvalid(
            string fromDateTime,
            string toDateTime,
            string samplePeriodLength,
            string includeProperties,
            string policyTransactionTypes,
            string products,
            string errorCodeExpected)
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", this.tenantId.ToString()),
                new Claim("OrganisationId", this.organisationId.ToString()),
            }));
            this.policyController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };

            // invalid paramaters for includeParameters
            var options = new PolicyTransactionPeriodicSummaryQueryOptionsModel();
            options.FromDateTime = fromDateTime;
            options.ToDateTime = toDateTime;
            options.SamplePeriodLengthString = samplePeriodLength;
            options.IncludeProperties = new List<string>() { includeProperties };

            // optional parameters
            options.PolicyTransactionTypes = new List<string>() { policyTransactionTypes };
            options.Products = new List<string>() { products };

            this.authorisationService
              .Setup(c => c.ApplyViewPolicyRestrictionsToFilters(It.IsAny<ClaimsPrincipal>(), It.IsAny<PolicyReadModelFilters>()))
              .Returns(Task.FromResult(true));

            // Act
            Func<Task> act = () => this.policyController.GetPeriodicSummary(options);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be(errorCodeExpected);
            exception.Which.Error.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("2020-01-05", "2020-01-06", "day", "CreatedCount", "newBusiness", null)]
        [InlineData("2020-01-05", "2023-01-06", "day", "CreatedCount", "newBusiness", null)]
        [InlineData("2020-01-05", "2023-01-06", "month", "CreatedCount", "newBusiness", null)]
        [InlineData("2020-01-05", "2023-01-06", "quarter", "CreatedCount", "newBusiness", null)]
        [InlineData("2020-01-05", "2023-01-06", "year", "CreatedCount", "newBusiness", null)]
        [InlineData("2020-01-05", "2023-01-06", "all", "CreatedCount", "newBusiness", null)]
        [InlineData("2020-01-05", "2023-01-06", "month", "CreatedCount", "newBusiness", null)]
        [InlineData("2020-01-05", "2023-01-06", "month ", "CreatedTotalPremium", "newBusiness", null)]
        public async Task GetPeriodicSummary_ReturnsResults_WhenParametersAreValid(
            string fromDateTime,
            string toDateTime,
            string samplePeriodLength,
            string includeProperties,
            string policyTransactionTypes,
            string products)
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", this.tenantId.ToString()),
                new Claim("OrganisationId", this.organisationId.ToString()),
            }));
            this.policyController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };
            var options = new PolicyTransactionPeriodicSummaryQueryOptionsModel();
            options.FromDateTime = fromDateTime;
            options.ToDateTime = toDateTime;
            options.SamplePeriodLengthString = samplePeriodLength;
            options.IncludeProperties = new List<string>()
            {
                includeProperties,
            };

            // optional parameters
            if (policyTransactionTypes != null)
            {
                options.PolicyTransactionTypes = new List<string>() { policyTransactionTypes };
            }

            if (products != null)
            {
                options.Products = new List<string>() { products };
            }

            this.authorisationService
              .Setup(c => c.ApplyViewPolicyRestrictionsToFilters(It.IsAny<ClaimsPrincipal>(), It.IsAny<PolicyReadModelFilters>()))
              .Returns(Task.FromResult(true));
            this.cqrsMediator
                .Setup(c => c.Send(It.IsAny<GetPolicyTransactionPeriodicSummariesQuery>(), default))
                .Returns(Task.FromResult(new List<PolicyTransactionPeriodicSummaryModel>()));

            // Act
            var result = await this.policyController.GetPeriodicSummary(options);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        /// <summary>
        /// The createPolicyHistory.
        /// </summary>
        /// <param name="policyNumber">The policyNumber<see cref="string"/>.</param>
        private void CreatePolicyHistory(string policyNumber)
        {
            var fakePolicyHistory = new List<FakePolicyReadModelDetails>
            {
                new FakePolicyReadModelDetails
                {
                    TenantId = TenantFactory.DefaultId,
                    PolicyNumber = policyNumber,
                    PolicyId = new Guid("00000000-0000-0000-0000-000000000001"),
                    QuoteId = new Guid("00000001-b077-4571-a5f9-eef9bc26cab0"),
                    QuoteType = QuoteType.NewBusiness,
                    CreatedTimestamp = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(365)),
                    InceptionDateTime = new LocalDate(2018, 12, 6).AtStartOfDayInZone(Timezones.AET).LocalDateTime,
                },

                new FakePolicyReadModelDetails
                {
                    TenantId = TenantFactory.DefaultId,
                    PolicyNumber = policyNumber,
                    PolicyId = new Guid("00000000-0000-0000-0000-000000000001"),
                    QuoteId = new Guid("00000002-b077-4571-a5f9-eef9bc26cab0"),
                    QuoteType = QuoteType.Adjustment,
                    CreatedTimestamp = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(200)),
                    InceptionDateTime = new LocalDate(2018, 12, 6).AtStartOfDayInZone(Timezones.AET).LocalDateTime,
                },

                new FakePolicyReadModelDetails
                {
                    TenantId = TenantFactory.DefaultId,
                    PolicyNumber = policyNumber,
                    PolicyId = new Guid("00000000-0000-0000-0000-000000000001"),
                    QuoteId = new Guid("00000003-b077-4571-a5f9-eef9bc26cab0"),
                    QuoteType = QuoteType.Renewal,
                    CreatedTimestamp = SystemClock.Instance.GetCurrentInstant(),
                    InceptionDateTime = new LocalDate(2018, 12, 6).AtStartOfDayInZone(Timezones.AET).LocalDateTime,
                },
            };
            this.fakePolicyHistory = fakePolicyHistory.AsEnumerable();
        }

        /// <summary>
        /// The setPolicyDetails.
        /// </summary>
        /// <param name="person">The person<see cref="PersonAggregate"/>.</param>
        private void SetPolicyDetails(PersonAggregate person)
        {
            var json = "{\"state\":\"premiumComplete\",\"questions\":{\"ratingPrimary\":{\"something\": 9000,\"ratingState\": \"QLD\"},\"ratingSecondary\":{\"travelCover\": \"No\",\"policyStartDate\": \"04/12/2019\",\"policyAdjustmentDate\": \"05/12/2019\"},\"contact\":{\"contactName\": \"aaa\",\"contactEmail\": \"aaa@aaa.com\",\"contactPhone\": \"0411111111\",\"contactMobile\": \"0322223333\",\"source\": \"Facebook\",\"deliveryMethod\": \"Email\"},\"personal\":{\"insuredFullName\": \"TEST001\",\"residentialAddress\": \"test\",\"residentialTown\": \"testsdfsdf\",\"residentialState\": \"QLD\",\"residentialPostcode\": \"4000\",\"postalAddress\": \"test\",\"postalTown\": \"test\",\"postalState\": \"QLD\",\"postalPostcode\": \"4000\"},\"disclosure\":{\"history\": \"No\",\"insolvency\": \"No\",\"declaration\": \"Yes\"},\"paymentOptions\":{\"paymentOption\": \"Monthly\"},\"paymentMethods\":{\"paymentMethodSelect\": \"Visa\",\"paymentMethod\": \"Visa\"},\"successPage\":{},},\"risk1\":{\"settings\":{\"riskName\":\"Something\",\"fieldCodePrefix\":\"S\"},\"ratingFactors\":{\"something\":9000,\"travelCover\":\"No\",\"ratingState\":\"QLD\",\"policyStartDate\":43803},\"checks\":{\"includeRisk\":true},\"premium\":{\"propertyCover\":680,\"travelCover\":0,\"basePremium\":680,\"premiumPercentACT\":0,\"premiumPercentNSW\":0,\"premiumPercentNT\":0,\"premiumPercentQLD\":1,\"premiumPercentSA\":0,\"premiumPercentTAS\":0,\"premiumPercentVIC\":0,\"premiumPercentWA\":0},\"other\":{\"something\":\"Val\"},\"statutoryRates\":{\"riskType\":\"General\",\"ESLApplicability\":\"Not Applicable\",\"jurisdiction\":\"QLD\",\"policyStartDate\":43803,\"ESLRate\":0,\"GSTRate\":0.1,\"SDRateACT\":0,\"SDRateNSW\":0.09,\"SDRateNT\":0.1,\"SDRateQLD\":0.09,\"SDRateSA\":0.11,\"SDRateTAS\":0.1,\"SDRateVIC\":0.1,\"SDRateWA\":0.1},\"payment\":{\"premium\":680,\"ESL\":0,\"GST\":68,\"SDACT\":0,\"SDNSW\":0,\"SDNT\":0,\"SDQLD\":67.32,\"SDSA\":0,\"SDTAS\":0,\"SDVIC\":0,\"SDWA\":0,\"total\":748}},\"risk2\":{\"settings\":{\"riskName\":\"Else\",\"fieldCodePrefix\":\"E\"},\"ratingFactors\":{\"something\":9000,\"travelCover\":\"No\",\"ratingState\":\"QLD\",\"policyStartDate\":43803},\"checks\":{\"includeRisk\":true},\"premium\":{\"propertyCover\":0,\"travelCover\":0,\"basePremium\":0,\"premiumPercentACT\":0,\"premiumPercentNSW\":0,\"premiumPercentNT\":0,\"premiumPercentQLD\":1,\"premiumPercentSA\":0,\"premiumPercentTAS\":0,\"premiumPercentVIC\":0,\"premiumPercentWA\":0},\"other\":{\"something\":\"Val\"},\"statutoryRates\":{\"riskType\":\"Professional Indemnity\",\"ESLApplicability\":\"Applicable\",\"jurisdiction\":\"QLD\",\"policyStartDate\":43803,\"ESLRate\":0.3,\"GSTRate\":0.1,\"SDRateACT\":0,\"SDRateNSW\":0.05,\"SDRateNT\":0.1,\"SDRateQLD\":0.09,\"SDRateSA\":0.11,\"SDRateTAS\":0.1,\"SDRateVIC\":0.1,\"SDRateWA\":0.1},\"payment\":{\"premium\":0,\"ESL\":0,\"GST\":0,\"SDACT\":0,\"SDNSW\":0,\"SDNT\":0,\"SDQLD\":0,\"SDSA\":0,\"SDTAS\":0,\"SDVIC\":0,\"SDWA\":0,\"total\":0}},\"triggers\":{\"softReferral\":{\"history\":false,\"insolvency\":false},\"hardReferral\":{},\"decline\":{},\"error\":{\"noCoverSelected\":false},},\"payment\":{\"outputVersion\": 2,\"priceComponents\": {\"basePremium\": \"$680.00\",\"ESL\": \"$0.00\",\"premiumGST\": \"$68.00\",\"stampDutyACT\": \"$0.00\",\"stampDutyNSW\": \"$0.00\",\"stampDutyNT\": \"$0.00\",\"stampDutyQLD\": \"$67.32\",\"stampDutySA\": \"$0.00\",\"stampDutyTAS\": \"$0.00\",\"stampDutyVIC\": \"$0.00\",\"stampDutyWA\": \"$0.00\",\"stampDutyTotal\": \"$67.32\",\"totalPremium\": \"$815.32\",\"commission\": \"$136.00\",\"commissionGST\": \"$13.60\",\"brokerFee\": \"$0.00\",\"brokerFeeGST\": \"$0.00\",\"underwriterFee\": \"$0.00\",\"underwriterFeeGST\": \"$0.00\",\"interest\": \"$0.00\",\"merchantFees\": \"$12.23\",\"transactionCosts\": \"$1.80\",\"totalGST\": \"$68.00\",\"totalPayable\": \"$829.35\",},\"total\": {\"premium\": \"$680.00\",\"ESL\": \"$0.00\",\"GST\": \"$68.00\",\"stampDuty\": \"$67.32\",\"serviceFees\": \"$0.00\",\"interest\": \"$0.00\",\"merchantFees\": \"$12.23\",\"transactionCosts\": \"$1.80\",\"payable\": \"$829.35\"},\"instalments\": {\"instalmentsPerYear\": \"12\",\"instalmentAmount\": \"$69.11\"}}}";

            var inceptionDate = new LocalDate(2020, 1, 6);
            var expiryDate = inceptionDate.PlusYears(1);
            var inceptionTimestamp = inceptionDate.AtStartOfDayInZone(Timezones.AET).ToInstant();
            var expiryTimestamp = expiryDate.AtStartOfDayInZone(Timezones.AET).ToInstant();
            var createdDate = new LocalDate(2019, 12, 6);
            var issuedTimestamp = createdDate.AtStartOfDayInZone(Timezones.AET).ToInstant();
            var quote = this.quoteAggregate.GetQuoteOrThrow(this.quote.Id);
            this.policyDetails = new FakePolicyReadModelDetails
            {
                Claims = Enumerable.Empty<IClaimReadModelSummary>(),
                CreatedTimestamp = this.quoteAggregate.CreatedTimestamp,
                CustomerFullName = person.FullName,
                CustomerId = new Guid("fca04c35-74b5-4f58-a93c-db3e7b8b3bf3"),
                CustomerPreferredName = person.PreferredName,
                Documents = Enumerable.Empty<QuoteDocumentReadModel>(),
                LatestPolicyPeriodStartDate = inceptionDate,
                ExpiryDateTime = expiryTimestamp.ToLocalDateTimeInAet(),
                ExpiryTimestamp = expiryTimestamp,
                InceptionDateTime = inceptionTimestamp.ToLocalDateTimeInAet(),
                InceptionTimestamp = inceptionTimestamp,
                LatestPolicyPeriodStartTimestamp = inceptionTimestamp,
                IssuedTimestamp = issuedTimestamp,
                IsAdjusted = false,
                TenantId = this.quoteAggregate.TenantId,
                OrganisationId = this.quoteAggregate.OrganisationId,
                PolicyNumber = this.testPolicyNumber,
                QuoteType = QuoteType.NewBusiness,
                PolicyCalculationResultJson = json,
                PolicyCalculationResultId = default,
                PolicyCalculationFormDataId = quote.LatestFormData.Id,
                LatestFormData = quote.LatestFormData.Data.Json,
            };
        }
    }

    /// <summary>
    /// Defines the <see cref="FakePolicyReadModelSummary" />.
    /// </summary>
#pragma warning disable SA1402 // File may only contain a single type
    internal class FakePolicyReadModelSummary : IPolicyReadModelSummary
#pragma warning restore SA1402 // File may only contain a single type
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the QuoteId.
        /// </summary>
        public Guid? QuoteId { get; set; }

        /// <summary>
        /// Gets or sets the Policy Title.
        /// </summary>
        public string PolicyTitle { get; set; }

        /// <summary>
        /// Gets or sets the Policy State.
        /// </summary>
        public string PolicyState { get; set; }

        /// <summary>
        /// Gets or sets the TenantId.
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// Gets or sets the OrganisationId.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the QuoteNumber.
        /// </summary>
        public string QuoteNumber { get; set; }

        /// <summary>
        /// Gets or sets the product feature setting.
        /// </summary>
        public ProductFeatureSetting ProductFeatureSetting { get; set; }

        /// <summary>
        /// Gets or sets invoice number.
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Gets or sets invoice time.
        /// </summary>
        public Instant InvoiceTimestamp { get; set; }

        /// <summary>
        /// Gets or sets quote submission time.
        /// </summary>
        public Instant SubmissionTimestamp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets invoice.
        /// </summary>
        public bool IsInvoiced { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether quote is paidfor.
        /// </summary>
        public bool IsPaidFor { get; set; }

        /// <summary>
        /// Gets or sets the Cancellation Time.
        /// </summary>
        public Instant? CancellationEffectiveTimestamp
        {
            get => this.CancellationEffectiveTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.CancellationEffectiveTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.CancellationEffectiveTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        /// <summary>
        /// Gets or sets the Cancellation date.
        /// </summary>
        public LocalDateTime? CancellationEffectiveDateTime { get; set; }

        public long? CancellationEffectiveTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the Cancellation Time.
        /// </summary>
        public Instant? AdjustmentEffectiveTimestamp
        {
            get => this.AdjustmentEffectiveTicksSinceEpoch.HasValue
                ? Instant.FromUnixTimeTicks(this.AdjustmentEffectiveTicksSinceEpoch.Value)
                : (Instant?)null;
            set => this.AdjustmentEffectiveTicksSinceEpoch = value?.ToUnixTimeTicks();
        }

        /// <summary>
        /// Gets or sets the Cancellation date.
        /// </summary>
        public LocalDateTime? AdjustmentEffectiveDateTime { get; set; }

        public long? AdjustmentEffectiveTicksSinceEpoch { get; set; }

        /// <summary>
        /// Gets or sets the PolicyNumber.
        /// </summary>
        public string PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsSubmitted.
        /// </summary>
        public bool IsSubmitted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsTestData.
        /// </summary>
        public bool IsTestData { get; set; }

        /// <summary>
        /// Gets or sets the ProductId.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets the ProductName.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the NameOfProduct.
        /// </summary>
        public string NameOfProduct { get; set; }

        /// <summary>
        /// Gets or sets the OwnerUserId.
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the CustomerId.
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the CustomerFullName.
        /// </summary>
        public string CustomerFullName { get; set; }

        /// <summary>
        /// Gets or sets the CustomerPreferredName.
        /// </summary>
        public string CustomerPreferredName { get; set; }

        /// <summary>
        /// Gets or sets the LastModifiedTime.
        /// </summary>
        public Instant LastModifiedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the LatestCalculationResultJson.
        /// </summary>
        public string LatestCalculationResultJson { get; set; }

        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        public QuoteStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether PolicyIssued.
        /// </summary>
        public bool PolicyIssued { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether PolicyIsCancelled.
        /// </summary>
        public bool PolicyIsCancelled { get; set; }

        /// <summary>
        /// Gets or sets the CreatedTimestamp.
        /// </summary>
        public Instant CreatedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the PolicyIssueDate.
        /// </summary>
        public LocalDateTime PolicyIssueDate { get; set; }

        /// <summary>
        /// Gets or sets the LatestCalculationResult.
        /// </summary>
        public Domain.ReadWriteModel.CalculationResult LatestCalculationResult { get; set; }

        /// <summary>
        /// Gets or sets the QuoteType.
        /// </summary>
        public QuoteType QuoteType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsAdjusted.
        /// </summary>
        public bool IsAdjusted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsCancelled.
        /// </summary>
        public bool IsCancelled { get; set; }

        /// <summary>
        /// Gets or sets the EffectiveDate.
        /// </summary>
        public LocalDateTime LatestPolicyPeriodStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the EffectiveTime.
        /// </summary>
        public Instant LatestPolicyPeriodStartTimestamp
        {
            get => Instant.FromUnixTimeTicks(this.LatestPolicyPeriodStartTicksSinceEpoch);
            set => this.LatestPolicyPeriodStartTicksSinceEpoch = value.ToUnixTimeTicks();
        }

        public long LatestPolicyPeriodStartTicksSinceEpoch { get; set; }

        public LocalDateTime InceptionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the InceptionTime.
        /// </summary>
        public Instant InceptionTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the PolicyExpiryDate.
        /// </summary>
        public LocalDateTime? ExpiryDateTime { get; set; }

        /// <summary>
        /// Gets or sets the ExpiryTime.
        /// </summary>
        public Instant? ExpiryTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the policy latest renewal effective date time.
        /// </summary>
        public Instant? LatestRenewalEffectiveTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the QuoteState.
        /// </summary>
        public string QuoteState { get; set; }

        /// <summary>
        /// Gets or sets the PolicyIssuedTime.
        /// </summary>
        public Instant IssuedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the Environment.
        /// </summary>
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets the PolicyId.
        /// </summary>
        public Guid PolicyId { get; set; }

        public string SerializedCalculationResult => string.Empty;

        public CalculationResultReadModel CalculationResult => throw new NotImplementedException();

        public bool IsTermBased => this.ExpiryTimestamp.HasValue;

        public bool AreTimestampsAuthoritative { get; set; }

        public DateTimeZone TimeZone { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="FakePolicyReadModelDetails" />.
    /// </summary>
#pragma warning disable SA1402 // File may only contain a single type
    internal class FakePolicyReadModelDetails : EntityReadModel<Guid>, IPolicyReadModelDetails
#pragma warning restore SA1402 // File may only contain a single type
    {
        private List<PolicyTransactionAndQuote> policyTransactions = new List<PolicyTransactionAndQuote>();

        public FakePolicyReadModelDetails()
        {
            this.TimeZone = Timezones.AET;
            this.policyTransactions.Add(
                new PolicyTransactionAndQuote
                {
                    PolicyTransaction = this.GetDisplayTransaction(new TestClock().Timestamp),
                });
        }

        /// <inheritdoc/>
        public Guid? CustomerPersonId { get; }

        /// <summary>
        /// Gets or sets the Policy Title.
        /// </summary>
        public string PolicyTitle { get; set; }

        /// <summary>
        /// Gets or sets the Policy State.
        /// </summary>
        public string PolicyState { get; set; }

        /// <summary>
        /// Gets or sets the OwnerFullName.
        /// </summary>
        public string OwnerFullName { get; set; }

        /// <summary>
        /// Gets or sets the name of the organisation this quote was created under.
        /// </summary>
        public string OrganisationName { get; set; }

        /// <summary>
        /// Gets or sets the Documents.
        /// </summary>
        public IEnumerable<QuoteDocumentReadModel> Documents { get; set; }

        /// <summary>
        /// Gets or sets the PolicyCalculationResultId.
        /// </summary>
        public Guid PolicyCalculationResultId { get; set; }

        /// <summary>
        /// Gets or sets the product feature setting.
        /// </summary>
        public ProductFeatureSetting ProductFeatureSetting { get; set; }

        /// <summary>
        /// Gets or sets the Cancellation Time.
        /// </summary>
        public Instant? CancellationEffectiveTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the cancellation date.
        /// </summary>
        public LocalDate? CancellationEffectiveDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether quote is discarded or not.
        /// </summary>
        public bool IsDiscarded { get; set; }

        /// <summary>
        /// Gets or sets the PolicyCalculationFormDataId.
        /// </summary>
        public Guid PolicyCalculationFormDataId { get; set; }

        /// <summary>
        /// Gets or sets the PolicyCalculationResultJson.
        /// </summary>
        public string PolicyCalculationResultJson { get; set; }

        /// <summary>
        /// Gets the Transactions.
        /// </summary>
        public IEnumerable<PolicyTransactionAndQuote> Transactions
        {
            get
            {
                return this.policyTransactions;
            }
        }

        /// <summary>
        /// Gets or sets invoice number.
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Gets or sets invoice time.
        /// </summary>
        public Instant InvoiceTimestamp { get; set; }

        /// <summary>
        /// Gets or sets quote submission time.
        /// </summary>
        public Instant SubmissionTimestamp { get; set; }

        /// <summary>
        /// Gets or sets policy inception time.
        /// </summary>
        public Instant PolicyInceptionTimestamp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets invoice.
        /// </summary>
        public bool IsInvoiced { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether quote is paidfor.
        /// </summary>
        public bool IsPaidFor { get; set; }

        /// <summary>
        /// Gets or sets the QuoteId.
        /// </summary>
        public Guid? QuoteId { get; set; }

        /// <summary>
        /// Gets or sets the OrganisationId.
        /// </summary>
        public Guid OrganisationId { get; set; }

        /// <summary>
        /// Gets or sets the Quote Title.
        /// </summary>
        public string QuoteTitle { get; set; }

        /// <summary>
        /// Gets or sets the QuoteNumber.
        /// </summary>
        public string QuoteNumber { get; set; }

        /// <summary>
        /// Gets or sets the product new id.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Gets or sets the PolicyNumber.
        /// </summary>
        public string PolicyNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsSubmitted.
        /// </summary>
        public bool IsSubmitted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsTestData.
        /// </summary>
        public bool IsTestData { get; set; }

        /// <summary>
        /// Gets or sets the ProductName.
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Gets or sets the NameOfProduct.
        /// </summary>
        public string NameOfProduct { get; set; }

        /// <summary>
        /// Gets or sets the OwnerUserId.
        /// </summary>
        public Guid? OwnerUserId { get; set; }

        /// <summary>
        /// Gets or sets the CustomerId.
        /// </summary>
        public Guid? CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the CustomerFullName.
        /// </summary>
        public string CustomerFullName { get; set; }

        /// <summary>
        /// Gets or sets the CustomerPreferredName.
        /// </summary>
        public string CustomerPreferredName { get; set; }

        /// <summary>
        /// Gets or sets the LatestCalculationResultJson.
        /// </summary>
        public string LatestCalculationResultJson { get; set; }

        /// <summary>
        /// Gets or sets the PolicyExpiryDate.
        /// </summary>
        public LocalDateTime? ExpiryDateTime { get; set; }

        /// <summary>
        /// Gets or sets the PolicyInceptionDate.
        /// </summary>
        public LocalDateTime InceptionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        public QuoteStatus Status { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether PolicyIssued.
        /// </summary>
        public bool PolicyIssued { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether PolicyIsCancelled.
        /// </summary>
        public bool PolicyIsCancelled { get; set; }

        /// <summary>
        /// Gets or sets the PolicyIssueDate.
        /// </summary>
        public LocalDateTime PolicyIssueDate { get; set; }

        /// <summary>
        /// Gets or sets the LatestCalculationResult.
        /// </summary>
        public Domain.ReadWriteModel.CalculationResult LatestCalculationResult { get; set; }

        /// <summary>
        /// Gets or sets the QuoteType.
        /// </summary>
        public QuoteType QuoteType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsAdjusted.
        /// </summary>
        public bool IsAdjusted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether IsCancelled.
        /// </summary>
        public bool IsCancelled { get; set; }

        /// <summary>
        /// Gets or sets the EffectiveDate.
        /// </summary>
        public LocalDate LatestPolicyPeriodStartDate { get; set; }

        /// <summary>
        /// Gets or sets the InceptionTime.
        /// </summary>
        public Instant InceptionTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the ExpiryTime.
        /// </summary>
        public Instant? ExpiryTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the EffectiveTime.
        /// </summary>
        public Instant LatestPolicyPeriodStartTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the policy latest renewal effective date time.
        /// </summary>
        public Instant? LatestRenewalEffectiveTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the LatestFormData.
        /// </summary>
        public string LatestFormData { get; set; }

        /// <summary>
        /// Gets or sets the QuoteState.
        /// </summary>
        public string QuoteState { get; set; }

        /// <summary>
        /// Gets or sets the PolicyIssuedTime.
        /// </summary>
        public Instant IssuedTimestamp { get; set; }

        /// <summary>
        /// Gets or sets the Environment.
        /// </summary>
        public DeploymentEnvironment Environment { get; set; }

        /// <summary>
        /// Gets or sets the Claims.
        /// </summary>
        public IEnumerable<IClaimReadModelSummary> Claims { get; set; }

        /// <summary>
        /// Gets or sets the PolicyId.
        /// </summary>
        public Guid PolicyId { get; set; }

        public Guid LatestCalculationResultId { get; set; }

        public Guid LatestCalculationResultFormDataId { get; set; }

        public string SerializedCalculationResult => string.Empty;

        public CalculationResultReadModel CalculationResult { get; set; }

        public long CancellationEffectiveTicksSinceEpoch { get; set; }

        public string SerializedLatestCalculationResult { get; set; }

        /// <summary>
        /// Gets or sets the quote expiry time.
        /// </summary>
        public Instant? QuoteExpiryTimestamp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the expiry is enabled.
        /// </summary>
        public bool ExpiryEnabled { get; set; }

        public Guid? CustomerOwnerUserId { get; }

        public Guid? QuoteOwnerUserId { get; }

        public Guid? PolicyOwnerUserId { get; }

        public bool IsTermBased { get; set; }

        public LocalDateTime? CancellationEffectiveDateTime { get; set; }

        public LocalDateTime? AdjustmentEffectiveDateTime { get; set; }

        public Instant? AdjustmentEffectiveTimestamp { get; set; }

        public LocalDateTime LatestPolicyPeriodStartDateTime { get; set; }

        public DateTimeZone TimeZone { get; set; }

        public bool AreTimestampsAuthoritative { get; set; }

        public Guid? DisplayTransactionReleaseId => this.Transactions.FirstOrDefault()?.PolicyTransaction.ProductReleaseId;

        public PolicyTransaction GetCurrentTransaction(Instant time)
        {
            return new FakePolicyTransaction(TenantFactory.DefaultId, Guid.NewGuid());
        }

        public PolicyTransaction GetCurrentTransaction(bool areTimestampsAuthoritative, DateTimeZone timeZone, Instant asAtTimestamp)
        {
            return this.GetCurrentTransaction(asAtTimestamp);
        }

        public string GetDetailStatus(Instant time)
        {
            return "active";
        }

        public string GetDetailStatus(bool areTimestampsAuthoritative, DateTimeZone timeZone, Instant time)
        {
            return "active";
        }

        public PolicyTransaction GetDisplayTransaction(Instant time)
        {
            var instant = new TestClock().Timestamp;
            var date = DateTime.Now.ToString("M/d/yy").ToLocalDateFromMdyy();
            var quoteDataSnapshot = this.CreateQuoteSnapshot(instant);
            var policyData = new PolicyData(quoteDataSnapshot);
            var data = new PolicyTransactionData(policyData);
            return new FakePolicyTransaction(TenantFactory.DefaultId, Guid.NewGuid(), data, Guid.NewGuid());
        }

        public PolicyTransaction GetDisplayTransaction(bool areTimestampsAuthoritative, DateTimeZone timeZone, Instant asAtTimestamp)
        {
            return this.GetDisplayTransaction(asAtTimestamp);
        }

        private QuoteDataSnapshot CreateQuoteSnapshot(Instant timestamp)
        {
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
            var calculationResultJson = CalculationResultJsonFactory.Create();
            var quoteData = QuoteFactory.QuoteDataRetriever(
                new CachingJObjectWrapper(formDataJson), new CachingJObjectWrapper(calculationResultJson));
            var calculationResult = Domain.ReadWriteModel.CalculationResult.CreateForNewPolicy(
                new CachingJObjectWrapper(calculationResultJson), quoteData);
            return new QuoteDataSnapshot(
                    new QuoteDataUpdate<Domain.Aggregates.Quote.FormData>(
                        Guid.NewGuid(), new Domain.Aggregates.Quote.FormData(formDataJson), timestamp),
                    new QuoteDataUpdate<Domain.ReadWriteModel.CalculationResult>(
                        Guid.NewGuid(), calculationResult, timestamp),
                    new QuoteDataUpdate<IPersonalDetails>(
                        Guid.NewGuid(), new FakePersonalDetails(), timestamp));
        }
    }
}
