// <copyright file="TinyUrlTextProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Providers.Text;

using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using UBind.Application.Automation;
using UBind.Application.Automation.Extensions;
using UBind.Application.Automation.Providers;
using UBind.Application.Automation.Providers.Text;
using UBind.Application.Services;
using UBind.Application.Tests.Automations.Fakes;
using UBind.Domain;
using UBind.Domain.Exceptions;
using UBind.Domain.Extensions;
using UBind.Domain.Models;
using UBind.Domain.Tests.Fakes;
using Xunit;

public class TinyUrlTextProviderTests
{
    private const string AppBaseUrl = "https://localhost:44366";
    private readonly JsonSerializerSettings settings;
    private readonly TestClock clock = new TestClock();
    private readonly Mock<IBaseUrlResolver> baseUrlResolverMock = new Mock<IBaseUrlResolver>();
    private readonly Mock<ICachingResolver> cachingResolverMock = new Mock<ICachingResolver>();

    private long sequence;
    private Guid tenantId;
    private DeploymentEnvironment environment = DeploymentEnvironment.Development;
    private Guid productId;

    public TinyUrlTextProviderTests()
    {
        this.settings = AutomationDeserializationConfiguration.ModelSettings;
        this.baseUrlResolverMock.Setup(x => x.GetBaseUrl(It.IsAny<Tenant>())).Returns(AppBaseUrl);
        this.cachingResolverMock = new Mock<ICachingResolver>();
        this.cachingResolverMock.Setup(x => x.GetTenantOrThrow(It.IsAny<Guid>()))
            .Returns(Task.FromResult(It.IsAny<Tenant>()));
        this.tenantId = Guid.NewGuid();
        this.productId = Guid.NewGuid();
    }

    [Theory]
    [InlineData("fdfdgdfg", "/portal/carl")] // relative URL
    [InlineData("fsdf8213", "https://www.customsite.com/its/a/coffee/day?id=3")] // absolute Custom URL
    public async Task TinyUrlTextProvider_ShouldReturnTinyUrl_Successfully(string token, string redirectUrl)
    {
        // Arrange
        var tinyUrl = new TinyUrl(this.tenantId, this.environment, redirectUrl, token, this.GetSequenceNumber(), this.clock.GetCurrentInstant());
        var url = $"{AppBaseUrl}/t/{tinyUrl.Token}";
        var dependencyProvider = this.SetupProvider(redirectUrl, url);
        var automationData = await MockAutomationData.CreateWithHttpTrigger(
            this.tenantId, Guid.NewGuid(), this.productId, this.environment, queryParams: $"?redirectUrl={redirectUrl}");
        var json = @"
            {
                ""tinyUrlText"": {
                    ""objectPathLookupText"": ""/trigger/httpRequest/getParameters/redirectUrl""
                }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<string>>>>(json, this.settings);
        var tinyUrlTextProvider = model.Build(dependencyProvider) as TinyUrlTextProvider;

        // Assert
        var result = (await tinyUrlTextProvider.Resolve(new ProviderContext(automationData))).GetValueOrThrowIfFailed();
        result.DataValue.Should().Be(url);
    }

    [Theory]
    [InlineData("")]
    [InlineData("//this/is/invalid/path")]
    [InlineData("mywebsite.com/it*s/a/coffee/day/id=3")]
    public async Task TinyUrlTextProvider_ShouldThrow_WhenRedirectUrlIsInvalid(string redirectUrl)
    {
        // Arrange
        var tinyUrl = new TinyUrl(this.tenantId, this.environment, redirectUrl, "aaz8iftu", this.GetSequenceNumber(), this.clock.GetCurrentInstant());
        var url = $"{AppBaseUrl}/t/{tinyUrl.Token}";
        var dependencyProvider = this.SetupProvider(redirectUrl, url);
        var automationData = await MockAutomationData.CreateWithHttpTrigger(queryParams: $"?redirectUrl={redirectUrl}");
        var json = @"
            {
                ""tinyUrlText"": {
                    ""objectPathLookupText"": ""/trigger/httpRequest/getParameters/redirectUrl""
                }
            }";

        // Act
        var model = JsonConvert.DeserializeObject<IBuilder<IProvider<Data<string>>>>(json, this.settings);
        var tinyUrlTextProvider = model.Build(dependencyProvider) as TinyUrlTextProvider;

        // Assert
        var result = async () => await tinyUrlTextProvider.Resolve(new ProviderContext(automationData));
        await result.Should().ThrowAsync<ErrorException>();
    }

    private ServiceProvider SetupProvider(string redirectUrl, string url)
    {
        var tinyUrlServiceMock = new Mock<ITinyUrlService>();
        tinyUrlServiceMock.Setup(x => x.GenerateAndPersistUrl(this.tenantId, this.environment, redirectUrl))
            .Returns(Task.FromResult(url));

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<ITinyUrlService>(x => tinyUrlServiceMock.Object);
        serviceCollection.AddScoped<IBaseUrlResolver>(x => this.baseUrlResolverMock.Object);
        serviceCollection.AddScoped<IBaseUrlResolver>(x => this.baseUrlResolverMock.Object);
        serviceCollection.AddScoped<ICachingResolver>(x => this.cachingResolverMock.Object);
        return serviceCollection.BuildServiceProvider();
    }

    private long GetSequenceNumber()
    {
        this.sequence++;
        return this.sequence;
    }
}
