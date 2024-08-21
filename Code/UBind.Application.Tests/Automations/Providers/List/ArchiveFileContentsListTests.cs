// <copyright file="ArchiveFileContentsListTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.List
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.List;
    using UBind.Application.Tests.Automations.Providers.File;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class ArchiveFileContentsListTests
    {
        private readonly string zipFile = "Automations\\Providers\\List\\TestFiles\\zipped-up-stuff.zip";
        private readonly string notAZipFile = "Automations\\Providers\\List\\TestFiles\\something-that-is-not-a-zip-file.zip";
        private readonly string zipFileWithWrongExtension = "Automations\\Providers\\List\\TestFiles\\zipped-up-stuff.ext";
        private readonly Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();

        public ArchiveFileContentsListTests()
        {
            this.serviceProviderMock.Setup(s => s.GetService(typeof(IClock))).Returns(new TestClock());
        }

        [Fact]
        public async Task ArchiveFileContentsListProvider_ShouldListArchiveContents_WithCorrectProperties()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""productFile"": {
                        ""repository"": ""quote"",
                        ""visibility"": ""private"",
                        ""filePath"": ""zipped-up-stuff.zip""
                    }
                }
            }";
            var archiveFileContentsListProviderBuilder = JsonConvert.DeserializeObject<ArchiveFileContentsListProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            archiveFileContentsListProviderBuilder.SourceFile = new TestFileProviderConfigModel { FilePath = this.zipFile };
            var archiveFileContentsListProvider
                = archiveFileContentsListProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();

            // Act
            var dataList = (await archiveFileContentsListProvider.Resolve(providerContext.Object)).GetValueOrThrowIfFailed();

            // Assert
            dataList.Should().NotBeNull();
            List<ArchiveEntryModel> list = dataList.ToList().Select(x => x as ArchiveEntryModel).ToList();
            list.Count.Should().Be(7);

            // Saml Logo File (3rd entry)
            list[2].IsFile.Should().BeTrue();
            list[2].IsFolder.Should().BeFalse();
            list[2].Name.Should().Be("saml-logo.png");
            list[2].Path.Should().Be("folder1/saml-logo.png");
            list[2].Size.Should().Be("4.9 KB");
            list[2].SizeBytes.Should().Be(5034);
            list[2].CompressedSize.Should().Be("4.7 KB");
            list[2].CompressedSizeBytes.Should().Be(4766);
            list[2].LastModifiedDateTime.Should().Be("2022-11-14T01:31:32Z");
            list[2].LastModifiedTicksSinceEpoch.Should().Be(16683894920000000);
            list[2].LastModifiedDate.Should().Be("14 Nov 2022");
            list[2].LastModifiedTime.Should().Be("1:31 am");

            // folder2 (4th entry)
            list[3].IsFile.Should().BeFalse();
            list[3].IsFolder.Should().BeTrue();
            list[3].Name.Should().Be("folder2");
            list[3].Path.Should().Be("folder2/");
            list[3].Size.Should().BeNull();
            list[3].SizeBytes.Should().BeNull();
            list[3].CompressedSize.Should().BeNull();
            list[3].CompressedSizeBytes.Should().BeNull();
            list[3].LastModifiedDateTime.Should().Be("2022-12-22T22:08:58Z");
            list[3].LastModifiedTicksSinceEpoch.Should().Be(16717469380000000);
            list[3].LastModifiedDate.Should().Be("22 Dec 2022");
            list[3].LastModifiedTime.Should().Be("10:08 pm");

            // folder2/sub (5th entry)
            list[4].Name.Should().Be("sub");
            list[4].Path.Should().Be("folder2/sub/");
        }

        [Fact]
        public async Task ArchiveFileContentsListProvider_ShouldGenerateError_WhenArchiveIsInUnsupportedFormatAsync()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""productFile"": {
                        ""repository"": ""quote"",
                        ""visibility"": ""private"",
                        ""filePath"": ""something-that-is-not-a-zip-file.zip""
                    }
                }
            }";
            var archiveFileContentsListProviderBuilder = JsonConvert.DeserializeObject<ArchiveFileContentsListProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            archiveFileContentsListProviderBuilder.SourceFile = new TestFileProviderConfigModel { FilePath = this.notAZipFile };
            var archiveFileContentsListProvider
                = archiveFileContentsListProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();
            providerContext.Setup(s => s.GetDebugContextForProviders(It.IsAny<string>())).Returns(Task.FromResult(new JObject()));

            // Act
            Func<Task> func = async () => await archiveFileContentsListProvider.Resolve(providerContext.Object);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("automation.archive.not.a.valid.zip.archive");
        }

        [Fact]
        public async Task ArchiveFileContentsListProvider_ShouldListArchiveContents_WhenCorrectFormatIsSpecifiedAndFilenameHasIncorrectExtension()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""productFile"": {
                        ""repository"": ""quote"",
                        ""visibility"": ""private"",
                        ""filePath"": ""zipped-up-stuff.ext""
                    },
                },
                ""format"": ""zip""
            }";
            var archiveFileContentsListProviderBuilder = JsonConvert.DeserializeObject<ArchiveFileContentsListProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            archiveFileContentsListProviderBuilder.SourceFile = new TestFileProviderConfigModel { FilePath = this.zipFileWithWrongExtension };
            var archiveFileContentsListProvider
                = archiveFileContentsListProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();
            providerContext.Setup(s => s.GetDebugContextForProviders(It.IsAny<string>())).Returns(Task.FromResult(new JObject()));

            // Act
            var dataList = (await archiveFileContentsListProvider.Resolve(providerContext.Object)).GetValueOrThrowIfFailed();

            // Assert
            dataList.Should().NotBeNull();
            List<ArchiveEntryModel> list = dataList.ToList().Select(x => x as ArchiveEntryModel).ToList();
            list.Count.Should().Be(7);
        }
    }
}
