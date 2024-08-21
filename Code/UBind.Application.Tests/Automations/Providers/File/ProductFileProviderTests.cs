// <copyright file="ProductFileProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.File
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;
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

    public class ProductFileProviderTests
    {
        [Theory]
        [InlineData(
            @"{
                ""repository"": ""quote"",
                ""outputFilename"": ""test.txt"",
                ""productAlias"": ""life"",
                ""environment"": ""development"",
                ""visibility"": ""Public"",
                ""filePath"": ""test.txt""
                }")]
        [InlineData(
            @"{
                ""repository"": ""claim"",
                ""outputFilename"": ""test.txt"",
                ""productAlias"": ""life"",
                ""environment"": ""development"",
                ""visibility"": ""Public"",
                ""filePath"": ""test.txt""
                }")]
        [InlineData(
            @"{
                ""repository"": ""quote"",
                ""outputFilename"": ""test.txt"",
                ""productAlias"": ""life"",
                ""environment"": ""staging"",
                ""visibility"": ""Public"",
                ""filePath"": ""test.txt""
                }")]
        [InlineData(
            @"{
                ""repository"": ""claim"",
                ""outputFilename"": ""test.txt"",
                ""productAlias"": ""life"",
                ""environment"": ""staging"",
                ""visibility"": ""Public"",
                ""filePath"": ""test.txt""
                }")]
        [InlineData(
            @"{
                ""repository"": ""quote"",
                ""outputFilename"": ""test.txt"",
                ""productAlias"": ""life"",
                ""environment"": ""production"",
                ""visibility"": ""Public"",
                ""filePath"": ""test.txt""
                }")]
        [InlineData(
            @"{
                ""repository"": ""claim"",
                ""outputFilename"": ""test.txt"",
                ""productAlias"": ""life"",
                ""environment"": ""production"",
                ""visibility"": ""Public"",
                ""filePath"": ""test.txt""
                }")]
        public async Task ProductFileProvider_Should_Return_FileInfo(string json)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var jsonObject = JObject.Parse(json);
            var productId = jsonObject.SelectToken("productAlias")?.ToString();
            var fileName = jsonObject.SelectToken("outputFilename")?.ToString();
            var repository = jsonObject.SelectToken("repository")?.ToString();
            var filePath = jsonObject.SelectToken("filePath")?.ToString();
            Enum.TryParse(jsonObject.SelectToken("environment")?.ToString(), true, out DeploymentEnvironment environment);
            var expectedContent = "The quick brown fox jumps over the lazy dog.";

            var tenant = new Tenant(tenantId, "test", "test", null, default, default, default);
            var product = new Product(tenantId, Guid.NewGuid(), "productname", productId, default);

            var clock = new TestClock();
            var fakeDevReleaseBuilder = FakeReleaseBuilder.CreateForProduct(tenantId, product.Id);
            if (repository == "quote")
            {
                fakeDevReleaseBuilder.WithQuoteAsset(fileName, expectedContent);
            }
            else
            {
                fakeDevReleaseBuilder.WithClaimAsset(fileName, expectedContent);
            }

            var fakeDevRelease = fakeDevReleaseBuilder.BuildDevRelease();
            var cachedRelease = new ActiveDeployedRelease(fakeDevRelease, environment, null);
            var mockReleaseQueryService = new Mock<IReleaseQueryService>();
            mockReleaseQueryService
                .Setup(s => s.GetRelease(It.IsAny<ReleaseContext>()))
                .Returns(cachedRelease);

            var mockTenantRepository = new Mock<ITenantRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            mockProductRepository.Setup(c => c.ProductIdIsAvailableInTenant(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);
            mockProductRepository.Setup(c => c.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).Returns(product);
            mockProductRepository.Setup(c => c.GetProductByAlias(It.IsAny<Guid>(), It.IsAny<string>())).Returns(product);
            var mockcachingResolver = new Mock<ICachingResolver>();
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductByAliasOrThrow(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductByAliasOrNull(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(product));
            var mockMediator = new Mock<ICqrsMediator>();
            mockMediator.Setup(s => s.Send(It.IsAny<GetProductFileContentsByFileNameQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Encoding.UTF8.GetBytes(expectedContent)));
            var serviceProvider = AutomationTestsHelper.CreateMockServiceProvider(
                mockReleaseQueryService.Object,
                mockTenantRepository.Object,
                mockProductRepository.Object,
                mockcachingResolver.Object,
                mockMediator.Object);
            var productFileProvider = this.GetProductFileProvider(json, serviceProvider);

            // Act
            var automationData = await MockAutomationData.CreateWithHttpTrigger(
                tenant.Id,
                default,
                product.Id,
                DeploymentEnvironment.Development);
            var fileInfo = (await productFileProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            fileInfo.DataValue.FileName.ToString().Should().Be(fileName);

            var fileContent = Encoding.UTF8.GetString(fileInfo.DataValue.Content);
            fileContent.Should().Be(expectedContent);
        }

        [Theory]
        [InlineData(@"{
                                ""repository"": ""quote"",
                                ""visibility"": ""Public"",
                                ""filePath"": ""test.txt""
                            }")]
        [InlineData(@"{
                                ""repository"": ""quote"",
                                ""environment"": ""development"",
                                ""outputFilename"": ""test.txt"",
                                ""visibility"": ""Public"",
                                ""filePath"": ""test.txt""
                            }")]
        [InlineData(@"{
                                ""repository"": ""quote"",
                                ""productAlias"": ""life"",
                                ""outputFilename"": ""test.txt"",
                                ""visibility"": ""Public"",
                                ""filePath"": ""test.txt""
                            }")]
        [InlineData(@"{
                                ""repository"": ""quote"",
                                ""productAlias"": ""life"",
                                ""environment"": ""development"",
                                ""visibility"": ""Public"",
                                ""filePath"": ""test.txt""
                            }")]
        [InlineData(@"{
                                ""repository"": ""quote"",
                                ""productAlias"": ""life"",
                                ""visibility"": ""Public"",
                                ""filePath"": ""test.txt""
                            }")]
        [InlineData(@"{
                                ""repository"": ""quote"",
                                ""environment"": ""development"",
                                ""visibility"": ""Public"",
                                ""filePath"": ""test.txt""
                            }")]
        [InlineData(@"{
                                ""repository"": ""quote"",
                                ""outputFilename"": ""test.txt"",
                                ""visibility"": ""Public"",
                                ""filePath"": ""test.txt""
                            }")]
        public async Task ProductFileProvider_Should_Use_ProductContext_When_Optional_Parameters_Is_Missing(string json)
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var jsonObject = JObject.Parse(json);
            var productId = jsonObject.SelectToken("productAlias")?.ToString();
            var fileName = jsonObject.SelectToken("outputFilename")?.ToString();

            var tenant = new Tenant(tenantId, "test", "test", null, default, default, default);
            Product product = string.IsNullOrEmpty(productId) ?
                new Product(tenant.Id, Guid.NewGuid(), "qwe", "life", default) :
                new Product(tenant.Id, Guid.NewGuid(), "productname", productId, default);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = Path.GetFileName(jsonObject.SelectToken("filePath")?.ToString());
            }

            var repository = jsonObject.SelectToken("repository")?.ToString();
            Enum.TryParse(jsonObject.SelectToken("environment")?.ToString(), true, out DeploymentEnvironment environment);
            var expectedContent = "The quick brown fox jumps over the lazy dog.";

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
            mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            mockProductRepository.Setup(c => c.ProductIdIsAvailableInTenant(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);
            mockProductRepository.Setup(c => c.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).Returns(product);
            mockProductRepository.Setup(c => c.GetProductByAlias(It.IsAny<Guid>(), It.IsAny<string>())).Returns(product);
            var mockcachingResolver = new Mock<ICachingResolver>();
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductByAliasOrThrow(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductByAliasOrNull(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductAliasOrThrowAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product.Details.Alias));
            mockcachingResolver.Setup(x => x.GetProductAliasOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(product.Details.Alias);
            var mockMediator = new Mock<ICqrsMediator>();
            mockMediator.Setup(s => s.Send(It.IsAny<GetProductFileContentsByFileNameQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Encoding.UTF8.GetBytes(expectedContent)));
            var serviceProvider = AutomationTestsHelper.CreateMockServiceProvider(
                mockReleaseQueryService.Object,
                mockTenantRepository.Object,
                mockProductRepository.Object,
                mockcachingResolver.Object,
                mockMediator.Object);
            var productFileProvider = this.GetProductFileProvider(json, serviceProvider);

            // Act
            var automationData = await MockAutomationData.CreateWithHttpTrigger(
                tenant.Id,
                default,
                product.Id,
                DeploymentEnvironment.Development);
            var fileInfo = (await productFileProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            fileInfo.DataValue.FileName.ToString().Should().Be(fileName);
            var fileContent = Encoding.UTF8.GetString(fileInfo.DataValue.Content);
            fileContent.Should().Be(expectedContent);
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
        public async Task ProductFileProvider_Throw_When_FileName_Is_InValid(string filename, string expectedErrorMessage)
        {
            // Arrange
            // Some special characters are not allowed in json string like \ ' { }
            // We will test only the special characters that allowed in json but not allowed in file name.
            var json = @"{
                                ""repository"": ""quote"",
                                ""outputFilename"": """ + filename + @""",
                                ""productAlias"": ""life"",
                                ""environment"": ""development"",
                                ""visibility"": ""Public"",
                                ""filePath"": """"
                          }";

            var tenantId = Guid.NewGuid();
            var jsonObject = JObject.Parse(json);
            var productAlias = jsonObject.SelectToken("productAlias")?.ToString();
            var fileName = jsonObject.SelectToken("outputFilename")?.ToString();
            var repository = jsonObject.SelectToken("repository")?.ToString();
            var visibility = jsonObject.SelectToken("visibility")?.ToString();
            var filePath = jsonObject.SelectToken("filePath")?.ToString();
            Enum.TryParse(jsonObject.SelectToken("environment")?.ToString(), true, out DeploymentEnvironment environment);
            var expectedContent = "The quick brown fox jumps over the lazy dog.";

            var tenant = new Tenant(tenantId, "test", "test", null, default, default, default);
            var product = new Product(tenant.Id, Guid.NewGuid(), "productname", productAlias, default);

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
            mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            mockProductRepository.Setup(c => c.ProductIdIsAvailableInTenant(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);
            mockProductRepository.Setup(c => c.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).Returns(product);
            mockProductRepository.Setup(c => c.GetProductByAlias(It.IsAny<Guid>(), It.IsAny<string>())).Returns(product);
            var mockcachingResolver = new Mock<ICachingResolver>();
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductByAliasOrThrow(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductByAliasOrNull(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(product));
            var mockMediator = new Mock<ICqrsMediator>();
            mockMediator.Setup(s => s.Send(It.IsAny<GetProductFileContentsByFileNameQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Encoding.UTF8.GetBytes(expectedContent)));
            var serviceProvider = AutomationTestsHelper.CreateMockServiceProvider(
                mockReleaseQueryService.Object,
                mockTenantRepository.Object,
                mockProductRepository.Object,
                mockcachingResolver.Object,
                mockMediator.Object);
            var productFileProvider = this.GetProductFileProvider(json, serviceProvider);

            // Act
            var automationData = await MockAutomationData.CreateWithHttpTrigger(
                tenant.Id,
                default,
                product.Id,
                DeploymentEnvironment.Development);
            Func<Task> act = async () => await productFileProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = (await act.Should().ThrowAsync<ErrorException>()).And;
            exception.Error.Code.Should().Be("file.name.invalid");
            exception.Error.AdditionalDetails.Should().Contain($"File path: {filePath}");
            exception.Error.AdditionalDetails.Should().Contain($"Output file name: {fileName}");
            exception.Error.AdditionalDetails.Should().Contain($"Repository: {repository}");
            exception.Error.AdditionalDetails.Should().Contain($"Visibility: {visibility}");
            exception.Error.AdditionalDetails.Should().Contain($"Invalid Reason: {expectedErrorMessage}");
        }

        [Fact]
        public async Task ProductFileProvider_Throw_When_ProductId_Cannot_Be_Resolve()
        {
            // Arrange
            var json = @"{
                                ""repository"": ""quote"",
                                ""outputFilename"": ""text.txt"",
                                ""productAlias"": ""life"",
                                ""environment"": ""development"",
                                ""visibility"": ""Public"",
                                ""filePath"": ""sample/test.txt""
                          }";

            var tenantId = Guid.NewGuid();
            var jsonObject = JObject.Parse(json);
            var productAlias = jsonObject.SelectToken("productAlias")?.ToString();
            var fileName = jsonObject.SelectToken("outputFilename")?.ToString();
            var repository = jsonObject.SelectToken("repository")?.ToString();
            var visibility = jsonObject.SelectToken("visibility")?.ToString();
            var filePath = jsonObject.SelectToken("filePath")?.ToString();
            var expectedContent = "The quick brown fox jumps over the lazy dog.";
            Enum.TryParse(jsonObject.SelectToken("environment")?.ToString(), true, out DeploymentEnvironment environment);
            var tenant = new Tenant(tenantId, "test", "test", null, default, default, default);
            var product = new Product(tenant.Id, Guid.NewGuid(), "productname", productAlias, default);
            var fakeDevRelease = FakeReleaseBuilder
                .CreateForProduct(tenantId, product.Id)
                .WithQuoteAsset(fileName, expectedContent)
                .BuildDevRelease();
            var cachedRelease = new ActiveDeployedRelease(fakeDevRelease, environment, null);
            var mockReleaseQueryService = new Mock<IReleaseQueryService>();
            var mockProductRepository = new Mock<IProductRepository>();
            var mockTenantRepository = new Mock<ITenantRepository>();
            mockReleaseQueryService
                .Setup(s => s.GetRelease(It.IsAny<ReleaseContext>()))
                .Returns(cachedRelease);

            // return true so simulate product is not existing in the tenant.
            mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            mockProductRepository.Setup(c => c.ProductIdIsAvailableInTenant(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);
            mockProductRepository.Setup(c => c.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).Returns(product);
            mockProductRepository.Setup(c => c.GetProductByAlias(It.IsAny<Guid>(), It.IsAny<string>())).Returns(default(Product));
            var mockcachingResolver = new Mock<ICachingResolver>();
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductByAliasOrThrow(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(default(Product)));
            mockcachingResolver.Setup(x => x.GetProductByAliasOrNull(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(default(Product)));
            var mockMediator = new Mock<ICqrsMediator>();
            mockMediator.Setup(s => s.Send(It.IsAny<GetProductFileContentsByFileNameQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Encoding.UTF8.GetBytes("asdf")));
            var serviceProvider = AutomationTestsHelper.CreateMockServiceProvider(
                mockReleaseQueryService.Object,
                mockTenantRepository.Object,
                mockProductRepository.Object,
                mockcachingResolver.Object,
                mockMediator.Object);
            var productFileProvider = this.GetProductFileProvider(json, serviceProvider);
            var expectedError = Errors.Automation.Provider.ProductNotFound(productAlias, null);

            // Act
            var automationData = await MockAutomationData.CreateWithHttpTrigger(
                tenant.Id,
                default,
                product.Id,
                DeploymentEnvironment.Development);
            Func<Task> func = async () => await productFileProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = await func.Should().ThrowAsync<ErrorException>();
            exception.Which.Error.Title.Should().Be(expectedError.Title);
            exception.Which.Error.Message.Should().Be(expectedError.Message);
            exception.Which.Error.AdditionalDetails.Should().Contain($"Environment: {environment}");
            exception.Which.Error.AdditionalDetails.Should().Contain($"File Path: {filePath}");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Output Filename: {fileName}");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Repository: {repository}");
            exception.Which.Error.AdditionalDetails.Should().Contain($"Visibility: {visibility}");
            mockcachingResolver.Verify(x => x.GetProductByAliasOrNull(tenant.Id, productAlias));
        }

        [Fact]
        public async Task ProductFileProvider_Throw_When_File_Not_Found_In_Release()
        {
            // Arrange
            var clock = new TestClock();
            var json = @"{
                                ""repository"": ""quote"",
                                ""outputFilename"": ""text.txt"",
                                ""productAlias"": ""life"",
                                ""environment"": ""development"",
                                ""visibility"": ""Public"",
                                ""filePath"": ""sample/test.txt""
                          }";

            var tenantId = Guid.NewGuid();
            var jsonObject = JObject.Parse(json);
            var productAlias = jsonObject.SelectToken("productAlias")?.ToString();
            var fileName = jsonObject.SelectToken("outputFilename")?.ToString();
            var repository = jsonObject.SelectToken("repository")?.ToString();
            var visibility = jsonObject.SelectToken("visibility")?.ToString();
            var filePath = jsonObject.SelectToken("filePath")?.ToString();
            Enum.TryParse(jsonObject.SelectToken("environment")?.ToString(), true, out DeploymentEnvironment environment);
            var tenant = new Tenant(tenantId, "test", "test", null, default, default, default);
            var product = new Product(tenant.Id, Guid.NewGuid(), "productname", productAlias, default);
            var fakeDevRelease = FakeReleaseBuilder.CreateForProduct(tenantId, product.Id).BuildDevRelease();

            var cachedRelease = new ActiveDeployedRelease(fakeDevRelease, environment, null);
            var mockReleaseQueryService = new Mock<IReleaseQueryService>();
            mockReleaseQueryService
                .Setup(s => s.GetRelease(It.IsAny<ReleaseContext>()))
                .Returns(cachedRelease);

            var mockTenantRepository = new Mock<ITenantRepository>();
            var mockProductRepository = new Mock<IProductRepository>();
            mockTenantRepository.Setup(c => c.GetTenantById(It.IsAny<Guid>())).Returns(tenant);
            mockProductRepository.Setup(c => c.ProductIdIsAvailableInTenant(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);
            mockProductRepository.Setup(c => c.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>(), false)).Returns(product);
            mockProductRepository.Setup(c => c.GetProductByAlias(It.IsAny<Guid>(), It.IsAny<string>())).Returns(product);
            var mockcachingResolver = new Mock<ICachingResolver>();
            mockcachingResolver.Setup(x => x.GetTenantOrThrow(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrThrow(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductByAliasOrThrow(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetTenantOrNull(It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(tenant));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductOrNull(It.IsAny<GuidOrAlias>(), It.IsAny<GuidOrAlias>())).Returns(Task.FromResult(product));
            mockcachingResolver.Setup(x => x.GetProductByAliasOrNull(It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(product));
            var mockMediator = new Mock<ICqrsMediator>();
            var mockFileContentRepository = new Mock<IFileContentRepository>();
            var query = new GetProductFileContentsByFileNameQuery(
                new ReleaseContext(tenantId, product.Id, environment, Guid.NewGuid()),
                WebFormAppType.Quote,
                FileVisibility.Private,
                "asdf.file");
            var handler = new GetProductFileContentsByFileNameQueryHandler(
                mockReleaseQueryService.Object,
                mockcachingResolver.Object,
                mockFileContentRepository.Object);
            mockMediator.Setup(s => s.Send(It.IsAny<GetProductFileContentsByFileNameQuery>(), It.IsAny<CancellationToken>()))
                .Returns(handler.Handle(query, default(CancellationToken)));
            var serviceProvider = AutomationTestsHelper.CreateMockServiceProvider(
                mockReleaseQueryService.Object,
                mockTenantRepository.Object,
                mockProductRepository.Object,
                mockcachingResolver.Object,
                mockMediator.Object);
            var productFileProvider = this.GetProductFileProvider(json, serviceProvider);

            // Act
            var automationData = await MockAutomationData.CreateWithHttpTrigger(
                tenant.Id,
                default,
                product.Id,
                DeploymentEnvironment.Development);
            Func<Task> act = async () => await productFileProvider.Resolve(new ProviderContext(automationData));

            // Assert
            var exception = (await act.Should().ThrowAsync<ErrorException>()).And;
            exception.Error.Code.Should().Be("product.file.not.found");
        }

        private ProductFileProvider GetProductFileProvider(string json, IServiceProvider serviceProvider)
        {
            var productFileProviderBuilder = JsonConvert.DeserializeObject<ProductFileProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            return (ProductFileProvider)productFileProviderBuilder.Build(serviceProvider);
        }
    }
}
