// <copyright file="ProductConfigurationProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Quote
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using UBind.Application.Quote;
    using UBind.Application.Releases;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class ProductConfigurationProviderTests
    {
        [Fact]
        public async Task GetProductConfiguration_LoadsProductConfigurationCorrectlyFromDevRelease()
        {
            // Arrange
            var releaseQueryService = new Mock<IReleaseQueryService>();

            var productConfiguration = this.GetProductConfigurationJson();
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var fakeDevRelease = FakeReleaseBuilder
                .CreateForProduct(tenantId, productId)
                .WithQuoteProductConfiguration(productConfiguration)
                .BuildDevRelease();
            var cachedRelease = new ActiveDeployedRelease(fakeDevRelease, DeploymentEnvironment.Development, null);
            var releaseContext = new ReleaseContext(tenantId, productId, DeploymentEnvironment.Development, fakeDevRelease.Id);
            releaseQueryService.Setup(c => c.GetRelease(releaseContext)).Returns(cachedRelease);

            var sut = new ProductConfigurationProvider(
                releaseQueryService.Object);

            // Act
            var model = await sut.GetProductConfiguration(releaseContext, WebFormAppType.Quote);

            // Assert
            model.Should().NotBeNull();
            model.QuoteWorkflow.IsSettlementRequired.Should().BeTrue();
            model.QuoteWorkflow.GetResultingState(Domain.Aggregates.Quote.QuoteAction.ReviewReferral, "Review").Should().Be("Review");
        }

        [Fact]
        public async Task GetProductConfiguration_DoesNotThrowException_WhenNoQuestionSetProperty()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var productConfigurationJson = this.GetProductConfigurationJson();
            var invalidConfigurationJson = $@"{{
                ""baseConfiguration"": {{
                    ""questionMetaData"": {{
                        ""InvalidQuestionSet"": {{
                            ""ratingPrimary"": {{
                                ""price"": {{
                                    ""canChangeWhenApproved"": false
                                }}
                            }}
                        }}
                    }}
                }}
            }}";
            var fakeRelease = FakeReleaseBuilder.CreateForProduct(tenantId, productId)
                .WithQuoteProductConfiguration(productConfigurationJson)
                .WithQuoteFormConfiguration(invalidConfigurationJson)
                .BuildRelease();
            var cachedRelease = new ActiveDeployedRelease(fakeRelease, DeploymentEnvironment.Staging, null);
            var releaseContext = new ReleaseContext(tenantId, productId, DeploymentEnvironment.Staging, fakeRelease.Id);
            var releaseQueryService = new Mock<IReleaseQueryService>();
            releaseQueryService
                .Setup(s => s.GetRelease(releaseContext))
                .Returns(cachedRelease);

            var sut = new ProductConfigurationProvider(releaseQueryService.Object);

            // Act
            Func<Task> act = async () => await sut.GetProductConfiguration(releaseContext, WebFormAppType.Quote);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetProductConfiguration_DoesNotThrowException_WhenThereIsQuestionSetProperty()
        {
            // Arrange
            var questionKey = "price";
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var previousCalculationData = CalculationResultJsonFactory.GetSampleWithCalculationValue(questionKey, "100");
            var newCalculationData = CalculationResultJsonFactory.GetSampleWithCalculationValue(questionKey, "200");
            var baseConfig = ConfigurationJsonFactory.GetSampleBaseConfigurationWithQuestionMetadata();
            var fakeRelease = FakeReleaseBuilder.CreateForProduct(tenantId, productId)
                .WithQuoteProductConfiguration(this.GetProductConfigurationJson())
                .WithQuoteFormConfiguration(baseConfig)
                .BuildRelease();
            var cachedRelease = new ActiveDeployedRelease(fakeRelease, DeploymentEnvironment.Staging, null);
            var releaseContext = new ReleaseContext(tenantId, productId, DeploymentEnvironment.Staging, fakeRelease.Id);
            var releaseQueryService = new Mock<IReleaseQueryService>();
            releaseQueryService
                .Setup(s => s.GetRelease(releaseContext))
                .Returns(cachedRelease);

            var sut = new ProductConfigurationProvider(releaseQueryService.Object);

            // Act
            Func<Task> act = async () => await sut.GetProductConfiguration(releaseContext, WebFormAppType.Quote);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetProductConfiguration_DoesNotThrowException_WhenThereIsNoQuestionSetProperty()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var baseConfig = ConfigurationJsonFactory.GetSampleBaseConfiguration();
            var fakeRelease = FakeReleaseBuilder.CreateForProduct(tenantId, productId)
                .WithQuoteProductConfiguration(this.GetProductConfigurationJson())
                .WithQuoteFormConfiguration(baseConfig)
                .BuildRelease();
            var cachedRelease = new ActiveDeployedRelease(fakeRelease, DeploymentEnvironment.Staging, null);
            var releaseContext = new ReleaseContext(tenantId, productId, DeploymentEnvironment.Staging, fakeRelease.Id);
            var releaseQueryService = new Mock<IReleaseQueryService>();
            releaseQueryService
                .Setup(s => s.GetRelease(releaseContext))
                .Returns(cachedRelease);

            var sut = new ProductConfigurationProvider(releaseQueryService.Object);

            // Act
            Func<Task> act = async () => await sut.GetProductConfiguration(releaseContext, WebFormAppType.Quote);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task GetProductConfiguration_LoadsClaimWorkflowCorrectly_FromDevRelease()
        {
            // Arrange
            var quoteProductConfigurationJson = @"{
            ""quoteNumberSource"": 2,
            ""quoteDataLocator"": {
                    ""inceptionDate"": {
                      ""object"": ""CalculationResult"",
                      ""path"": ""questions.ratingSecondary.policyStartDateFormatted""
                    },
                    ""expiryDate"": {
                      ""object"": ""CalculationResult"",
                      ""path"": ""questions.ratingSecondary.policyEndDateFormatted""
                    }
            },
            ""quoteWorkflow"": {
                            ""isSettlementRequired"": true,
                            ""bindOptions"": 3,
                            ""transitions"": [
                                {
                                  ""action"": ""Quote"",
                                  ""requiredStates"": [""Nascent""],
                                  ""resultingState"": ""Incomplete""
                                },
                                {
                                  ""action"": ""Review"",
                                  ""requiredStates"": [""Incomplete"", ""Approved""],
                                  ""resultingState"": ""Review""
                                }
                            ]
                    }
}";
            var claimProductConfigurationJson = @"{
            ""quoteNumberSource"": 2,
            ""quoteDataLocator"": {
                    ""inceptionDate"": {
                      ""object"": ""CalculationResult"",
                      ""path"": ""questions.ratingSecondary.policyStartDateFormatted""
                    },
                    ""expiryDate"": {
                      ""object"": ""CalculationResult"",
                      ""path"": ""questions.ratingSecondary.policyEndDateFormatted""
                    }
            },
            ""claimWorkflow"": {
                            ""isSettlementRequired"": true,
                            ""bindOptions"": 3,
                            ""transitions"": [
                                {
                                  ""action"": ""Claim"",
                                  ""requiredStates"": [""Nascent""],
                                  ""resultingState"": ""Incomplete""
                                },
                                {
                                  ""action"": ""AutoApproval"",
                                  ""requiredStates"": [""Incomplete""],
                                  ""resultingState"": ""Approved""
                                }
                            ]
                    }
}";
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var fakeRelease = FakeReleaseBuilder.CreateForProduct(tenantId, productId)
                .WithQuoteProductConfiguration(quoteProductConfigurationJson)
                .WithClaimProductConfiguration(claimProductConfigurationJson)
                .BuildDevRelease();
            var cachedRelease = new ActiveDeployedRelease(fakeRelease, DeploymentEnvironment.Development, null);
            var releaseContext = new ReleaseContext(tenantId, productId, DeploymentEnvironment.Development, fakeRelease.Id);
            var releaseQueryService = new Mock<IReleaseQueryService>();
            releaseQueryService
                .Setup(s => s.GetRelease(releaseContext))
                .Returns(cachedRelease);

            var sut = new ProductConfigurationProvider(releaseQueryService.Object);

            // Act
            var model = await sut.GetProductConfiguration(releaseContext, WebFormAppType.Claim);

            // Assert
            model.Should().NotBeNull();
            var resultingState = model.ClaimWorkflow.GetResultingState(Domain.Aggregates.Claim.ClaimActions.AutoApproval, "Incomplete");
            resultingState.Should().Be("Approved");
        }

        private string GetProductConfigurationJson()
        {
            var json = @"{
            ""quoteNumberSource"": 2,
            ""quoteDataLocator"": {
                    ""inceptionDate"": {
                      ""object"": ""CalculationResult"",
                      ""path"": ""questions.ratingSecondary.policyStartDateFormatted""
                    },
                    ""expiryDate"": {
                      ""object"": ""CalculationResult"",
                      ""path"": ""questions.ratingSecondary.policyEndDateFormatted""
                    }
            },
            ""quoteWorkflow"": {
                            ""isSettlementRequired"": true,
                            ""bindOptions"": 3,
                            ""transitions"": [
                                {
                                  ""action"": ""Quote"",
                                  ""requiredStates"": [""Nascent""],
                                  ""resultingState"": ""Incomplete""
                                },
                                {
                                  ""action"": ""Review"",
                                  ""requiredStates"": [""Incomplete"", ""Approved""],
                                  ""resultingState"": ""Review""
                                }
                            ]
                    }
}";

            return json;
        }
    }
}
