// <copyright file="MsWordTemplateProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.FileHandling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Export;
    using UBind.Application.FileHandling;
    using UBind.Application.FileHandling.Template_Provider;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Configuration;
    using UBind.Domain.Events;
    using UBind.Domain.Product;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class MsWordTemplateProviderTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();

        [Fact]
        public void Invoke_ReturnsTextDocumentFromTemplate()
        {
            // TODO
            // Add document for testing
            var dependencyProvider = new Mock<IExporterDependencyProvider>();
            var productConfiguration = new Mock<IProductConfiguration>();
            var workflowProvider = new DefaultQuoteWorkflowProvider();
            var quoteExpirySettingsProvider = new DefaultExpirySettingsProvider();

            var templateNameJson = @"{ ""text"": ""document-quote-schedule.dotx"", ""type"": ""fixed"" }";
            var templateNameModel = JsonConvert.DeserializeObject<FixedTextProviderModel>(
                templateNameJson, this.BuildConverter());
            var outputFileNameJson = @"{ ""text"": ""document-quote-schedule.pdf"", ""type"": ""fixed"" }";
            var outputFileNameModel = JsonConvert.DeserializeObject<FixedTextProviderModel>(
                outputFileNameJson, this.BuildConverter());

            Mock<EventExporterCondition> condition = new Mock<EventExporterCondition>();
            Mock<IFileContentsLoader> fileContentsLoader
                = new Mock<IFileContentsLoader>();
            Mock<IMsWordEngineService> msWordEngineService
                = new Mock<IMsWordEngineService>();

            fileContentsLoader
                .Setup(l => l.Load(
                    It.IsAny<ReleaseContext>(),
                    templateNameModel.Text))
                .Returns("somecontent");

            MsWordTemplateFileProvider provider
                = new MsWordTemplateFileProvider(
                    templateNameModel?.Build(dependencyProvider.Object, productConfiguration.Object),
                    outputFileNameModel?.Build(dependencyProvider.Object, productConfiguration.Object),
                    condition.Object,
                    new List<IJObjectProvider>(),
                    fileContentsLoader.Object,
                    msWordEngineService.Object);

            var quote = QuoteAggregate.CreateNewBusinessQuote(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                DeploymentEnvironment.Development,
                QuoteExpirySettings.Default,
                this.performingUserId,
                SystemClock.Instance.GetCurrentInstant(),
                Guid.NewGuid(),
                Timezones.AET);
            var eventType = QuoteEventTypeMap.Map(quote.Aggregate.UnsavedEvents.Last());
            var applicationEvent = new ApplicationEvent(
                Guid.NewGuid(),
                eventType.First(),
                quote.Aggregate,
                quote.Id,
                0,
                "1",
                quote.ProductReleaseId.Value);

            var attachment = provider.Invoke(applicationEvent);

            // Assert
            Assert.NotNull(attachment);
        }

        private TextProviderModelConverter BuildConverter()
        {
            return new TextProviderModelConverter(
                new TypeMap
                {
                     { "fixed", typeof(FixedTextProviderModel) },
                });
        }

        private class DummyModel
        {
            public IExporterModel<ITextProvider> Text { get; set; }
        }
    }
}
