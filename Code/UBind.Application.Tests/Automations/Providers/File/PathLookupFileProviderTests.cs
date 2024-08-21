////// <copyright file="PathLookupFileProviderTests.cs" company="uBind">
////// Copyright (c) uBind. All rights reserved.
////// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765


////namespace UBind.Application.Tests.Automations.Providers.File
////{
////    using System;
////    using System.Collections.Generic;
////    using System.Collections.ObjectModel;
////    using System.Net.Http;
////    using System.Text;
////    using System.Threading.Tasks;
////    using FluentAssertions;
////    using Microsoft.Extensions.Primitives;
////    using Moq;
////    using MorseCode.ITask;
////    using Newtonsoft.Json;
////    using Newtonsoft.Json.Linq;
////    using UBind.Application.Automation;
////    using UBind.Application.Automation.Actions;
////    using UBind.Application.Automation.Data;
////    using UBind.Application.Automation.Error;
////    using UBind.Application.Automation.Providers;
////    using UBind.Application.Automation.Providers.File;
////    using UBind.Application.Automation.Providers.Object;
////    using UBind.Application.Tests.Automations.Fakes;
////    using UBind.Domain;
////    using UBind.Domain.Exceptions;
////    using UBind.Domain.ReadWriteModel;
////    using UBind.Domain.Repositories;
////    using UBind.Domain.Tests.Fakes;
////    using Xunit;
////    using Action = UBind.Application.Automation.Actions.Action;

////    public class PathLookupFileProviderTests
////    {
////        private readonly Mock<IFileContentRepository> fileContentRepository = new Mock<IFileContentRepository>();
////        private readonly Mock<IServiceProvider> dependencyProvider = new Mock<IServiceProvider>();
////        private readonly Mock<IObjectProvider> objectProvider = new Mock<IObjectProvider>();

////        private readonly string json =
////            @"{
////            ""path"": ""/questions/ratingPrimary/testAttachment"",
////            ""dataObject"": {
////                ""entityObject"": {
////                    ""dynamicEntity"": {
////                        ""entityType"": {
////                            ""objectPathLookupText"": ""trigger.httpRequest.getParameters.entityType""
////                        },
////                        ""entityId"": {
////                            ""objectPathLookupText"": ""trigger.httpRequest.getParameters.entityId""
////                        }
////                    }
////                }
////                }
////            }";

////        [Fact]
////        public async Task PathLookupFileProvider_ShouldReturnQuoteFileAttachment_WhenQuoteIdProvided()
////        {
////            // Arrange
////            var entityId = Guid.NewGuid();
////            AutomationData automationData = CreateAutomationData("quote", entityId);

////            var configModel =
////                JsonConvert.DeserializeObject<PathLookupFileProviderConfigModel>(
////                    this.json, AutomationDeserializationConfiguration.ModelSettings);

////            var provider = configModel.Build(this.dependencyProvider.Object);

////            var attachmentId = Guid.NewGuid();
////            var filename = "PoliceReport.pdf";

////            var questions = new JObject(
////                new JProperty("ratingPrimary", new JObject(
////                    new JProperty("testAttachment", $"{filename}:application/pdf:{attachmentId}::"))));

////            var dataObject = new Dictionary<string, object>()
////            {
////                { "id", Guid.NewGuid() },
////                { "questions", questions },
////                { "environment", Domain.DeploymentEnvironment.Development },
////            };

////            this.objectProvider
////               .Setup(o => o.Resolve(It.IsAny<ProviderContext>()))
////               .Returns(Task.FromResult(new Data<object>(new ReadOnlyDictionary<string, object>(dataObject))).AsITask());

////            var byteContent = Encoding.ASCII.GetBytes("Sample quote text");
////            this.fileContentRepository
////                .Setup(f => f.GetFileContent(It.IsAny<Guid>(), It.IsAny<Guid>()))
////                .Returns(FileContent.CreateFromBytes(Guid.NewGuid(), Guid.NewGuid(), byteContent));

