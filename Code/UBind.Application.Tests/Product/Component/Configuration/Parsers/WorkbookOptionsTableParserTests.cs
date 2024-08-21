// <copyright file="WorkbookOptionsTableParserTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Product.Component.Configuration.Parsers
{
    using System.Collections.Generic;
    using FluentAssertions;
    using UBind.Application.Product.Component.Configuration.Parsers;
    using UBind.Domain.Product.Component.Form;
    using Xunit;

    [Collection(WorkbookParserCollection.Name)]
    public class WorkbookOptionsTableParserTests
    {
        private WorkbookParserFixture fixture;

        public WorkbookOptionsTableParserTests(WorkbookParserFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void Parse_ParsesTableCorrectly_WhenStructureIsValid()
        {
            // Arrange
            var parser = new WorkbookOptionsTableParser(
                this.fixture.Workbook,
                this.fixture.ColumnToPropertyMapRegistry);

            // Act
            List<OptionSet> optionSets = parser.Parse();

            // Assert
            optionSets[0].Name.Should().Be("Occupations");
            optionSets[0].Options.Count.Should().Be(51);
            optionSets[0].Options[0].Label.Should().Be("Test 1");
            optionSets[0].Options[0].Properties["tradeId"].ToObject<int>().Should().Be(12345);
            optionSets[0].Options[0].Properties["contactPerson"].ToObject<string>().Should().Be("Mary Blige");
            optionSets[0].Options[0].Properties["requiresCertification"].ToObject<bool>().Should().BeTrue();
            optionSets[0].Options[1].Label.Should().BeNull();
            optionSets[2].Name.Should().Be("Liability Limit");
            optionSets[2].Options.Count.Should().Be(3);
            optionSets[2].Options[2].Icon.Should().Be("icon-10million");
            optionSets[24].Name.Should().Be("Payment Methods");
            optionSets[24].Options[1].HideConditionExpression.Should().NotBeNullOrEmpty();
            optionSets[24].Options[0].DisabledConditionExpression.Should().NotBeNullOrEmpty();
            optionSets[24].Options[0].SearchableText.Should().NotBeNullOrEmpty();
        }
    }
}
