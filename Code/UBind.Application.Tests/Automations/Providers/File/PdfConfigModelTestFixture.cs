// <copyright file="PdfConfigModelTestFixture.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Reflection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Moq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.FileHandling;
    using UBind.Application.Tests.Automations.Fakes;
    using SystemIO = System.IO;

    public class PdfConfigModelTestFixture
    {
        public PdfConfigModelTestFixture()
        {
            this.GenerateTemplateSourceContent();
            this.GenerateMockObjects();

            var triggerRequest = new TriggerRequest(
                    "pdfMerge",
                    HttpMethod.Get.ToString(),
                    string.Empty,
                    new Dictionary<string, StringValues>
                    {
                        { "Connection", "Keep-Alive" },
                        { "Accept", "*/*" },
                        { "Accept-Encoding", "gzip, deflate, br" },
                    });
            this.Data = AutomationData.CreateFromHttpRequest(
                Guid.NewGuid(),
                default,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Domain.DeploymentEnvironment.Development,
                triggerRequest,
                MockAutomationData.GetDefaultServiceProvider());

            this.PdfEngineService = new PdfEngineService(this.PdfLoggerMock.Object);
            this.MsWordEngineService = new MsWordEngineService(this.MsWordLoggerMock.Object);
            this.ExpectedOutputContent = SystemIO.File.ReadAllBytes($"{this.TestTemplatePath}\\doc-ub-4120b.pdf");
        }

        public Mock<IServiceProvider> DependencyProviderMock { get; private set; }

        public Mock<IBuilder<IProvider<Data<FileInfo>>>> SourceFileProviderMock { get; private set; }

        public Mock<IProvider<Data<FileInfo>>> ProviderDataInfoMock { get; private set; }

        public Mock<IPdfEngineService> PdfEngineServiceMock { get; private set; }

        public Mock<IBuilder<IProvider<Data<string>>>> OutputFileNameProviderMock { get; private set; }

        public Mock<IProvider<Data<string>>> ProviderDataStringMock { get; private set; }

        public Mock<ILogger<PdfEngineService>> PdfLoggerMock { get; private set; }

        public Mock<ILogger<MsWordEngineService>> MsWordLoggerMock { get; private set; }

        public PdfEngineService PdfEngineService { get; private set; }

        public MsWordEngineService MsWordEngineService { get; private set; }

        public byte[] ExpectedOutputContent { get; private set; }

        public AutomationData Data { get; private set; }

        public byte[] TemplateSourceContent { get; private set; }

        public Data<FileInfo> FakeDataFileInfo { get; private set; }

        public string TestTemplatePath { get; private set; }

        public void GenerateMockObjects()
        {
            this.DependencyProviderMock = new Mock<IServiceProvider>(MockBehavior.Strict);
            this.SourceFileProviderMock = new Mock<IBuilder<IProvider<Data<FileInfo>>>>(MockBehavior.Strict);
            this.ProviderDataInfoMock = new Mock<IProvider<Data<FileInfo>>>(MockBehavior.Strict);
            this.PdfEngineServiceMock = new Mock<IPdfEngineService>(MockBehavior.Strict);
            this.OutputFileNameProviderMock = new Mock<IBuilder<IProvider<Data<string>>>>(MockBehavior.Strict);
            this.ProviderDataStringMock = new Mock<IProvider<Data<string>>>(MockBehavior.Strict);
            this.PdfLoggerMock = new Mock<ILogger<PdfEngineService>>(MockBehavior.Strict);
            this.MsWordLoggerMock = new Mock<ILogger<MsWordEngineService>>(MockBehavior.Strict);
        }

        private void GenerateTemplateSourceContent()
        {
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().Location);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var testFolder = Path.GetDirectoryName(codeBasePath);
            this.TestTemplatePath = Path.Combine(testFolder, "Templates\\Documents");
            var fileName = "doc-ub-4120b.doc";
            var testFile = this.TestTemplatePath + $"\\{fileName}";
            this.TemplateSourceContent = SystemIO.File.ReadAllBytes(testFile);

            this.FakeDataFileInfo = new Data<FileInfo>(
                new FileInfo(fileName, this.TemplateSourceContent));
        }
    }
}
