// <copyright file="FlexCelCalculationServiceTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.FlexCel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.FlexCel;
    using UBind.Application.Releases;
    using UBind.Application.ResourcePool;
    using UBind.Application.Services.Email;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Product;
    using UBind.Domain.Tests;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class FlexCelCalculationServiceTests
    {
        private static readonly Guid TenantId = Guid.NewGuid();
        private static readonly Guid ProductId = Guid.NewGuid();
        private readonly IErrorNotificationService errorNotificationService;
        private string questionSets = "[[\"Value\"],[\"\"],[\"\"],[\"Renewal\"],[\"\"],[\"24-02-2020\"],[\"5000000\"],[\"5000000\"],[\"\"],[\"100\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"100\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"22-02-2020\"],[0],[\"\"],[\"\"],[\"None\"],[15000],[2019],[\"\"],[\"30-06-2020\"],[\"\"],[\"\"],[\"\"],[\"Payment test\"],[\"gene.rivera@aptiture.com\"],[\"0312345678\"],[\"\"],[\"\"],[\"gene rivera\"],[\"\"],[\"\"],[\"\"],[\"qwerty\"],[\"123456789\"],[\"12312312312\"],[\"2019\"],[\"\"],[\"1000 Wellington\"],[\"Geurie\"],[\"NSW\"],[\"2300\"],[\"No\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"No\"],[\"\"],[\"No\"],[\"\"],[\"No\"],[\"No\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"1\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"122\"],[123],[\"\"],[\"\"],[\"\"],[\"No\"],[\"\"],[\"No\"],[\"\"],[\"\"],[\"No\"],[\"\"],[\"No\"],[\"\"],[\"No\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"No\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"No\"],[\"\"],[\"\"],[\"21-02-2020\"],[\"Bronze\"],[\"\"],[\"\"],[\"\"],[\"No\"],[\"No\"],[\"No\"],[\"No\"],[\"No\"],[\"No\"],[\"Yes\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"Yes\"],[\"\"],[\"No\"],[\"\"],[\"\"],[\"\"],[\"\"],[0],[\"Yes\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"Credit Card\"],[\"Yearly\"],[\"Visa\"],[\"\"],[\"\"],[\"\"],[\"Visa\"],[\"\"],[\"\"],[\"12-22\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"Yearly\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"review\"],[\"edit\"],[\"reviewReviewerPerform\"],[\"\"],[\"\"],[\"\"],[\"\"],[\"\"]]";
        private string repeatingQuestions = "[[\"Value 1\",\"Value 2\",\"Value 3\",\"Value 4\",\"Value 5\",\"Value 6\",\"Value 7\",\"Value 8\",\"Value 9\",\"Value 10\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"Principal Name\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"1988\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"12\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"qwerty\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"],[\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\"]]";
        private SpreadsheetPoolService spreadsheetPoolService;
        private SpreadsheetCalculationService flexCelCalculationService;
        private SpreadsheetCalculationDataModel calculationDataModel;
        private IClock clock;
        private ILogger<IResourcePool> logger;
        private ILogger<SpreadsheetCalculationService> logger2;
        private byte[] testBinaryWorkBook;

        public FlexCelCalculationServiceTests()
        {
            this.clock = SystemClock.Instance;
            this.logger = new Mock<ILogger<IResourcePool>>().Object;
            this.logger2 = new Mock<ILogger<SpreadsheetCalculationService>>().Object;
            this.errorNotificationService = new Mock<IErrorNotificationService>().Object;

            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().Location);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);
            var filePath = Path.Combine(dirPath, "FlexCel\\flexCelWorkbookRatingFactorTest.xlsx");

            this.testBinaryWorkBook = System.IO.File.ReadAllBytes(filePath);
            var globalReleaseCache = new Mock<IGlobalReleaseCache>().Object;
            this.spreadsheetPoolService = new SpreadsheetPoolService(null, globalReleaseCache, this.clock, this.logger, this.errorNotificationService);
            var release = FakeReleaseBuilder.CreateForProduct(TenantId, ProductId)
                .WithQuoteWorkbookContent(this.testBinaryWorkBook)
                .BuildDevRelease();
            var cachedRelease = new ActiveDeployedRelease(release, DeploymentEnvironment.Development, null);
            var releaseQueryService = new Mock<IReleaseQueryService>();
            releaseQueryService
                .Setup(s => s.GetRelease(It.IsAny<ReleaseContext>()))
                .Returns(cachedRelease);
            this.flexCelCalculationService = new SpreadsheetCalculationService(
                this.spreadsheetPoolService,
                releaseQueryService.Object,
                this.logger2);

            this.calculationDataModel = new SpreadsheetCalculationDataModel();
            this.calculationDataModel.FormModel = new JObject();
            this.calculationDataModel.QuestionAnswers = JArray.Parse(this.questionSets);
            this.calculationDataModel.RepeatingQuestionAnswers = JArray.Parse(this.repeatingQuestions);
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public void GetQuoteCalculation_Results_Should_Used_Additional_Rating_Factors_When_Passed()
        {
            // Arrange
            IAdditionalRatingFactors additionalRatingFactors = new AdditionalRatingFactors(new Dictionary<string, dynamic>
            {
                { RatingFactorConstants.Quote.LastPremium, 100 },
                { RatingFactorConstants.Claim.TotalClaimsAmount, 1000 },
            });
            var releaseContext = new ReleaseContext(TenantId, ProductId, DeploymentEnvironment.Development, Guid.NewGuid());

            // Act
            var result = this.flexCelCalculationService.GetQuoteCalculation(
                releaseContext, this.calculationDataModel, additionalRatingFactors);

            // Assert
            var jResult = JObject.Parse(result.CalculationJson);
            int injectedLastPremium = jResult["risk1"]["additionalRatingFactors"]["lastPremium"].Value<int>();
            int injectedTotalClaims = jResult["risk1"]["additionalRatingFactors"]["totalClaimsAmount"].Value<int>();

            injectedLastPremium.Should().Be(100);
            injectedTotalClaims.Should().Be(1000);
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public void GetQuoteCalculation_Results_Should_Not_Use_Additional_Rating_Factors_When_Not_Passed()
        {
            // Arrange
            var releaseContext = new ReleaseContext(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                Guid.NewGuid());

            // Act
            ReleaseCalculationOutput result = this.flexCelCalculationService.GetQuoteCalculation(
                releaseContext, this.calculationDataModel);

            // Assert
            var jResult = JObject.Parse(result.CalculationJson);
            int injectedLastPremium = jResult["risk1"]["additionalRatingFactors"]["lastPremium"].Value<int>();
            int injectedTotalClaims = jResult["risk1"]["additionalRatingFactors"]["totalClaimsAmount"].Value<int>();

            injectedLastPremium.Should().Be(0);
            injectedTotalClaims.Should().Be(0);
        }
    }
}
