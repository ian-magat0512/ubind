// <copyright file="RazorTextProviderTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using RazorEngine.Templating;
    using UBind.Application.Export;
    using UBind.Application.Person;
    using UBind.Application.Queries.AssetFile;
    using UBind.Application.Queries.Customer;
    using UBind.Application.Releases;
    using UBind.Application.Services;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Configuration;
    using UBind.Domain.Dto;
    using UBind.Domain.Entities;
    using UBind.Domain.Events;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class RazorTextProviderTests
    {
        private readonly Guid? performingUserId = Guid.NewGuid();
        private Guid tenantId = Guid.NewGuid();
        private string questionMetaData = "{\"questionMetaData\":{\"questionSets\":{\"ratingPrimary\":{\"textTest\":{\"dataType\":\"text\"},\"numberTestA\":{\"dataType\":\"number\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"numberTestB\":{\"dataType\":\"number\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"currencyTestA\":{\"dataType\":\"currency\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"currencyTestB\":{\"dataType\":\"currency\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"percentTestA\":{\"dataType\":\"percent\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"percentTestB\":{\"dataType\":\"percent\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"percentTestC\":{\"dataType\":\"percent\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"percentTestD\":{\"dataType\":\"percent\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"booleanTestA\":{\"dataType\":\"boolean\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"booleanTestB\":{\"dataType\":\"boolean\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"phoneTestA\":{\"dataType\":\"phone\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"phoneTestB\":{\"dataType\":\"phone\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"phoneTestC\":{\"dataType\":\"phone\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"phoneTestD\":{\"dataType\":\"phone\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"phoneTestE\":{\"dataType\":\"phone\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"phoneTestF\":{\"dataType\":\"phone\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"phoneTestG\":{\"dataType\":\"phone\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"abnTestA\":{\"dataType\":\"abn\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"abnTestB\":{\"dataType\":\"abn\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"testRepeating\":{\"dataType\":\"repeating\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false},\"emailTest\":{\"dataType\":\"email\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"}}},\"repeatingQuestionSets\":{\"testRepeating\":{\"repeatingXnumberTestA\":{\"dataType\":\"number\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXnumberTestB\":{\"dataType\":\"number\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXcurrencyTestA\":{\"dataType\":\"currency\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXcurrencyTestB\":{\"dataType\":\"currency\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXpercentTestA\":{\"dataType\":\"percent\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXpercentTestB\":{\"dataType\":\"percent\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXpercentTestC\":{\"dataType\":\"percent\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXpercentTestD\":{\"dataType\":\"percent\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXbooleanTestA\":{\"dataType\":\"boolean\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXbooleanTestB\":{\"dataType\":\"boolean\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXphoneTestA\":{\"dataType\":\"phone\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXphoneTestB\":{\"dataType\":\"phone\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXphoneTestC\":{\"dataType\":\"phone\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXphoneTestD\":{\"dataType\":\"phone\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXphoneTestE\":{\"dataType\":\"phone\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXphoneTestF\":{\"dataType\":\"phone\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXphoneTestG\":{\"dataType\":\"phone\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXabnTestA\":{\"dataType\":\"abn\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXabnTestB\":{\"dataType\":\"abn\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"},\"repeatingXemailTest\":{\"dataType\":\"email\",\"displayable\":true,\"canChangeWhenApproved\":false,\"private\":false,\"resetForNewQuotes\":false,\"summaryLabelExpression\":\"\",\"summaryPositionExpression\":\"\"}},\"repeating2\":{},\"repeating3\":{},\"repeating4\":{},\"repeating5\":{}}}}";

        private object FormModel => new
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

        private object RawFormModel => new
        {
            textTest = "5000000",
            numberTestA = "5000000",
            numberTestB = "6000000.50",
            currencyTestA = "5000000",
            currencyTestB = "6000000.123",
            percentTestA = "100%",
            percentTestB = "50.1",
            percentTestC = "50.12345",
            percentTestD = "5000.123",
            booleanTestA = "true",
            booleanTestB = "yes",
            phoneTestA = "+61312345678",
            phoneTestB = "+61412345678",
            phoneTestC = "0412345678",
            phoneTestD = "0512345678",
            phoneTestE = "1300123456",
            phoneTestF = "1-800-123456",
            phoneTestG = "131234",
            abnTestA = "12345678900",
            abnTestB = "01234567890",
            emailTest = "TeST@EmAIL.com",
            testRepeating = new List<object>
            {
                new
                {
                    repeatingXnumberTestA = "5000000",
                    repeatingXnumberTestB = "6000000.50",
                    repeatingXcurrencyTestA = "5000000",
                    repeatingXcurrencyTestB = "6000000.50",
                    repeatingXpercentTestA = "50",
                    repeatingXpercentTestB = "50.1",
                    repeatingXpercentTestC = "50.12345",
                    repeatingXpercentTestD = "5000.123",
                    repeatingXbooleanTestA = "true",
                    repeatingXbooleanTestB = "false",
                    repeatingXphoneTestA = "+61312345678",
                    repeatingXphoneTestB = "+61412345678",
                    repeatingXphoneTestC = "0412345678",
                    repeatingXphoneTestD = "0512345678",
                    repeatingXphoneTestE = "1300123456",
                    repeatingXphoneTestF = "1-800-123456",
                    repeatingXphoneTestG = "131234",
                    repeatingXabnTestA = "12345678900",
                    repeatingXabnTestB = "01234567890",
                    repeatingXemailTest = "TeST@EmAIL.com",
                },
            },
        };

        [Fact]
        public async Task Invoke_CanReadFormData_CheckIfRawJsonIsPrettified()
        {
            // Arrange
            var template = @"
                            numberTestA = @Model.Form[""numberTestA""]
                            numberTestB = @Model.Form[""numberTestB""]
                            currencyTestA = @Model.Form[""currencyTestA""]
                            currencyTestB = @Model.Form[""currencyTestB""]
                            percentTestA = @Model.Form[""percentTestA""]
                            percentTestB = @Model.Form[""percentTestB""]
                            percentTestC = @Model.Form[""percentTestC""]
                            percentTestD = @Model.Form[""percentTestD""]
                            booleanTestA = @Model.Form[""booleanTestA""]
                            booleanTestB = @Model.Form[""booleanTestB""]
                            phoneTestA = @Model.Form[""phoneTestA""]
                            phoneTestB = @Model.Form[""phoneTestB""]
                            phoneTestC = @Model.Form[""phoneTestC""]
                            phoneTestD = @Model.Form[""phoneTestD""]
                            phoneTestE = @Model.Form[""phoneTestE""]
                            phoneTestF = @Model.Form[""phoneTestF""]
                            phoneTestG = @Model.Form[""phoneTestG""]
                            abnTestA = @Model.Form[""abnTestA""]
                            abnTestB = @Model.Form[""abnTestB""]
                            emailTest = @Model.Form[""emailTest""]
                            repeatingXnumberTestA = @Model.Form[""testRepeating""][0][""repeatingXnumberTestA""]
                            repeatingXnumberTestB = @Model.Form[""testRepeating""][0][""repeatingXnumberTestB""]
                            repeatingXcurrencyTestA = @Model.Form[""testRepeating""][0][""repeatingXcurrencyTestA""]
                            repeatingXcurrencyTestB = @Model.Form[""testRepeating""][0][""repeatingXcurrencyTestB""]
                            repeatingXpercentTestA = @Model.Form[""testRepeating""][0][""repeatingXpercentTestA""]
                            repeatingXpercentTestB = @Model.Form[""testRepeating""][0][""repeatingXpercentTestB""]
                            repeatingXpercentTestC = @Model.Form[""testRepeating""][0][""repeatingXpercentTestC""]
                            repeatingXpercentTestD = @Model.Form[""testRepeating""][0][""repeatingXpercentTestD""]
                            repeatingXbooleanTestA = @Model.Form[""testRepeating""][0][""repeatingXbooleanTestA""]
                            repeatingXbooleanTestB = @Model.Form[""testRepeating""][0][""repeatingXbooleanTestB""]
                            repeatingXphoneTestA = @Model.Form[""testRepeating""][0][""repeatingXphoneTestA""]
                            repeatingXphoneTestB = @Model.Form[""testRepeating""][0][""repeatingXphoneTestB""]
                            repeatingXphoneTestC = @Model.Form[""testRepeating""][0][""repeatingXphoneTestC""]
                            repeatingXphoneTestD = @Model.Form[""testRepeating""][0][""repeatingXphoneTestD""]
                            repeatingXphoneTestE = @Model.Form[""testRepeating""][0][""repeatingXphoneTestE""]
                            repeatingXphoneTestF = @Model.Form[""testRepeating""][0][""repeatingXphoneTestF""]
                            repeatingXphoneTestG = @Model.Form[""testRepeating""][0][""repeatingXphoneTestG""]
                            repeatingXabnTestA = @Model.Form[""testRepeating""][0][""repeatingXabnTestA""]
                            repeatingXabnTestB = @Model.Form[""testRepeating""][0][""repeatingXabnTestB""]
                            repeatingXemailTest = @Model.Form[""testRepeating""][0][""repeatingXemailTest""]

@Raw(Model.Form.PrettyPrintHtmlTables())
";

            var sut = this.CreateSut(template, true);
            var applicationEvent = this.CreateApplicationEvent(true);

            // Act
            var text = await sut.Invoke(applicationEvent);

            // Assert Question Sets

            // number
            Assert.Contains("numberTestA = 5,000,000", text);
            Assert.Contains("numberTestB = 6,000,000.50", text);

            // Contains
            Assert.Contains("currencyTestA = $5,000,000", text);
            Assert.Contains("currencyTestB = $6,000,000.12", text);

            // percent
            Assert.Contains("percentTestA = 100%", text);
            Assert.Contains("percentTestB = 50.1%", text);
            Assert.Contains("percentTestC = 50.12345%", text);
            Assert.Contains("percentTestD = 5,000.123%", text);

            // boolean
            Assert.Contains("booleanTestA = Yes", text);
            Assert.Contains("booleanTestB = Yes", text);

            // phone
            Assert.Contains("phoneTestA = +61 (3) 1234-5678", text);
            Assert.Contains("phoneTestB = +61 412 345 678", text);
            Assert.Contains("phoneTestC = 0412 345 678", text);
            Assert.Contains("phoneTestD = (05) 1234-5678", text);
            Assert.Contains("phoneTestE = 1300 123 456", text);
            Assert.Contains("phoneTestF = 1800 123 456", text);
            Assert.Contains("phoneTestG = 13 12 34", text);
            Assert.Contains("abnTestA = 12 345 678 900", text);
            Assert.Contains("abnTestB = 01 234 567 890", text);
            Assert.Contains("emailTest = test@email.com", text);

            // Assert Repeating Question Sets

            // number
            Assert.Contains("repeatingXnumberTestA = 5,000,000", text);
            Assert.Contains("repeatingXnumberTestB = 6,000,000.50", text);

            // currency
            Assert.Contains("repeatingXcurrencyTestA = $5,000,000", text);
            Assert.Contains("repeatingXcurrencyTestB = $6,000,000.50", text);

            // percent
            Assert.Contains("repeatingXpercentTestA = 50%", text);
            Assert.Contains("repeatingXpercentTestB = 50.1%", text);
            Assert.Contains("repeatingXpercentTestC = 50.12345%", text);
            Assert.Contains("repeatingXpercentTestD = 5,000.123%", text);

            // boolean
            Assert.Contains("repeatingXbooleanTestA = Yes", text);
            Assert.Contains("booleanTestB = No", text);

            // phone
            Assert.Contains("repeatingXphoneTestA = +61 (3) 1234-5678", text);
            Assert.Contains("repeatingXphoneTestB = +61 412 345 678", text);
            Assert.Contains("repeatingXphoneTestC = 0412 345 678", text);
            Assert.Contains("repeatingXphoneTestD = (05) 1234-5678", text);
            Assert.Contains("repeatingXphoneTestE = 1300 123 456", text);
            Assert.Contains("repeatingXphoneTestF = 1800 123 456", text);
            Assert.Contains("repeatingXphoneTestG = 13 12 34", text);
            Assert.Contains("repeatingXabnTestA = 12 345 678 900", text);
            Assert.Contains("repeatingXabnTestB = 01 234 567 890", text);
            Assert.Contains("repeatingXemailTest = test@email.com", text);
        }

        [Fact]
        public async Task Invoke_ReturnsTextFromTemplate()
        {
            // Arrange
            var template = @"
This is a text line.
";
            var sut = this.CreateSut(template);
            var applicationEvent = this.CreateApplicationEvent();

            // Act
            var text = await sut.Invoke(applicationEvent);

            // Assert
            Assert.Contains("This is a text line", text);
        }

        [Fact]
        public async Task Invoke_CanReadFormData_WhenApplicationHasFormData()
        {
            // Arrange
            var template = @"
This is a text line.
This is from the model: @Model.Form[""email""].
";
            var sut = this.CreateSut(template);
            var applicationEvent = this.CreateApplicationEvent();

            // Act
            var text = await sut.Invoke(applicationEvent);

            // Assert
            Assert.Contains("jim@something.com", text);
        }

        [Fact]
        public async Task Invoke_CanPrettyPrintAllFormData_WhenApplicationHasFormData()
        {
            // Arrange
            var template = @"
New business submitted:

@Model.Form.PrettyPrint();
";
            var sut = this.CreateSut(template);
            var applicationEvent = this.CreateApplicationEvent();

            // Act
            var text = await sut.Invoke(applicationEvent);

            // Assert
            Assert.Contains("Going around and grooming doggz", text);
        }

        [Fact]
        public async Task Invoke_CanPrettyPrintAllTableFormData_WhenApplicationHasFormData()
        {
            var template = @"
New business submitted:

@Model.Form.PrettyPrintHtmlTables();
";
            var sut = this.CreateSut(template);
            var applicationEvent = this.CreateApplicationEvent();
            var text = await sut.Invoke(applicationEvent);
            Assert.Contains("&lt;td class=&quot;summary-value&quot;&gt;Going around and grooming doggz&lt;/td&gt;", text);
        }

        [Fact]
        public async Task Invoke_PayableComponents_CheckIfExposed()
        {
            var template = @"
Price Breakdown :

Total Payable : @Model.Calculation[""payment.payableComponents.totalPayable""]
";
            var sut = this.CreateSut(template);
            var applicationEvent = this.CreateApplicationEvent();

            // Act
            var text = await sut.Invoke(applicationEvent);

            // Assert
            Assert.Contains("Total Payable : $125.84", text);
        }

        [Fact]
        public async Task Invoke_RefundComponents_CheckIfExposed()
        {
            // Arrange
            var template = @"
Price Breakdown :

Total Payable : @Model.Calculation[""payment.refundComponents.totalPayable""]
";
            var sut = this.CreateSut(template);
            var applicationEvent = this.CreateApplicationEvent();
            var test = applicationEvent.Aggregate.GetQuoteOrThrow(applicationEvent.QuoteId).LatestCalculationResult.Data.Json;

            // Act
            var text = await sut.Invoke(applicationEvent);

            // Assert
            Assert.Contains("Total Payable : $0.00", text);
        }

        private RazorTextProvider CreateSut(string template, bool isUsedMetaData = false)
        {
            var dependencyProvider = new Mock<IExporterDependencyProvider>();
            IProductConfiguration productConfiguration;

            var templateNameJson = @"{ ""text"": ""foo.cshtml"" }";
            var templateNameModel = JsonConvert.DeserializeObject<FixedTextProviderModel>(templateNameJson);
            var emailConfiguration = new Mock<IEmailInvitationConfiguration>();
            var configurationService = new Mock<IConfigurationService>();
            configurationService.Setup(x => x.GetProductComponentConfiguration(
                It.IsAny<ReleaseContext>(),
                It.IsAny<WebFormAppType>()))
                .Returns(Task.FromResult(this.GetSampleProductComponentConfigurationForQuote()));
            var organisationService = new Mock<IOrganisationService>();
            var personDetails = new FakePersonalDetails();
            var personAggregate = PersonAggregate.CreatePersonFromPersonalDetails(
                this.tenantId,
                Guid.NewGuid(),
                personDetails,
                this.performingUserId,
                default);
            var customer = new CustomerReadModelDetail
            {
                Id = Guid.NewGuid(),
                PrimaryPersonId = Guid.NewGuid(),
                Environment = DeploymentEnvironment.Staging,
                UserId = Guid.NewGuid(),
                IsTestData = false,
                OwnerUserId = this.performingUserId.Value,
                OwnerPersonId = this.performingUserId.Value,
                OwnerFullName = "Bob Smith",
                UserIsBlocked = false,
                FullName = "Randy Walsh",
                NamePrefix = "Mr",
                FirstName = "Randy",
                LastName = "Walsh",
                PreferredName = "Rando",
                Email = "r.walsh@testemail.com",
                TenantId = this.tenantId,
                DisplayName = "Randy Walsh",
                OrganisationId = TenantFactory.DefaultId,
                OrganisationName = "Default Organisation",
            };

            var mediator = new Mock<ICqrsMediator>();
            mediator.Setup(s => s.Send(It.IsAny<GetCustomerByIdQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(customer));
            var productConfigurationProvider = new Mock<IProductConfigurationProvider>();
            var tenantService = new Mock<ITenantService>();
            var tenant = new Tenant(
                this.tenantId,
                TenantFactory.DefaultName,
                TenantFactory.DefaultAlias,
                null,
                default,
                default,
                SystemClock.Instance.GetCurrentInstant());
            tenantService.Setup(t => t.GetTenant(It.IsAny<Guid>())).Returns(tenant);
            var productService = new Mock<UBind.Application.IProductService>();
            var product = new Product(
                TenantFactory.DefaultId,
                ProductFactory.DefaultId,
                ProductFactory.DefaultProductName,
                ProductFactory.DefaultProductName,
                SystemClock.Instance.GetCurrentInstant());
            productService.Setup(p => p.GetProductById(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(product);
            var cachingResolverMock = new Mock<ICachingResolver>();
            cachingResolverMock.Setup(s => s.GetTenantOrThrow(It.IsAny<Guid>())).Returns(Task.FromResult(tenant));
            cachingResolverMock.Setup(s => s.GetProductOrThrow(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(product));

            var personService = new Mock<IPersonService>();
            var razorEngineService = RazorEngineService.Create();
            IFormDataPrettifier formDataPrettifier = new FormDataPrettifier(new FormDataFieldFormatterFactory(), NullLogger<FormDataPrettifier>.Instance);
            mediator
                .Setup(s => s.Send(It.IsAny<GetProductFileContentsByFileNameQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(System.Text.Encoding.UTF8.GetBytes(template)));
            productService.Setup(x => x.GetProductById(TenantFactory.DefaultId, ProductFactory.DefaultId)).Returns(product);

            List<string> displayableFields = isUsedMetaData ? null : new List<string>();
            if (displayableFields != null)
            {
                displayableFields.Add("businessActivities");
                displayableFields.Add("BasePremium");
            }

            List<string> repeatingDisplayableFields = new List<string>();
            DisplayableFieldDto displayableFieldsDto = new DisplayableFieldDto(displayableFields, repeatingDisplayableFields, true, true);

            FormDataSchema dataSchema = new FormDataSchema(JObject.Parse(this.questionMetaData));
            productConfiguration = new ProductConfiguration("{}", dataSchema);
            var config = this.CreateApplicationEvent(isUsedMetaData);
            configurationService
                .Setup(cs => cs.GetDisplayableFieldsAsync(It.IsAny<ReleaseContext>(), WebFormAppType.Quote))
                .Returns(Task.FromResult(displayableFieldsDto));

            organisationService
                .Setup(o => o.GetOrganisationSummaryForTenantIdAndOrganisationId(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new OrganisationReadModelSummary
                {
                    Alias = "sample",
                });

            var sut = new RazorTextProvider(
                templateNameModel?.Build(dependencyProvider.Object, productConfiguration),
                razorEngineService,
                emailConfiguration.Object,
                configurationService.Object,
                personService.Object,
                tenantService.Object,
                productService.Object,
                productConfiguration,
                SystemClock.Instance,
                formDataPrettifier,
                organisationService.Object,
                mediator.Object,
                cachingResolverMock.Object,
                new Mock<ILogger>().Object);
            return sut;
        }

        private ApplicationEvent CreateApplicationEvent(bool isUsedMetaData = false)
        {
            var formDataObj = new
            {
                formModel = isUsedMetaData ? this.RawFormModel : this.FormModel,
                questionAnswers = new { },
                repeatingQuestionAnswers = new { },
            };

            var quote = QuoteFactory.CreateNewBusinessQuote(TenantFactory.DefaultId);
            var quoteAggregate = quote.Aggregate
                .WithCustomerDetails(quote.Id)
                .WithFormData(quote.Id)
                .WithCalculationResult(quote.Id)
                .WithCustomer()
                .WithQuoteNumber(quote.Id);

            var formDataString = JsonConvert.SerializeObject(formDataObj);
            var formData = new Domain.Aggregates.Quote.FormData(formDataString);
            quote.UpdateFormData(formData, this.performingUserId, SystemClock.Instance.GetCurrentInstant());
            var eventType = QuoteEventTypeMap.Map(quoteAggregate.UnsavedEvents.Last());
            var applicationEvent = new ApplicationEvent(
                Guid.NewGuid(),
                eventType.First(),
                quoteAggregate,
                quote.Id,
                0,
                "1",
                quote.ProductReleaseId.Value);
            return applicationEvent;
        }

        private TextProviderModelConverter BuildConverter()
        {
            return new TextProviderModelConverter(
                new TypeMap
                {
                    { "fixed", typeof(FixedTextProviderModel) },
                });
        }

        private IProductComponentConfiguration GetSampleProductComponentConfigurationForQuote()
        {
            Component component = new Component
            {
                Form = new Form
                {
                    TextElements = new List<TextElement>
                    {
                        new TextElement
                        {
                            Category = "Organisation",
                            Name = "Name",
                            Text = "ABC Insurance Ltd.",
                        },
                        new TextElement
                        {
                            Category = "Product",
                            Name = "Title",
                            Text = "General Liability",
                        },
                    },
                },
            };

            var mockConfig = new Mock<IProductComponentConfiguration>();
            mockConfig.Setup(x => x.Component).Returns(component);
            mockConfig.Setup(x => x.Version).Returns("2.0.0");
            mockConfig.Setup(x => x.IsVersion2OrGreater).Returns(true);
            mockConfig.Setup(x => x.IsVersion1).Returns(false);
            return mockConfig.Object;
        }
    }
}
