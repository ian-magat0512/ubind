// <copyright file="PersonPropertyHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Helpers;
    using UBind.Domain.ReadModel;
    using Xunit;

    public class PersonPropertyHelperTests
    {
        public static IEnumerable<object[]> Data => new List<object[]>
        {
            new object[]
            {
                new PersonCommonProperties(), string.Empty,
            },
            new object[]
            {
                new PersonCommonProperties
                {
                    FirstName = " ",
                    FullName = " ",
                    Email = " ",
                    MobilePhoneNumber = string.Empty,
                },
                string.Empty,
            },
            new object[]
            {
                new PersonCommonProperties
                {
                    FirstName = "Alice",
                    FullName = "Patricia Andrews",
                    Email = "patricia@email.com",
                    MobilePhoneNumber = "0412341234",
                },
                "Alice",
            },
            new object[]
            {
                new PersonCommonProperties
                {
                    LastName = " Bob",
                    FullName = "Patricia Andrews",
                    Email = "patricia@email.com",
                    MobilePhoneNumber = "0412341234",
                },
                "Bob",
            },
            new object[]
            {
                new PersonCommonProperties
                {
                    FirstName = "Bob ",
                    LastName = " Robertson",
                    Email = "patricia@email.com",
                    MobilePhoneNumber = "0412341234",
                },
                "Bob Robertson",
            },
            new object[]
            {
                new PersonCommonProperties
                {
                    FullName = "Patricia Andrews",
                    Email = "patricia@email.com",
                    MobilePhoneNumber = "0412341234",
                },
                "Patricia Andrews",
            },
            new object[]
            {
                new PersonCommonProperties
                {
                    Email = "patricia@email.com",
                    MobilePhoneNumber = "0412341234",
                },
                "patricia@email.com",
            },
            new object[]
            {
                new PersonCommonProperties
                {
                    MobilePhoneNumber = "0412341234",
                },
                "0412341234",
            },
        };

        [Theory]
        [MemberData(nameof(Data))]
        public void PersonPropertyHelper_GetDisplayNameFromPersonalDetails_ReturnsCorrectStringValue(
            PersonCommonProperties properties, string expectedDisplayName)
        {
            // Act
            var displayName = PersonPropertyHelper.GetDisplayName(new PersonalDetails(Guid.NewGuid(), properties));

            // Assert
            displayName.Should().Be(expectedDisplayName);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData(" ", "")]
        [InlineData("Alice", "Alice")]
        [InlineData(" Bob   ", "Bob")]
        [InlineData("Juan Santiago", "Juan Santiago")]
        [InlineData("Carl Ian Geoff", "Carl Geoff")]
        [InlineData("Carl Ian Geoff LastName", "Carl LastName")]
        public void PersonPropertyHelper_GetDisplayName_ReturnsCorrectStringFormatLength_WhenInputIsCorrect(
            string rawFullName, string expectedDisplayName)
        {
            // Act
            var displayName = PersonPropertyHelper.GetDisplayName(rawFullName);

            // Assert
            displayName.Should().Be(expectedDisplayName);
        }
    }
}
