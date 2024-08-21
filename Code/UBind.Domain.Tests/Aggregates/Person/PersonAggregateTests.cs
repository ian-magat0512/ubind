// <copyright file="PersonAggregateTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Aggregates.Person
{
    using System;
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class PersonAggregateTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();

        [Theory]
        [InlineData("Ms", "MC", "Mary", "Christopher", "Mary Christopher", "MC")]
        [InlineData(null, "MC", "Mary", "Christopher", "Mary Christopher", "MC")]
        [InlineData("", "MC", "Mary", "Christopher", "Mary Christopher", "MC")]
        [InlineData(" ", "MC", "Mary", "Christopher", "Mary Christopher", "MC")]
        [InlineData("Ms", null, "Mary", "Christopher", "Mary Christopher", "Mary")]
        [InlineData("Ms", "", "Mary", "Christopher", "Mary Christopher", "Mary")]
        [InlineData("Ms", " ", "Mary", "Christopher", "Mary Christopher", "Mary")]
        [InlineData("Ms", "MC", null, "Christopher", "Mary Christopher", "MC")]
        [InlineData("Ms", "MC", "", "Christopher", "Mary Christopher", "MC")]
        [InlineData("Ms", "MC", " ", "Christopher", "Mary Christopher", "MC")]
        [InlineData("Ms", "MC", "Mary", null, "Mary Christopher", "MC")]
        [InlineData("Ms", "MC", "Mary", "", "Mary Christopher", "MC")]
        [InlineData("Ms", "MC", "Mary", " ", "Mary Christopher", "MC")]
        [InlineData("Ms", "MC", "Mary", "Christopher", null, "MC")]
        [InlineData("Ms", "MC", "Mary", "Christopher", "", "MC")]
        [InlineData("Ms", "MC", "Mary", "Christopher", " ", "MC")]
        [InlineData("Ms", null, null, "Christopher", "Mary Christopher", "Ms Christopher")]
        [InlineData("Ms", "", null, "Christopher", "Mary Christopher", "Ms Christopher")]
        [InlineData("Ms", " ", null, "Christopher", "Mary Christopher", "Ms Christopher")]
        [InlineData("Ms", null, "", "Christopher", "Mary Christopher", "Ms Christopher")]
        [InlineData("Ms", null, " ", "Christopher", "Mary Christopher", "Ms Christopher")]
        [InlineData(null, null, null, "Christopher", "Mary Christopher", "Mary Christopher")]
        [InlineData("", null, null, "Christopher", "Mary Christopher", "Mary Christopher")]
        [InlineData(" ", null, null, "Christopher", "Mary Christopher", "Mary Christopher")]
        [InlineData(null, null, null, null, null, "Sir/Madam")]
        [InlineData(null, null, null, null, "", "Sir/Madam")]
        [InlineData(null, null, null, null, " ", "Sir/Madam")]
        public void GreetingName_ReturnsCorrectValue_OnDifferentInputs(
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
            var person = PersonAggregate.CreatePersonFromPersonalDetails(
                TenantFactory.DefaultId, Guid.NewGuid(), personalDetails, this.performingUserId, SystemClock.Instance.GetCurrentInstant());

            // Assert
            person.GreetingName.Should().Be(expectedValue);
        }

        [Fact]
        public void AssociateUserAccount_ReturnsAnError_WhenAnExistingUserIsAlreadyPresent()
        {
            var personDetails = new FakePersonalDetails();
            personDetails.NamePrefix = "Ms";
            personDetails.FirstName = "Maria";
            personDetails.LastName = "Cruz";
            var person = PersonAggregate.CreatePersonFromPersonalDetails(
                TenantFactory.DefaultId, Guid.NewGuid(), personDetails, this.performingUserId, SystemClock.Instance.GetCurrentInstant());
            person.AssociateWithUserAccount(Guid.NewGuid(), this.performingUserId, SystemClock.Instance.GetCurrentInstant());

            // Act
            Action action = () => person.AssociateWithUserAccount(Guid.NewGuid(), this.performingUserId, SystemClock.Instance.GetCurrentInstant());

            // Assert
            action.Should().Throw<ErrorException>();
            var exception = Assert.Throws<ErrorException>(action);
            exception.Error.Message.Should()
                .Be(Errors.Person.CannotAssociateAUserAccountForAPersonWithExistingUser(person.Id).Message);
        }

        [Fact]
        public void CreateUserAccount_ReturnsAnError_WhenAnExistingUserIsAlreadyPresent()
        {
            // Arrange
            var personDetails = new FakePersonalDetails();
            personDetails.NamePrefix = "Ms";
            personDetails.FirstName = "Maria";
            personDetails.LastName = "Cruz";
            var person = PersonAggregate.CreatePersonFromPersonalDetails(
                TenantFactory.DefaultId, Guid.NewGuid(), personDetails, this.performingUserId, SystemClock.Instance.GetCurrentInstant());
            person.RecordUserAccountCreatedForPerson(Guid.NewGuid(), this.performingUserId, SystemClock.Instance.GetCurrentInstant());
            var secondUserId = Guid.NewGuid();

            // Act
            Action action = () => person.RecordUserAccountCreatedForPerson(secondUserId, this.performingUserId, SystemClock.Instance.GetCurrentInstant());

            // Assert
            action.Should().Throw<ErrorException>();
            var exception = Assert.Throws<ErrorException>(action);
            exception.Error.Message.Should()
                .Be(Errors.Person.CannotCreateAUserAccountForAPersonWithExistingUser(secondUserId).Message);
        }
    }
}
