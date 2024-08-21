// <copyright file="ValueTypeTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.ValueTypes
{
    using System;
    using FluentAssertions;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ValueTypes;
    using Xunit;

    public class ValueTypeTests
    {
        [Theory]
        [InlineData("www.example.com")]
        [InlineData("https://example.com")]
        [InlineData("https://www.website.com/")]
        [InlineData("http://www.website.com/path/to/page")]
        [InlineData("www.subdomain.example.com")]
        [InlineData("//website.com/path/to/page")]

        public void WebAddress_ShouldNotThrowException_WhenCreatingValidWebsiteAddress(string websiteAddress)
        {
            Action action = () =>
            {
                var website = new WebAddress(websiteAddress);
                website.ToString().Should().Be(websiteAddress);
            };

            action.Should().NotThrow<ErrorException>();
        }

        [Theory]
        [InlineData("plainaddress")]
        [InlineData("tel:45212136")]
        [InlineData("/path/to/page")]
        [InlineData("sms:45212136")]
        [InlineData("javascript:alert(\'hello\');")]
        public void WebAddress_ShouldThrowException_WhenCreatingInvalidWebsiteAddress(string websiteAddress)
        {
            Action action = () => { WebAddress webAddress = new WebAddress(websiteAddress); };
            action.Should().Throw<ErrorException>().And.Error.Title.Should().Be("Web Address invalid");
        }

        [Theory]
        [InlineData("403240016")]
        [InlineData("0732105432")]
        [InlineData("61206547985")]
        [InlineData("+61403240016")]
        public void PhoneNumber_ShouldNotThrowException_WhenCreatingValidAustralianPhoneNumber(string phoneNumber)
        {
            Action action = () =>
            {
                var phone = new PhoneNumber(phoneNumber);
                phone.ToString().Should().Be(phoneNumber);
            };

            action.Should().NotThrow<ErrorException>();
        }

        [Theory]
        [InlineData("email@example.com")]
        [InlineData("firstname.lastname@example.com")]
        [InlineData("email@subdomain.example.com")]
        [InlineData("firstname+lastname@example.com")]
        [InlineData("email@123.123.123.123")]
        public void EmailAddress_ShouldNotThrowException_WhenCreatingValidEmailAddress(string emailAddress)
        {
            Action action = () =>
            {
                var email = new EmailAddress(emailAddress);
                email.ToString().Should().Be(emailAddress);
            };

            action.Should().NotThrow<ErrorException>();
        }

        [Theory]
        [InlineData("#@%^%#$@#$@#.com")]
        [InlineData("@example.com")]
        [InlineData("email.example.com")]
        [InlineData("email@example@example.com")]
        public void EmailAddress_ShouldThrowException_WhenCreatingInvalidEmailAddress(string emailAddress)
        {
            Action action = () => { var email = new EmailAddress(emailAddress); };

            action.Should().Throw<ErrorException>().And.Error.Title.Should().Be("Email address invalid");
        }

        [Fact]
        public void ValueType_Address_ShouldHaveCorrectPersistedInformation()
        {
            var expectedAddress = "Expected address";
            var expectedSuburb = "Expected suburb";
            var expectedPostcode = "Expected postcode";
            var expectedState = "VIC";

            var address = new Address(expectedAddress, expectedSuburb, expectedPostcode, expectedState);
            address.Line1.Should().Be(expectedAddress);
            address.Suburb.Should().Be(expectedSuburb);
            address.Postcode.Should().Be(expectedPostcode);
            address.State.Should().Be((State)Enum.Parse(typeof(State), expectedState));
        }

        [Theory]
        [InlineData("http://www.test.com")]
        [InlineData("https://test.com")]
        [InlineData("https://www.test.com/")]
        [InlineData("http://www.test.com/path/to/page")]
        [InlineData("http://www.subdomain.example.com/page/to/page")]
        [InlineData("http://www.subdomain.example.com")]
        [InlineData("http://192.168.1.1:85")]
        [InlineData("https://example-website-here.test.com")]
        [InlineData("http://*.test.com")]
        [InlineData("http://192.168.1.1")]
        [InlineData("http://localhost:80")]
        [InlineData("http://localhost")]
        public void URL_ShouldNotThrowException_WhenCreatingValidURL(string testUrl)
        {
            Action action = () =>
            {
                var url = new Url(testUrl);
                url.ToString().Should().Be(testUrl);
            };

            action.Should().NotThrow<ErrorException>();
        }

        [Theory]
        [InlineData("www.test.com")] //without scheme
        [InlineData("https://System.Collections.Generic.List`1[System.Object]")] //with invalid characters
        public void URL_ShouldThrowException_WhenCreatingInvalidURL(string testUrl)
        {
            Action action = () =>
            {
                var url = new Url(testUrl);
                url.ToString().Should().Be(testUrl);
            };

            action.Should().Throw<ErrorException>();
        }
    }
}
