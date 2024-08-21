// <copyright file="WorkbookQuestionSetsTableParserTests.cs" company="uBind">
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
    public class WorkbookQuestionSetsTableParserTests
    {
        private WorkbookParserFixture fixture;

        public WorkbookQuestionSetsTableParserTests(WorkbookParserFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void Parse_ParsesTableCorrectly_WhenStructureIsValid()
        {
            // Arrange
            var parser = new WorkbookQuestionSetsTableParser(
                this.fixture.Workbook,
                this.fixture.FieldFactory,
                this.fixture.OptionSets,
                this.fixture.ColumnToPropertyMapRegistry);

            // Act
            List<QuestionSet> questionSets = parser.Parse();

            // Assert
            questionSets.Count.Should().Be(19);
            questionSets[0].Fields.Count.Should().Be(10);
            questionSets[1].Fields.Count.Should().Be(0);
            questionSets[1].Name.Should().Be("Rating Secondary");
            questionSets[2].Fields[0].Name.Should().Be("Policy Start Date");
            questionSets[2].Fields[0].GetType().Should().Be(typeof(HiddenField));
            questionSets[0].Fields[1].GetType().Should().Be(typeof(DropDownSelectField));
            ((DropDownSelectField)questionSets[0].Fields[1]).Label.Should().Be("Occupation");
            questionSets[0].Fields[2].GetType().Should().Be(typeof(SearchSelectField));
            SearchSelectField searchSelectField = (SearchSelectField)questionSets[0].Fields[2];
            searchSelectField.OptionsRequest.Should().NotBeNull();
            searchSelectField.OptionsRequest.UrlExpression.Should().Be(
                "getApplicationUrl() + '/api/v1/tenant/' + getTenantId() + '/product/' + getProductId() + "
                + "'/environment/' + getEnvironment() + '/automations/gnafAddressSearch?searchTerm=' + "
                + "fieldInputValue + '&maxResults=20'");
            searchSelectField.OptionsRequest.HttpVerb.Should().Be("GET");
            searchSelectField.CalculationWorkbookCellLocation.Value.SheetIndex.Should().Be(1);
            searchSelectField.CalculationWorkbookCellLocation.Value.RowIndex.Should().Be(9);
            searchSelectField.CalculationWorkbookCellLocation.Value.ColIndex.Should().Be(7);
            searchSelectField.BootstrapColumnsExtraSmall.Should().Be(12);
        }
    }
}
