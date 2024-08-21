// <copyright file="IPersonDataExtensionsTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Extensions
{
    using FluentAssertions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class IPersonDataExtensionsTests
    {
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
