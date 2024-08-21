// <copyright file="WorkbookTriggersTableParserTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Product.Component.Configuration.Parsers
{
    using System.Collections.Generic;
    using FluentAssertions;
    using UBind.Application.Product.Component.Configuration.Parsers;
    using UBind.Domain.Product.Component;
    using Xunit;

    [Collection(WorkbookParserCollection.Name)]
    public class WorkbookTriggersTableParserTests
    {
        private WorkbookParserFixture fixture;

        public WorkbookTriggersTableParserTests(WorkbookParserFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void Parse_ParsesTableCorrectly_WhenStructureIsValid()
        {
            // Arrange
            var parser = new WorkbookTriggersTableParser(this.fixture.Workbook, this.fixture.ColumnToPropertyMapRegistry);

            // Act
            List<Trigger> triggers = parser.ParseForTriggers();

            // Assert
            triggers.Count.Should().Be(13);
            triggers[0].TypeName.Should().Be("Review");
            triggers[0].CustomerMessage.Should().Be("Because you have selected $5,000 tools cover your quote needs to "
                + "be reviewed before you can complete your purchase.");
            triggers[4].Type.Should().Be("endorsement");
            triggers[4].DisplayPrice.Should().BeTrue();
            triggers[11].ReviewerExplanation.Should().Be("A decline has been triggered manually.");
        }
    }
}
