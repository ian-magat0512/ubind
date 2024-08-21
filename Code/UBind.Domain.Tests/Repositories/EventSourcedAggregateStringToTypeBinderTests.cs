// <copyright file="EventSourcedAggregateStringToTypeBinderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Repositories;

using FluentAssertions;
using UBind.Domain.Aggregates.Quote.Entities;
using UBind.Domain.Entities;
using UBind.Domain.Repositories;
using Xunit;

public class EventSourcedAggregateStringToTypeBinderTests
{
    [Theory]
    [InlineData("UBind.Domain", "UBind.Domain.Entities.ClaimFileAttachments", typeof(ClaimFileAttachment))]
    [InlineData("UBind.Domain", "UBind.Domain.Aggregates.Quote.Entities.PolicyTransaction", typeof(PolicyTransactionOld))]
    [InlineData("UBind.Domain", "System.Collections.Generic.List`1[[UBind.Domain.Aggregates.Quote.Entities.PolicyTransaction, UBind.Domain]]", typeof(List<PolicyTransactionOld>))]
    public void BindToType_ReturnsExpectedType_ForValidInput(string assemblyName, string typeName, Type expectedType)
    {
        // Arrange
        var binder = new EventSourcedAggregateStringToTypeBinder();

        // Act
        Type result = binder.BindToType(assemblyName, typeName);

        // Assert
        result.Should().Be(expectedType);
    }

    [Fact]
    public void BindToType_ReturnsNull_ForInvalidInput()
    {
        // Arrange
        var binder = new EventSourcedAggregateStringToTypeBinder();
        string assemblyName = "UBind.Domain";
        string typeName = "UnknownType";

        // Act
        Type result = binder.BindToType(assemblyName, typeName);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void BindToName_SetsAssemblyNameAndTypeName_ForValidInput()
    {
        // Arrange
        var binder = new EventSourcedAggregateStringToTypeBinder();
        Type serializedType = typeof(ClaimFileAttachment);
        string expectedAssemblyName = "UBind.Domain";
        string expectedTypeName = "UBind.Domain.Entities.ClaimFileAttachment";

        // Act
        binder.BindToName(serializedType, out string assemblyName, out string typeName);

        // Assert
        assemblyName.Should().Be(expectedAssemblyName);
        typeName.Should().Be(expectedTypeName);
    }
}
