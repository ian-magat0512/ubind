// <copyright file="FormDataTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Aggregates.Quote
{
    using NodaTime;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class FormDataTests
    {
        [Fact]
        public void UpdateDates_WritesNewDatesUsingCorrectFormat()
        {
            // Arrange
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates(new LocalDate(2020, 02, 20), 12);
            var sut = new FormData(formDataJson);

            // Act
#pragma warning disable CS0618 // Type or member is obsolete
            var newFormData = sut.UpdateDates(new LocalDate(2021, 02, 20), new LocalDate(2022, 02, 20), FormDataPaths.Default);
#pragma warning restore CS0618 // Type or member is obsolete

            // Assert
            Assert.Contains(@"""policyStartDate"": ""20/02/2021""", newFormData.Json);
            Assert.Contains(@"""policyEndDate"": ""20/02/2022""", newFormData.Json);
        }
    }
}
