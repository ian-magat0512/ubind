// <copyright file="JObjectExtensionTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1600

namespace UBind.Domain.Tests.Extensions
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain.Extensions;
    using UBind.Domain.Json;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class JObjectExtensionTests
    {
        [Fact]
        public void FindValue_ReturnsFail_WhenNoMatchingValueIsFound()
        {
            // Arrange
            var json = CalculationResultJsonFactory.Create();
            var jObject = JObject.Parse(json);

            // Act
            var result = jObject.FindValue<string>("foobar");

            // Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void FindValue_ReturnsValue_WhenMatchIsFoundInArray()
        {
            // Arrange
            var json = @"{
    ""foo"" : [
        {
            ""bar"": ""baz""
        }
    ]
}";
            var jObject = JObject.Parse(json);

            // Act
            var result = jObject.FindValue<string>("bar");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("baz", result.Value);
        }

        [Fact]
        public void FindValue_ReturnsValue_WhenMatchIsFound()
        {
            // Arrange
            var json = @"{ ""foo"" : ""bar"" }";
            var jObject = JObject.Parse(json);

            // Act
            var result = jObject.FindValue<string>("foo");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("bar", result.Value);
        }

        [Fact]
        public void FindValue_ReturnsFirstMatchingParsableDate_WhenOnlySecondIsTransformable()
        {
            // Arrange
            var json = @"{
    ""x0"": ""blah"",
    ""x1"": ""2002-04-07""
}";
            var jObject = JObject.Parse(json);

            // Act
            var result = jObject.FindValue<string, LocalDate>("x", v => v.TryParseAsLocalDate());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(new LocalDate(2002, 4, 7), result.Value);
        }

        [Fact]
        public void FindValue_ReturnsFirstMatchingParsableDate_WhenMultipleAreTransformable()
        {
            // Arrange
            var json = @"{
    ""x0"": ""blah"",
    ""x1"": ""2002-04-07"",
    ""x2"": ""2000-01-01""
}";
            var jObject = JObject.Parse(json);

            // Act
            var result = jObject.FindValue<string, LocalDate>("x", v => v.TryParseAsLocalDate());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(new LocalDate(2002, 4, 7), result.Value);
        }

        [Theory]
        [InlineData("foo.integerProperty", PatchRules.None)]
        [InlineData("foo.integerProperty", PatchRules.PropertyExists)]
        [InlineData("foo.nullPropery", PatchRules.PropertyExists)]
        [InlineData("foo.nullPropery", PatchRules.PropertyIsMissingOrNullOrEmpty)]
        [InlineData("foo.emptyStringProperty", PatchRules.PropertyIsMissingOrNullOrEmpty)]
        [InlineData("foo.nonExistantProperty", PatchRules.None)]
        [InlineData("foo.nonExistantProperty", PatchRules.PropertyDoesNotExist)]
        [InlineData("foo.nonExistantProperty", PatchRules.PropertyIsMissingOrNullOrEmpty)]
        [InlineData("foo.nonExistantProperty.nonExistantProperty", PatchRules.None)]
        [InlineData("foo.objectProperty.nonExistantProperty", PatchRules.ParentMustExist)]
        public void CanPatchProperty_ReturnsTrue_WhenRulesPermit(
            string path, PatchRules rules)
        {
            // Arrange
            var jObject = JObject.Parse(@"{
    ""foo"": {
        ""integerProperty"": 7,
        ""nullPropery"": null,
        ""emptyStringProperty"": """",
        ""arrayProperty"": [],
        ""objectProperty"": {}
    }
}");
            var jsonPath = new JsonPath(path);

            // Act
            var result = jObject.CanPatchProperty(jsonPath, rules);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Theory]
        ////[InlineData("foo.integerProperty", PatchRules.PropertyDoesNotExist)]
        ////[InlineData("foo.nullPropery", PatchRules.PropertyIsMissingOrNullOrEmpty)]
        ////[InlineData("foo.emptyStringProperty", PatchRules.PropertyDoesNotExist)]
        ////[InlineData("foo.emptyStringProperty", PatchRules.PropertyDoesNotExist)]
        ////[InlineData("foo.nonExistantproperty", PatchRules.PropertyExists)]
        [InlineData("foo.nonExistantProperty.nonExistantProperty", PatchRules.ParentMustExist)]
        ////[InlineData("foo.integerProperty.nonExistantProperty", PatchRules.None)]
        ////[InlineData("foo.nullPropery.nonExistantProperty", PatchRules.None)]
        ////[InlineData("foo.emptyStringProperty.nonExistantProperty", PatchRules.None)]
        ////[InlineData("foo.arrayProperty.nonExistantProperty", PatchRules.None)]
        public void CanPatchProperty_ReturnsFalse_WhenRulesDoNotPermit(
            string path, PatchRules rules)
        {
            // Arrange
            var jObject = JObject.Parse(@"{
    ""foo"": {
        ""integerProperty"": 7,
        ""nullPropery"": null,
        ""emptyStringProperty"": """",
        ""arrayProperty"": [],
        ""objectProperty"": {}
    }
}");
            var jsonPath = new JsonPath(path);

            // Act
            var result = jObject.CanPatchProperty(jsonPath, rules);

            // Assert
            Assert.False(result.IsSuccess);
        }

        [Theory]
        [InlineData("foo.integerProperty")]
        [InlineData("foo.nullPropery")]
        [InlineData("foo.emptyStringProperty")]
        [InlineData("foo.nonExistantProperty")]
        [InlineData("foo.nonExistantProperty.nonExistantProperty")]
        [InlineData("foo.objectProperty.nonExistantProperty")]
        public void PatchProperty_Succeeds_WhenPropertyCanBePatched(
            string path)
        {
            // Arrange
            var valueToPatch = Guid.NewGuid().ToString();
            var jObject = JObject.Parse(@"{
    ""foo"": {
        ""integerProperty"": 7,
        ""nullPropery"": null,
        ""emptyStringProperty"": """",
        ""arrayProperty"": [],
        ""objectProperty"": {}
    }
}");
            var jsonPath = new JsonPath(path);

            // Act
            jObject.PatchProperty(jsonPath, valueToPatch);

            // Assert
            var patchedProperty = jObject.SelectToken(path);
            Assert.NotNull(patchedProperty);
            Assert.Equal(valueToPatch, patchedProperty.Value<string>());
        }

        [Theory]
        [InlineData("foo.integerProperty.nonExistantProperty")]
        [InlineData("foo.nullPropery.nonExistantProperty")]
        [InlineData("foo.emptyStringProperty.nonExistantProperty")]
        [InlineData("foo.arrayProperty.nonExistantProperty")]
        public void PatchProperty_ThrowsWhenPropertyCannotBePatched(string path)
        {
            // Arrange
            var valueToPatch = Guid.NewGuid().ToString();
            var jObject = JObject.Parse(@"{
    ""foo"": {
        ""integerProperty"": 7,
        ""nullPropery"": null,
        ""emptyStringProperty"": """",
        ""arrayProperty"": [],
        ""objectProperty"": {}
    }
}");
            var jsonPath = new JsonPath(path);

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => jObject.PatchProperty(jsonPath, valueToPatch));
        }

        [Fact]
        public void Flatten_WorksCorrectly()
        {
            // Arrange
            var input = new
            {
                X = "foo",
                Y = 1,
                Z = new List<object>
                {
                    new { A = "a1", B = "1" },
                    new { A = "a2", B = "2" },
                },
            };

            var jObject = JObject.FromObject(input);

            // Act
            var output = jObject.Flatten();

            // Assert
            output.Should().NotBeNull();
        }

        [Theory]
        [InlineData("Quotes[0].CreatedDate", "2020-01-01")]
        [InlineData("Quotes[0].Customer.DisplayName", "Peter Smith")]
        [InlineData("Quotes[0].Calculation.Payment.PayableComponents.TotalPayable", "$1,231")]
        [InlineData("Quotes[0].Questions.RatingState", "VIC")]
        [InlineData("Events[0].CreatedDate", "2020-02-01")]
        [InlineData("Events[0].Tags[2]", "One last tag")]
        public void GetObject_JsonPathProvided_ShouldReturnCorrectValue(string path, string value)
        {
            // Arrange
            var jObject = GetJsonObject().CapitalizePropertyNames() as JObject;

            // Act
            var jValue = jObject.GetToken(path).ToString();

            // Assert
            jValue.Should().Be(value);
        }

        [Theory]
        [InlineData("/quotes/0/createdDate", "2020-01-01")]
        [InlineData("/quotes/0/customer/displayName", "Peter Smith")]
        [InlineData("/quotes/0/calculation/payment/payableComponents/totalPayable", "$1,231")]
        [InlineData("/quotes/0/questions/ratingState", "VIC")]
        [InlineData("/events/0/createdDate", "2020-02-01")]
        [InlineData("/events/0/tags/2", "One last tag")]
        public void GetObject_JsonPointerProvided_ShouldReturnCorrectValue(string path, string value)
        {
            // Arrange
            var jObject = GetJsonObject();

            // Act
            var jValue = jObject.GetToken(path).ToString();

            // Assert
            jValue.Should().Be(value);
        }

        [Theory]
        [InlineData("Quotes[0].CreatedDate", "/quotes/0/createdDate")]
        [InlineData("Quotes[0].Customer.DisplayName", "/quotes/0/customer/displayName")]
        [InlineData(
            "Quotes[0].Calculation.Payment.PayableComponents.TotalPayable",
            "/quotes/0/calculation/payment/payableComponents/totalPayable")]
        [InlineData("Quotes[0].Questions.RatingState", "/quotes/0/questions/ratingState")]
        [InlineData("Events[0].CreatedDate", "/events/0/createdDate")]
        [InlineData("Events[0].Tags[2]", "/events/0/tags/2")]
        public void GetObject_JsonPointerOrPath_ShouldReturnIdenticalValues(string jsonPath, string jsonPointer)
        {
            // Arrange
            var pointerObject = GetJsonObject();
            var pathObject = pointerObject.CapitalizePropertyNames() as JObject;

            // Act
            var jsonPathValue = pathObject.GetToken(jsonPath).ToString();
            var jsonPointerValue = pointerObject.GetToken(jsonPointer).ToString();

            // Assert
            jsonPathValue.Should().Be(jsonPointerValue);
        }

        private static JObject GetJsonObject()
        {
            var jsonString = @"{
    ""quotes"": [
        {
            ""createdDate"": ""2020-01-01"",
            ""quoteReference"": ""JSDEBN"",
            ""policyTransactionType"": ""newBusiness"",
            ""customer"": {
                ""displayName"": ""Peter Smith""
            },
            ""calculation"": {
                ""payment"": {
                    ""payableComponents"": {
                        ""totalPayable"": ""$1,231""
                    }
                }
            },
            ""questions"": {
                ""ratingState"": ""VIC""
            }
        },
    ],
    ""events"": [
        {
            ""createdDate"": ""2020-02-01"",
            ""createdTime"": ""21:57:39+00:00"",
            ""eventType"": ""quoteActualised"",
            ""tags"": [
                ""My tag name"",
                ""Another tag"",
                ""One last tag""
            ]
        },
    ]
}";
            return JObject.Parse(jsonString);
        }
    }
}
