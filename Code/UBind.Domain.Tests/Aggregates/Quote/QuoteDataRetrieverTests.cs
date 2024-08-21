// <copyright file="QuoteDataRetrieverTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Aggregates.Quote
{
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Json;
    using Xunit;

    public class QuoteDataRetrieverTests
    {
        [Fact]
        public void QuoteDataRetriever_Should_Return_Quote_Data_When_Configured_Correctly()
        {
            // Arrage
            var fakeConfig = new FakeDataLocatorConfig(DefaultQuoteDatumLocations.Instance, DefaultDataLocations.Instance);
            var quoteDataRetriever = new StandardQuoteDataRetriever(fakeConfig, this.GetFormData(), this.GetCalculationData());

            // Act
            var insuredName = quoteDataRetriever.Retrieve(StandardQuoteDataField.InsuredName);
            var totalPremium = quoteDataRetriever.Retrieve<decimal?>(StandardQuoteDataField.TotalPremium);
            var customerName = quoteDataRetriever.Retrieve(StandardQuoteDataField.CustomerName);
            var customerEmail = quoteDataRetriever.Retrieve(StandardQuoteDataField.CustomerEmail);
            var customerMobile = quoteDataRetriever.Retrieve(StandardQuoteDataField.CustomerMobile);
            var customerPhone = quoteDataRetriever.Retrieve(StandardQuoteDataField.CustomerPhone);
            var currencyCode = quoteDataRetriever.Retrieve(StandardQuoteDataField.CurrencyCode);

            var address = quoteDataRetriever.Retrieve<UBind.Domain.ValueTypes.Address>(StandardQuoteDataField.Address);

            var tradingName = quoteDataRetriever.Retrieve(StandardQuoteDataField.TradingName);
            var abn = quoteDataRetriever.Retrieve(StandardQuoteDataField.Abn);
            var numberOfInstallment = quoteDataRetriever.Retrieve<int>(StandardQuoteDataField.NumberOfInstallments);
            var isRunOff = quoteDataRetriever.Retrieve<bool>(StandardQuoteDataField.IsRunOffPolicy);

            var businessEndDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.BusinessEndDate);
            var cancellationDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.CancellationEffectiveDate);
            var effectiveDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.EffectiveDate);
            var expiryDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.ExpiryDate);
            var inceptionDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.InceptionDate);

            // Assert
            insuredName.Should().Be("Arthur Cruz Jr.");
            totalPremium.Should().Be(110);
            customerName.Should().Be("Arthur Cruz");
            customerEmail.Should().Be("arthur.cruz@ubind.io");
            customerMobile.Should().Be("04 12345678");
            customerPhone.Should().Be("04 12345678");
            currencyCode.Should().Be("AUD");

            address.Line1.Should().Be("1 Foo Street");
            address.Suburb.Should().Be("Fooville");
            address.State.Should().Be(UBind.Domain.ValueTypes.State.VIC);
            address.Postcode.Should().Be("3000");

            tradingName.Should().Be("My trading name");
            abn.Should().Be("12345678901");
            numberOfInstallment.Should().Be(12);
            isRunOff.Should().Be(false);
            businessEndDate.Should().Be(new LocalDate(2021, 09, 20));
            cancellationDate.Should().Be(new LocalDate(2021, 09, 20));
            effectiveDate.Should().Be(new LocalDate(2021, 09, 20));
            expiryDate.Should().Be(new LocalDate(2021, 09, 20));
            inceptionDate.Should().Be(new LocalDate(2021, 09, 20));
        }

        [Fact]
        public void QuoteDataRetriever_Should_Use_DataLocator_Instead_Of_QuoteDataLocator()
        {
            // Arrage
            var dataLocators = new DataLocators();
            dataLocators.InsuredName = new System.Collections.Generic.List<DataLocation>();
            dataLocators.InsuredName.Add(new DataLocation(DataSource.FormData, "somethingthatdoesnotexists"));
            dataLocators.InsuredName.Add(new DataLocation(DataSource.FormData, "insuredFullName"));
            dataLocators.InsuredName.Add(new DataLocation(DataSource.FormData, "somethingthatdoesnotexists"));
            var fakeConfig = new FakeDataLocatorConfig(DefaultQuoteDatumLocations.Instance, dataLocators);
            var quoteDataRetriever = new StandardQuoteDataRetriever(fakeConfig, this.GetFormData(), this.GetCalculationData());

            // Act
            var insuredName = quoteDataRetriever.Retrieve(StandardQuoteDataField.InsuredName);

            // Assert
            insuredName.Should().Be("Arthur Cruz Jr.");
        }

        [Fact]
        public void QuoteDataRetriever_Should_Use_QuoteDataLocator_When_DataLocator_Is_Not_Resolvable()
        {
            // This is for backward compatibility.
            // Arrage
            var dataLocators = new DataLocators();
            dataLocators.InsuredName = new System.Collections.Generic.List<DataLocation>();
            dataLocators.InsuredName.Add(new DataLocation(DataSource.FormData, "somethingthatdoesnotexists"));
            dataLocators.InsuredName.Add(new DataLocation(DataSource.FormData, "somethingthatdoesnotexists"));
            dataLocators.InsuredName.Add(new DataLocation(DataSource.FormData, "somethingthatdoesnotexists"));
            var fakeConfig = new FakeDataLocatorConfig(DefaultQuoteDatumLocations.Instance, dataLocators);
            var quoteDataRetriever = new StandardQuoteDataRetriever(fakeConfig, this.GetFormData(), this.GetCalculationData());

            // Act
            var insuredName = quoteDataRetriever.Retrieve(StandardQuoteDataField.InsuredName);

            // Assert
            insuredName.Should().Be("Arthur Cruz2");
        }

        [Fact]
        public void QuoteDataRetriever_Should_Use_QuoteDataLocator_When_DataLocator_Is_Not_Available()
        {
            // This is for backward compatibility.

            // Arrage
            var fakeConfig = new FakeDataLocatorConfig(DefaultQuoteDatumLocations.Instance, null);
            var quoteDataRetriever = new StandardQuoteDataRetriever(fakeConfig, this.GetFormData(), this.GetCalculationData());

            // Act
            var insuredName = quoteDataRetriever.Retrieve(StandardQuoteDataField.InsuredName);
            var totalPremium = quoteDataRetriever.Retrieve<decimal?>(StandardQuoteDataField.TotalPremium);
            var customerName = quoteDataRetriever.Retrieve(StandardQuoteDataField.CustomerName);
            var customerEmail = quoteDataRetriever.Retrieve(StandardQuoteDataField.CustomerEmail);
            var customerMobile = quoteDataRetriever.Retrieve(StandardQuoteDataField.CustomerMobile);
            var customerPhone = quoteDataRetriever.Retrieve(StandardQuoteDataField.CustomerPhone);
            var currencyCode = quoteDataRetriever.Retrieve(StandardQuoteDataField.CurrencyCode);

            var address = quoteDataRetriever.Retrieve<UBind.Domain.ValueTypes.Address>(StandardQuoteDataField.Address);

            var tradingName = quoteDataRetriever.Retrieve(StandardQuoteDataField.TradingName);
            var abn = quoteDataRetriever.Retrieve(StandardQuoteDataField.Abn);
            var numberOfInstallment = quoteDataRetriever.Retrieve<int>(StandardQuoteDataField.NumberOfInstallments);
            var isRunOff = quoteDataRetriever.Retrieve<bool>(StandardQuoteDataField.IsRunOffPolicy);

            var businessEndDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.BusinessEndDate);
            var cancellationDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.CancellationEffectiveDate);
            var effectiveDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.EffectiveDate);
            var expiryDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.ExpiryDate);
            var inceptionDate = quoteDataRetriever.Retrieve<LocalDate?>(StandardQuoteDataField.InceptionDate);

            // Assert
            insuredName.Should().Be("Arthur Cruz2");
            totalPremium.Should().Be(110);
            customerName.Should().Be("Arthur Cruz");
            customerEmail.Should().Be("arthur.cruz@ubind.io");
            customerMobile.Should().Be("04 12345678");
            customerPhone.Should().Be("04 12345678");
            currencyCode.Should().Be("AUD");

            address.Line1.Should().Be("1 Foo Street");
            address.Suburb.Should().Be("Fooville");
            address.State.Should().Be(UBind.Domain.ValueTypes.State.VIC);
            address.Postcode.Should().Be("3000");

            tradingName.Should().Be("My trading name");
            abn.Should().Be("12345678901");
            numberOfInstallment.Should().Be(12);
            isRunOff.Should().Be(false);
            businessEndDate.Should().Be(new LocalDate(2021, 09, 20));
            cancellationDate.Should().Be(new LocalDate(2021, 09, 20));
            effectiveDate.Should().Be(new LocalDate(2021, 09, 20));
            expiryDate.Should().Be(new LocalDate(2021, 09, 20));
            inceptionDate.Should().Be(new LocalDate(2021, 09, 20));
        }

        private CachingJObjectWrapper GetFormData()
        {
            var formModel = $@"{{
                  ""formModel"": {{
                    ""startDate"": ""20/09/21"",
                    ""endDate"": ""20/09/21"",
                    ""effectiveDate"": ""20/09/21"",
                    ""cancellationDate"": ""20/09/21"",
                    ""contactName"": ""Arthur Cruz"",
                    ""contactEmail"": ""arthur.cruz@ubind.io"",
                    ""contactMobile"": ""04 12345678"",
                    ""contactPhone"": ""04 12345678"",
                    ""insuredName"": ""Arthur Cruz2"",
                    ""insuredFullName"": ""Arthur Cruz Jr."",
                    ""contactAddressLine1"": ""1 Foo Street"",
                    ""contactAddressSuburb"": ""Fooville"",
                    ""contactAddressState"": ""VIC"",
                    ""contactAddressPostcode"": ""3000"",
                    ""inceptionDate"": ""20/09/21"",
                    ""expiryDate"": ""20/09/21"",
                    ""abn"": ""12345678901"",
                    ""tradingName"": ""My trading name"",
                    ""numberOfInstallments"": ""12"",
                    ""runoffQuestion"": ""no"",
                    ""businessEndDate"": ""20/09/21""
                  }}
                }}";
            return new CachingJObjectWrapper(formModel);
        }

        private CachingJObjectWrapper GetCalculationData()
        {
            var calculationModel = @"{
                                        ""payment"": {
                                            ""currencyCode"": ""AUD"",
                                            ""total"": {
                                                ""premium"": 110
                                            }
                                        }
                                    }";
            return new CachingJObjectWrapper(calculationModel);
        }
    }
}
