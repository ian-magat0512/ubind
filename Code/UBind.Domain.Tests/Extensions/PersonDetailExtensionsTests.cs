// <copyright file="PersonDetailExtensionsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Extensions
{
    using FluentAssertions;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class PersonDetailExtensionsTests
    {
        [Fact]
        public void SetNameComponentsFromFullNameIfNoneAlreadySet_SetsFirstName_WhenNameComponentsNotSet()
        {
            // Arrange
            var personCommonProperties = new PersonCommonProperties();
            personCommonProperties.FullName = "Francisco Ian Felter Velasco";
            personCommonProperties.PreferredName = "Ian Velasco";

            // Act
            personCommonProperties.SetNameComponentsFromFullNameIfNoneAlreadySet();

            // Assert
            personCommonProperties.FirstName.Should().Be("Francisco");
        }

        [Fact]
        public void SetNameComponentsFromFullNameIfNoneAlreadySet_SetsLastName_WhenNameComponentsNotSet()
        {
            // Arrange
            var personCommonProperties = new PersonCommonProperties();
            personCommonProperties.FullName = "Francisco Ian Felter Velasco";
            personCommonProperties.PreferredName = "Ian Velasco";

            // Act
            personCommonProperties.SetNameComponentsFromFullNameIfNoneAlreadySet();

            // Assert
            personCommonProperties.LastName.Should().Be("Velasco");
        }

        [Fact]
        public void SetNameComponentsFromFullNameIfNoneAlreadySet_SetsMiddleNames_WhenNameComponentsNotSet()
        {
            // Arrange
            var personCommonProperties = new PersonCommonProperties();
            personCommonProperties.FullName = "Francisco Ian Felter Velasco";
            personCommonProperties.PreferredName = "Ian Velasco";

            // Act
            personCommonProperties.SetNameComponentsFromFullNameIfNoneAlreadySet();

            // Assert
            personCommonProperties.MiddleNames.Should().Be("Ian Felter");
        }

        [Theory]
        [InlineData("First", null, null)]
        [InlineData(null, "Middle", null)]
        [InlineData(null, null, "Last")]
        [InlineData("First", "middle", "Last")]
        public void SetNameComponentsFromFullNameIfNoneAlreadySet_DoesNotSetNameComponents_WhenAnyComponentSet(
            string firstName, string middleNames, string lastName)
        {
            // Arrange
            var personCommonProperties = new PersonCommonProperties();
            personCommonProperties.FullName = "Francisco Ian Felter Velasco";
            personCommonProperties.PreferredName = "Ian Velasco";
            personCommonProperties.FirstName = firstName;
            personCommonProperties.MiddleNames = middleNames;
            personCommonProperties.LastName = lastName;

            // Act
            personCommonProperties.SetNameComponentsFromFullNameIfNoneAlreadySet();

            // Assert
            personCommonProperties.FirstName.Should().Be(firstName);
            personCommonProperties.MiddleNames.Should().Be(middleNames);
            personCommonProperties.LastName.Should().Be(lastName);
        }

        [Fact]
        public void SetBasicFullName_SetsFullNameFormatToBasic()
        {
            // Arrange
            var personCommonProperties = new PersonCommonProperties();
            personCommonProperties.FullName = "Francisco Ian Felter Velasco";
            personCommonProperties.PreferredName = "Ian Velasco";
            personCommonProperties.NamePrefix = "Mr";
            personCommonProperties.FirstName = "Francisco";
            personCommonProperties.LastName = "Velasco";
            personCommonProperties.NameSuffix = string.Empty;
            personCommonProperties.MiddleNames = null;

            // Act
            personCommonProperties.SetNameComponentsFromFullNameIfNoneAlreadySet();
            personCommonProperties.SetBasicFullName();

            // Assert
            personCommonProperties.FullName.Should().Be("Francisco Velasco");
        }

        [Theory]
        [InlineData("Mr", "Ian Velasco", "Francisco", "Velasco", "Francisco Ian Felter Velasco", "Ian Velasco")]
        [InlineData("Mr", "", "Francisco", "Velasco", "Francisco Ian Felter Velasco", "Francisco")]
        [InlineData("Mr", "", "", "Velasco", "Francisco Ian Felter Velasco", "Mr Velasco")]
        [InlineData("", "", "", "Velasco", "Francisco Ian Felter Velasco", "Francisco Ian Felter Velasco")]
        [InlineData("", "", "", "Velasco", "", "Sir/Madam")]
        public void GetGreetingName_ReturnsTheExpectedValue(
            string namePrefix,
            string preferredName,
            string firstName,
            string lastName,
            string fullName,
            string expectedValue)
        {
            // Arrange
            var personalDetails = new FakePersonalDetails();
            personalDetails.NamePrefix = namePrefix;
            personalDetails.PreferredName = preferredName;
            personalDetails.FirstName = firstName;
            personalDetails.LastName = lastName;
            personalDetails.FullName = fullName;

            // Act
            var result = personalDetails.GetGreetingName();

            // Assert
            result.Should().Be(expectedValue);
        }
    }
}
