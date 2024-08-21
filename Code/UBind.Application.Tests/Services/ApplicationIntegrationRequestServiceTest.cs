// <copyright file="ApplicationIntegrationRequestServiceTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Flurl.Http.Testing;
    using Moq;
    using NodaTime;
    using UBind.Application.Export;
    using UBind.Application.Releases;
    using UBind.Application.Services;
    using UBind.Application.Tests.Fakes;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Product;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Unit/Integration tests for ApplicationIntegrationRequestService.
    /// </summary>
    public class ApplicationIntegrationRequestServiceTest
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private Mock<IQuoteAggregateResolverService> quoteAggregateResolverService = new Mock<IQuoteAggregateResolverService>();
        private Mock<IReleaseQueryService> releaseQueryService = new Mock<IReleaseQueryService>();

        private IClock wristWatch;
        private string integrationConfigJson = @"{
            ""webServiceIntegrations"" : [
                {
                    ""id"" : ""Eevee"",
                    ""requestType"": ""GET"",
                    ""url"" : {
                        ""type"": ""url"",
                        ""urlString"": ""https://test.com/pokemon"",
                        ""pathParameter"": ""eevee""
                    },
                    ""authMethod"": {
                        ""authenticationType"": ""Bearer"",
                        ""authToken"": ""1234456""
                    },
                    ""headers"": [],
                    ""responseTemplate"" : {
                        ""type"": ""dotLiquid"",
                        ""templateString"": ""<p>My name is {{name}}</p>""
                    }
                }  
            ]
        }";

        [Fact(Skip = "Very slow test. Only run when required.")]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task ExecuteIntegrationRequest_PublicAPI_Succeeds()
        {
            // Arrange
            this.wristWatch = SystemClock.Instance;
            var tenant = new Tenant(Guid.NewGuid(), "Pallet Town", "pallet", null, default, default, this.wristWatch.GetCurrentInstant());
            var product = new Product(tenant.Id, Guid.NewGuid(), "Ash Ketchum", "alias", this.wristWatch.GetCurrentInstant());
            var quote = QuoteAggregate.CreateNewBusinessQuote(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                product.Id,
                DeploymentEnvironment.Development,
                QuoteExpirySettings.Default,
                this.performingUserId,
                this.wristWatch.GetCurrentInstant(),
                Guid.NewGuid(),
                Timezones.AET,
                false,
                null,
                true);
            var quoteAggregate = quote.Aggregate;
            quote.UpdateFormData(new Domain.Aggregates.Quote.FormData("{}"), this.performingUserId, this.wristWatch.GetCurrentInstant().PlusTicks(5000));
            var configProvider = new DefaultQuoteWorkflowProvider();
            var quoteExpirySettingsProvider = new DefaultExpirySettingsProvider();
            var tenantRepository = new Mock<ITenantRepository>();
            var cachingResolver = new Mock<ICachingResolver>();
            var quoteAggregateRepository = new Mock<IQuoteAggregateRepository>();
            var productRepository = new Mock<IProductRepository>();
            var releaseQueryService = new Mock<IReleaseQueryService>();
            var productConfigurationProvider = new DefaultProductConfigurationProvider();
            var integrationConfigProvider = new IntegrationConfigurationProvider(
                releaseQueryService.Object,
                new Mock<IExporterDependencyProvider>().Object);
            var devRelease = new DevRelease(tenant.Id, product.Id, this.wristWatch.GetCurrentInstant());
            var details = new ReleaseDetails(
                WebFormAppType.Quote,
                string.Empty,
                string.Empty,
                this.integrationConfigJson,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                Enumerable.Empty<Asset>(),
                Enumerable.Empty<Asset>(),
                null,
                this.wristWatch.GetCurrentInstant());
            var cachedRelease = new ActiveDeployedRelease(devRelease, DeploymentEnvironment.Development, null);
            var payloadJson = "{}";
            var webIntegrationId = "Eevee";

            cachingResolver.Setup(x => x.GetTenantOrThrow(tenant.Id)).Returns(Task.FromResult(tenant));
            tenantRepository.Setup(r => r.GetTenantByAlias(tenant.Details.Alias, It.IsAny<bool>())).Returns(tenant);
            this.quoteAggregateResolverService.Setup(e => e.GetQuoteAggregateForQuote(tenant.Id, quote.Id)).Returns(quoteAggregate);
            releaseQueryService.Setup(d => d.GetRelease(It.IsAny<ReleaseContext>())).Returns(cachedRelease);

            var service = new ApplicationIntegrationRequestService(
                cachingResolver.Object,
                productConfigurationProvider,
                integrationConfigProvider,
                this.quoteAggregateResolverService.Object,
                this.releaseQueryService.Object);

            WebServiceIntegrationResponse response = null;
            var httpTest = new HttpTest();
            httpTest.RespondWithJson(new { name = "eevee" }, 200);

            // Act
            using (httpTest)
            {
                response = await service.ExecuteRequest(
                    tenant.Id,
                    webIntegrationId,
                    quote.Id,
                    product.Id,
                    DeploymentEnvironment.Development,
                    payloadJson);
            }

            // Assert
            var expectedResponse = @"<p>My name is eevee</p>";
            Assert.NotNull(response);
            Assert.True(response.Code.Equals(HttpStatusCode.OK));
            Assert.Equal(expectedResponse, response.ResponseJson, true);
        }
    }
}
