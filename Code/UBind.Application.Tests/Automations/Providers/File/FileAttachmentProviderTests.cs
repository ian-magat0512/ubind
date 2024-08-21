// <copyright file="FileAttachmentProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.File
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Attachment;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Helpers;
    using UBind.Application.Queries.AssetFile;
    using UBind.Application.Releases;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class FileAttachmentProviderTests
    {
        /// <summary>
        /// Unit test for file attachment provider for generating file attachment.
        /// </summary>
        /// <returns>Return Task.</returns>
        [Fact]
        public async Task FileAttachmentProviderTests_Should_Return_TextFile_FileAttachement()
        {
            // Arrange
            var json = @"{
                            ""sourceFile"": {
                                ""textFile"": {
                                    ""sourceData"": ""Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."",
                                    ""outputFileName"": ""lorem_ipsum.txt""
                                }
                            },
                            ""outputFileName"": ""testing2.txt"",
                            ""includeCondition"": {
                            ""textEndsWithCondition"": {
                                    ""text"": ""pikachu"",
                                    ""endsWith"": ""chu""
                                    }
                            }
                        }";

            var fileAttachmentProviderBuilder = JsonConvert.DeserializeObject<FileAttachmentProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            var fileAttachmentProvider = fileAttachmentProviderBuilder.Build(new Mock<IServiceProvider>().AddLoggers().Object);

            // Act
            var fileAttachmentInfo = (await fileAttachmentProvider.Resolve(null)).GetValueOrThrowIfFailed();

            // Assert
            fileAttachmentInfo.DataValue.FileName.ToString().Should().Be("testing2.txt");

            var mimeType = ContentTypeHelper.GetMimeTypeForFileExtension("testing2.txt");
            fileAttachmentInfo.DataValue.MimeType.Should().Be(mimeType);
            var content = Encoding.UTF8.GetString(fileAttachmentInfo.DataValue.File.Content);
            content.Should().Be("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.");

            fileAttachmentInfo.DataValue.IsIncluded.Should().Be(true);
        }

        [Fact]
        public async Task FileAttachmentProviderTests_Should_Return_ProductFile_FileAttachement()
        {
            // Arrange
            var json = @"{
                            ""sourceFile"": {
                                ""productFile"": {
                                    ""repository"": ""quote"",
                                    ""outputFileName"": ""test.txt"",
                                    ""productAlias"": ""life"",
                                    ""environment"": ""development"",
                                    ""visibility"": ""Public"",
                                    ""filePath"": ""test.txt""
                                }
                            },
                            ""includeCondition"": {
                            ""textEndsWithCondition"": {
                                    ""text"": ""pikachu"",
                                    ""endsWith"": ""chu""
                                    }
                            }
                        }";

            var tenantId = Guid.NewGuid();
            var jsonObject = JObject.Parse(json);
            var productAlias = jsonObject.SelectToken("sourceFile.productFile.productAlias")?.ToString();
            var fileName = jsonObject.SelectToken("sourceFile.productFile.outputFileName")?.ToString();
            var formType = jsonObject.SelectToken("sourceFile.productFile.formType")?.ToString();
            var filePath = jsonObject.SelectToken("sourceFile.productFile.filePath")?.ToString();
            Enum.TryParse(jsonObject.SelectToken("sourceFile.productFile.environment")?.ToString(), true, out DeploymentEnvironment environment);
            var expectedContent = "The quick brown fox jumps over the lazy dog.";

            var tenant = new Tenant(tenantId, "test", "test", null, default, default, default);
            var product = new Product(tenantId, Guid.NewGuid(), productAlias, productAlias, default);
            var clock = new TestClock();
            var fakeDevRelease = FakeReleaseBuilder
                .CreateForProduct(tenantId, product.Id)
                .WithQuoteAsset(fileName, expectedContent)
                .BuildDevRelease();
            var cachedRelease = new ActiveDeployedRelease(fakeDevRelease, environment, null);

            var mockReleaseQueryService = new Mock<IReleaseQueryService>();
            mockReleaseQueryService
                .Setup(s => s.GetRelease(It.IsAny<ReleaseContext>()))
                .Returns(cachedRelease);

            var mockTenantRepository = new Mock<ITenantRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            mockProductRepository.Setup(c => c.ProductIdIsAvailableInTenant(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);
            mockProductRepository.Setup(c => c.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).Returns(product);
            mockProductRepository.Setup(c => c.GetProductByAlias(It.IsAny<Guid>(), It.IsAny<string>())).Returns(product);
            mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            var mockcachingResolver = new Mock<ICachingResolver>();
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductByAliasOrThrow(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductByAliasOrNull(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            var mockMediator = new Mock<ICqrsMediator>();
            mockMediator.Setup(s => s.Send(It.IsAny<GetProductFileContentsByFileNameQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Encoding.UTF8.GetBytes(expectedContent)));
            var serviceProvider = AutomationTestsHelper.CreateMockServiceProvider(
                mockReleaseQueryService.Object,
                mockTenantRepository.Object,
                mockProductRepository.Object,
                mockcachingResolver.Object,
                mockMediator.Object);
            var fileAttachmentProviderBuilder = JsonConvert.DeserializeObject<FileAttachmentProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            var fileAttachmentProvider = fileAttachmentProviderBuilder.Build(serviceProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger(
                tenant.Id,
                default,
                product.Id,
                DeploymentEnvironment.Development);

            // Act
            var fileAttachmentInfo = (await fileAttachmentProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            fileAttachmentInfo.DataValue.FileName.ToString().Should().Be(fileName);

            // fileAttachmentInfo.DataValue.MimeType.Should().Be(MimeMapping.GetMimeMapping(fileName));
            var content = Encoding.UTF8.GetString(fileAttachmentInfo.DataValue.File.Content);
            content.Should().Be("The quick brown fox jumps over the lazy dog.");

            fileAttachmentInfo.DataValue.IsIncluded.Should().Be(true);
        }

        [Theory]
        [InlineData("", "Filename is blank.")]
        [InlineData("morethantwohundredfiftycharactersmorethantwohundredfiftycharactersmorethantwohundredfiftycharactersmorethantwohundredfiftycharactersmorethantwohundredfiftycharactersmorethantwohundredfiftycharactersmorethantwohundredfiftycharactersmorethantwohundredfiftycharacters", "Filename is too long (must be 255 characters or less)")]
        [InlineData("filename<.txt", "Filename contains an invalid character(s): <")]
        [InlineData("filename>.txt", "Filename contains an invalid character(s): >")]
        [InlineData("file:name.txt", "Filename contains an invalid character(s): :")]
        [InlineData("filename/asd.txt", "Filename contains an invalid character(s): /")]
        [InlineData("filen|amezz.txt", "Filename contains an invalid character(s): |")]
        [InlineData("file?namerr.txt", "Filename contains an invalid character(s): ?")]
        [InlineData("*filenamenff.txt", "Filename contains an invalid character(s): *")]
        public async void FileAttachmentProvider_Throw_When_FileName_Is_InValid(string filename, string expectedErrorMessage)
        {
            // Arrange
            // Some special characters are not allowed in json string like \ ' { }
            // We will test only the special characters that allowed in json but not allowed in file name.
            var json = @"{
                            ""sourceFile"": {
                                ""textFile"": {
                                    ""sourceData"": ""Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."",
                                    ""outputFileName"": ""lorem_ipsum.txt""
                                }
                            },
                            ""outputFileName"": """ + filename + @""",
                            ""includeCondition"": {
                            ""textEndsWithCondition"": {
                                    ""text"": ""pikachu"",
                                    ""endsWith"": ""chu""
                                    }
                            }
                        }";

            var fileAttachmentProviderBuilder = JsonConvert.DeserializeObject<FileAttachmentProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            var fileAttachmentProvider = fileAttachmentProviderBuilder.Build(new Mock<IServiceProvider>().AddLoggers().Object);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            Func<Task> func = async () => await fileAttachmentProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be("Attachment must have a valid filename");

            exception.Which.Error.AdditionalDetails.Should().Contain($"Source Filename: lorem_ipsum.txt");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Output Filename: {filename}");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Invalid Reason: {expectedErrorMessage}");
        }

        [Fact]
        public async Task FileAttachmentProvider_ShouldReturnNull_WhenIsIncludedReturnsFalse()
        {
            // Arrange
            var automationData = await MockAutomationData.CreateWithHttpTrigger();
            var json = @"{
                            ""sourceFile"": {
                                ""textFile"": {
                                    ""sourceData"": ""Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."",
                                    ""outputFileName"": ""lorem_ipsum.txt""
                                }
                            },
                            ""outputFileName"": ""testing2.txt"",
                            ""includeCondition"": {
                                ""textIsEqualToCondition"": {
                                    ""text"": {
                                        ""objectPathLookupText"": {
                                            ""path"": ""/trigger/httpRequest/getParameters/includeDoc1"",
                                            ""valueIfNotFound"": ""No""
                                        }
                                    },
                                    ""isEqualTo"": ""Yes"",
                                    ""ignoreCase"": true
                                }
                            }
                        }";

            var fileAttachmentProviderBuilder = JsonConvert.DeserializeObject<FileAttachmentProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            var fileAttachmentProvider = fileAttachmentProviderBuilder.Build(new Mock<IServiceProvider>().AddLoggers().Object);

            // Act
            var resolvedFileAttachmentInfo = (await fileAttachmentProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            resolvedFileAttachmentInfo.Should().Be(null);
        }
    }
}
