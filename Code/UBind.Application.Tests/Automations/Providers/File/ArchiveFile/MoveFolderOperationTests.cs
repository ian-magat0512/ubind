// <copyright file="MoveFolderOperationTests.cs" company="uBind">
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

    public class MoveFolderOperationTests
    {
        private readonly string iagLogoFile = "Automations\\Providers\\File\\TestFiles\\iag.png";
        private readonly string zipFile = "Automations\\Providers\\List\\TestFiles\\zipped-up-stuff.zip";
        private readonly string samlLogoFile = "Automations\\Providers\\File\\TestFiles\\saml-logo.png";
        private readonly Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();

        public MoveFolderOperationTests()
        {
            this.serviceProviderMock.Setup(s => s.GetService(typeof(IClock))).Returns(new TestClock());
        }

        [Fact]
        public async Task MoveFolderOperation_FolderNoLongerExists_AtOriginalLocation()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""outputFilename"": ""test.zip"",
                        ""operations"": [
                            {
                                ""addFolder"": {
                                    ""folderName"": ""test-folder-1"",
                                }
                            },
                            {
                                ""addFolder"": {
                                    ""folderName"": ""test-folder-2"",
                                }
                            },
                            {
                                ""moveFolder"": {
                                    ""sourceFolderPath"": ""test-folder-1/"",
                                    ""destinationFolderPath"": ""test-folder-2/"",
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
            list[0].Name.Should().Be("test-folder-2");
            list[0].Path.Should().Be("test-folder-2/");

            list[1].IsFile.Should().BeFalse();
            list[1].IsFolder.Should().BeTrue();
            list[1].Name.Should().Be("test-folder-1");
            list[1].Path.Should().Be("test-folder-2/test-folder-1/");
        }

        [Fact]
        public async Task MoveFolderOperation_CreatesDestinationFolder_WhenFolderNotFound()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""outputFilename"": ""test.zip"",
                        ""operations"": [
                            {
                                ""addFolder"": {
                                    ""folderName"": ""test-folder-1"",
                                }
                            },
                            {
                                ""moveFolder"": {
                                    ""sourceFolderPath"": ""test-folder-1/"",
                                    ""destinationFolderPath"": ""test-folder-2/test-folder-3/"",
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
            var archiveFileContentsListProvider
                = archiveFileContentsListProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();
            providerContext.Setup(s => s.GetDebugContextForProviders(It.IsAny<string>())).Returns(Task.FromResult(new JObject()));

            // Act
            var dataList = (await archiveFileContentsListProvider.Resolve(providerContext.Object)).GetValueOrThrowIfFailed();

            // Assert
            dataList.Should().NotBeNull();
            List<ArchiveEntryModel> list = dataList.ToList().Select(x => x as ArchiveEntryModel).ToList();
            list.Count.Should().Be(3);

            list[0].IsFile.Should().BeFalse();
            list[0].IsFolder.Should().BeTrue();
            list[0].Name.Should().Be("test-folder-2");
            list[0].Path.Should().Be("test-folder-2/");

            list[1].IsFile.Should().BeFalse();
            list[1].IsFolder.Should().BeTrue();
            list[1].Name.Should().Be("test-folder-3");
            list[1].Path.Should().Be("test-folder-2/test-folder-3/");

            list[2].IsFile.Should().BeFalse();
            list[2].IsFolder.Should().BeTrue();
            list[2].Name.Should().Be("test-folder-1");
            list[2].Path.Should().Be("test-folder-2/test-folder-3/test-folder-1/");
        }

        [Fact]
        public async Task MoveFolderOperation_MovesDescendentFilesAndFolders()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""sourceFile"": {
                            ""productFile"": {
                                ""repository"": ""quote"",
                                ""visibility"": ""private"",
                                ""filePath"": ""zipped-up-stuff.zip""
                            }
                        },
                        ""operations"": [
                            {
                                ""addFolder"": {
                                    ""folderName"": ""destination-1"",
                                }
                            },
                            {
                                ""moveFolder"": {
                                    ""sourceFolderPath"": ""folder1"",
                                    ""destinationFolderPath"": ""destination-1"",
                                }
                            },
                            {
                                ""moveFolder"": {
                                    ""sourceFolderPath"": ""folder2"",
                                    ""destinationFolderPath"": ""destination-2"",
                                }
                            },
                        ]
                    }
                }
            }";

            var archiveFileContentsListProviderBuilder = JsonConvert.DeserializeObject<ArchiveFileContentsListProviderConfigModel>(
                json, AutomationDeserializationConfiguration.ModelSettings);
            ArchiveFileProviderConfigModel archiveFileProviderBuilder
                = (ArchiveFileProviderConfigModel)archiveFileContentsListProviderBuilder.SourceFile;
            archiveFileProviderBuilder.SourceFile = new TestFileProviderConfigModel { FilePath = this.zipFile };
            var archiveFileContentsListProvider
                = archiveFileContentsListProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();
            providerContext.Setup(s => s.GetDebugContextForProviders(It.IsAny<string>())).Returns(Task.FromResult(new JObject()));

            // Act
            var dataList = (await archiveFileContentsListProvider.Resolve(providerContext.Object)).GetValueOrThrowIfFailed();

            // Assert
            dataList.Should().NotBeNull();
            List<ArchiveEntryModel> list = dataList.ToList().Select(x => x as ArchiveEntryModel).ToList();
            list.Count.Should().Be(9);

            list[0].IsFile.Should().BeTrue();
            list[0].IsFolder.Should().BeFalse();
            list[0].Name.Should().Be("cgu_500px.png");
            list[0].Path.Should().Be("cgu_500px.png");

            list[1].IsFile.Should().BeTrue();
            list[1].IsFolder.Should().BeFalse();
            list[1].Name.Should().Be("iag.png");
            list[1].Path.Should().Be("iag.png");

            list[2].IsFile.Should().BeFalse();
            list[2].IsFolder.Should().BeTrue();
            list[2].Name.Should().Be("destination-1");
            list[2].Path.Should().Be("destination-1/");

            list[3].IsFile.Should().BeFalse();
            list[3].IsFolder.Should().BeTrue();
            list[3].Name.Should().Be("folder1");
            list[3].Path.Should().Be("destination-1/folder1/");

            list[4].IsFile.Should().BeTrue();
            list[4].IsFolder.Should().BeFalse();
            list[4].Name.Should().Be("saml-logo.png");
            list[4].Path.Should().Be("destination-1/folder1/saml-logo.png");

            list[5].IsFile.Should().BeFalse();
            list[5].IsFolder.Should().BeTrue();
            list[5].Name.Should().Be("destination-2");
            list[5].Path.Should().Be("destination-2/");

            list[6].IsFile.Should().BeFalse();
            list[6].IsFolder.Should().BeTrue();
            list[6].Name.Should().Be("folder2");
            list[6].Path.Should().Be("destination-2/folder2/");

            list[7].IsFile.Should().BeFalse();
            list[7].IsFolder.Should().BeTrue();
            list[7].Name.Should().Be("sub");
            list[7].Path.Should().Be("destination-2/folder2/sub/");

            list[8].IsFile.Should().BeTrue();
            list[8].IsFolder.Should().BeFalse();
            list[8].Name.Should().Be("Customer view on laptop.png");
            list[8].Path.Should().Be("destination-2/folder2/sub/Customer view on laptop.png");
        }

        [Fact]
        public async Task MoveFolderOperation_MovesFolderToRoot_WhenNoDestinationFolderSpecified()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""outputFilename"": ""test.zip"",
                        ""operations"": [
                            {
                                ""addFolder"": {
                                    ""folderName"": ""test-folder-1"",
                                    ""destinationFolderPath"": ""sub""
                                }
                            },
                            {
                                ""moveFolder"": {
                                    ""sourceFolderPath"": ""sub/test-folder-1/""
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
            list[0].Name.Should().Be("sub");
            list[0].Path.Should().Be("sub/");

            list[1].IsFile.Should().BeFalse();
            list[1].IsFolder.Should().BeTrue();
            list[1].Name.Should().Be("test-folder-1");
            list[1].Path.Should().Be("test-folder-1/");
        }

        [Fact]
        public async Task MoveFolderOperation_ReplacesFileAsSpecified_WhenDestinationFolderContainsFileWithSameName()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""outputFilename"": ""test.zip"",
                        ""operations"": [
                            {
                                ""addFolder"": {
                                    ""folderName"": ""source-folder""
                                }
                            },
                            {
                                ""addFolder"": {
                                    ""folderName"": ""destination-folder""
                                }
                            },
                            {
                                ""addFile"": {
                                    ""destinationFileName"": ""target"",
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""saml-logo.png""
                                        }
                                    },
                                    ""destinationFolderPath"": ""destination-folder"",
                                }
                            },
                            {
                                ""moveFolder"": {
                                    ""sourceFolderPath"": ""source-folder"",
                                    ""destinationFolderName"": ""target"",
                                    ""destinationFolderPath"": ""destination-folder"",
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
            ((AddFileOperationConfigModel)operationBuilders[2]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.samlLogoFile };
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
            list[0].Name.Should().Be("destination-folder");
            list[0].Path.Should().Be("destination-folder/");

            list[1].IsFile.Should().BeFalse();
            list[1].IsFolder.Should().BeTrue();
            list[1].Name.Should().Be("target");
            list[1].Path.Should().Be("destination-folder/target/");
        }

        [Fact]
        public async Task MoveFolderOperation_ReplacesFolderAsSpecified_WhenDestinationFolderContainsFolderWithSameName()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""outputFilename"": ""test.zip"",
                        ""operations"": [
                            {
                                ""addFolder"": {
                                    ""folderName"": ""source-folder""
                                }
                            },
                            {
                                ""addFolder"": {
                                    ""folderName"": ""destination-folder/target""
                                }
                            },
                            {
                                ""addFile"": {
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""saml-logo.png""
                                        }
                                    },
                                    ""destinationFolderPath"": ""destination-folder/target"",
                                }
                            },
                            {
                                ""moveFolder"": {
                                    ""sourceFolderPath"": ""source-folder"",
                                    ""destinationFolderName"": ""target"",
                                    ""destinationFolderPath"": ""destination-folder"",
                                    ""whenDestinationFolderContainsFolderWithSameName"": ""replace""
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
            ((AddFileOperationConfigModel)operationBuilders[2]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.samlLogoFile };
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
            list[0].Name.Should().Be("destination-folder");
            list[0].Path.Should().Be("destination-folder/");

            list[1].IsFile.Should().BeFalse();
            list[1].IsFolder.Should().BeTrue();
            list[1].Name.Should().Be("target");
            list[1].Path.Should().Be("destination-folder/target/");
        }

        [Fact]
        public async Task MoveFolderOperation_MergesFolderAsSpecified_WhenDestinationFolderContainsFolderWithSameName()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""outputFilename"": ""test.zip"",
                        ""operations"": [
                            {
                                ""addFolder"": {
                                    ""folderName"": ""source-folder""
                                }
                            },
                            {
                                ""addFile"": {
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""iag.png""
                                        }
                                    },
                                    ""destinationFolderPath"": ""source-folder"",
                                }
                            },
                            {
                                ""addFolder"": {
                                    ""folderName"": ""destination-folder/target""
                                }
                            },
                            {
                                ""addFile"": {
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""saml-logo.png""
                                        }
                                    },
                                    ""destinationFolderPath"": ""destination-folder/target"",
                                }
                            },
                            {
                                ""moveFolder"": {
                                    ""sourceFolderPath"": ""source-folder"",
                                    ""destinationFolderName"": ""target"",
                                    ""destinationFolderPath"": ""destination-folder"",
                                    ""whenDestinationFolderContainsFolderWithSameName"": ""merge""
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
            ((AddFileOperationConfigModel)operationBuilders[1]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.iagLogoFile };
            ((AddFileOperationConfigModel)operationBuilders[3]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.samlLogoFile };
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
            list[0].Name.Should().Be("destination-folder");
            list[0].Path.Should().Be("destination-folder/");

            list[1].IsFile.Should().BeFalse();
            list[1].IsFolder.Should().BeTrue();
            list[1].Name.Should().Be("target");
            list[1].Path.Should().Be("destination-folder/target/");

            list[2].IsFile.Should().BeTrue();
            list[2].IsFolder.Should().BeFalse();
            list[2].Name.Should().Be("saml-logo.png");
            list[2].Path.Should().Be("destination-folder/target/saml-logo.png");

            list[3].IsFile.Should().BeTrue();
            list[3].IsFolder.Should().BeFalse();
            list[3].Name.Should().Be("iag.png");
            list[3].Path.Should().Be("destination-folder/target/iag.png");
        }

        [Fact]
        public async Task MoveFolderOperation_RaisesErrorByDefault_WhenDestinationFolderContainsFolderWithSameNameAsync()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""outputFilename"": ""test.zip"",
                        ""operations"": [
                            {
                                ""addFolder"": {
                                    ""folderName"": ""source-folder""
                                }
                            },
                            {
                                ""addFile"": {
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""iag.png""
                                        }
                                    },
                                    ""destinationFolderPath"": ""source-folder"",
                                }
                            },
                            {
                                ""addFolder"": {
                                    ""folderName"": ""destination-folder/target""
                                }
                            },
                            {
                                ""addFile"": {
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""saml-logo.png""
                                        }
                                    },
                                    ""destinationFolderPath"": ""destination-folder/target"",
                                }
                            },
                            {
                                ""moveFolder"": {
                                    ""sourceFolderPath"": ""source-folder"",
                                    ""destinationFolderName"": ""target"",
                                    ""destinationFolderPath"": ""destination-folder"",
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
            ((AddFileOperationConfigModel)operationBuilders[1]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.iagLogoFile };
            ((AddFileOperationConfigModel)operationBuilders[3]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.samlLogoFile };
            var archiveFileContentsListProvider
                = archiveFileContentsListProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();
            providerContext.Setup(s => s.GetDebugContextForProviders(It.IsAny<string>())).Returns(Task.FromResult(new JObject()));

            // Act
            Func<Task> func = async () => await archiveFileContentsListProvider.Resolve(providerContext.Object);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be(
                "automation.archive.move.folder.operation.destination.folder.already.exists");
        }
    }
}
