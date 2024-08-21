// <copyright file="DictionaryConverterTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.JsonConverters
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Newtonsoft.Json;
    using UBind.Domain.JsonConverters;
    using Xunit;

    public class DictionaryConverterTests
    {
        [Fact]
        public void DictionaryConverter_ConvertsArraysToLists()
        {
            // Arrange
            var json = @"{
    ""foo"": {
        ""bar"": [
            {
                ""baz"": 1,
            },
            {
                ""baz"": 2
            },
        ]
    }
}";

            // Act
            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, new DictionaryConverter());

            // Assert
            ((Dictionary<string, object>)result["foo"])["bar"].Should().BeAssignableTo<IList<object>>();
        }

        [Fact]
        public void DictionaryConverter_HandlesComplexObjectsWithNestedObjectsAndArrays()
        {
            // Arrange
            var obj = new Dictionary<string, object>
            {
                { "value", (object)"v" },
                {
                    "object",
                    new Dictionary<string, object>
                    {
                        { "nestedValue", "nv" },
                        {
                            "nestedObject",
                            new Dictionary<string, object>
                            {
                                { "doubleNestedValue", "dnv" },
                            }
                        },
                    }
                },
                {
                    "array",
                    new List<object> { 1, 2, 3 }
                },
            };

            // Act
            var json = JsonConvert.SerializeObject(obj);
            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, new DictionaryConverter());

            // Assert
            result.Should().ContainKey("value");
            result["value"].Should().Be("v");
            result.Should().ContainKey("object");
            result["object"].Should().BeAssignableTo<IDictionary<string, object>>();
            ((IDictionary<string, object>)result["object"]).Should().ContainKey("nestedValue");
            ((IDictionary<string, object>)result["object"])["nestedValue"].Should().Be("nv");
            ((IDictionary<string, object>)result["object"]).Should().ContainKey("nestedObject");
            ((IDictionary<string, object>)result["object"])["nestedObject"].Should().BeAssignableTo<IDictionary<string, object>>();
            ((IDictionary<string, object>)((IDictionary<string, object>)result["object"])["nestedObject"]).Should().ContainKey("doubleNestedValue");
            ((IDictionary<string, object>)((IDictionary<string, object>)result["object"])["nestedObject"])["doubleNestedValue"].Should().Be("dnv");
            result["array"].Should().BeAssignableTo<IList<object>>();
            ((IList<object>)result["array"]).First().Should().Be(1);
        }
    }
}
