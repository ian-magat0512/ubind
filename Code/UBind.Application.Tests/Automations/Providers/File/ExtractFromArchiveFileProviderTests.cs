// <copyright file="ExtractFromArchiveFileProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.File
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class ExtractFromArchiveFileProviderTests
    {
        private readonly string zipFile = "Automations\\Providers\\File\\TestFiles\\zipped-up-stuff.zip";
        private readonly string zipFileWithPassword = "Automations\\Providers\\File\\TestFiles\\zipped-up-stuff-pw.zip";
        private readonly string notAZipFile = "Automations\\Providers\\File\\TestFiles\\something-that-is-not-a-zip-file.zip";
        private readonly string zipFileWithWrongExtension = "Automations\\Providers\\File\\TestFiles\\zipped-up-stuff.ext";
        private readonly Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();

        public ExtractFromArchiveFileProviderTests()
        {
            this.serviceProviderMock.Setup(s => s.GetService(typeof(IClock))).Returns(new TestClock());
        }

        [Fact]
        public async Task ExtractFromArchiveFileProvider_ExtractsFileFromArchive_WithoutPasswordProtection()
        {
            // Arrange
            var json = @"{
                ""outputFilename"": ""Test Image One.png"",
                ""sourceFile"": {
                    ""productFile"": {
                        ""repository"": ""quote"",
                        ""visibility"": ""private"",
                        ""filePath"": ""zipped-up-stuff.zip""
                    }
                },
                ""filePath"": ""cgu_500px.png""
            }";
            var extractFromArchiveFileProviderBuilder = JsonConvert.DeserializeObject<ExtractFromArchiveFileProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            extractFromArchiveFileProviderBuilder.SourceFile = new TestFileProviderConfigModel { FilePath = this.zipFile };
            var extractFromArchiveFileProvider
                = extractFromArchiveFileProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();
            providerContext.Setup(s => s.GetDebugContextForProviders(It.IsAny<string>())).Returns(Task.FromResult(new JObject()));

            // Act
            var fileInfo = (await extractFromArchiveFileProvider.Resolve(providerContext.Object)).GetValueOrThrowIfFailed();

            // Assert
            fileInfo.DataValue.Should().NotBeNull();
            fileInfo.DataValue.FileName.ToString().Should().Be("Test Image One.png");
            fileInfo.DataValue.Content.Length.Should().Be(17283);
        }

        [Fact]
        public async Task ExtractFromArchiveFileProvider_ExtractsFileFromArchive_WithPasswordProtection()
        {
            // Arrange
            var json = @"{
                ""outputFilename"": ""Test Image Two.png"",
                ""sourceFile"": {
                    ""productFile"": {
                        ""repository"": ""quote"",
                        ""visibility"": ""private"",
                        ""filePath"": ""zipped-up-stuff-pw.zip""
                    }
                },
                ""password"": ""kSD(8fHJIUOH@#GFHb"",
                ""filePath"": ""folder1/saml-logo.png""
            }";
            var extractFromArchiveFileProviderBuilder = JsonConvert.DeserializeObject<ExtractFromArchiveFileProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            extractFromArchiveFileProviderBuilder.SourceFile = new TestFileProviderConfigModel { FilePath = this.zipFileWithPassword };
            var extractFromArchiveFileProvider
                = extractFromArchiveFileProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();
            providerContext.Setup(s => s.GetDebugContextForProviders(It.IsAny<string>())).Returns(Task.FromResult(new JObject()));

            // Act
            var fileInfo = (await extractFromArchiveFileProvider.Resolve(providerContext.Object)).GetValueOrThrowIfFailed();

            // Assert
            fileInfo.DataValue.Should().NotBeNull();
            fileInfo.DataValue.FileName.ToString().Should().Be("Test Image Two.png");
            fileInfo.DataValue.Content.Length.Should().Be(5034);
        }

        [Fact]
        public async Task ExtractFromArchiveFileProvider_GeneratesError_WhenPasswordIsMissingAsync()
        {
            // Arrange
            var json = @"{
                ""outputFilename"": ""Test Image Two.png"",
                ""sourceFile"": {
                    ""productFile"": {
                        ""repository"": ""quote"",
                        ""visibility"": ""private"",
                        ""filePath"": ""zipped-up-stuff-pw.zip""
                    }
                },
                ""filePath"": ""folder1/saml-logo.png""
            }";
            var extractFromArchiveFileProviderBuilder = JsonConvert.DeserializeObject<ExtractFromArchiveFileProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            extractFromArchiveFileProviderBuilder.SourceFile = new TestFileProviderConfigModel { FilePath = this.zipFileWithPassword };
            var extractFromArchiveFileProvider
                = extractFromArchiveFileProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();
            providerContext.Setup(s => s.GetDebugContextForProviders(It.IsAny<string>())).Returns(Task.FromResult(new JObject()));

            // Act
            Func<Task> func = async () => await extractFromArchiveFileProvider.Resolve(providerContext.Object);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("automation.archive.no.password.supplied");
        }

        [Fact]
        public async Task ExtractFromArchiveFileProvider_GeneratesError_WhenPasswordIsWrongAsync()
        {
            // Arrange
            var json = @"{
                ""outputFilename"": ""Test Image Two.png"",
                ""sourceFile"": {
                    ""productFile"": {
                        ""repository"": ""quote"",
                        ""visibility"": ""private"",
                        ""filePath"": ""zipped-up-stuff-pw.zip""
                    }
                },
                ""password"": ""wrong-password"",
                ""filePath"": ""folder1/saml-logo.png""
            }";
            var extractFromArchiveFileProviderBuilder = JsonConvert.DeserializeObject<ExtractFromArchiveFileProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            extractFromArchiveFileProviderBuilder.SourceFile = new TestFileProviderConfigModel { FilePath = this.zipFileWithPassword };
            var extractFromArchiveFileProvider
                = extractFromArchiveFileProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();
            providerContext.Setup(s => s.GetDebugContextForProviders(It.IsAny<string>())).Returns(Task.FromResult(new JObject()));

            // Act
            Func<Task> func = async () => await extractFromArchiveFileProvider.Resolve(providerContext.Object);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("automation.archive.wrong.password.supplied");
        }

        [Fact]
        public async Task ExtractFromArchiveFileProvider_ShouldGenerateError_WhenArchiveIsInUnsupportedFormatAsync()
        {
            // Arrange
            var json = @"{
                ""outputFilename"": ""Test Image Two.png"",
                ""sourceFile"": {
                    ""productFile"": {
                        ""repository"": ""quote"",
                        ""visibility"": ""private"",
                        ""filePath"": ""something-that-is-not-a-zip-file.zip""
                    }
                },
                ""filePath"": ""folder1/saml-logo.png""
            }";
            var extractFromArchiveFileProviderBuilder = JsonConvert.DeserializeObject<ExtractFromArchiveFileProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            extractFromArchiveFileProviderBuilder.SourceFile = new TestFileProviderConfigModel { FilePath = this.notAZipFile };
            var extractFromArchiveFileProvider
                = extractFromArchiveFileProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();
            providerContext.Setup(s => s.GetDebugContextForProviders(It.IsAny<string>())).Returns(Task.FromResult(new JObject()));

            // Act
            Func<Task> func = async () => await extractFromArchiveFileProvider.Resolve(providerContext.Object);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("automation.archive.not.a.valid.zip.archive");
        }

        [Fact]
        public async Task ExtractFromArchiveFileProvider_ShouldExtractFile_WhenCorrectFormatIsSpecifiedAndFilenameHasIncorrectExtension()
        {
            // Arrange
            var json = @"{
                ""outputFilename"": ""Test Image Three.png"",
                ""sourceFile"": {
                    ""productFile"": {
                        ""repository"": ""quote"",
                        ""visibility"": ""private"",
                        ""filePath"": ""zipped-up-stuff.ext""
                    }
                },
                ""format"": ""zip"",
                ""filePath"": ""folder1/saml-logo.png""
            }";
            var extractFromArchiveFileProviderBuilder = JsonConvert.DeserializeObject<ExtractFromArchiveFileProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            extractFromArchiveFileProviderBuilder.SourceFile = new TestFileProviderConfigModel { FilePath = this.zipFileWithWrongExtension };
            var extractFromArchiveFileProvider
                = extractFromArchiveFileProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();
            providerContext.Setup(s => s.GetDebugContextForProviders(It.IsAny<string>())).Returns(Task.FromResult(new JObject()));

            // Act
            var fileInfo = (await extractFromArchiveFileProvider.Resolve(providerContext.Object)).GetValueOrThrowIfFailed();

            // Assert
            fileInfo.DataValue.Should().NotBeNull();
            fileInfo.DataValue.FileName.ToString().Should().Be("Test Image Three.png");
            fileInfo.DataValue.Content.Length.Should().Be(5034);
        }
    }
}
