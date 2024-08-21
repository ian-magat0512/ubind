// <copyright file="SmsClientTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Sms.Clickatell
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;
    using FluentAssertions;
    using UBind.Application.Sms;
    using UBind.Application.Sms.Clickatell;
    using UBind.Domain.Tests;
    using UBind.Domain.ValueTypes;
    using Xunit;

    public class SmsClientTests
    {
        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task SmsSend_Succeeds_WhenSmsAndCredentialsAreValid()
        {
            // Arrange
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var mockSmsConfiguration = MockSMSConfiguration.CreateValidConfiguration();
            var smsClient = new ClickatellClient(mockSmsConfiguration);
            var from = new PhoneNumber(string.Empty);
            var to = new List<PhoneNumber>
                {
                    new PhoneNumber("2799900001"),
                };
            var content = "Hello Message!!!";
            var sms = new Sms(to, from, content);

            // Act
            var response = await smsClient.SendSms(sms);

            // Assert
            response.ErrorData.Should().BeNull();
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task SMSSend_Should_ThrowException_WhenNoRecipientNumberAsync()
        {
            // Arrange
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var mockSMSConfiguration = MockSMSConfiguration.CreateValidConfiguration();
            var smsClient = new ClickatellClient(mockSMSConfiguration);
            var from = new PhoneNumber(string.Empty);
            var to = new List<PhoneNumber>
                {
                    new PhoneNumber(string.Empty),
                };
            var content = "Hello Message!!!";
            var sms = new Sms(to, from, content);

            // Act
            Func<Task> act = async () => await smsClient.SendSms(sms);

            // Assert
            var exception = await act.Should().ThrowAsync<Exception>();
            exception.Which.Message.Contains("The string supplied did not seem to be a phone number.");
        }

        [Fact]
        [Trait("TestCategory", TestCategory.Slow)]
        public async Task SMSSend_Should_ReturnErrorResponse_WhenInvalidConfiguration()
        {
            // Arrange
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var mockSMSConfiguration = MockSMSConfiguration.CreateInvalidConfiguration();
            var smsClient = new ClickatellClient(mockSMSConfiguration);
            var from = new PhoneNumber(string.Empty);
            var to = new List<PhoneNumber>
                {
                    new PhoneNumber("2799900001"),
                };
            var content = "Hello Message!!!";
            var sms = new Sms(to, from, content);

            // Act
            var response = await smsClient.SendSms(sms);

            // Assert
            response.ResponseType.Should().Be(SmsResponseType.Error);
            response.ErrorData.ToString().Should().Contain("Invalid or missing integration API Key.");
        }

        public class MockSMSConfiguration : ISmsConfiguration
        {
            private MockSMSConfiguration(string url, string apiKey, string apiId)
            {
                this.Url = url;
                this.ApiKey = apiKey;
                this.ApiId = apiId;
            }

            public string Url { get; set; }

            public string ApiId { get; set; }

            public string ApiKey { get; set; }

            public static MockSMSConfiguration CreateValidConfiguration()
            {
                return new MockSMSConfiguration(
                    "https://platform.clickatell.com/v1/message",
                    "fzjVPmBHSIaXVFIYRlo9sQ==",
                    "1843729332db47b7bf8e00dcee11a3ee==");
            }

            public static MockSMSConfiguration CreateInvalidConfiguration()
            {
                return new MockSMSConfiguration(
                    "https://platform.clickatell.com/v1/message",
                    "invalidapikey",
                    "1843729332db47b7bf8e00dcee11a3ee==");
            }
        }
    }
}
