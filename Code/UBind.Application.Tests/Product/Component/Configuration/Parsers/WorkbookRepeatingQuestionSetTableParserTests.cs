// <copyright file="WorkbookRepeatingQuestionSetTableParserTests.cs" company="uBind">
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
    public class WorkbookRepeatingQuestionSetTableParserTests
    {
        private WorkbookParserFixture fixture;

        public WorkbookRepeatingQuestionSetTableParserTests(WorkbookParserFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void Parse_ParsesTableCorrectly_WhenStructureIsValid()
        {
            // Arrange
            var parser = new WorkbookRepeatingQuestionSetsTableParser(
                this.fixture.Workbook,
                this.fixture.FieldFactory,
                this.fixture.OptionSets,
                this.fixture.ColumnToPropertyMapRegistry);

            // Act
            List<RepeatingQuestionSet> repeatingQuestionSets = parser.Parse();

            // Assert
            repeatingQuestionSets.Count.Should().Be(5);
            repeatingQuestionSets[0].Fields.Count.Should().Be(2);
            repeatingQuestionSets[1].Fields.Count.Should().Be(0);
            repeatingQuestionSets[1].Name.Should().Be("Repeating 2");
            repeatingQuestionSets[0].Fields[0].Name.Should().Be("First Name");
            repeatingQuestionSets[0].Fields[0].GetType().Should().Be(typeof(SingleLineTextField));
            repeatingQuestionSets[0].Fields[0].DataType.Should().Be(DataType.Name);
            SingleLineTextField field = repeatingQuestionSets[0].Fields[0] as SingleLineTextField;
            field.CalculationWorkbookCellLocation.Value.SheetIndex.Should().Be(2);
            field.CalculationWorkbookCellLocation.Value.RowIndex.Should().Be(7);
            field.CalculationWorkbookCellLocation.Value.ColIndex.Should().Be(9);
        }
    }
}
