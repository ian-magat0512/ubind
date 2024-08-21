// <copyright file="JsonPrettifierTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Helpers
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Application.Helpers;
    using Xunit;

    public class JsonPrettifierTests
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
        public void PrettyPrint_HumanizesJsonKeys()
        {
            // Arrange
            var input = JsonConvert.SerializeObject(this.TestJson);

            // Act
            var output = JsonPrettifier.PrettyPrint(input);

            // Assert
            Assert.Contains("Rating state", output);
        }
    }
}
