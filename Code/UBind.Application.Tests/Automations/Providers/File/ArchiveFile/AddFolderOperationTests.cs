// <copyright file="AddFolderOperationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.File.ArchiveFile
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
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.Automation.Providers.File.ArchiveFile;
    using UBind.Application.Automation.Providers.List;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class AddFolderOperationTests
    {
        private readonly string iagLogoFile = "Automations\\Providers\\File\\TestFiles\\iag.png";
        private readonly Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();

        public AddFolderOperationTests()
        {
            this.serviceProviderMock.Setup(s => s.GetService(typeof(IClock))).Returns(new TestClock());
        }

        [Fact]
        public async Task AddFolderOperation_AddsEntriesForParentFoldersFirst_WhenDeepFolderIsCreated()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""outputFilename"": ""my-test.zip"",
                        ""operations"": [
                            {
                                ""addFolder"": {
                                    ""folderName"": ""deep"",
                                    ""destinationFolderPath"": ""a/b/c/"",
                                    ""whenDestinationFolderNotFound"": ""create""
                                }
                            }
                        ]
                    }
                }
            }";

            var archiveFileContentsListProviderBuilder = JsonConvert.DeserializeObject<ArchiveFileContentsListProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            var archiveFileContentsListProvider
                = archiveFileContentsListProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();
            providerContext.Setup(s => s.GetDebugContextForProviders(It.IsAny<string>())).Returns(Task.FromResult(new JObject()));

            // Act
            var dataList = (await archiveFileContentsListProvider.Resolve(providerContext.Object)).GetValueOrThrowIfFailed();

            // Assert
            dataList.Should().NotBeNull();
            List<ArchiveEntryModel> list = dataList.ToList().Select(x => x as ArchiveEntryModel).ToList();
            list.Count.Should().Be(4);

            list[0].IsFile.Should().BeFalse();
            list[0].IsFolder.Should().BeTrue();
            list[0].Name.Should().Be("a");
            list[0].Path.Should().Be("a/");

            list[1].IsFile.Should().BeFalse();
            list[1].IsFolder.Should().BeTrue();
            list[1].Name.Should().Be("b");
            list[1].Path.Should().Be("a/b/");

            list[2].IsFile.Should().BeFalse();
            list[2].IsFolder.Should().BeTrue();
            list[2].Name.Should().Be("c");
            list[2].Path.Should().Be("a/b/c/");

            list[3].IsFile.Should().BeFalse();
            list[3].IsFolder.Should().BeTrue();
            list[3].Name.Should().Be("deep");
            list[3].Path.Should().Be("a/b/c/deep/");
        }

        [Fact]
        public async Task AddFolderOperation_RaisesErrorByDefault_WhenFileExistsAsync()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""outputFilename"": ""my-test.zip"",
                        ""operations"": [
                            {
                                ""addFile"": {
                                    ""destinationFileName"": ""test-1"",
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""iag.png""
                                        }
                                    },
                                    ""destinationFolderPath"": ""test-folder-1/""
                                }
                            },
                            {
                                ""addFolder"": {
                                    ""folderName"": ""test-1"",
                                    ""destinationFolderPath"": ""test-folder-1/""
                                }
                            }
                        ]
                    }
                }
            }";

            var archiveFileContentsListProviderBuilder = JsonConvert.DeserializeObject<ArchiveFileContentsListProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            ArchiveFileProviderConfigModel archiveFileProviderBuilder
                = (ArchiveFileProviderConfigModel)archiveFileContentsListProviderBuilder.SourceFile;
            var operationBuilders = archiveFileProviderBuilder.Operations.ToList();
            ((AddFileOperationConfigModel)operationBuilders[0]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.iagLogoFile };
            var archiveFileContentsListProvider
                = archiveFileContentsListProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();
            providerContext.Setup(s => s.GetDebugContextForProviders(It.IsAny<string>())).Returns(Task.FromResult(new JObject()));

            // Act
            Func<Task> func = async () => await archiveFileContentsListProvider.Resolve(providerContext.Object);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).And.Error.Code.Should().Be(
                "automation.archive.add.folder.operation.entry.already.exists");
        }

        [Fact]
        public async Task AddFolderOperation_ReplacesFileAsSpecified_WhenFileExists()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""outputFilename"": ""my-test.zip"",
                        ""operations"": [
                            {
                                ""addFile"": {
                                    ""destinationFileName"": ""test-1"",
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""iag.png""
                                        }
                                    },
                                    ""destinationFolderPath"": ""test-folder-1/""
                                }
                            },
                            {
                                ""addFolder"": {
                                    ""folderName"": ""test-1"",
                                    ""destinationFolderPath"": ""test-folder-1/"",
                                    ""whenDestinationFolderContainsFileWithSameName"": ""replace""
                                }
                            }
                        ]
                    }
                }
            }";

            var archiveFileContentsListProviderBuilder = JsonConvert.DeserializeObject<ArchiveFileContentsListProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            ArchiveFileProviderConfigModel archiveFileProviderBuilder
                = (ArchiveFileProviderConfigModel)archiveFileContentsListProviderBuilder.SourceFile;
            var operationBuilders = archiveFileProviderBuilder.Operations.ToList();
            ((AddFileOperationConfigModel)operationBuilders[0]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.iagLogoFile };
            var archiveFileContentsListProvider
                = archiveFileContentsListProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();
            providerContext.Setup(s => s.GetDebugContextForProviders(It.IsAny<string>())).Returns(Task.FromResult(new JObject()));

            // Act
            var dataList = (await archiveFileContentsListProvider.Resolve(providerContext.Object)).GetValueOrThrowIfFailed();

            // Assert
            dataList.Should().NotBeNull();
            List<ArchiveEntryModel> list = dataList.ToList().Select(x => x as ArchiveEntryModel).ToList();
            list.Count.Should().Be(2);

            list[0].IsFile.Should().BeFalse();
            list[0].IsFolder.Should().BeTrue();
            list[0].Name.Should().Be("test-folder-1");
            list[0].Path.Should().Be("test-folder-1/");

            list[1].IsFile.Should().BeFalse();
            list[1].IsFolder.Should().BeTrue();
            list[1].Name.Should().Be("test-1");
            list[1].Path.Should().Be("test-folder-1/test-1/");
        }
    }
}
