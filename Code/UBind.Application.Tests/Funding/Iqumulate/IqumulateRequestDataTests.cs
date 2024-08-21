// <copyright file="IqumulateRequestDataTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Funding.IqumulatePfo
{
    using UBind.Application.Funding.Iqumulate;
    using UBind.Application.Tests.Funding.Iqumulate;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="IqumulateRequestDataTests" />.
    /// </summary>
    public class IqumulateRequestDataTests
    {
        /// <summary>
        /// The Constructor_SetsDataFromPersonalData.
        /// </summary>
        [Fact]
        public void Constructor_SetsDataFromPersonalData()
        {
            // Arrange
            var quote = QuoteFactory.CreateNewBusinessQuote();
            var personalDetails = new FakePersonalDetails();
            var quoteData = QuoteDataFactory.GetIQumulateSample();

            // Act
            var sut = new IqumulateRequestData(
                quoteData,
                new TestIqumulateConfiguration(),
                quote,
                personalDetails,
                false);

            // Assert
            Assert.Equal(personalDetails.Email, sut.CustomerEmail);
            Assert.Equal(personalDetails.MobilePhone, sut.MobileNumber);
            Assert.Equal(personalDetails.HomePhone, sut.TelephoneNumber);
        }
    }
}
