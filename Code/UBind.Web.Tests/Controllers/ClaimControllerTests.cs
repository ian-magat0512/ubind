// <copyright file="ClaimControllerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method should be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Humanizer;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application;
    using UBind.Application.Authorisation;
    using UBind.Application.Dashboard.Model;
    using UBind.Application.Queries.Claim;
    using UBind.Application.Services;
    using UBind.Application.Tests.Fakes;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Claim.Workflow;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Authentication;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Json;
    using UBind.Domain.NumberGenerators;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ReferenceNumbers;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.ReadModels.Claim;
    using UBind.Web.Controllers;
    using UBind.Web.ResourceModels;
    using UBind.Web.ResourceModels.Claim;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="ClaimControllerTests"/>.
    /// </summary>
    public class ClaimControllerTests
    {
        /// <summary>
        /// Defines the currentOrganisation.
        /// </summary>
        private readonly string currentOrganisation = Guid.NewGuid().ToString();

        /// <summary>
        /// Defines the Performing UserId.
        /// </summary>
        private readonly Guid? performingUserId = Guid.NewGuid();

        /// <summary>
        /// Defines the userId.
        /// </summary>
        private readonly Guid userId = new Guid("A1B2A3B4-3333-2222-1111-000000000000");

        private readonly IQuoteWorkflow quoteWorkflow = new DefaultQuoteWorkflow();

        private readonly Mock<IAuthorisationService> authorisationService = new Mock<IAuthorisationService>();

        private readonly Mock<ICqrsMediator> cqrsMediator = new Mock<ICqrsMediator>();

        /// <summary>
        /// Defines the claimController.
        /// </summary>
        private ClaimController claimController;

        /// <summary>
        /// Defines the quote.
        /// </summary>
        private QuoteAggregate quoteAggregate;

        /// <summary>
        /// Defines the quote.
        /// </summary>
        private Quote quote;

        public ClaimControllerTests()
        {
            var claimWorkflowProvider = new Mock<IClaimWorkflowProvider>();
            var userAuthenticationData = new Mock<IUserAuthenticationData>();
            var claimAggregateRepository = new Mock<IClaimAggregateRepository>();
            var referenceNumberGenerator = new Mock<IQuoteReferenceNumberGenerator>();
            var claimReadModelRepository = new Mock<IClaimReadModelRepository>();
            var quoteAggregateRepository = new Mock<IQuoteAggregateRepository>();
            var customerAggregateRepository = new Mock<ICustomerAggregateRepository>();
            var personAggregateRepository = new Mock<IPersonAggregateRepository>();
            var claimNumberRepository = new Mock<IClaimNumberRepository>();
            var systemAlertService = new Mock<ISystemAlertService>();
            var customerService = new Mock<ICustomerService>();
            var userService = new Mock<Application.User.IUserService>();
            var productRepository = new Mock<IProductRepository>();
            var settingService = new Mock<IFeatureSettingService>();
            var claimService = new Mock<IClaimService>();
            var quoteService = new Mock<IQuoteService>();
            var configurationService = new Mock<IConfigurationService>();
            var claimProductConfigurationProvider = new DefaultProductConfigurationProvider();
            var formDataPrettifier = new Mock<IFormDataPrettifier>();
            var claimAttachmentService = new Mock<IApplicationClaimFileAttachmentService>();
            var policyService = new Mock<IPolicyService>();
            var organisationService = new Mock<IOrganisationService>();
            var cachingResolver = new Mock<ICachingResolver>();

            var tenant = TenantFactory.Create(Tenant.MasterTenantId);
            this.quote = QuoteAggregate.CreateNewBusinessQuote(
                tenant.Id,
                Guid.Parse(this.currentOrganisation),
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                QuoteExpirySettings.Default,
                this.performingUserId,
                SystemClock.Instance.GetCurrentInstant(),
                Guid.NewGuid(),
                Timezones.AET);
            this.quoteAggregate = this.quote.Aggregate;

            var person = PersonAggregate.CreatePerson(
               tenant.Id,
               tenant.Details.DefaultOrganisationId,
               this.performingUserId,
               SystemClock.Instance.GetCurrentInstant());
            var personCommonProperties = new PersonCommonProperties()
            {
                FullName = "Ann Claire",
                PreferredName = "Ann",
            };

            var personDetails = new PersonalDetails(tenant.Id, personCommonProperties);
            person.Update(personDetails, this.performingUserId, SystemClock.Instance.GetCurrentInstant());
            var customer = CustomerAggregate.CreateNewCustomer(
                tenant.Id,
                person,
                DeploymentEnvironment.Staging,
                this.performingUserId,
                null,
                SystemClock.Instance.GetCurrentInstant());
            this.quoteAggregate.RecordAssociationWithCustomer(customer, person, this.performingUserId, SystemClock.Instance.GetCurrentInstant());
            this.quoteAggregate.UpdateCustomerDetails(person, this.performingUserId, SystemClock.Instance.GetCurrentInstant(), this.quote.Id);
            var formnDataSchema = new FormDataSchema(new JObject());
            var formData = new Domain.Aggregates.Quote.FormData(FormDataJsonFactory.GetSampleWithStartAndEndDates());
            var calculationData = new CachingJObjectWrapper(CalculationResultJsonFactory.Create());
            var formDataId = this.quote.UpdateFormData(formData, this.performingUserId, SystemClock.Instance.GetCurrentInstant());
            var quoteDataRetreiver = QuoteFactory.QuoteDataRetriever(formData, calculationData);
            var calculationResult = CalculationResult.CreateForNewPolicy(calculationData, quoteDataRetreiver);
            calculationResult.FormDataId = formDataId;
            this.quote.RecordCalculationResult(
                calculationResult,
                calculationData,
                SystemClock.Instance.GetCurrentInstant(),
                formnDataSchema,
                false,
                this.performingUserId);
            var quote = this.quoteAggregate.GetQuoteOrThrow(this.quote.Id);
            var additionalPropertyValueServiceMock = new Mock<IAdditionalPropertyValueService>();
            var newBusinessquote = quote as NewBusinessQuote;
            newBusinessquote.IssuePolicy(
                quote.LatestCalculationResult.Id,
                () => "policy001",
                QuoteFactory.ProductConfiguation,
                new DefaultPolicyTransactionTimeOfDayScheme(),
                default,
                SystemClock.Instance.GetCurrentInstant(),
                this.quoteWorkflow);

            var claimAggregate = ClaimAggregate.CreateForPolicy(
                 "AAAAAA",
                 this.quoteAggregate,
                 Guid.NewGuid(),
                 "Peter Parker",
                 "test",
                 this.performingUserId,
                 SystemClock.Instance.GetCurrentInstant());
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
           {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Customer.Humanize()),
                new Claim("Tenant", TenantFactory.DefaultId.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
                new Claim("CustomerId", Guid.NewGuid().ToString()),
           }));

            claimService.Setup(s => s.ChangeClaimState(
                It.IsAny<ReleaseContext>(),
                It.IsAny<ClaimAggregate>(),
                It.IsAny<ClaimActions>(),
                It.IsAny<string?>())).ReturnsAsync(claimAggregate);
            this.cqrsMediator.Setup(s => s.Send(It.IsAny<GetClaimSummaryByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(() =>
            {
                var crms = Mock.Of<IClaimReadModelSummary>();

                crms.Status = "updated";

                return crms;
            });

            this.cqrsMediator.Setup(s => s.Send(It.IsAny<GetClaimsMatchingFiltersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    var crmsList = Mock.Of<List<IClaimReadModelSummary>>();
                    var crms = Mock.Of<IClaimReadModelSummary>();
                    crms.Status = "Incomplete";
                    crmsList.Add(crms);
                    return crmsList;
                });

            claimNumberRepository.Setup(c => c.ConsumeForProduct(
                this.quoteAggregate.TenantId,
                this.quoteAggregate.ProductId,
                this.quoteAggregate.Environment))
                .Returns("1");
            customerAggregateRepository.Setup(c => c.GetById(tenant.Id, this.quote.CustomerId.Value)).Returns(customer);
            quoteAggregateRepository.Setup(q => q.GetById(tenant.Id, this.quote.Id)).Returns(this.quoteAggregate);
            personAggregateRepository.Setup(p => p.GetById(tenant.Id, customer.PrimaryPersonId)).Returns(person);

            IClaimReadModelSummary claimReadModel = new ClaimReadModelSummary
            {
                Id = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CustomerFullName = "John Smith",
                ProductName = "Test Product",
                ClaimNumber = "123AAA",
                ClaimReference = "123AAAREF",
                Status = "Notified",
                CreatedTicksSinceEpoch = SystemClock.Instance.GetCurrentInstant().ToUnixTimeTicks(),
                LastModifiedTicksSinceEpoch = SystemClock.Instance.GetCurrentInstant().ToUnixTimeTicks(),
            };
            claimReadModelRepository.Setup(c => c.GetSummaryById(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(claimReadModel);

            cachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            cachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            cachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            cachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));

            this.claimController = new ClaimController(
                claimService.Object,
                null,
                configurationService.Object,
                claimProductConfigurationProvider,
                formDataPrettifier.Object,
                policyService.Object,
                organisationService.Object,
                claimReadModelRepository.Object,
                this.authorisationService.Object,
                this.cqrsMediator.Object,
                cachingResolver.Object,
                customerService.Object,
                additionalPropertyValueServiceMock.Object,
                settingService.Object);

            settingService.Setup(m => m.TenantHasActiveFeature(tenant.Id, Feature.ClaimsManagement)).Returns(true);
        }

        /// <summary>
        /// The CreateClaim_ReturnsOk_WhenUserIsPolicyCustomer.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        [Fact]
        public async Task CreateClaim_ReturnsOk_WhenUserIsPolicyCustomer()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Customer.Humanize()),
                new Claim("Tenant", TenantFactory.DefaultId.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
                new Claim("CustomerId", this.quoteAggregate.CustomerId.ToString()),
            }));

            this.claimController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };

            var claimCreateModel = new ClaimCreateModel
            {
                PolicyId = this.quoteAggregate.Id,
            };

            // Act
            var result = await this.claimController.CreateClaim(claimCreateModel);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetClaims_Succeeds_WhenStatusIsIncompleteAsync()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Customer.Humanize()),
                new Claim("Tenant", TenantFactory.DefaultId.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
                new Claim("CustomerId", this.quoteAggregate.CustomerId.ToString()),
            }));
            this.claimController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };

            // Act
            var options = new QueryOptionsModel()
            {
                Statuses = new List<string>() { "Incomplete", "Acknowledged", "Approved" },
            };

            var task = await this.claimController.GetClaims(options);

            // Assert
            Assert.IsType<OkObjectResult>(task);
        }

        [Fact]
        public async Task WithdrawClaim_Should_Return_ClaimSetModel_When_Successful()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Customer.Humanize()),
                new Claim("Tenant", TenantFactory.DefaultId.ToString()),
                new Claim("CustomerId", this.quoteAggregate.CustomerId.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
            }));

            this.claimController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };

            // Act
            var result = await this.claimController.WithdrawClaim(It.IsAny<Guid>());

            var claimSetModel = (result as OkObjectResult)?.Value as ClaimSetModel;

            // Assert
            claimSetModel.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPeriodicSummary_ReturnsForbiddenResult_WhenAgentUserIsNotAuthorizedAsync()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", TenantFactory.DefaultId.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
            }));
            this.claimController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };
            var options = new ClaimPeriodicSummaryQueryOptionsModel();
            options.FromDateTime = Instant.FromDateTimeUtc(DateTime.Now.AddMonths(-1).ToUniversalTime()).ToLocalDateInAet().ToIso8601();
            options.ToDateTime = Instant.FromDateTimeUtc(DateTime.Now.ToUniversalTime()).ToLocalDateInAet().ToIso8601();
            options.SamplePeriodLengthString = SamplePeriodLength.Day.ToString();
            options.IncludeProperties = new List<string>()
            {
                nameof(ClaimPeriodicSummaryModel.SettledCount),
            };

            this.authorisationService
                .Setup(c => c.ApplyViewClaimRestrictionsToFilters(It.IsAny<ClaimsPrincipal>(), It.IsAny<EntityListFilters>()))
                .Throws(new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    Permission.ViewClaims, "claim", null)));

            // Act
            Func<Task> act = () => this.claimController.GetPeriodicSummary(options);

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
                new Claim("Tenant", TenantFactory.DefaultId.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
            }));
            this.claimController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };

            // required includeProperties is missing
            var options = new ClaimPeriodicSummaryQueryOptionsModel();
            options.FromDateTime = Instant.FromDateTimeUtc(DateTime.Now.AddMonths(-1).ToUniversalTime()).ToLocalDateInAet().ToIso8601();
            options.ToDateTime = Instant.FromDateTimeUtc(DateTime.Now.ToUniversalTime()).ToLocalDateInAet().ToIso8601();
            options.SamplePeriodLengthString = SamplePeriodLength.Day.ToString();
            this.authorisationService
              .Setup(c => c.ApplyViewClaimRestrictionsToFilters(It.IsAny<ClaimsPrincipal>(), It.IsAny<EntityListFilters>()))
              .Returns(Task.FromResult(true));

            // Act
            Func<Task> act = () => this.claimController.GetPeriodicSummary(options);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("required.request.parameter.missing");
            exception.Which.Error.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-05T00:00:00.0000000", "day", "SettledCount", "dev", "request.parameter.invalid")]
        [InlineData("20-01-2020", "2020-01-05T00:00:00.0000000", "day", "SettledCount", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "20-01-2050", "day", "SettledCount", "dev", "request.parameter.invalid")]
        [InlineData("2023-03-05T00:00:00.0000000", "2020-01-05T00:00:00.0000000", "day", "SettledCount", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "", "day", "SettledCount", "dev", "required.request.parameter.missing")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-10T00:00:00.0000000", "", "SettledCount", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-10T00:00:00.0000000", "days", "SettledCount", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-10T00:00:00.0000000", "months", "SettledCount", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-10T00:00:00.0000000", "day", null, "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-10T00:00:00.0000000", "day", "TotalPremium", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-10T00:00:00.0000000", "day", "Total,Premium", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-10T00:00:00.0000000", "day", "Total*Premium", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-10T00:00:00.0000000", "day", "Total|Premium", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-10T00:00:00.0000000", "*", "SettledCount", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "month", "SettledCount", "dev*", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "quarter", "SettledCount", "dev, deborah-", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "quarter", "SettledCount", ",,", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "month", "ProcessedCount, CreatedTotalPremium,SettledCount", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "month", "ProcessedCount", "dev,devtwo", "request.parameter.invalid")]
        public async Task GetPeriodicSummary_ReturnsError_WhenParametersAreInvalid(
            string fromDateTime,
            string toDateTime,
            string samplePeriodLength,
            string includeProperties,
            string products,
            string errorCodeExpected)
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", TenantFactory.DefaultId.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
            }));
            this.claimController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };

            // invalid paramaters for includeParameters
            var options = new ClaimPeriodicSummaryQueryOptionsModel();
            options.FromDateTime = fromDateTime;
            options.ToDateTime = toDateTime;
            options.SamplePeriodLengthString = samplePeriodLength;
            options.IncludeProperties = new List<string>() { includeProperties };

            // optional parameters
            options.Products = new List<string>() { products };

            this.authorisationService
              .Setup(c => c.ApplyViewClaimRestrictionsToFilters(It.IsAny<ClaimsPrincipal>(), It.IsAny<EntityListFilters>()))
              .Returns(Task.FromResult(true));

            // Act
            Func<Task> act = () => this.claimController.GetPeriodicSummary(options);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be(errorCodeExpected);
            exception.Which.Error.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-06T00:00:00.0000000", "day", "SettledCount", null)]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "day", "SettledCount", null)]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "month", "SettledCount", null)]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "quarter", "SettledCount", null)]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "year", "SettledCount", null)]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "all", "SettledCount", null)]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "month", "ProcessedCount", null)]
        public async Task GetPeriodicSummary_ReturnsResults_WhenParametersAreValid(
            string fromDateTime,
            string toDateTime,
            string samplePeriodLength,
            string includeProperties,
            string products)
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", TenantFactory.DefaultId.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
            }));
            this.claimController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };
            var options = new ClaimPeriodicSummaryQueryOptionsModel();
            options.FromDateTime = fromDateTime;
            options.ToDateTime = toDateTime;
            options.SamplePeriodLengthString = samplePeriodLength;
            options.IncludeProperties = new List<string>()
            {
                includeProperties,
            };

            if (products != null)
            {
                options.Products = new List<string>() { products };
            }

            this.authorisationService
              .Setup(c => c.ApplyViewClaimRestrictionsToFilters(It.IsAny<ClaimsPrincipal>(), It.IsAny<EntityListFilters>()))
              .Returns(Task.FromResult(true));
            this.cqrsMediator
                .Setup(c => c.Send(It.IsAny<GetClaimPeriodicSummariesQuery>(), default))
                .Returns(Task.FromResult(new List<ClaimPeriodicSummaryModel>()));

            // Act
            var result = await this.claimController.GetPeriodicSummary(options);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
