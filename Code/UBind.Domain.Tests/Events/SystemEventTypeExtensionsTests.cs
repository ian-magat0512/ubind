// <copyright file="SystemEventTypeExtensionsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Tests.Events;

using System;
using FluentAssertions;
using UBind.Domain.Events;
using Xunit;

[SystemEventTypeExtensionInitialize]
public class SystemEventTypeExtensionsTests
{
    [Fact]
    public void GetPersistenceInHoursOrNull_NotThrowException_WhenPeriodUnitIsValid()
    {
        // Arrange
        var func = () => Enum.GetValues(typeof(SystemEventType))
            .Cast<SystemEventType>().Select(value => (int?)value.GetPersistenceInHoursOrNull());

        // Act
        func.Should().NotThrow<ArgumentOutOfRangeException>();
    }
}