////            var path = configModel.Path.Build(this.dependencyProvider.Object);

////            // Act
////            var pathLookupFileProvider =
////                this.CreateProvider(path, null, this.objectProvider.Object, this.fileContentRepository.Object);

////            var fileInfo = await pathLookupFileProvider.Resolve(new ProviderContext(automationData));

////            // Assert
////            fileInfo.DataValue.Should().NotBeNull();
////            fileInfo.DataValue.FileName.ToString().Should().Be(filename);
////            fileInfo.DataValue.Content.Should().BeEquivalentTo(byteContent);
////        }

////        [Fact]
////        public async Task PathLookupFileProvider_ShouldReturnFile_WhenExistsInAutomationData()
////        {
////            // Arrange
////            var fileContent = Encoding.UTF8.GetBytes("This is the file contents");
////            var expectedValue = new FileInfo("myfile.txt", fileContent);
////            var fileProvider = new StaticProvider<Data<FileInfo>>(expectedValue);
////            var serviceProvider = new Mock<IServiceProvider>().AddLoggers().Object;
////            var propertyNameProvider = new StaticProvider<Data<string>>("myFile");
////            var setVariableAction = new SetVariableAction(
////                "Set variable",
////                "setPropertyValue",
////                string.Empty,
////                false,
////                null,
////                Enumerable.Empty<ErrorCondition>(),
////                Enumerable.Empty<ErrorCondition>(),
////                Enumerable.Empty<Action>(),
////                propertyNameProvider,
////                fileProvider,
////                new TestClock(),
////                null,
////                null,
////                serviceProvider);
////            var automationData = await MockAutomationData.CreateWithHttpTrigger();
////            var actionData = setVariableAction.CreateActionData();
////            automationData.AddActionData(actionData);
////            await setVariableAction.Execute(new ProviderContext(automationData), actionData);
////            var path = new StaticProvider<Data<string>>("/variables/myFile");

////            // Act
////            var pathLookupFileProvider =
////                this.CreateProvider(path, null, null, this.fileContentRepository.Object);

////            var fileInfo = await pathLookupFileProvider.Resolve(new ProviderContext(automationData));

////            // Assert
////            fileInfo.DataValue.Should().NotBeNull();
////            fileInfo.DataValue.FileName.ToString().Should().Be("myfile.txt");
////            fileInfo.DataValue.Content.Should().BeEquivalentTo(fileContent);
////        }

////        [Fact]
////        public async Task PathLookupFileProvider_ShouldReturnFileAttachment_WhenPolicyIdProvided()
////        {
////            // Arrange
////            var entityId = Guid.NewGuid();
////            AutomationData automationData = CreateAutomationData("policy", entityId);

////            var configModel =
////                JsonConvert.DeserializeObject<PathLookupFileProviderConfigModel>(
////                    this.json, AutomationDeserializationConfiguration.ModelSettings);

////            var provider = configModel.Build(this.dependencyProvider.Object);

////            var attachmentId = Guid.NewGuid();
////            var filename = "PoliceReport.pdf";

////            var questions = new JObject(
////                new JProperty("ratingPrimary", new JObject(
////                    new JProperty("testAttachment", $"{filename}:application/pdf:{attachmentId}::"))));

////            var dataObject = new Dictionary<string, object>()
////            {
////                { "id", Guid.NewGuid() },
////                { "questions", questions },
////                { "environment", Domain.DeploymentEnvironment.Development },
////            };

////            this.objectProvider
////               .Setup(o => o.Resolve(It.IsAny<ProviderContext>()))
////               .Returns(Task.FromResult(new Data<object>(new ReadOnlyDictionary<string, object>(dataObject))).AsITask());

