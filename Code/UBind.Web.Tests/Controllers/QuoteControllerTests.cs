// <copyright file="QuoteControllerTests.cs" company="uBind">
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
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NodaTime;
    using RedLockNet;
    using StackExchange.Redis;
    using UBind.Application;
    using UBind.Application.Authorisation;
    using UBind.Application.Dashboard.Model;
    using UBind.Application.Queries.FeatureSettings;
    using UBind.Application.Queries.Quote;
    using UBind.Application.Services;
    using UBind.Application.Services.Search;
    using UBind.Application.Services.SystemEmail;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Workflow;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Dto;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Search;
    using UBind.Domain.Services;
    using UBind.Domain.Services.AdditionalPropertyValue;
    using UBind.Domain.Services.QuoteExpiry;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Configuration;
    using UBind.Web.Controllers;
    using UBind.Web.ResourceModels;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="QuoteControllerTests" />.
    /// </summary>
    public class QuoteControllerTests
    {
        private readonly Mock<ICqrsMediator> mockMediator;
        private readonly Guid currentTenant = TenantFactory.DefaultId;

        /// <summary>
        /// Defines the currentOrganisation.
        /// </summary>
        private readonly string currentOrganisation = Guid.NewGuid().ToString();

        /// <summary>
        /// Defines the userId.
        /// </summary>
        private readonly Guid userId = new Guid("A1B2A3B4-3333-2222-1111-000000000000");

        /// <summary>
        /// Defines the quoteAggregateResolverService.
        /// </summary>
        private readonly Mock<IQuoteAggregateResolverService> quoteAggregateResolverService;

        private readonly Mock<IQuoteExpirySettingsProvider> quoteExpirySettingsProvider;

        /// <summary>
        /// Defines the quoteService.
        /// </summary>
        private readonly IQuoteService quoteService;

        /// <summary>
        /// Defines the quotePortalController.
        /// </summary>
        private readonly QuoteController quotePortalController;

        /// <summary>
        /// Defines the quoteRepository.
        /// </summary>
        private readonly Mock<IQuoteReadModelRepository> quoteRepository;

        /// <summary>
        /// Defines the quote.
        /// </summary>
        private readonly FakeQuoteReadModelDetails quote = new FakeQuoteReadModelDetails();

        private readonly Mock<IAuthorisationService> authorisationService;

        /// <summary>
        /// Defines the quoteId.
        /// </summary>
        private Guid quoteId;

        private Mock<IAdditionalPropertyValueService> additionalPropertyValueService;

        public QuoteControllerTests()
        {
            var clock = new Mock<IClock>();
            this.quoteRepository = new Mock<IQuoteReadModelRepository>();
            var quoteVersionRepository = new Mock<IQuoteVersionReadModelRepository>();
            var quoteDocumentRepository = new Mock<IQuoteDocumentReadModelRepository>();
            var configurationService = new Mock<IConfigurationService>();
            var userRepository = new Mock<IUserAggregateRepository>();
            var settingService = new Mock<IFeatureSettingService>();
            var policyService = new Mock<IPolicyService>();
            var quoteAggregateRepository = new Mock<IQuoteAggregateRepository>();
            var quoteSearchIndexService = new Mock<ISearchableEntityService<IQuoteSearchResultItemReadModel, QuoteReadModelFilters>>();
            var productConfigProvider = new Mock<IProductConfigurationProvider>();
            var productFeatureService = new Mock<IProductFeatureSettingService>();
            var productService = new Mock<UBind.Application.IProductService>();
            var formDataPrettifier = new Mock<IFormDataPrettifier>();
            this.quoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();
            this.quoteExpirySettingsProvider = new Mock<IQuoteExpirySettingsProvider>();
            this.additionalPropertyValueService = new Mock<IAdditionalPropertyValueService>();
            this.quoteExpirySettingsProvider.Setup(s => s.Retrieve(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(QuoteExpirySettings.Default);
            var organisationService = new Mock<IOrganisationService>();
            this.quoteId = Guid.NewGuid();

            this.mockMediator = new Mock<ICqrsMediator>();
            var mockAggregateLockingService = new Mock<IAggregateLockingService>();
            mockAggregateLockingService
                .Setup(a => a.CreateLockOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<AggregateType>()))
                .ReturnsAsync(It.IsAny<IRedLock>());
            var applicationQuoteService = new ApplicationQuoteService(
                quoteAggregateRepository.Object,
                new Mock<ITenantRepository>().Object,
                new Mock<IUserAggregateRepository>().Object,
                new Mock<IPersonAggregateRepository>().Object,
                new Mock<IQuoteWorkflowProvider>().Object,
                new Mock<ICustomerService>().Object,
                this.quoteExpirySettingsProvider.Object,
                new Mock<ICustomerAggregateRepository>().Object,
                new Mock<IUserLoginEmailRepository>().Object,
                policyService.Object,
                new Mock<IHttpContextPropertiesResolver>().Object,
                new TestClock(),
                this.quoteAggregateResolverService.Object,
                new Mock<IOrganisationReadModelRepository>().Object,
                new Mock<IProductFeatureSettingService>().Object,
                new Mock<ISystemEmailService>().Object,
                this.mockMediator.Object,
                new Mock<IQuoteSystemEventEmitter>().Object);
            var quoteExpirySettingsProvider = new Mock<IQuoteExpirySettingsProvider>();
            quoteExpirySettingsProvider.Setup(s => s.Retrieve(It.IsAny<QuoteAggregate>()))
                .ReturnsAsync(QuoteExpirySettings.Default);

            this.quoteService = new QuoteService(
                this.quoteRepository.Object,
                new Mock<IQuoteVersionReadModelRepository>().Object,
                new Mock<IQuoteAggregateRepository>().Object,
                new Mock<IHttpContextPropertiesResolver>().Object,
                new Mock<IQuoteAggregateResolverService>().Object,
                new TestClock(),
                new Mock<IUBindDbContext>().Object,
                NullLogger<QuoteService>.Instance,
                new Mock<IRedisConfiguration>().Object,
                new Mock<IConnectionMultiplexer>().Object,
                new Mock<ICachingResolver>().Object);

            this.authorisationService = new Mock<IAuthorisationService>();
            this.quotePortalController = new QuoteController(
                this.quoteService,
                applicationQuoteService,
                new Mock<IConfigurationService>().Object,
                new Mock<ISearchableEntityService<IQuoteSearchResultItemReadModel, QuoteReadModelFilters>>().Object,
                new Mock<IProductConfigurationProvider>().Object,
                formDataPrettifier.Object,
                new Mock<Application.User.IUserService>().Object,
                this.authorisationService.Object,
                new TestClock(),
                new Mock<ICachingResolver>().Object,
                this.mockMediator.Object,
                this.additionalPropertyValueService.Object);

            this.mockMediator.Setup(m => m.Send(It.IsAny<UserHasActiveFeatureSettingQuery>(), CancellationToken.None))
                .ReturnsAsync(true);
        }

        [Fact]
        public async void GetDetailsForQuoteOrPolicy_ReturnsOkObject_WhenUserIsAgentOwnerForRecord()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", this.currentTenant.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
            }));
            this.quotePortalController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };

            this.quote.TenantId = this.currentTenant;
            this.quote.OrganisationId = Guid.Parse(this.currentOrganisation);
            this.quote.Environment = DeploymentEnvironment.Development;
            this.quote.OwnerUserId = this.userId;
            this.quote.LatestCalculationResult = new CalculationResultReadModel(null);
            this.quote.Documents = Enumerable.Empty<QuoteDocumentReadModel>();
            this.quoteRepository.Setup(m => m.GetQuoteDetails(this.currentTenant, this.quoteId)).Returns(this.quote);
            this.additionalPropertyValueService.Setup(
                apvs => apvs.GetAdditionalPropertyValuesByEntityTypeAndEntityId(
                    It.IsAny<Guid>(), It.IsAny<AdditionalPropertyEntityType>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(this.GenerateFakeAdditionalPropertyValueDtos()));

            // Act
            IActionResult result = await this.quotePortalController.GetQuoteDetails(
                this.quoteId, DeploymentEnvironment.Development.ToString());

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetDetailsForQuoteOrPolicy_ReturnsForbiddenResult_WhenAgentUserIsNotAuthorizedAsync()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", this.currentTenant.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
            }));
            this.quotePortalController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };
            this.quote.TenantId = this.currentTenant;
            this.quote.OwnerUserId = new Guid("12345678-3333-2222-1111-000000000000");
            this.quote.Environment = DeploymentEnvironment.Development;
            this.quoteRepository.Setup(m => m.GetQuoteDetails(this.currentTenant, this.quoteId)).Returns(this.quote);
            this.authorisationService
                .Setup(c => c.ThrowIfUserCannotViewQuote(It.IsAny<ClaimsPrincipal>(), It.IsAny<IQuoteReadModelDetails>()))
                .Throws(new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    Permission.ViewQuotes, "quote", this.quote.QuoteId.ToString())));

            // Act
            Func<Task> act = () => this.quotePortalController.GetQuoteDetails(
                this.quoteId, DeploymentEnvironment.Development.ToString());

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("permission.required.to.access.resource");
            exception.Which.Error.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetDetailsForQuoteOrPolicy_ReturnsForbidden_WhenRecordIsForDifferentTenantAsync()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", this.currentTenant.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
            }));
            this.quotePortalController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };
            this.quote.TenantId = this.currentTenant;
            this.quote.Environment = DeploymentEnvironment.Development;
            this.quoteRepository.Setup(m => m.GetQuoteDetails(this.currentTenant, this.quoteId)).Returns(this.quote);
            this.authorisationService
                .Setup(c => c.ThrowIfUserCannotViewQuote(It.IsAny<ClaimsPrincipal>(), It.IsAny<IQuoteReadModelDetails>()))
                .Throws(new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    Permission.ViewQuotes, "quote", this.quote.QuoteId.ToString())));

            // Act
            Func<Task> act = () => this.quotePortalController.GetQuoteDetails(
                this.quoteId, DeploymentEnvironment.Development.ToString());

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("permission.required.to.access.resource");
            exception.Which.Error.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Fact]
        public async void GetDetailsForQuoteOrPolicy_ReturnsOkObject_WhenUserIsCustomerForRecord()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Customer.Humanize()),
                new Claim("Tenant", this.currentTenant.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
                new Claim("CustomerId", new Guid("12345678-3333-2222-1111-000000000000").ToString()),
            }));
            this.quotePortalController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };
            this.quote.TenantId = this.currentTenant;
            this.quote.OrganisationId = Guid.Parse(this.currentOrganisation);
            this.quote.Environment = DeploymentEnvironment.Development;
            this.quote.CustomerId = new Guid("12345678-3333-2222-1111-000000000000");
            this.quote.LatestCalculationResult = new CalculationResultReadModel(null);
            this.quote.Documents = Enumerable.Empty<QuoteDocumentReadModel>();
            this.quoteRepository.Setup(m => m.GetQuoteDetails(this.quote.TenantId, this.quoteId)).Returns(this.quote);
            this.additionalPropertyValueService.Setup(
                apvs => apvs.GetAdditionalPropertyValuesByEntityTypeAndEntityId(It.IsAny<Guid>(), It.IsAny<AdditionalPropertyEntityType>(), It.IsAny<Guid>())).Returns(
                Task.FromResult(this.GenerateFakeAdditionalPropertyValueDtos()));

            // Act
            IActionResult result = await this.quotePortalController.GetQuoteDetails(
                this.quoteId, DeploymentEnvironment.Development.ToString());

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetDetailsForQuoteOrPolicy_ReturnsForbidden_WhenUserIsNotCustomerForRecordAsync()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Customer.Humanize()),
                new Claim("Tenant", this.currentTenant.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
                new Claim("CustomerId", new Guid("12345678-3333-2222-1111-000000000000").ToString()),
            }));
            this.quotePortalController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };
            this.quote.TenantId = this.currentTenant;
            this.quote.Environment = DeploymentEnvironment.Development;
            this.quote.CustomerId = new Guid("12345678-3333-2222-AAAA-000000000000");
            this.quoteRepository.Setup(m => m.GetQuoteDetails(this.quote.TenantId, this.quoteId)).Returns(this.quote);
            this.authorisationService
                .Setup(c => c.ThrowIfUserCannotViewQuote(It.IsAny<ClaimsPrincipal>(), It.IsAny<IQuoteReadModelDetails>()))
                .Throws(new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    Permission.ViewQuotes, "quote", this.quote.QuoteId.ToString())));

            // Act
            Func<Task> act = () => this.quotePortalController.GetQuoteDetails(
                this.quoteId, DeploymentEnvironment.Development.ToString());

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("permission.required.to.access.resource");
            exception.Which.Error.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetPeriodicSummary_ReturnsForbiddenResult_WhenAgentUserIsNotAuthorizedAsync()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", this.currentTenant.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
            }));
            this.quotePortalController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };
            var options = new QuotePeriodicSummaryQueryOptionsModel();
            options.FromDateTime = Instant.FromDateTimeUtc(DateTime.Now.AddMonths(-1).ToUniversalTime()).ToLocalDateTimeInAet().ToExtendedIso8601();
            options.ToDateTime = Instant.FromDateTimeUtc(DateTime.Now.ToUniversalTime()).ToLocalDateTimeInAet().ToExtendedIso8601();
            options.SamplePeriodLengthString = SamplePeriodLength.Day.ToString();
            options.IncludeProperties = new List<string>()
            {
                nameof(QuotePeriodicSummaryModel.ConvertedCount),
            };

            this.authorisationService
                .Setup(c => c.ApplyViewQuoteRestrictionsToFilters(It.IsAny<ClaimsPrincipal>(), It.IsAny<QuoteReadModelFilters>()))
                .Throws(new ErrorException(Errors.General.PermissionRequiredToAccessResource(
                    Permission.ViewQuotes, "quote", null)));

            // Act
            Func<Task> act = () => this.quotePortalController.GetPeriodicSummary(options);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("permission.required.to.access.resource");
            exception.Which.Error.HttpStatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetPeriodicSummary_ReturnsError_WhenMissingRequireParameters()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", this.currentTenant.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
            }));
            this.quotePortalController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };

            // required includeProperties is missing
            var options = new QuotePeriodicSummaryQueryOptionsModel();
            options.FromDateTime = Instant.FromDateTimeUtc(DateTime.Now.AddMonths(-1).ToUniversalTime()).ToLocalDateTimeInAet().ToExtendedIso8601();
            options.ToDateTime = Instant.FromDateTimeUtc(DateTime.Now.ToUniversalTime()).ToLocalDateTimeInAet().ToExtendedIso8601();
            options.SamplePeriodLengthString = SamplePeriodLength.Day.ToString();
            this.authorisationService
              .Setup(c => c.ApplyViewQuoteRestrictionsToFilters(It.IsAny<ClaimsPrincipal>(), It.IsAny<QuoteReadModelFilters>()))
              .Returns(Task.FromResult(true));

            // Act
            Func<Task> act = () => this.quotePortalController.GetPeriodicSummary(options);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("required.request.parameter.missing");
            exception.Which.Error.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetPeriodicSummary_ReturnsError_WhenInvalidParametersFromDateAndToDate()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", this.currentTenant.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
            }));
            this.quotePortalController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };

            // invalid formats for the fromDate and toDate
            var options = new QuotePeriodicSummaryQueryOptionsModel();
            options.FromDateTime = Instant.FromDateTimeUtc(DateTime.Now.AddMonths(-1).ToUniversalTime()).ToLocalDateInAet().ToString();
            options.ToDateTime = Instant.FromDateTimeUtc(DateTime.Now.ToUniversalTime()).ToLocalDateInAet().ToString();
            options.SamplePeriodLengthString = SamplePeriodLength.Day.ToString();
            options.IncludeProperties = new List<string>()
            {
                nameof(QuotePeriodicSummaryModel.ConvertedCount),
            };
            this.authorisationService
              .Setup(c => c.ApplyViewQuoteRestrictionsToFilters(It.IsAny<ClaimsPrincipal>(), It.IsAny<QuoteReadModelFilters>()))
              .Returns(Task.FromResult(true));

            // Act
            Func<Task> act = () => this.quotePortalController.GetPeriodicSummary(options);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("request.parameter.invalid");
            exception.Which.Error.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetPeriodicSummary_ReturnsError_WhenInvalidParametersSamplePeriodLength()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, this.userId.ToString()),
                new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                new Claim("Tenant", this.currentTenant.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
            }));
            this.quotePortalController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };

            // invalid paramaters for samplePeriodLength
            var options = new QuotePeriodicSummaryQueryOptionsModel();
            options.FromDateTime = Instant.FromDateTimeUtc(DateTime.Now.AddMonths(-1).ToUniversalTime()).ToLocalDateTimeInAet().ToExtendedIso8601();
            options.ToDateTime = Instant.FromDateTimeUtc(DateTime.Now.ToUniversalTime()).ToLocalDateTimeInAet().ToExtendedIso8601();
            options.SamplePeriodLengthString = "days";
            options.IncludeProperties = new List<string>()
            {
                nameof(QuotePeriodicSummaryModel.ConvertedCount),
            };
            this.authorisationService
              .Setup(c => c.ApplyViewQuoteRestrictionsToFilters(It.IsAny<ClaimsPrincipal>(), It.IsAny<QuoteReadModelFilters>()))
              .Returns(Task.FromResult(true));

            // Act
            Func<Task> act = () => this.quotePortalController.GetPeriodicSummary(options);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be("request.parameter.invalid");
            exception.Which.Error.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-05T00:00:00.0000000", "day", "ConvertedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("20-01-2020", "2020-01-05T00:00:00.0000000", "day", "ConvertedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "20-01-2050", "day", "ConvertedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2023-03-05T00:00:00.0000000", "2020-01-05T00:00:00.0000000", "day", "ConvertedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "", "day", "ConvertedCount", "newBusiness", "dev", "required.request.parameter.missing")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-10T00:00:00.0000000", "", "ConvertedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-10T00:00:00.0000000", "days", "ConvertedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-10T00:00:00.0000000", "months", "ConvertedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-10T00:00:00.0000000", "day", null, "newBusiness", "dev", "required.request.parameter.missing")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-10T00:00:00.0000000", "day", "TotalPremium", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-10T00:00:00.0000000", "day", "Total,Premium", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-10T00:00:00.0000000", "day", "Total*Premium", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-10T00:00:00.0000000", "day", "Total|Premium", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-10T00:00:00.0000000", "*", "ConvertedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-06T00:00:00.0000000", "day", "ConvertedCount", "oldBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "day", "ConvertedCount", "newBusiness, oldBusiness", null, "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "month", "ConvertedCount", "newBusiness", "dev*", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "quarter", "ConvertedCount", "newBusiness", "dev, deborah-", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "quarter", "ConvertedCount", "newBusiness", ",,", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "month", "CreatedCount, CreatedTotalPremium,ConvertedCount", "newBusiness", "dev", "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "month", "CreatedCount", "newBusinessrenewal", null, "request.parameter.invalid")]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "month", "CreatedCount", "newBusiness", "dev,devtwo", "request.parameter.invalid")]
        public async Task GetPeriodicSummary_ReturnsError_WhenParametersAreInvalid(
            string fromDate,
            string toDate,
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
                new Claim("Tenant", this.currentTenant.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
            }));
            this.quotePortalController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };

            // invalid paramaters for includeParameters
            var options = new QuotePeriodicSummaryQueryOptionsModel();
            options.FromDateTime = fromDate;
            options.ToDateTime = toDate;
            options.SamplePeriodLengthString = samplePeriodLength;
            options.IncludeProperties = includeProperties == null ? Enumerable.Empty<string>() : new List<string>() { includeProperties };

            // optional parameters
            options.PolicyTransactionTypes = policyTransactionTypes == null ? Enumerable.Empty<string>() : new List<string>() { policyTransactionTypes };
            options.Products = products == null ? Enumerable.Empty<string>() : new List<string>() { products };

            this.authorisationService
              .Setup(c => c.ApplyViewQuoteRestrictionsToFilters(It.IsAny<ClaimsPrincipal>(), It.IsAny<QuoteReadModelFilters>()))
              .Returns(Task.FromResult(true));

            // Act
            Func<Task> act = () => this.quotePortalController.GetPeriodicSummary(options);

            // Assert
            var exception = await act.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Code.Should().Be(errorCodeExpected);
            exception.Which.Error.HttpStatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData("2020-01-05T00:00:00.0000000", "2020-01-06T00:00:00.0000000", "day", "ConvertedCount", "newBusiness", null)]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "day", "ConvertedCount", "newBusiness", null)]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "month", "ConvertedCount", "newBusiness", null)]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "quarter", "ConvertedCount", "newBusiness", null)]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "year", "ConvertedCount", "newBusiness", null)]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "all", "ConvertedCount", "newBusiness", null)]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "month", "CreatedCount", "newBusiness", null)]
        [InlineData("2020-01-05T00:00:00.0000000", "2023-01-06T00:00:00.0000000", "month ", "CreatedTotalPremium", "newBusiness", null)]
        public async Task GetPeriodicSummary_ReturnsResults_WhenParametersAreValid(
            string fromDate,
            string toDate,
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
                new Claim("Tenant", this.currentTenant.ToString()),
                new Claim("OrganisationId", this.currentOrganisation),
            }));
            this.quotePortalController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = claimsPrincipal },
            };
            var options = new QuotePeriodicSummaryQueryOptionsModel();
            options.FromDateTime = fromDate;
            options.ToDateTime = toDate;
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
              .Setup(c => c.ApplyViewQuoteRestrictionsToFilters(It.IsAny<ClaimsPrincipal>(), It.IsAny<QuoteReadModelFilters>()))
              .Returns(Task.FromResult(true));
            this.mockMediator
                .Setup(c => c.Send(It.IsAny<GetQuotePeriodicSummariesQuery>(), default))
                .Returns(Task.FromResult(new List<QuotePeriodicSummaryModel>()));

            // Act
            var result = await this.quotePortalController.GetPeriodicSummary(options);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        private List<AdditionalPropertyValueDto> GenerateFakeAdditionalPropertyValueDtos()
        {
            var contextId = Guid.NewGuid();
            return new List<AdditionalPropertyValueDto>
            {
                new AdditionalPropertyValueDto
                {
                    Value = "Prop A Value",
                    AdditionalPropertyDefinition = new AdditionalPropertyDefinitionDto
                    {
                        Id = Guid.NewGuid(),
                        Alias = "prop-a",
                        Name = "Prop A",
                        ContextId = contextId,
                        ContextType = AdditionalPropertyDefinitionContextType.Tenant,
                        DefaultValue = string.Empty,
                        EntityType = AdditionalPropertyEntityType.Quote,
                        IsRequired = true,
                        IsUnique = true,
                        PropertyType = AdditionalPropertyDefinitionType.Text,
                    },
                },
                new AdditionalPropertyValueDto
                {
                    Value = "Prop B Value",
                    AdditionalPropertyDefinition = new AdditionalPropertyDefinitionDto
                    {
                        Id = Guid.NewGuid(),
                        Alias = "prop-b",
                        Name = "Prop B",
                        ContextId = contextId,
                        ContextType = AdditionalPropertyDefinitionContextType.Tenant,
                        DefaultValue = string.Empty,
                        EntityType = AdditionalPropertyEntityType.Quote,
                        IsRequired = true,
                        IsUnique = true,
                        PropertyType = AdditionalPropertyDefinitionType.Text,
                    },
                },
                new AdditionalPropertyValueDto
                {
                    Value = "Prop C Value",
                    AdditionalPropertyDefinition = new AdditionalPropertyDefinitionDto
                    {
                        Id = Guid.NewGuid(),
                        Alias = "prop-c",
                        Name = "Prop C",
                        ContextId = contextId,
                        ContextType = AdditionalPropertyDefinitionContextType.Tenant,
                        DefaultValue = string.Empty,
                        EntityType = AdditionalPropertyEntityType.Quote,
                        IsRequired = true,
                        IsUnique = true,
                        PropertyType = AdditionalPropertyDefinitionType.Text,
                    },
                },
                new AdditionalPropertyValueDto
                {
                    Value = "Prop D Value",
                    AdditionalPropertyDefinition = new AdditionalPropertyDefinitionDto
                    {
                        Id = Guid.NewGuid(),
                        Alias = "prop-d",
                        Name = "Prop D",
                        ContextId = contextId,
                        ContextType = AdditionalPropertyDefinitionContextType.Tenant,
                        DefaultValue = string.Empty,
                        EntityType = AdditionalPropertyEntityType.Quote,
                        IsRequired = true,
                        IsUnique = true,
                        PropertyType = AdditionalPropertyDefinitionType.Text,
                    },
                },
            };
        }
    }
}
