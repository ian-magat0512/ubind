// <copyright file="GemBoxDataSourceHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Helper
{
    using FluentAssertions;
    using Newtonsoft.Json.Linq;
    using UBind.Application.FileHandling.GemBoxServices.Helpers;
    using Xunit;

    public class GemBoxDataSourceHelperTests
    {
        [Fact]
        public void Format_ShouldWrapJObjectAndAllJObjectTypeProperties()
        {
            // Arrange
            var obj = this.GenerateObject();

            // the main object is wrapped in an array
            var expectedObject = new JArray(JObject.FromObject(
                new
                {
                    Id = "Test",

                    // a jobject property that is, originally, of type jobject is wrapped in an array
                    PersonalInfo = new[]
                    {
                        new { FirstName = "Taylor", LastName = "Sheeesh", },
                    },
                    Pets = new dynamic[]
                    {
                        new { Type = "Dog", Name = "Bogart" },
                        new { Type = "Iguana", Name = "Alawi" },
                        new
                        {
                            Type = "Cockroach",
                            Name = "Antonio",

                            // another jobject property that is, originally, of type jobject is wrapped in an array
                            SpecialAbility = new[]
                            {
                                new
                                {
                                    Name = "Terrorise",
                                    Description = "Injects fear into humans which makes them jump, scream and dance in terror",
                                },
                            },
                        },
                    },
                }));

            // Act
            var actualObject = GemBoxDataSourceHelper.Format(obj);

            // Assert
            var isEqual = JToken.DeepEquals(actualObject, expectedObject);
            isEqual.Should().BeTrue();
        }

        private JObject GenerateObject()
        {
            var obj = new
            {
                Id = "Test",

                // a jobject property that is of type jobject
                PersonalInfo = new
                {
                    FirstName = "Taylor",
                    LastName = "Sheeesh",
                },
                Pets = new dynamic[]
                {
                    new { Type = "Dog", Name = "Bogart" },
                    new { Type = "Iguana", Name = "Alawi" },
                    new
                    {
                        Type = "Cockroach",
                        Name = "Antonio",

                        // another jobject property that is of jobject
                        SpecialAbility = new
                        {
                            Name = "Terrorise",
                            Description = "Injects fear into humans which makes them jump, scream and dance in terror",
                        },
                    },
                },
            };
            return JObject.FromObject(obj);
        }
    }
}