////            var byteContent = Encoding.ASCII.GetBytes("Sample quote text");
////            this.fileContentRepository
////                .Setup(f => f.GetFileContent(It.IsAny<Guid>(), It.IsAny<Guid>()))
////                .Returns(FileContent.CreateFromBytes(Guid.NewGuid(), Guid.NewGuid(), byteContent));

////            var path = configModel.Path.Build(this.dependencyProvider.Object);

////            // Act
////            var pathLookupFileProvider =
////                this.CreateProvider(path, null, this.objectProvider.Object, this.fileContentRepository.Object);

////            var fileInfo = await pathLookupFileProvider.Resolve(new ProviderContext(automationData));

////            // Assert
////            fileInfo.DataValue.Should().NotBeNull();
////            fileInfo.DataValue.FileName.ToString().Should().Be(filename);
////            fileInfo.DataValue.Content.Should().BeEquivalentTo(byteContent);
////        }

////        [Fact]
////        public async Task PathLookupFileProvider_ShouldReturnClaimFileAttachment_WhenClaimIdProvided()
////        {
////            // Arrange
////            var entityId = Guid.NewGuid();
////            AutomationData automationData = CreateAutomationData("claim", entityId);

////            var configModel =
////                JsonConvert.DeserializeObject<PathLookupFileProviderConfigModel>(
////                    this.json, AutomationDeserializationConfiguration.ModelSettings);

////            var provider = configModel.Build(this.dependencyProvider.Object);

////            var attachmentId = Guid.NewGuid();
////            var filename = "ClaimPoliceReport.pdf";

////            var questions = new JObject(
////                new JProperty("ratingPrimary", new JObject(
////                    new JProperty("testAttachment", $"{filename}:application/pdf:{attachmentId}::"))));

////            var dataObject = new Dictionary<string, object>()
////            {
////                { "id", Guid.NewGuid() },
////                { "questions", questions },
////                { "environment", Domain.DeploymentEnvironment.Development },
////            };

////            this.objectProvider
////               .Setup(o => o.Resolve(It.IsAny<ProviderContext>()))
////               .Returns(Task.FromResult(new Data<object>(new ReadOnlyDictionary<string, object>(dataObject))).AsITask());

////            var byteContent = Encoding.ASCII.GetBytes("Sample claim text");
////            this.fileContentRepository
////                .Setup(f => f.GetFileContent(It.IsAny<Guid>(), It.IsAny<Guid>()))
////                .Returns(FileContent.CreateFromBytes(Guid.NewGuid(), Guid.NewGuid(), byteContent));

////            var path = configModel.Path.Build(this.dependencyProvider.Object);

////            // Act
////            var pathLookupFileProvider =
////                this.CreateProvider(path, null, this.objectProvider.Object, this.fileContentRepository.Object);
////            var fileInfo = await pathLookupFileProvider.Resolve(new ProviderContext(automationData));

////            // Assert
////            fileInfo.DataValue.Should().NotBeNull();
////            fileInfo.DataValue.FileName.ToString().Should().Be(filename);
////            fileInfo.DataValue.Content.Should().BeEquivalentTo(byteContent);
////        }

////        [Fact]
////        public async Task PathLookupFileProvider_ShouldReturnValueIfNotFound_WhenPathDoesNotExist()
////        {
////            // Arrange
////            var valueNotFoundJson =
////                @"{
////                    ""path"": ""/some/path/that/does/not/exist"",
////                    ""dataObject"": {
////                        ""entityObject"": {
////                            ""dynamicEntity"": {
////                                ""entityType"":{
////                                    ""objectPathLookupText"": ""trigger.httpRequest.getParameters.entityType""
////                                },
////                                ""entityId"": {
////                                    ""objectPathLookupText"": ""trigger.httpRequest.getParameters.entityId""
////                                }
////                            }
////                        }
////                    },
////                    ""valueIfNotFound"": {
////                        ""textFile"": {
////                            ""outputFilename"": ""ValueNotFound.txt"",
////                            ""sourceData"": ""test""
////                        }
////                    }
////                }";

