// <copyright file="QuoteDataFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Fakes
{
    using System.Collections.Generic;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Application.Funding.Iqumulate;
    using UBind.Domain.Aggregates.Quote.DataLocator.StandardQuoteDataRetriever;
    using UBind.Domain.Json;
    using UBind.Domain.ValueTypes;

    /// <summary>
    /// Factory for generating quote data for tests.
    /// </summary>
    public static class QuoteDataFactory
    {
        /// <summary>
        /// Gets a sample quote data instance.
        /// </summary>
        /// <returns>A new instance of <see cref="StandardQuoteDataRetriever"/>.</returns>
        public static StandardQuoteDataRetriever GetSample()
        {
            var startDate = SystemClock.Instance.GetCurrentInstant().InZone(Timezones.AET).Date;
            return GetSample(startDate, durationInMonths: 12);
        }

        /// <summary>
        /// Gets a sample quote data instance.
        /// </summary>
        /// <param name="startDate">The start date to use.</param>
        /// <param name="durationInMonths">The duration to set the end date from.</param>
        /// <returns>A new instance of <see cref="StandardQuoteDataRetriever"/>With dates as specified.</returns>
        public static StandardQuoteDataRetriever GetSample(LocalDate startDate, int durationInMonths)
        {
            return GetSample(startDate, startDate.PlusMonths(durationInMonths));
        }

        /// <summary>
        /// Gets a sample quote data instance.
        /// </summary>
        /// <param name="startDate">The start date to use.</param>
        /// <param name="duration">The duration to set the end date from.</param>
        /// <returns>A new instance of <see cref="StandardQuoteDataRetriever"/>With dates as specified.</returns>
        public static StandardQuoteDataRetriever GetSample(LocalDate startDate, Period duration)
        {
            return GetSample(startDate, startDate.Plus(duration));
        }

        /// <summary>
        /// Gets a sample quote data instance.
        /// </summary>
        /// <param name="startDate">The start date to use.</param>
        /// <param name="endDate">The end date to use.</param>
        /// <param name="effectiveDate">The effectivity date.</param>
        /// <param name="cancellationDate">The cancellation date.</param>
        /// <returns>A new instance of <see cref="StandardQuoteDataRetriever"/>With dates as specified.</returns>
        public static StandardQuoteDataRetriever GetSample(LocalDate startDate, LocalDate endDate, LocalDate? effectiveDate = null, LocalDate? cancellationDate = null)
        {
            if (!effectiveDate.HasValue)
            {
                effectiveDate = startDate;
            }

            var cancellation = cancellationDate.HasValue
                ? LocalDatePattern.Iso.Format(cancellationDate.Value).ToString()
                : string.Empty;

            var formModel = $@"{{
                  ""formModel"": {{
                    ""startDate"": ""{LocalDatePattern.Iso.Format(startDate)}"",
                    ""endDate"": ""{LocalDatePattern.Iso.Format(endDate)}"",
                    ""effectiveDate"": ""{LocalDatePattern.Iso.Format(effectiveDate.Value)}"",
                    ""cancellationDate"": ""{cancellation}"",
                    ""contactName"": ""John Smith"",
                    ""insuredName"": ""John Smith"",
                    ""contactAddressLine1"": ""1 Foo Street"",
                    ""contactAddressSuburb"": ""Fooville"",
                    ""contactAddressState"": ""VIC"",
                    ""contactAddressPostcode"": ""3000"",
                    ""inceptionDate"": ""{LocalDatePattern.Iso.Format(startDate)}"",
                    ""expiryDate"": ""{LocalDatePattern.Iso.Format(endDate)}"",
	                ""abn"": ""12345678901"",
	                ""tradingName"": ""My trading name"",
	                ""numberOfInstallments"": ""12"",
	                ""runoffQuestion"": ""no"",
	                ""businessEndDate"": ""{LocalDatePattern.Iso.Format(startDate)}""
                  }}
                }}";

            var calculationModel = @"{
	                                    ""payment"": {
		                                    ""CurrencyCode"": ""AUD"",
		                                    ""Total"": {
			                                    ""Premium"": 110
		                                    }
	                                    }
                                    }";

            var formData = new CachingJObjectWrapper(formModel);
            var calculationData = new CachingJObjectWrapper(calculationModel);
            return new StandardQuoteDataRetriever(formData, calculationData);
        }

        public static IQumulateQuoteData GetIQumulateSample()
        {
            return new IQumulateQuoteData
            {
                General = new Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever.GeneralData
                {
                    PaymentMethod = "either",
                },
                Introducer = new Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever.IntroducerData
                {
                    AffinitySchemeCode = "ABC",
                    IntroducerContactEmail = "no-reply@ubind.io",
                },
                Client = new Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever.ClientData
                {
                    LegalName = "Test Company Pty Ltd",
                    Abn = "44123123123",
                    StreetAddress = new UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever.Address
                    {
                        StreetLine1 = "123 Test St",
                        Suburb = "Testville",
                        Postcode = "3000",
                        State = State.VIC,
                    },
                    Email = "no-reply@ubind.io",
                    FirstName = "John",
                    Borrowers = new List<Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever.Borrower>
                    {
                        new Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever.Borrower
                        {
                            FirstName = "John",
                            LastName = "Smith",
                            DateOfBirth = "1990-01-01",
                            DriverLicense = "055551234",
                        },
                    },
                },
                Policies = new List<Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever.PolicyData>
                {
                    new Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever.PolicyData
                    {
                        PolicyNumber = "ABC-123",
                        PolicyClassCode = "MVN",
                        PolicyUnderwriterCode = "ABC",
                        PolicyInceptionDate = "2020-01-01",
                        PolicyExpiryDate = "20201-01-01",
                        PolicyAmount = "5000.00",
                    },
                },
            };
        }
    }
}
