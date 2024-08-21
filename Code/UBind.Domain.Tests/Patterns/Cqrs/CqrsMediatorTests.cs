// <copyright file="CqrsMediatorTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Patterns.Cqrs;

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UBind.Application.Services.Email;
using UBind.Domain.Patterns.Cqrs;
using Xunit;

public class CqrsMediatorTests
{
    private readonly Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
    private readonly Mock<IErrorNotificationService> errorNotificationService = new Mock<IErrorNotificationService>();

    /// <summary>
    /// Executing a command within a request scope where the request intent is read-only should throw an exception
    /// if the command has a request intent of read-write.
    /// </summary>
    [Fact]
    public async Task Send_ThrowsException_WhenRequestScopeIntentIsReadOnlyAndCommandRequestIntentIsReadWrite()
    {
        // Arrange
        var mediator = new CqrsMediator(this.serviceProviderMock.Object, NullLogger<CqrsMediator>.Instance, this.errorNotificationService.Object);
        var commandMock = new Mock<ICommand>();
        var requestContextMock = new Mock<ICqrsRequestContext>();
        requestContextMock.SetupGet(x => x.RequestScopeCreated).Returns(true);
        requestContextMock.SetupGet(x => x.RequestIntent).Returns(RequestIntent.ReadOnly);
        this.serviceProviderMock.Setup(x => x.GetService(typeof(ICqrsRequestContext)))
            .Returns(requestContextMock.Object);

        // Act
        Func<Task> act = () => mediator.Send(commandMock.Object);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