////            var entityId = Guid.NewGuid();
////            AutomationData automationData = CreateAutomationData("quote", entityId);

////            var configModel =
////                JsonConvert.DeserializeObject<PathLookupFileProviderConfigModel>(
////                    valueNotFoundJson, AutomationDeserializationConfiguration.ModelSettings);

////            var provider = configModel.Build(this.dependencyProvider.Object);

////            var attachmentId = Guid.NewGuid();
////            var filename = "PoliceReport.pdf";

////            var questions = new JObject(
////                new JProperty("ratingPrimary", new JObject(
////                    new JProperty("testAttachment", $"{filename}:application/pdf:{attachmentId}::"))));

////            var dataObject = new Dictionary<string, object>()
////            {
////                { "id", Guid.NewGuid() },
////                { "questions", questions },
////                { "environment", Domain.DeploymentEnvironment.Development },
////            };

////            this.objectProvider
////               .Setup(o => o.Resolve(It.IsAny<ProviderContext>()))
////               .Returns(Task.FromResult(new Data<object>(new ReadOnlyDictionary<string, object>(dataObject))).AsITask());

////            var byteContent = Encoding.ASCII.GetBytes("Sample quote text");
////            this.fileContentRepository
////                .Setup(f => f.GetFileContent(It.IsAny<Guid>(), It.IsAny<Guid>()))
////                .Returns(FileContent.CreateFromBytes(Guid.NewGuid(), Guid.NewGuid(), byteContent));

////            var path = configModel.Path.Build(this.dependencyProvider.Object);

////            var notFoundFile = new FileInfo("notfound.txt", Encoding.ASCII.GetBytes("test"));
////            var valueIfNotFound = configModel.ValueIfNotFound.Build(this.dependencyProvider.Object);

////            // Act
////            var pathLookupFileProvider =
////                this.CreateProvider(path, valueIfNotFound, this.objectProvider.Object, this.fileContentRepository.Object);

////            var fileInfo = await pathLookupFileProvider.Resolve(new ProviderContext(automationData));

////            // Assert
////            fileInfo.DataValue.Should().NotBeNull();
////            fileInfo.DataValue.FileName.ToString().Should().Be("ValueNotFound.txt");
////            fileInfo.DataValue.Content.Should().BeEquivalentTo(Encoding.ASCII.GetBytes("test"));
////        }

////        [Fact]
////        public async Task PathLookupFileProvider_ShouldThrowException_WhenPathDoesNotExistAsync()
////        {
////            // Arrange
////            var pathValue = @"/some/path/that/does/not/exist";
////            var valueNotFoundJson =
////                $@"{{
////                ""path"": ""{pathValue}"",
////                    ""dataObject"": {{
////                        ""entityObject"": {{
////                            ""dynamicEntity"": {{
////                                ""entityType"":{{
////                                    ""objectPathLookupText"": ""trigger.httpRequest.getParameters.entityType""
////                                }},
////                                ""entityId"": {{
////                                    ""objectPathLookupText"": ""trigger.httpRequest.getParameters.entityId""
////                                }}
////                            }}
////                        }}
////                    }},
////                }}";

////            var entityId = Guid.NewGuid();
////            AutomationData automationData = CreateAutomationData("quote", entityId);

////            var configModel =
////                JsonConvert.DeserializeObject<PathLookupFileProviderConfigModel>(
////                    valueNotFoundJson, AutomationDeserializationConfiguration.ModelSettings);

////            var provider = configModel.Build(this.dependencyProvider.Object);

////            var attachmentId = Guid.NewGuid();
////            var filename = "PoliceReport.pdf";

////            var questions = new JObject(
////                new JProperty("ratingPrimary", new JObject(
////                    new JProperty("testAttachment", $"{filename}:application/pdf:{attachmentId}::"))));

////            var dataObject = new Dictionary<string, object>()
////            {
////                { "id", Guid.NewGuid() },
////                { "questions", questions },
////                { "environment", Domain.DeploymentEnvironment.Development },
////            };

