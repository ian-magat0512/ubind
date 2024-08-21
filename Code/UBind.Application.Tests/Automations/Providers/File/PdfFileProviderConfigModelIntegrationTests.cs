// <copyright file="PdfFileProviderConfigModelIntegrationTests.cs" company="uBind">
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
    using MorseCode.ITask;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.FileHandling;
    using UBind.Domain.Exceptions;
    using Xunit;

    /// <summary>
    /// Unit tests for <see cref="PdfFileProviderConfigModel"/>.
    /// </summary>
    public class PdfFileProviderConfigModelIntegrationTests : IClassFixture<PdfConfigModelTestFixture>
    {
        private readonly PdfConfigModelTestFixture fixture;
        private MSWordFileProviderConfigModel msFileProviderConfigModel;

        public PdfFileProviderConfigModelIntegrationTests(PdfConfigModelTestFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact(Skip = "Should not include temporarily until the interop assembly is properly setup in the build agents")]
        public async Task Resolve_ShouldInvokeBuildOfMsWordFileBuildProvider_WhenMsWordFileIsConfiguredButOutputFileNameAndSourceFileAreNot()
        {
            // Arrange
            this.msFileProviderConfigModel = new MSWordFileProviderConfigModel
            {
                SourceFile = this.fixture.SourceFileProviderMock.Object,
            };

            this.fixture.SourceFileProviderMock.Setup(s => s.Build(It.IsAny<IServiceProvider>())).
                Returns(this.fixture.ProviderDataInfoMock.Object);
            this.fixture.ProviderDataInfoMock.Setup(pdi => pdi.Resolve(It.IsAny<ProviderContext>()))
                .Returns(Task.FromResult(ProviderResult<Data<FileInfo>>.Success(this.fixture.FakeDataFileInfo)).AsITask());
            this.fixture.DependencyProviderMock.Setup(dpm => dpm.GetService(typeof(IPdfEngineService))).
                Returns(this.fixture.PdfEngineService);
            this.fixture.DependencyProviderMock.Setup(dpm => dpm.GetService(typeof(IMsWordEngineService))).
                Returns(this.fixture.MsWordEngineService);

            var pdfFileProviderConfigModel = new PdfFileProviderConfigModel
            {
                SourceFile = this.msFileProviderConfigModel,
            };

            // Act
            var provider = pdfFileProviderConfigModel.Build(this.fixture.DependencyProviderMock.Object);
            var dataFileInfo = (await provider.Resolve(new ProviderContext(this.fixture.Data))).GetValueOrThrowIfFailed();

            // Assert
            provider.Should().NotBeNull();
            dataFileInfo.DataValue.Content.Length.Should().Be(this.fixture.ExpectedOutputContent.Length);
            dataFileInfo.DataValue.FileName.ToString().Should().Be("doc-ub-4120b.pdf");
        }

        [Fact(Skip = "Should not include temporarily until the interop assembly is properly setup in the build agents")]
        public async Task Resolve_ShouldInvokeBuildOfMsWordFileBuildProvider_WhenOutputFileNameAndSourceFileAreConfiguredButMsWordFileIsNot()
        {
            // Arrange
            var expectedOutputFile = "output.pdf";

            var fakeDataString = new Data<string>(expectedOutputFile);

            this.fixture.SourceFileProviderMock.Setup(s => s.Build(It.IsAny<IServiceProvider>())).
                Returns(this.fixture.ProviderDataInfoMock.Object);
            this.fixture.OutputFileNameProviderMock.Setup(o => o.Build(It.IsAny<IServiceProvider>())).
                Returns(this.fixture.ProviderDataStringMock.Object);
            this.fixture.ProviderDataStringMock.Setup(pdsm => pdsm.Resolve(It.IsAny<ProviderContext>()))
                .Returns(Task.FromResult(ProviderResult<Data<string>>.Success(fakeDataString)).AsITask());
            this.fixture.DependencyProviderMock.Setup(d => d.GetService(typeof(IPdfEngineService))).
                Returns(this.fixture.PdfEngineService);
            this.fixture.ProviderDataInfoMock.Setup(pdi => pdi.Resolve(It.IsAny<ProviderContext>()))
                .Returns(Task.FromResult(ProviderResult<Data<FileInfo>>.Success(this.fixture.FakeDataFileInfo)).AsITask());

            var pdfFileProviderConfigModel = new PdfFileProviderConfigModel
            {
                SourceFile = this.fixture.SourceFileProviderMock.Object,
                OutputFileName = this.fixture.OutputFileNameProviderMock.Object,
            };

            // Act
            var provider = pdfFileProviderConfigModel.Build(this.fixture.DependencyProviderMock.Object);
            var dataFileInfo = (await provider.Resolve(new ProviderContext(this.fixture.Data))).GetValueOrThrowIfFailed();

            // Assert
            provider.Should().NotBeNull();
            dataFileInfo.DataValue.FileName.ToString().Should().Be(expectedOutputFile);
            dataFileInfo.DataValue.Content.Length.Should().Be(this.fixture.ExpectedOutputContent.Length);
        }

        [Fact(Skip = "Should not include temporarily until the interop assembly is properly setup in the build agents")]
        public async Task Resolve_ShouldInvokeBuildOfMsWordFileBuildProvider_WhenSourceFileIsConfiguredButMsWordFileAndOutputFileNameAreNot()
        {
            // Arrange
            this.fixture.SourceFileProviderMock.Setup(s => s.Build(It.IsAny<IServiceProvider>())).
                Returns(this.fixture.ProviderDataInfoMock.Object);
            this.fixture.ProviderDataInfoMock.Setup(p => p.Resolve(It.IsAny<ProviderContext>()))
                .Returns(Task.FromResult(ProviderResult<Data<FileInfo>>.Success(this.fixture.FakeDataFileInfo)).AsITask());
            this.fixture.DependencyProviderMock.Setup(d => d.GetService(typeof(IPdfEngineService))).
                Returns(this.fixture.PdfEngineService);

            var pdfFileProviderConfigModel = new PdfFileProviderConfigModel
            {
                SourceFile = this.fixture.SourceFileProviderMock.Object,
            };

            // Act
            var provider = pdfFileProviderConfigModel.Build(this.fixture.DependencyProviderMock.Object);
            var dataFileInfo = (await provider.Resolve(new ProviderContext(this.fixture.Data))).GetValueOrThrowIfFailed();

            // Assert
            provider.Should().NotBeNull();
            dataFileInfo.DataValue.FileName.ToString().Should().Be("doc-ub-4120b.pdf");
            dataFileInfo.DataValue.Content.Length.Should().Be(this.fixture.ExpectedOutputContent.Length);
        }

        [Fact(Skip = "This doesn't work because the output path is different from the input path. Needs refactoring.")]
        public void Build_ThrowsErrorException_WhenNeitherTheMsWordFileAndSourceFileAreConfigured()
        {
            // Arrange
            this.fixture.DependencyProviderMock.Setup(d => d.GetService(typeof(IPdfEngineService))).
                Returns(this.fixture.PdfEngineService);

            var pdfFileProviderConfigModel = new PdfFileProviderConfigModel();

            // Act
            Action act = () => pdfFileProviderConfigModel.Build(this.fixture.DependencyProviderMock.Object);

            // Assert
            act.Should().Throw<ErrorException>().And.Error.Code.Should().Be("automations.source.pdf.build.error");
        }
    }
}
