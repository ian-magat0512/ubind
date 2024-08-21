// <copyright file="ResourcePoolTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.ResourcePool;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using UBind.Application.ResourcePool;
using UBind.Application.Services.Email;
using UBind.Domain.Exceptions;
using Xunit;

public class ResourcePoolTests
{
    private readonly TestResourcePool resourcePool;

    public ResourcePoolTests()
    {
        var clockMock = new Mock<IClock>();
        var loggerMock = new Mock<ILogger<IResourcePool>>();
        var errorNotificationService = new Mock<IErrorNotificationService>().Object;
        this.resourcePool = new TestResourcePool(clockMock.Object, loggerMock.Object, errorNotificationService);
    }

    [Fact]
    public void AcquireResource_SuccessfullyAcquiresResource_WhenCalled()
    {
        // Act
        var resource = this.resourcePool.AcquireResource();

        // Assert
        resource.Should().NotBeNull();
        this.resourcePool.GetUsageCount().Should().Be(1);
    }

    [Fact]
    public void AcquireResource_AddsResourceToPool_WhenPoolExhausted()
    {
        // Act
        var resource1 = this.resourcePool.AcquireResource();
        var resource2 = this.resourcePool.AcquireResource();

        // Assert
        this.resourcePool.GetUsageCount().Should().Be(2);
    }

    [Fact]
    public void ReleaseResource_ReturnsResourceToPool_WhenCalled()
    {
        // Arrange
        var resource = this.resourcePool.AcquireResource();

        // Act
        this.resourcePool.ReleaseResource(resource);

        // Assert
        this.resourcePool.GetUsageCount().Should().Be(0);
        this.resourcePool.GetResourceCount().Should().Be(1);
    }

    [Fact]
    public void AcquireResource_ThrowsObjectDisposedException_WhenDisposed()
    {
        // Arrange
        var clockMock = new Mock<IClock>();
        var loggerMock = new Mock<ILogger<IResourcePool>>();
        var errorNotificationService = new Mock<IErrorNotificationService>().Object;
        var resourcePool = new Mock<ResourcePool>(clockMock.Object, loggerMock.Object, errorNotificationService)
        {
            CallBase = true,
        };

        // Dispose the resource pool
        resourcePool.Object.Dispose();

        // Act and Assert
        var exception = Assert.Throws<ErrorException>(() => resourcePool.Object.AcquireResource());
        exception.Error.Code.Should().Be("resource.pool.object.disposed");
    }

    [Fact]
    public void AcquireResource_ThrowsErrorException_WithSpecificErrorMessage_ForMaxRetries()
    {
        // Arrange
        var clockMock = new Mock<IClock>();
        var loggerMock = new Mock<ILogger<IResourcePool>>();
        var errorNotificationService = new Mock<IErrorNotificationService>().Object;

        var resourcePool = new Mock<ResourcePool>(clockMock.Object, loggerMock.Object, errorNotificationService)
        {
            CallBase = true,
        };

        // Simulate reaching maximum retries
        IResourcePoolMember? instance;
        resourcePool.SetupSequence(rp => rp.TryDequeueResource(out instance))
                .Returns(false)
                .Returns(false)
                .Returns(false)
                .Returns(false)
                .Returns(false)
                .Returns(false)
                .Returns(false)
                .Returns(false)
                .Returns(false)
                .Returns(false);    // 10th retry

        // Act and Assert
        var exception = Assert.Throws<ErrorException>(() => resourcePool.Object.AcquireResource());
        exception.Error.Code.Should().Be("resource.pool.max.retries.reached");
    }
}

internal class TestResourcePool : ResourcePool
{
    public TestResourcePool(IClock clock, ILogger<IResourcePool> logger, IErrorNotificationService errorNotificationService)
        : base(clock, logger, errorNotificationService)
    {
    }

    public override string GetDebugIdentifier()
    {
        return "TestResourcePool";
    }

    protected override IResourcePoolMember CreateResource()
    {
        return new Mock<IResourcePoolMember>().Object;
    }
}