////            this.objectProvider
////               .Setup(o => o.Resolve(It.IsAny<ProviderContext>()))
////               .Returns(Task.FromResult(new Data<object>(new ReadOnlyDictionary<string, object>(dataObject))).AsITask());

////            var byteContent = Encoding.ASCII.GetBytes("Sample quote text");
////            this.fileContentRepository
////                .Setup(f => f.GetFileContent(It.IsAny<Guid>(), It.IsAny<Guid>()))
////                .Returns(FileContent.CreateFromBytes(Guid.NewGuid(), Guid.NewGuid(), byteContent));

////            var path = configModel.Path.Build(this.dependencyProvider.Object);

////            var notFoundFile = new FileInfo("notfound.txt", Encoding.ASCII.GetBytes("test"));

////            // Act
////            var pathLookupFileProvider =
////                this.CreateProvider(path, null, this.objectProvider.Object, this.fileContentRepository.Object);

////            Func<Task> func = async () => await pathLookupFileProvider.Resolve(new ProviderContext(automationData));

////            // Assert
////            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code
////                .Should().Be(Errors.Automation.PathLookupValueNotFound(null, null).Code);
////        }

////        [Fact]
////        public async Task PathLookupFileProvider_ShouldUseAutomationData_WhenOnlyPathIsProvided()
////        {
////            // Arrange
////            var pathOnlyJson =
////                @"{
////                ""path"": ""/questions/ratingPrimary/testAttachment"",
////                    ""valueIfNotFound"": {
////                        ""textFile"": {
////                            ""outputFilename"": ""NotFoundFilename.txt"",
////                            ""sourceData"": ""test""
////                        }
////                    }
////                }";

////            var entityId = Guid.NewGuid();
////            AutomationData automationData = CreateAutomationData("quote", entityId);

////            var configModel =
////                JsonConvert.DeserializeObject<PathLookupFileProviderConfigModel>(
////                    pathOnlyJson, AutomationDeserializationConfiguration.ModelSettings);

////            var provider = configModel.Build(this.dependencyProvider.Object);

////            var attachmentId = Guid.NewGuid();
////            var filename = "PoliceReport.pdf";

////            var questions = new JObject(
////                new JProperty("ratingPrimary", new JObject(
////                    new JProperty("testAttachment", $"{filename}:application/pdf:{attachmentId}::"))));

////            var dataObject = new Dictionary<string, object>()
////            {
////                { "id", Guid.NewGuid() },
////                { "questions", questions },
////                { "environment", DeploymentEnvironment.Development },
////            };

////            this.objectProvider
////               .Setup(o => o.Resolve(It.IsAny<ProviderContext>()))
////               .Returns(Task.FromResult(new Data<object>(new ReadOnlyDictionary<string, object>(dataObject))).AsITask());

////            var byteContent = Encoding.ASCII.GetBytes("Sample quote text");
////            this.fileContentRepository
////                .Setup(f => f.GetFileContent(It.IsAny<Guid>(), It.IsAny<Guid>()))
////                .Returns(FileContent.CreateFromBytes(Guid.NewGuid(), Guid.NewGuid(), byteContent));

////            var path = configModel.Path.Build(this.dependencyProvider.Object);

////            var valueIfNotFound = configModel.ValueIfNotFound.Build(this.dependencyProvider.Object);

////            // Act
////            var pathLookupFileProvider =
////                this.CreateProvider(
////                    path, valueIfNotFound, this.objectProvider.Object, this.fileContentRepository.Object);
////            var fileInfo = await pathLookupFileProvider.Resolve(new ProviderContext(automationData));

////            // Assert
////            fileInfo.DataValue.Should().NotBeNull();
////        }

////        [Fact]
////        public async Task PathLookupFileProvider_ShouldUseAsPath_WhenJsonPointerOnlyIsProvided()
////        {
////            // Arrange
////            var jsonPointer = @"/questions/ratingPrimary/testAttachment";

