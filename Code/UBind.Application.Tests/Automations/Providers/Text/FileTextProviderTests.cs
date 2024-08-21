// <copyright file="FileTextProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Text
{
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.Automation.Providers.Text;
    using UBind.Application.Tests.Automations.Fakes;
    using Xunit;

    public class FileTextProviderTests
    {
        [Fact]
        public async Task FileTextProvider_ShouldReturnTextValue_FromFileProvider()
        {
            // Arrange
            string textContent = "The sly fox jumps over the lazy dog";
            var fileContent = Encoding.ASCII.GetBytes(textContent);
            var fileProvider = new StaticProvider<Data<FileInfo>>(new Data<FileInfo>(new FileInfo("test.txt", fileContent)));
            var provider = new FileTextProvider(fileProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            var resolvedString = await provider.Resolve(new ProviderContext(automationData));

            // Assert
            resolvedString.GetValueOrThrowIfFailed().DataValue.Should().Be(textContent);
        }

        [Fact]
        public async Task FileTextProvider_ShouldReturnEmptyString_IfFileContentIsEmpty()
        {
            // Arrange
            var fileContent = Encoding.ASCII.GetBytes(string.Empty);
            var fileProvider = new StaticProvider<Data<FileInfo>>(new Data<FileInfo>(new FileInfo("test.txt", fileContent)));
            var provider = new FileTextProvider(fileProvider);
            var automationData = await MockAutomationData.CreateWithHttpTrigger();

            // Act
            var resolvedString = (await provider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();

            // Assert
            resolvedString.Should().Be(null);
        }
    }
}
