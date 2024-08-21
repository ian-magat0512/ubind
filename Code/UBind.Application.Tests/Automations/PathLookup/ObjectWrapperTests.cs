// <copyright file="ObjectWrapperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.PathLookup
{
    using FluentAssertions;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.PathLookup;
    using Xunit;

    public class ObjectWrapperTests
    {
        [Fact]
        public void Wrap_ReturnsDataString_WhenObjectIsString()
        {
            // Arrange
            JToken obj = JToken.FromObject("foo");

            // Act
            var data = ObjectWrapper.Wrap(obj);

            // Assert
            data.Should().BeOfType<Data<string>>();
        }

        [Fact]
        public void Wrap_ReturnsDataLong_WhenObjectIsLong()
        {
            // Arrange
            JToken obj = JToken.FromObject(1L);

            // Act
            var data = ObjectWrapper.Wrap(obj);

            // Assert
            data.Should().BeOfType<Data<long>>();
        }

        [Fact]
        public void Wrap_ReturnsDataDecimal_WhenObjectIsDecimal()
        {
            // Arrange
            JToken obj = JToken.FromObject(1m);

            // Act
            var data = ObjectWrapper.Wrap(obj);

            // Assert
            data.Should().BeOfType<Data<decimal>>();
        }

        [Fact]
        public void Wrap_ReturnsDataByteArray_WhenObjectIsByteArray()
        {
            // Arrange
            byte[] byteArray = { 0x01, 0x02, 0x03, 0x04 };
            JToken obj = JToken.FromObject(byteArray);

            // Act
            var data = ObjectWrapper.Wrap(obj);

            // Assert
            data.Should().BeOfType<Data<byte[]>>();
        }
    }
}
