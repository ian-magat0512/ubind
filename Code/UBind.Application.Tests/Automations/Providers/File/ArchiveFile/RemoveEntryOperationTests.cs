// <copyright file="RemoveEntryOperationTests.cs" company="uBind">
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

    public class RemoveEntryOperationTests
    {
        private readonly string iagLogoFile = "Automations\\Providers\\File\\TestFiles\\iag.png";
        private readonly string samlLogoFile = "Automations\\Providers\\File\\TestFiles\\saml-logo.png";
        private readonly Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();

        public RemoveEntryOperationTests()
        {
            this.serviceProviderMock.Setup(s => s.GetService(typeof(IClock))).Returns(new TestClock());
        }

        [Fact]
        public async Task RemoveEntryOperation_RemovesFile_WhenFileExists()
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
                                            ""filePath"": ""saml-logo.png""
                                        }
                                    }
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
                                    ""destinationFolderPath"": ""test-folder-1/"",
                                }
                            },
                            {
                                ""removeEntry"": {
                                    ""path"": ""test-folder-1/iag.png""
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

            list[0].IsFile.Should().BeTrue();
            list[0].IsFolder.Should().BeFalse();
            list[0].Name.Should().Be("saml-logo.png");
            list[0].Path.Should().Be("saml-logo.png");

            list[1].IsFile.Should().BeFalse();
            list[1].IsFolder.Should().BeTrue();
            list[1].Name.Should().Be("test-folder-1");
            list[1].Path.Should().Be("test-folder-1/");
        }

        [Fact]
        public async Task RemoveEntryOperation_RemovesFolder_WhenFolderExists()
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
                                            ""filePath"": ""saml-logo.png""
                                        }
                                    }
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
                                    ""destinationFolderPath"": ""test-folder-1/"",
                                }
                            },
                            {
                                ""removeEntry"": {
                                    ""path"": ""test-folder-1""
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
            list.Count.Should().Be(1);

            list[0].IsFile.Should().BeTrue();
            list[0].IsFolder.Should().BeFalse();
            list[0].Name.Should().Be("saml-logo.png");
            list[0].Path.Should().Be("saml-logo.png");
        }

        [Fact]
        public async Task RemoveEntryOperation_RaisesErrorByDefault_WhenFileDoesNotExistAsync()
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
                                            ""filePath"": ""saml-logo.png""
                                        }
                                    }
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
                                    ""destinationFolderPath"": ""test-folder-1/"",
                                }
                            },
                            {
                                ""removeEntry"": {
                                    ""path"": ""test-folder-1/a-non-existent-logo.png""
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
                "automation.archive.remove.entry.operation.entry.not.found");
        }
    }
}