////            var entityId = Guid.NewGuid();
////            AutomationData automationData = CreateAutomationData("quote", entityId);

////            var configModel =
////                JsonConvert.DeserializeObject<PathLookupFileProviderConfigModel>(
////                    $"\"{jsonPointer}\"", AutomationDeserializationConfiguration.ModelSettings);

////            var provider = configModel.Build(this.dependencyProvider.Object);

////            var attachmentId = Guid.NewGuid();
////            var filename = "PoliceReport.pdf";

////            var questions = new JObject(
////                new JProperty("ratingPrimary", new JObject(
////                    new JProperty("testAttachment", $"{filename}:application/pdf:{attachmentId}::"))));

////            var dataObject = new Dictionary<string, object>()
////            {
////                { "id", Guid.NewGuid() },
////                { "questions", questions },
////                { "environment", Domain.DeploymentEnvironment.Development },
////            };

////            this.objectProvider
////               .Setup(o => o.Resolve(It.IsAny<ProviderContext>()))
////               .Returns(Task.FromResult(new Data<object>(new ReadOnlyDictionary<string, object>(dataObject))).AsITask());

////            var byteContent = Encoding.ASCII.GetBytes("Sample quote text");
////            this.fileContentRepository
////                .Setup(f => f.GetFileContent(It.IsAny<Guid>(), It.IsAny<Guid>()))
////                .Returns(FileContent.CreateFromBytes(Guid.NewGuid(), Guid.NewGuid(), byteContent));

////            var path = configModel.Path.Build(this.dependencyProvider.Object);

////            // Act
////            var pathLookupFileProvider =
////                this.CreateProvider(
////                    path, null, this.objectProvider.Object, this.fileContentRepository.Object);
////            var fileInfo = await pathLookupFileProvider.Resolve(new ProviderContext(automationData));

////            // Assert
////            ((StaticBuilder<Data<string>>)configModel.Path).Value.DataValue.Should().Be(jsonPointer);
////            fileInfo.DataValue.Should().NotBeNull();
////        }

////        [Fact]
////        public async Task PathLookupFileProvider_ShouldThrowException_WhenEnvironmentMismatchAsync()
////        {
////            // Arrange
////            var entityId = Guid.NewGuid();
////            AutomationData automationData = CreateAutomationData("quote", entityId);

////            var configModel =
////                JsonConvert.DeserializeObject<PathLookupFileProviderConfigModel>(
////                    this.json, AutomationDeserializationConfiguration.ModelSettings);

////            var provider = configModel.Build(this.dependencyProvider.Object);

////            var attachmentId = Guid.NewGuid();
////            var filename = "PoliceReport.pdf";

////            var questions = new JObject(
////                new JProperty("ratingPrimary", new JObject(
////                    new JProperty("testAttachment", $"{filename}:application/pdf:{attachmentId}::"))));

////            var environment = Domain.DeploymentEnvironment.Staging; // Cause environment mismatch
////            var dataObject = new Dictionary<string, object>()
////            {
////                { "id", Guid.NewGuid() },
////                { "questions", questions },
////                { "environment", environment },
////            };

////            this.objectProvider
////               .Setup(o => o.Resolve(It.IsAny<ProviderContext>()))
////               .Returns(Task.FromResult(new Data<object>(new ReadOnlyDictionary<string, object>(dataObject))).AsITask());

////            var byteContent = Encoding.ASCII.GetBytes("Sample quote text");
////            this.fileContentRepository
////                .Setup(f => f.GetFileContent(It.IsAny<Guid>(), It.IsAny<Guid>()))
////                .Returns(FileContent.CreateFromBytes(Guid.NewGuid(), Guid.NewGuid(), byteContent));

////            var path = configModel.Path.Build(this.dependencyProvider.Object);

