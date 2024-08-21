// <copyright file="JsonHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using FluentAssertions;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using Xunit;

    public class JsonHelperTests
    {
        private object TestJson => new
        {
            ratingState = "VIC",
            employeeCount = "1",
            subContractorCount = "1",
            PLLimit = "$20,000,000",
            name = "Jim",
            email = "jim@something.com",
            phone = string.Empty,
            mobile = "0412456789",
            animalsInTransit = "Yes",
            trailerCover = "Yes",
            toolsCover = "Yes",
            accidentCover = "Plus Illness",
            accidentPerWeek = "$750",
            policyStartDate = "05/10/2017",
            personalHeading = string.Empty,
            applicantName = "ABC Company Pty Ltd",
            tradingName = "Groomz",
            ABN = "12456789456",
            occupation = "Mobile Dog Grooming",
            businessActivities = "Going around and grooming doggz",
            estimatedTurnover = "100000",
            annualWages = "75000",
            annualSubContactorCosts = "10000",
            GSTRegistered = "Yes",
            businessHeading = string.Empty,
            businessAddress = "1/52 Factory Drive",
            businessTown = "Thomastown",
            businessState = "VIC",
            businessPostcode = "3456",
            postalHeading = string.Empty,
            postalAddress = string.Empty,
            postalTown = string.Empty,
            postalState = string.Empty,
            postalPostcode = string.Empty,
            employees = new List<object>
            {
                new
                {
                  name = "Willy Duggan",
                  DOB = "05/04/1976",
                  height = "180",
                  weight = "85",
                  existingConditions = "Sore back",
                },
            },
            subContractors = new List<object>
            {
                new
                {
                  name = "Mary Mede",
                  DOB = "14/08/1985",
                  height = "170",
                  weight = "65",
                  existingConditions = "Bung hip",
                },
            },
            trailers = new List<object>
            {
                new
                {
                    year = "2001",
                    make = "Toyota",
                    modelNo = "TR123456",
                    registrationNo = "ABC123",
                    value = "5000",
                },
            },
            specifiedItems = new List<object>
            {
                new
                {
                    type = "Grooming Kit",
                    make = "Great Groomz",
                    modelNo = "456789",
                    serialNo = "12345679",
                    value = "2500",
                },
            },
            disclosureHeading = string.Empty,
            history = "No",
            historyDetails = string.Empty,
            insolvency = "No",
            insolvencyDetails = string.Empty,
            declaration = "Yes",
            paymentOption = "Monthly",
            paymentMethod = string.Empty,
            creditCardPhoneNumber = string.Empty,
            accountName = "Jim Smith",
            BSB = "063123",
            accountNumber = "123456789",
            paymentOptionEmailInvoice = string.Empty,
            directDebitDeclaration = "Yes",
        };

        [Fact]
        public void CapitalizePropertyNames_Capitalizes_ThePropertyNamesOfObjectTypeToken()
        {
            // Arrange
            var jToken = JToken.FromObject(this.TestJson);

            // Act
            var output = JsonConvert.SerializeObject(JsonHelper.CapitalizePropertyNames(jToken));

            // Assert
            Assert.Contains("RatingState", output);
            Assert.Contains("Height", output);
        }

        [Fact]
        public void CapitalizePropertyNames_Capitalizes_TheItemsOfArrayTypeToken()
        {
            // Arrange
            var jToken = JToken.FromObject(this.TestJson);
            var jArray = jToken.SelectToken("employees");

            // Act
            var output = JsonConvert.SerializeObject(JsonHelper.CapitalizePropertyNames(jArray));

            // Assert
            Assert.Contains("Height", output);
        }

        [Fact]
        public void CapitalizePropertyNames_Capitalizes_TheNameOfPropertyTypeToken()
        {
            // Arrange
            var jToken = JToken.FromObject(this.TestJson);
            var jProperty = jToken.Children().First();

            // Act
            var output = JsonHelper.CapitalizePropertyNames(jProperty);

            // Assert
            Assert.Equal("RatingState", ((JProperty)output).Name);
        }

        [Fact]
        public void CapitalizePropertyNames_Return_TokenValue_OfStringTypeToken()
        {
            // Arrange
            var stringValue = "stringValue";
            var jToken = JToken.FromObject(stringValue);

            // Act
            var output = JsonHelper.CapitalizePropertyNames(jToken);

            // Assert
            Assert.Equal(JTokenType.String, output.Type);
            Assert.Contains(stringValue, JsonConvert.SerializeObject(output));
        }

        [Fact]
        public void CapitalizePropertyNames_Return_TokenValue_OfIntegerTypeToken()
        {
            // Arrange
            var integerValue = 1;
            var jToken = JToken.FromObject(integerValue);

            // Act
            var output = JsonHelper.CapitalizePropertyNames(jToken);

            // Assert
            Assert.Equal(JTokenType.Integer, output.Type);
            Assert.Equal("1", JsonConvert.SerializeObject(output));
        }

        [Fact]
        public void CapitalizePropertyNames_Return_TokenValue_OfBooleanTypeToken()
        {
            // Arrange
            var booleanValue = true;
            var jToken = JToken.FromObject(booleanValue);

            // Act
            var output = JsonHelper.CapitalizePropertyNames(jToken);

            // Assert
            Assert.Equal(JTokenType.Boolean, output.Type);
            Assert.Equal("true", JsonConvert.SerializeObject(output));
        }

        [Fact]
        public void CapitalizePropertyNames_Return_TokenValue_OfByteTypeToken()
        {
            // Arrange
            var bytesValue = new UTF8Encoding().GetBytes("byteValue");
            var jToken = JToken.FromObject(bytesValue);

            // Act
            var output = JsonHelper.CapitalizePropertyNames(jToken);

            // Assert
            Assert.Equal(JTokenType.Bytes, output.Type);
            Assert.Equal(bytesValue, output);
        }

        [Fact]
        public void CapitalizePropertyNames_Return_TokenValue_OfDateTypeToken()
        {
            // Arrange
            var dateTimeValue = new DateTime(2020, 10, 10);
            var jToken = JToken.FromObject(dateTimeValue);

            // Act
            var output = JsonHelper.CapitalizePropertyNames(jToken);

            // Assert
            Assert.Equal(JTokenType.Date, output.Type);
            Assert.Contains("2020-10-10", JsonConvert.SerializeObject(output));
        }

        [Fact]
        public void CapitalizePropertyNames_Return_TokenValue_OfFloatTypeToken()
        {
            // Arrange
            var floatValue = 134.45E-2f;
            var jToken = JToken.FromObject(floatValue);

            // Act
            var output = JsonHelper.CapitalizePropertyNames(jToken);

            // Assert
            Assert.Equal(JTokenType.Float, output.Type);
            Assert.Equal("1.3445", JsonConvert.SerializeObject(output));
        }

        [Fact]
        public void CapitalizePropertyNames_Return_TokenValue_OfGuidTypeToken()
        {
            // Arrange
            var guidValue = Guid.NewGuid();
            var jToken = JToken.FromObject(guidValue);

            // Act
            var output = JsonHelper.CapitalizePropertyNames(jToken);

            // Assert
            Assert.Equal(JTokenType.Guid, output.Type);
            Assert.Contains(guidValue.ToString(), JsonConvert.SerializeObject(output));
        }

        [Fact]
        public void CapitalizePropertyNames_Return_TokenValue_OfUriTypeToken()
        {
            // Arrange
            var uriString = "http://www.google.com";
            var uriValue = new Uri(uriString);
            var jToken = JToken.FromObject(uriValue);

            // Act
            var output = JsonHelper.CapitalizePropertyNames(jToken);

            // Assert
            Assert.Equal(JTokenType.Uri, output.Type);
            Assert.Contains(uriString, JsonConvert.SerializeObject(output));
        }

        [Fact]
        public void CapitalizePropertyNames_Return_TokenValue_OfTimespanTypeToken()
        {
            // Arrange
            var timespan = new TimeSpan(1, 0, 0);
            var jToken = JToken.FromObject(timespan);

            // Act
            var output = JsonHelper.CapitalizePropertyNames(jToken);

            // Assert
            Assert.Equal(JTokenType.TimeSpan, output.Type);
            Assert.Contains("01:00:00", JsonConvert.SerializeObject(output));
        }

        [Fact]
        public void CapitalizePropertyNames_Return_TokenValue_OfConstructorTypeToken()
        {
            // Arrange
            var constructorValue = @"new Something(null)";
            var jToken = JToken.Parse(constructorValue);

            // Act
            var output = JsonHelper.CapitalizePropertyNames(jToken);

            // Assert
            Assert.Equal(JTokenType.Constructor, output.Type);
            Assert.Equal(constructorValue, JsonConvert.SerializeObject(output));
        }

        [Fact]
        public void CapitalizePropertyNames_Return_TokenValue_OfCommentTypeToken()
        {
            // Arrange
            var commentValue = @"/*this is a comment*/";
            var jToken = JToken.Parse(commentValue);

            // Act
            var output = JsonHelper.CapitalizePropertyNames(jToken);

            // Assert
            Assert.Equal(JTokenType.Comment, output.Type);
            Assert.Equal(commentValue, JsonConvert.SerializeObject(output));
        }

        [Fact]
        public void CapitalizePropertyNames_Return_TokenValue_OfUndefinedTypeToken()
        {
            // Arrange
            var undefinedValue = @"undefined";
            var jToken = JToken.Parse(undefinedValue);

            // Act
            var output = JsonHelper.CapitalizePropertyNames(jToken);

            // Assert
            Assert.Equal(JTokenType.Undefined, output.Type);
            Assert.Equal(undefinedValue, JsonConvert.SerializeObject(output));
        }

        [Fact]
        public void CapitalizePropertyNames_Return_TokenValue_OfNullTypeToken()
        {
            // Arrange
            var nullValue = @"null";
            var jToken = JToken.Parse(nullValue);

            // Act
            var output = JsonHelper.CapitalizePropertyNames(jToken);

            // Assert
            Assert.Equal(JTokenType.Null, output.Type);
            Assert.Equal(nullValue, JsonConvert.SerializeObject(output));
        }

        [Fact]
        public void ReplaceBackslashQuoteWithDoubleBackslashQuote_ReplacesWithDoubleBacklashQuote_InJsonString()
        {
            // Arrange
            string jsonString = "\"validation\": \"Validators.compose([ValidationService.customExpression('getQuoteType() == \\'newBusiness\\' || fieldValue != \\'\\'', 'You must answer this question')])\"";

            // Act
            string fixedJsonString = JsonHelper.ReplaceBackslashQuoteWithDoubleBackslashQuote(jsonString);

            // Assert
            fixedJsonString.Should().Be("\"validation\": \"Validators.compose([ValidationService.customExpression('getQuoteType() == \\\\'newBusiness\\\\' || fieldValue != \\\\'\\\\'', 'You must answer this question')])\"");
        }
    }
}
