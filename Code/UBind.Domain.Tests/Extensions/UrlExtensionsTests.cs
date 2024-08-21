// <copyright file="UrlExtensionsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Extensions
{
    using FluentAssertions;
    using Flurl;
    using UBind.Domain.Extensions;
    using Xunit;

    public class UrlExtensionsTests
    {
        [Fact]
        public void AddQueryParameter_WhenUrlAlreadyHasParameter_ShouldAddNewParameter()
        {
            // Arrange
            var url = new Url("https://example.com?existingParam=value");

            // Act
            url = url.SetQueryParam("newParam", "newValue");

            // Assert
            url.QueryParams.Should().Contain(p => p.Name == "existingParam" && p.Value.ToString() == "value");
            url.QueryParams.Should().Contain(p => p.Name == "newParam" && p.Value.ToString() == "newValue");
            url.ToString().Should().Be("https://example.com?existingParam=value&newParam=newValue");
        }

        [Fact]
        public void AddPathWithQueryParameter_WhenUrlAlreadyHasParameter_ShouldMergeParameters()
        {
            // Arrange
            var url = new Url("https://example.com").SetQueryParam("existingParam", "value");

            // Act
            url = url.AppendPathWithQuery("newPath/anotherPath?newParam=newValue");

            // Assert
            url.QueryParams.Should().Contain(p => p.Name == "existingParam" && p.Value.ToString() == "value");
            url.QueryParams.Should().Contain(p => p.Name == "newParam" && p.Value.ToString() == "newValue");
            url.ToString().Should().Be("https://example.com/newPath/anotherPath?existingParam=value&newParam=newValue");
        }
    }
}