////            // Act
////            var pathLookupFileProvider =
////                this.CreateProvider(path, null, this.objectProvider.Object, this.fileContentRepository.Object);
////            Func<Task> func = async () => await pathLookupFileProvider.Resolve(new ProviderContext(automationData));

////            // Assert
////            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("environment.mismatch");
////        }

////        [Fact]
////        public async Task PathLookupFileProvider_ShouldThrowException_WhenImplementationNotSupportedAsync()
////        {
////            // Arrange
////            var entityId = Guid.NewGuid();
////            AutomationData automationData = CreateAutomationData("quote", entityId);

////            var configModel =
////                JsonConvert.DeserializeObject<PathLookupFileProviderConfigModel>(
////                    this.json, AutomationDeserializationConfiguration.ModelSettings);

////            var provider = configModel.Build(this.dependencyProvider.Object);

////            var attachmentId = Guid.NewGuid();
////            var filename = "PoliceReport.pdf";

////            // Return different attachment metadata
////            var metadata =
////                $"{{\"filename\": \"{filename}\", \"filetype\": \"application/pdf\", \"attachmentId\": \"{attachmentId}\"}}";
////            var questions = new JObject(
////                new JProperty("ratingPrimary", new JObject(
////                    new JProperty("testAttachment", metadata))));

////            var environment = Domain.DeploymentEnvironment.Development;
////            var dataObject = new Dictionary<string, object>()
////            {
////                { "id", Guid.NewGuid() },
////                { "questions", questions },
////                { "environment", environment },
////            };

////            this.objectProvider
////               .Setup(o => o.Resolve(It.IsAny<ProviderContext>()))
////               .Returns(Task.FromResult(new Data<object>(new ReadOnlyDictionary<string, object>(dataObject))).AsITask());

////            var byteContent = Encoding.ASCII.GetBytes("Sample quote text");
////            this.fileContentRepository
////                .Setup(f => f.GetFileContent(It.IsAny<Guid>(), It.IsAny<Guid>()))
////                .Returns(FileContent.CreateFromBytes(Guid.NewGuid(), Guid.NewGuid(), byteContent));

////            var path = configModel.PathLookup.Build(this.dependencyProvider.Object);

////            // Act
////            var pathLookupFileProvider =
////                this.CreateProvider(path, null, this.objectProvider.Object, this.fileContentRepository.Object);

////            Func<Task> func = async () => await pathLookupFileProvider.Resolve(new ProviderContext(automationData));

////            // Assert
////            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code
////                .Should().Be(Errors.Automation.PathLookupValueInvalidType(null, null, null, null).Code);
////        }

////        private static AutomationData CreateAutomationData(string entityType, Guid entityId)
////        {
////            var triggerRequest = new TriggerRequest(
////                $"testObjectPathLookupFile?entityType={entityType}&entityId={entityId}",
////                HttpMethod.Get.ToString(),
////                string.Empty,
////                new Dictionary<string, StringValues>
////                {
////                    { "Connection", "Keep-Alive" },
////                    { "Accept", "*/*" },
////                    { "Accept-Encoding", "gzip, deflate, br" },
////                });
////            return AutomationData.CreateFromHttpRequest(
////                Guid.NewGuid(),
////                Guid.NewGuid(),
////                Guid.NewGuid(),
////                Guid.NewGuid(),
////                Domain.DeploymentEnvironment.Development,
////                triggerRequest,
////                MockAutomationData.GetDefaultServiceProvider());
////        }

////        private PathLookupFileProvider CreateProvider(
////            IProvider<Data<string>> path,
////            IProvider<Data<FileInfo>> valueIfNotFound,
////            IObjectProvider dataObject,
////            IFileContentRepository fileContentRepository)
////        {
////            var provider = new PathLookupFileProvider(
////                path,
////                valueIfNotFound,
////                fileContentRepository,
////                null,
////                null,
////                null,
////                null,
////                null,
////                null);
////            return provider;
////        }
////    }
////}
