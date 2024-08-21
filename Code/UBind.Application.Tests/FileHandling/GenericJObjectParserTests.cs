// <copyright file="GenericJObjectParserTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.FileHandling
{
    using FluentAssertions;
    using Newtonsoft.Json.Linq;
    using UBind.Application.FileHandling.Template_Provider;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="GenericJObjectParser"/>.
    /// </summary>
    public class GenericJObjectParserTests
    {
        [Fact]
        public void Initialization_ShouldBeAbleToParse_AnObjectTypeToken()
        {
            // Arrange
            var token = new JObject
            {
                { "fooNumber", 123 },
                { "fooName", "John Doe" },
                { "fooFlag", true },
            };
            string key = "Foo";

            // Act
            var fooParser = new GenericJObjectParser(key, token);
            var emptyPrefixParser = new GenericJObjectParser(string.Empty, token);

            // Assert
            var fooNumber = fooParser.JsonObject.GetValue("FooFooNumber").Value<long>();
            var fooName = fooParser.JsonObject.GetValue("FooFooName").Value<string>();
            var fooFlag = fooParser.JsonObject.GetValue("FooFooFlag").Value<bool>();
            fooNumber.Should().Be(123);
            fooName.Should().Be("John Doe");
            fooFlag.Should().BeTrue();

            var number = emptyPrefixParser.JsonObject.GetValue("FooNumber").Value<long>();
            var name = emptyPrefixParser.JsonObject.GetValue("FooName").Value<string>();
            var flag = emptyPrefixParser.JsonObject.GetValue("FooFlag").Value<bool>();
            number.Should().Be(123);
            name.Should().Be("John Doe");
            flag.Should().BeTrue();
        }

        [Fact]
        public void Initialization_ShouldParseAndFlatten_ArrayTypeToken()
        {
            var arrayToken = new JArray
            {
                new JValue("El Dorado"),
                new JValue("Arendelle"),
                new JValue("Far Far Away"),
            };
            var objectToken = new JObject
            {
                { "color", "white speckled" },
                { "breed", "Appaloosa" },
                { "gender", "Female" },
            };
            var token = new JObject
            {
                { "name", "Jane Doe" },
                { "flag", false },
                { "city", arrayToken },
                { "horse", objectToken },
            };

            // Act
            var parser = new GenericJObjectParser(string.Empty, token);

            // Assert
            var name = parser.JsonObject.GetValue("Name").Value<string>();
            var flag = parser.JsonObject.GetValue("Flag").Value<bool>();
            var city1 = parser.JsonObject.GetValue("City1").Value<string>();
            var city2 = parser.JsonObject.GetValue("City2").Value<string>();
            var city3 = parser.JsonObject.GetValue("City3").Value<string>();
            var horseColor = parser.JsonObject.GetValue("HorseColor").Value<string>();
            var horseBreed = parser.JsonObject.GetValue("HorseBreed").Value<string>();
            var horseGender = parser.JsonObject.GetValue("HorseGender").Value<string>();

            name.Should().Be("Jane Doe");
            flag.Should().BeFalse();
            city1.Should().Be("El Dorado");
            city2.Should().Be("Arendelle");
            city3.Should().Be("Far Far Away");
            horseColor.Should().Be("white speckled");
            horseBreed.Should().Be("Appaloosa");
            horseGender.Should().Be("Female");
        }

        [Fact]
        public void Initialization_ShouldParseAnObjectTypeTokenWithDuplicateKeys_WithoutThrowingASystemArgumentException()
        {
            // Arrange
            var objectToken2 = new JObject
            {
                { "id", "sample value 1" },
            };
            var objectToken1 = new JObject
            {
                { "tenantId", "sample value 1" },
                { "tenant", objectToken2 },
            };
            var sampleToken = new JObject
            {
                { "dataModel", objectToken1 },
            };
            string key = "Foo";

            // Act
            var fooParser = new GenericJObjectParser(key, sampleToken);

            // Assert
            var objectCount = fooParser.JsonObject.Count;
            var sampleKey1 = fooParser.JsonObject.GetValue("FooDataModelTenantId").Value<string>();

            objectCount.Should().Be(1);
            sampleKey1.Should().Be("sample value 1");
        }

        [Fact]
        public void Initialization_ShouldCapitalizeFirstLetterOfObjectKeys()
        {
            // Arrange
            var obj = JObject.FromObject(
                new
                {
                    property0 = new { childProperty0 = "Test", childProperty1 = "Test" },
                    property1 = new { childProperty0 = "Test", childProperty1 = "Test" },
                    property2 = new { childProperty0 = "Test", childProperty1 = "Test" },
                });

            // Act
            var flattenedObject = new GenericJObjectParser(string.Empty, obj, true).JsonObject;
            var unFlattenedObject = new GenericJObjectParser(string.Empty, obj, false).JsonObject;

            // Assert

            // flattenedObject
            foreach (var property in flattenedObject.Properties())
            {
                char.IsUpper(property.Name[0]).Should().BeTrue();
            }

            // Object with two-level hierarchy structure
            foreach (var property in unFlattenedObject.Properties())
            {
                char.IsUpper(property.Name[0]).Should().BeTrue();
                var childObj = (JObject)property.Value;
                foreach (var childProperty in childObj.Properties())
                {
                    char.IsUpper(childProperty.Name[0]).Should().BeTrue();
                }
            }
        }

        [Fact]
        public void Initialization_ShouldFlattenTheObjectStructure_WhenFlattenDataObjectParameterIsTrue()
        {
            // Arrange
            var obj = this.GenerateObject();

            var expectedObject = JObject.FromObject(
                new
                {
                    Id = "Test",
                    PersonalInfoFirstName = "Taylor",
                    PersonalInfoLastName = "Sheeesh",
                    Pets1Type = "Dog",
                    Pets1Name = "Bogart",
                    Pets2Type = "Iguana",
                    Pets2Name = "Alawi",
                    Pets3Type = "Cockroach",
                    Pets3Name = "Antonio",
                });

            // Act
            var parser = new GenericJObjectParser(string.Empty, obj, true);
            var parsedObject = parser.JsonObject;

            // Assert
            parsedObject.Should().Equal(expectedObject);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void Initialization_ShouldFlattenTheObjectStructureBasedOnStartIndex_WhenFlattenDataObjectParameterIsTrueAndStartIndexIsProvided(int startIndex)
        {
            // Arrange
            var obj = this.GenerateObject();

            var expectedObject = JObject.FromObject(
                new
                {
                    Id = "Test",
                    PersonalInfoFirstName = "Taylor",
                    PersonalInfoLastName = "Sheeesh",
                });
            expectedObject.Add($"Pets{startIndex}Type", "Dog");
            expectedObject.Add($"Pets{startIndex}Name", "Bogart");
            expectedObject.Add($"Pets{startIndex + 1}Type", "Iguana");
            expectedObject.Add($"Pets{startIndex + 1}Name", "Alawi");
            expectedObject.Add($"Pets{startIndex + 2}Type", "Cockroach");
            expectedObject.Add($"Pets{startIndex + 2}Name", "Antonio");

            // Act
            var parser = new GenericJObjectParser(string.Empty, obj, true, startIndex);
            var parsedObject = parser.JsonObject;

            // Assert
            parsedObject.Should().Equal(expectedObject);
        }

        [Fact]
        public void Initialization_ShouldNotFlattenTheObjectStructure_WhenFlattenDataObjectParameterIsFalse()
        {
            // Arrange
            var obj = this.GenerateObject();

            var notExpectedObject = JObject.FromObject(
                new
                {
                    Id = "Test",
                    PersonalInfoFirstName = "Taylor",
                    PersonalInfoLastName = "Sheeesh",
                    Pets1Type = "Dog",
                    Pets1Name = "Bogart",
                    Pets2Type = "Iguana",
                    Pets2Name = "Alawi",
                    Pets3Type = "Cockroach",
                    Pets3Name = "Antonio",
                });

            var expectedObject = JObject.FromObject(
                new
                {
                    Id = "Test",
                    PersonalInfo = new
                    {
                        FirstName = "Taylor",
                        LastName = "Sheeesh",
                    },
                    Pets = new dynamic[]
                    {
                        new { Type = "Dog", Name = "Bogart" },
                        new { Type = "Iguana", Name = "Alawi" },
                        new { Type = "Cockroach", Name = "Antonio" },
                    },
                });

            // Act
            var parser = new GenericJObjectParser(string.Empty, obj, false);
            var parsedObject = parser.JsonObject;

            // Assert
            parsedObject.Should().NotEqual(notExpectedObject);

            // This must be used in asserting nested objects
            var areTheseNestedObjectsEqual = JToken.DeepEquals(parsedObject, expectedObject);
            areTheseNestedObjectsEqual.Should().BeTrue();
        }

        private JObject GenerateObject()
        {
            var obj = new
            {
                id = "Test",
                personalInfo = new
                {
                    firstName = "Taylor",
                    lastName = "Sheeesh",
                },
                pets = new dynamic[]
                {
                    new { type = "Dog", name = "Bogart" },
                    new { type = "Iguana", name = "Alawi" },
                    new { type = "Cockroach", name = "Antonio" },
                },
            };
            return JObject.FromObject(obj);
        }
    }
}
