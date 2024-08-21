// <copyright file="ArchiveFileProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.File
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

    public class ArchiveFileProviderTests
    {
        private readonly string cguLogoFile = "Automations\\Providers\\File\\TestFiles\\cgu_500px.png";
        private readonly string iagLogoFile = "Automations\\Providers\\File\\TestFiles\\iag.png";
        private readonly string samlLogoFile = "Automations\\Providers\\File\\TestFiles\\saml-logo.png";
        private readonly string zipFile = "Automations\\Providers\\List\\TestFiles\\zipped-up-stuff.zip";
        private readonly Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();

        public ArchiveFileProviderTests()
        {
            this.serviceProviderMock.Setup(s => s.GetService(typeof(IClock))).Returns(new TestClock());
        }

        [Fact]
        public async Task ArchiveFileProvider_AddFileOperation_HandlesProblemsWithDecisionsCorrectly()
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
                                ""addFile"": {
                                    ""destinationFileName"": ""test-0.png"",
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""cgu_500px.png""
                                        }
                                    }
                                }
                            },
                            {
                                ""addFile"": {
                                    ""destinationFileName"": ""test-1.png"",
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""cgu_500px.png""
                                        }
                                    },
                                    ""destinationFolderPath"": ""test-folder-1/"",
                                    ""runCondition"": true,
                                    ""whenDestinationFolderNotFound"": ""create""
                                }
                            },
                            {
                                ""addFile"": {
                                    ""destinationFileName"": ""test-2.png"",
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""cgu_500px.png""
                                        }
                                    },
                                    ""destinationFolderPath"": ""test-folder-1/""
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
                                    ""runCondition"": {
                                        ""textIsEqualToCondition"": {
                                            ""text"": ""abc"",
                                            ""isEqualTo"": ""abc""
                                        }
                                    },
                                    ""whenDestinationFolderContainsEntryWithSameName"": ""replace""
                                }
                            },
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
                                    ""whenDestinationFolderContainsEntryWithSameName"": ""skip""
                                }
                            },
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
                                    ""runCondition"": {
                                        ""textIsEqualToCondition"": {
                                            ""text"": ""abc"",
                                            ""isEqualTo"": ""abc1""
                                        }
                                    },
                                    ""whenDestinationFolderContainsEntryWithSameName"": ""replace""
                                }
                            },
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
                                    ""runCondition"": false,
                                    ""whenDestinationFolderContainsEntryWithSameName"": ""replace""
                                }
                            },
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
                                    ""destinationFolderPath"": ""test-folder-2/"",
                                    ""runCondition"": false,
                                    ""whenDestinationFolderNotFound"": ""skip""
                                }
                            },
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
                                    ""destinationFolderPath"": ""test-folder-2/"",
                                    ""whenDestinationFolderNotFound"": ""end""
                                }
                            },
                            {
                                ""addFile"": {
                                    ""destinationFileName"": ""test-3.png"",
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""saml-logo.png""
                                        }
                                    },
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
            archiveFileProviderBuilder.SourceFile = new TestFileProviderConfigModel { FilePath = this.zipFile };
            var operationBuilders = archiveFileProviderBuilder.Operations.ToList();
            ((AddFileOperationConfigModel)operationBuilders[0]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.cguLogoFile };
            ((AddFileOperationConfigModel)operationBuilders[1]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.cguLogoFile };
            ((AddFileOperationConfigModel)operationBuilders[2]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.cguLogoFile };
            ((AddFileOperationConfigModel)operationBuilders[3]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.iagLogoFile };
            ((AddFileOperationConfigModel)operationBuilders[4]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.samlLogoFile };
            ((AddFileOperationConfigModel)operationBuilders[5]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.samlLogoFile };
            ((AddFileOperationConfigModel)operationBuilders[6]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.samlLogoFile };
            ((AddFileOperationConfigModel)operationBuilders[7]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.samlLogoFile };
            ((AddFileOperationConfigModel)operationBuilders[8]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.samlLogoFile };
            ((AddFileOperationConfigModel)operationBuilders[9]).SourceFile
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
            list.Count.Should().Be(11);

            list[0].IsFile.Should().BeTrue();
            list[0].IsFolder.Should().BeFalse();
            list[0].Name.Should().Be("cgu_500px.png");
            list[0].Path.Should().Be("cgu_500px.png");

            list[1].IsFile.Should().BeFalse();
            list[1].IsFolder.Should().BeTrue();
            list[1].Name.Should().Be("folder1");
            list[1].Path.Should().Be("folder1/");

            list[2].IsFile.Should().BeTrue();
            list[2].IsFolder.Should().BeFalse();
            list[2].Name.Should().Be("saml-logo.png");
            list[2].Path.Should().Be("folder1/saml-logo.png");

            list[3].IsFile.Should().BeFalse();
            list[3].IsFolder.Should().BeTrue();
            list[3].Name.Should().Be("folder2");
            list[3].Path.Should().Be("folder2/");

            list[4].IsFile.Should().BeFalse();
            list[4].IsFolder.Should().BeTrue();
            list[4].Name.Should().Be("sub");
            list[4].Path.Should().Be("folder2/sub/");

            list[5].IsFile.Should().BeTrue();
            list[5].IsFolder.Should().BeFalse();
            list[5].Name.Should().Be("Customer view on laptop.png");
            list[5].Path.Should().Be("folder2/sub/Customer view on laptop.png");

            list[6].IsFile.Should().BeTrue();
            list[6].IsFolder.Should().BeFalse();
            list[6].Name.Should().Be("iag.png");
            list[6].Path.Should().Be("iag.png");

            list[7].IsFile.Should().BeTrue();
            list[7].IsFolder.Should().BeFalse();
            list[7].Name.Should().Be("test-0.png");
            list[7].Path.Should().Be("test-0.png");

            list[8].IsFile.Should().BeFalse();
            list[8].IsFolder.Should().BeTrue();
            list[8].Name.Should().Be("test-folder-1");
            list[8].Path.Should().Be("test-folder-1/");

            list[9].IsFile.Should().BeTrue();
            list[9].IsFolder.Should().BeFalse();
            list[9].Name.Should().Be("test-2.png");
            list[9].Path.Should().Be("test-folder-1/test-2.png");

            // test-1 is last because it was replaced last
            list[10].IsFile.Should().BeTrue();
            list[10].IsFolder.Should().BeFalse();
            list[10].Name.Should().Be("test-1.png");
            list[10].Path.Should().Be("test-folder-1/test-1.png");
        }

        [Fact]
        public async Task ArchiveFileProvider_OperationsStop_WhenDecisionIsEnd()
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
                                ""addFile"": {
                                    ""destinationFileName"": ""test-0.png"",
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""cgu_500px.png""
                                        }
                                    }
                                }
                            },
                            {
                                ""addFile"": {
                                    ""destinationFileName"": ""test-0.png"",
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""cgu_500px.png""
                                        }
                                    },
                                    ""whenDestinationFolderContainsEntryWithSameName"": ""end""
                                }
                            },
                            {
                                ""addFile"": {
                                    ""destinationFileName"": ""test-1.png"",
                                    ""sourceFile"": {
                                        ""productFile"": {
                                            ""repository"": ""quote"",
                                            ""visibility"": ""private"",
                                            ""filePath"": ""cgu_500px.png""
                                        }
                                    }
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
            archiveFileProviderBuilder.SourceFile = new TestFileProviderConfigModel { FilePath = this.zipFile };
            var operationBuilders = archiveFileProviderBuilder.Operations.ToList();
            ((AddFileOperationConfigModel)operationBuilders[0]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.cguLogoFile };
            ((AddFileOperationConfigModel)operationBuilders[1]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.cguLogoFile };
            ((AddFileOperationConfigModel)operationBuilders[2]).SourceFile
                = new TestFileProviderConfigModel { FilePath = this.cguLogoFile };

            var archiveFileContentsListProvider
                = archiveFileContentsListProviderBuilder.Build(this.serviceProviderMock.Object);
            var providerContext = new Mock<IProviderContext>();
            providerContext.Setup(s => s.GetDebugContextForProviders(It.IsAny<string>())).Returns(Task.FromResult(new JObject()));

            // Act
            var dataList = (await archiveFileContentsListProvider.Resolve(providerContext.Object)).GetValueOrThrowIfFailed();

            // Assert
            dataList.Should().NotBeNull();
            List<ArchiveEntryModel> list = dataList.ToList().Select(x => x as ArchiveEntryModel).ToList();
            list.Count.Should().Be(8);

            list[0].IsFile.Should().BeTrue();
            list[0].IsFolder.Should().BeFalse();
            list[0].Name.Should().Be("cgu_500px.png");
            list[0].Path.Should().Be("cgu_500px.png");

            list[1].IsFile.Should().BeFalse();
            list[1].IsFolder.Should().BeTrue();
            list[1].Name.Should().Be("folder1");
            list[1].Path.Should().Be("folder1/");

            list[2].IsFile.Should().BeTrue();
            list[2].IsFolder.Should().BeFalse();
            list[2].Name.Should().Be("saml-logo.png");
            list[2].Path.Should().Be("folder1/saml-logo.png");

            list[3].IsFile.Should().BeFalse();
            list[3].IsFolder.Should().BeTrue();
            list[3].Name.Should().Be("folder2");
            list[3].Path.Should().Be("folder2/");

            list[4].IsFile.Should().BeFalse();
            list[4].IsFolder.Should().BeTrue();
            list[4].Name.Should().Be("sub");
            list[4].Path.Should().Be("folder2/sub/");

            list[5].IsFile.Should().BeTrue();
            list[5].IsFolder.Should().BeFalse();
            list[5].Name.Should().Be("Customer view on laptop.png");
            list[5].Path.Should().Be("folder2/sub/Customer view on laptop.png");

            list[6].IsFile.Should().BeTrue();
            list[6].IsFolder.Should().BeFalse();
            list[6].Name.Should().Be("iag.png");
            list[6].Path.Should().Be("iag.png");

            list[7].IsFile.Should().BeTrue();
            list[7].IsFolder.Should().BeFalse();
            list[7].Name.Should().Be("test-0.png");
            list[7].Path.Should().Be("test-0.png");
        }

        [Fact]
        public async Task ArchiveFileProvider_AutoDetectsZipFromOutputFilename_WhenCreatingAZipFileAndFormatNotSpecified()
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
        }

        [Fact]
        public async Task ArchiveFileProvider_RaisesError_WhenCreatingAZipFileAndFormatNotSpecifiedAndUnknownExtensionAsync()
        {
            // Arrange
            var json = @"{
                ""sourceFile"": {
                    ""archiveFile"": {
                        ""outputFilename"": ""my-test-zip"",
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
            Func<Task> func = async () => await archiveFileContentsListProvider.Resolve(providerContext.Object);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).And.Error.Code.Should().Be(
                "automation.archive.format.not.specified");
        }

        [Fact]
        public async Task ArchiveFileProvider_SkipsOperations_WhenRunConditionEvaluatesToFalse()
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
                                    ""runCondition"": false
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
            list.Count.Should().Be(0);
        }

        [Fact]
        public async Task ArchiveFileProvider_RunsOperations_WhenRunConditionEvaluatesToTrue()
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
                                    ""runCondition"": true
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
