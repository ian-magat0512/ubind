// <copyright file="MockDependencyExtension.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using UBind.Application.Automation.Actions;
using UBind.Application.Automation.Attachment;
using UBind.Application.Automation.Providers.File;
using UBind.Application.Automation.Providers.Object;
using UBind.Application.Automation.Providers.Text;

/// <summary>
/// Adds the loggers dependencies to mocks and service collections.
/// Speeds up the coding process of adding logs to all unit tests.
/// </summary>
public static class MockDependencyExtension
{
    public static Mock<IServiceProvider> AddLoggers(this Mock<IServiceProvider> dependencyProvider)
    {
        dependencyProvider.Setup(x => x.GetService(typeof(ILogger<DynamicObjectProvider>))).Returns(new Mock<ILogger<DynamicObjectProvider>>().Object);
        dependencyProvider.Setup(x => x.GetService(typeof(ILogger<SendEmailAction>))).Returns(new Mock<ILogger<SendEmailAction>>().Object);
        dependencyProvider.Setup(x => x.GetService(typeof(ILogger<FileAttachmentProvider>))).Returns(new Mock<ILogger<FileAttachmentProvider>>().Object);
        dependencyProvider.Setup(x => x.GetService(typeof(ILogger<PdfFileProvider>))).Returns(new Mock<ILogger<PdfFileProvider>>().Object);
        dependencyProvider.Setup(x => x.GetService(typeof(ILogger<MSWordFileProvider>))).Returns(new Mock<ILogger<MSWordFileProvider>>().Object);
        dependencyProvider.Setup(x => x.GetService(typeof(ILogger<DynamicObjectProvider>))).Returns(new Mock<ILogger<DynamicObjectProvider>>().Object);
        dependencyProvider.Setup(x => x.GetService(typeof(ILogger<RazorTextProvider>))).Returns(new Mock<ILogger<RazorTextProvider>>().Object);

        return dependencyProvider;
    }

    public static ServiceCollection AddLoggers(this ServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ILogger<DynamicObjectProvider>>(x => new Mock<ILogger<DynamicObjectProvider>>().Object);
        serviceCollection.AddScoped<ILogger<SendEmailAction>>(x => new Mock<ILogger<SendEmailAction>>().Object);
        serviceCollection.AddScoped<ILogger<FileAttachmentProvider>>(x => new Mock<ILogger<FileAttachmentProvider>>().Object);
        serviceCollection.AddScoped<ILogger<PdfFileProvider>>(x => new Mock<ILogger<PdfFileProvider>>().Object);
        serviceCollection.AddScoped<ILogger<MSWordFileProvider>>(x => new Mock<ILogger<MSWordFileProvider>>().Object);
        serviceCollection.AddScoped<ILogger<DynamicObjectProvider>>(x => new Mock<ILogger<DynamicObjectProvider>>().Object);
        serviceCollection.AddScoped<ILogger<RazorTextProvider>>(x => new Mock<ILogger<RazorTextProvider>>().Object);

        return serviceCollection;
    }
}
