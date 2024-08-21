// <copyright file="MsExcelFileProviderConfigModelTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.File
{
    using System;
    using FluentAssertions;
    using Moq;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.Automation.Providers.Object;
    using UBind.Application.FileHandling;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="MsExcelFileProviderConfigModel"/>.
    /// </summary>
    public class MsExcelFileProviderConfigModelTests
    {
        private readonly Mock<IServiceProvider> dependencyProviderMock;
        private readonly Mock<IMsExcelEngineService> msExcelEngineServiceMock;
        private readonly Mock<IBuilder<IProvider<Data<string>>>> outputFileNameMock;
        private readonly Mock<IProvider<Data<string>>> outputFileNameMockResult;
        private readonly Mock<IBuilder<IProvider<Data<FileInfo>>>> sourceFileMock;
        private readonly Mock<IProvider<Data<FileInfo>>> sourceFileMockResult;
        private readonly Mock<IBuilder<IObjectProvider>> dataObjectMock;
        private readonly Mock<IObjectProvider> dataObjectMockResult;

        public MsExcelFileProviderConfigModelTests()
        {
            this.dependencyProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            this.msExcelEngineServiceMock = new Mock<IMsExcelEngineService>(MockBehavior.Strict);

            this.sourceFileMock = new Mock<IBuilder<IProvider<Data<FileInfo>>>>(MockBehavior.Strict);
            this.sourceFileMockResult = new Mock<IProvider<Data<FileInfo>>>(MockBehavior.Strict);
            this.outputFileNameMock = new Mock<IBuilder<IProvider<Data<string>>>>(MockBehavior.Strict);
            this.outputFileNameMockResult = new Mock<IProvider<Data<string>>>(MockBehavior.Strict);
            this.dataObjectMock = new Mock<IBuilder<IObjectProvider>>(MockBehavior.Strict);
            this.dataObjectMockResult = new Mock<IObjectProvider>(MockBehavior.Strict);
        }

        [Fact]
        public void Build_ShouldReturnExcelFileProvider_WhenConfigurationIsComplete()
        {
            // Arrange
            this.outputFileNameMock.Setup(o => o.Build(It.IsAny<IServiceProvider>()))
                .Returns(this.outputFileNameMockResult.Object);
            this.sourceFileMock.Setup(s => s.Build(It.IsAny<IServiceProvider>()))
                .Returns(this.sourceFileMockResult.Object);
            this.dataObjectMock.Setup(d => d.Build(It.IsAny<IServiceProvider>()))
                .Returns(this.dataObjectMockResult.Object);
            this.dependencyProviderMock.Setup(dpm => dpm.GetService(typeof(IMsExcelEngineService)))
                .Returns(this.msExcelEngineServiceMock.Object);

            var configModel = new MsExcelFileProviderConfigModel
            {
                OutputFileName = this.outputFileNameMock.Object,
                SourceFile = this.sourceFileMock.Object,
                DataObject = this.dataObjectMock.Object,
            };

            // Act
            var provider = configModel.Build(this.dependencyProviderMock.Object);

            // Assert
            provider.Should().NotBeNull();
        }

        [Fact]
        public void Build_ThrowsException_WhenSourceFileIsNotConfigured()
        {
            // Arrange
            this.outputFileNameMock.Setup(o => o.Build(It.IsAny<IServiceProvider>()))
                .Returns(this.outputFileNameMockResult.Object);
            this.dataObjectMock.Setup(d => d.Build(It.IsAny<IServiceProvider>()))
                .Returns(this.dataObjectMockResult.Object);
            this.dependencyProviderMock.Setup(dpm => dpm.GetService(typeof(IMsExcelEngineService)))
                .Returns(this.msExcelEngineServiceMock.Object);

            var configModel = new MsExcelFileProviderConfigModel
            {
                OutputFileName = this.outputFileNameMock.Object,
                DataObject = this.dataObjectMock.Object,
            };

            // Act + Assert
            var errorException = Assert.Throws<ErrorException>(
                () => configModel.Build(this.dependencyProviderMock.Object));

            var providerErrorData = new JObject()
                {
                    { ErrorDataKey.ErrorMessage, "SourceFile property is missing." },
                };
            var expectedError = Errors.Automation.InvalidAutomationConfiguration(providerErrorData);
            errorException.Error.Code.Should().Be(expectedError.Code);
            errorException.Error.AdditionalDetails.Should().Contain(expectedError.AdditionalDetails);
        }
    }
}
