// <copyright file="AddFileOperationTests.cs" company="uBind">
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

    public class AddFileOperationTests
    {
        private readonly string iagLogoFile = "Automations\\Providers\\File\\TestFiles\\iag.png";
        private readonly string samlLogoFile = "Automations\\Providers\\File\\TestFiles\\saml-logo.png";
        private readonly string randomTextFile = "Automations\\Providers\\File\\TestFiles\\random-text.txt";
        private readonly Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();

        public AddFileOperationTests()
        {
            this.serviceProviderMock.Setup(s => s.GetService(typeof(IClock))).Returns(new TestClock());
        }

        [Fact]
        public async Task AddFileOperation_AddsFiles_WithDifferentCompressionLevels()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""outputFilename"": ""test.zip"",
                        ""operations"": [
                            {
                                ""addFile"": {
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""random-text.txt""
                                        }
                                    },
                                    ""compressionLevel"": ""noCompression"",
                                    ""destinationFileName"": ""noCompression.txt"",
                                }
                            },
                            {
                                ""addFile"": {
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""random-text.txt""
                                        }
                                    },
                                    ""compressionLevel"": ""fastest"",
                                    ""destinationFileName"": ""fastest.txt"",
                                }
                            },
                            {
                                ""addFile"": {
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""random-text.txt""
                                        }
                                    },
                                    ""compressionLevel"": ""optimal"",
                                    ""destinationFileName"": ""optimal.txt"",
                                }
                            },
                            {
                                ""addFile"": {
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""random-text.txt""
                                        }
                                    },
                                    ""compressionLevel"": ""smallestSize"",
                                    ""destinationFileName"": ""smallestSize.txt"",
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
            operationBuilders.ForEach(ob => ((AddFileOperationConfigModel)ob).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.randomTextFile });
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

            list[0].IsFile.Should().BeTrue();
            list[0].IsFolder.Should().BeFalse();
            list[0].Name.Should().Be("noCompression.txt");

            list[1].IsFile.Should().BeTrue();
            list[1].IsFolder.Should().BeFalse();
            list[1].Name.Should().Be("fastest.txt");

            list[2].IsFile.Should().BeTrue();
            list[2].IsFolder.Should().BeFalse();
            list[2].Name.Should().Be("optimal.txt");

            list[3].IsFile.Should().BeTrue();
            list[3].IsFolder.Should().BeFalse();
            list[3].Name.Should().Be("smallestSize.txt");

            // check the size differences make sense
            list[0].CompressedSizeBytes.Value.Should().BeGreaterThan(list[1].CompressedSizeBytes.Value);
            list[1].CompressedSizeBytes.Value.Should().BeGreaterThan(list[2].CompressedSizeBytes.Value);

            // TODO: Until we upgrade to .NET 6, Optimal and SmallestSize will be the same. After we finish the upgrade
            // we can change BeGreaterOrEqualTo to BeGreaterThan.
            list[2].CompressedSizeBytes.Value.Should().BeGreaterOrEqualTo(list[3].CompressedSizeBytes.Value);
        }

        [Fact]
        public async Task AddFileOperation_CreatesFolderByDefault_WhenDestinationFolderNotFound()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""outputFilename"": ""test.zip"",
                        ""operations"": [
                            {
                                ""addFile"": {
                                    ""destinationFileName"": ""test-1.png"",
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""saml-logo.png""
                                        }
                                    },
                                    ""destinationFolderPath"": ""test-folder-1/"",
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
            list[0].Name.Should().Be("test-folder-1");
            list[0].Path.Should().Be("test-folder-1/");

            list[1].IsFile.Should().BeTrue();
            list[1].IsFolder.Should().BeFalse();
            list[1].Name.Should().Be("test-1.png");
            list[1].Path.Should().Be("test-folder-1/test-1.png");
            list[1].SizeBytes.Should().Be(5034);
        }

        [Fact]
        public async Task AddFileOperation_RaisesErrorByDefault_WhenEntryExistsAsync()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""outputFilename"": ""test.zip"",
                        ""operations"": [
                            {
                                ""addFile"": {
                                    ""destinationFileName"": ""test-1.png"",
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""saml-logo.png""
                                        }
                                    },
                                    ""destinationFolderPath"": ""test-folder-1/"",
                                }
                            },
                            {
                                ""addFile"": {
                                    ""destinationFileName"": ""test-1.png"",
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""iag.png""
                                        }
                                    },
                                    ""destinationFolderPath"": ""test-folder-1/"",
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
                = new TestFileProviderConfigModel { FilePath = this.samlLogoFile };
            ((AddFileOperationConfigModel)operationBuilders[1]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.iagLogoFile };
            var archiveFileContentsListProvider
                = archiveFileContentsListProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();
            providerContext.Setup(s => s.GetDebugContextForProviders(It.IsAny<string>())).Returns(Task.FromResult(new JObject()));

            // Act
            Func<Task> func = async () => await archiveFileContentsListProvider.Resolve(providerContext.Object);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be(
                "automation.archive.add.file.operation.entry.already.exists");
        }

        [Fact]
        public async Task AddFileOperation_ReplacesEntryAsSpecified_WhenFileExists()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""outputFilename"": ""test.zip"",
                        ""operations"": [
                            {
                                ""addFile"": {
                                    ""destinationFileName"": ""test-1.png"",
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""saml-logo.png""
                                        }
                                    },
                                    ""destinationFolderPath"": ""test-folder-1/"",
                                }
                            },
                            {
                                ""addFile"": {
                                    ""destinationFileName"": ""test-1.png"",
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""iag.png""
                                        }
                                    },
                                    ""destinationFolderPath"": ""test-folder-1/"",
                                    ""whenDestinationFolderContainsEntryWithSameName"": ""replace""
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
                = new TestFileProviderConfigModel { FilePath = this.samlLogoFile };
            ((AddFileOperationConfigModel)operationBuilders[1]).SourceFile
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

            list[1].IsFile.Should().BeTrue();
            list[1].IsFolder.Should().BeFalse();
            list[1].Name.Should().Be("test-1.png");
            list[1].Path.Should().Be("test-folder-1/test-1.png");
            list[1].SizeBytes.Should().Be(16624);
        }

        [Fact]
        public async Task AddFileOperation_ReplacesEntryAsSpecified_WhenFolderExists()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""outputFilename"": ""test.zip"",
                        ""operations"": [
                            {
                                ""addFolder"": {
                                    ""folderName"": ""test-2"",
                                    ""destinationFolderPath"": ""test-folder-1/test-1"",
                                }
                            },
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
                                    ""destinationFolderPath"": ""test-folder-1/"",
                                    ""whenDestinationFolderContainsEntryWithSameName"": ""replace""
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

            list[1].IsFile.Should().BeTrue();
            list[1].IsFolder.Should().BeFalse();
            list[1].Name.Should().Be("test-1");
            list[1].Path.Should().Be("test-folder-1/test-1");
            list[1].SizeBytes.Should().Be(16624);
        }

        [Fact]
        public async Task AddFileOperation_AddsFileToRoot_WhenNoDestinationFolderSpecified()
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
            list.Count.Should().Be(1);

            list[0].IsFile.Should().BeTrue();
            list[0].IsFolder.Should().BeFalse();
            list[0].Name.Should().Be("test-1");
            list[0].Path.Should().Be("test-1");
        }
    }
}
