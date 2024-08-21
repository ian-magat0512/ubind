// <copyright file="ObjectHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Xunit;

    public class ObjectHelperTests
    {
        private object TestObject => new
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

        private object TestList => new List<object>
        {
            "Grooming Kit",
            "Great Groomz",
            "456789",
            "12345679",
            "2500",
        };

        [Fact]
        public void ToDictionary_SuccessfulConversion_AnyObjectParameter()
        {
            // Arrange
            var obj = this.TestObject;

            // Act
            var output = ObjectHelper.ToDictionary(obj);

            // Assert
            output["email"].ToString().Should().Be("jim@something.com");
            var specifiedItems = ObjectHelper.ToDictionary(((List<object>)output["specifiedItems"]).First());
            specifiedItems["type"].Should().Be("Grooming Kit");
            var trailers = ObjectHelper.ToDictionary(((List<object>)output["trailers"]).First());
            trailers["value"].Should().Be("5000");
        }

        [Fact]
        public async Task ToDictionary_InvalidCastException_WhenConvertingListParameters()
        {
            // Arrange
            var list = this.TestList;

            // Act
            Func<Task> action = () => Task.Run(() =>
            {
                // Assuming ToDictionary tries to cast obj to a Dictionary
                return ObjectHelper.ToDictionary(list);
            });

            // Awaiting the assertion to catch exceptions thrown by the Task
            await action.Should().ThrowAsync<InvalidCastException>();
        }
    }
}
