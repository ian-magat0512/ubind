// <copyright file="WorkbookStylesTableParserTests.cs" company="uBind">
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
    public class WorkbookStylesTableParserTests
    {
        private WorkbookParserFixture fixture;

        public WorkbookStylesTableParserTests(WorkbookParserFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void Parse_ParsesTableCorrectly_WhenStructureIsValid()
        {
            // Arrange
            var parser = new WorkbookStylesTableParser(this.fixture.Workbook, this.fixture.ColumnToPropertyMapRegistry);

            // Act
            List<Style> styles = parser.ParseForStyles();

            // Assert
            styles.Count.Should().Be(231 - 5 - 26);
            styles[0].Category.Should().Be("Panels");
            styles[0].Selector.Should().Be("body");
            styles[0].FontFamily.Should().Be("Roboto");
            styles[0].FontSize.Should().Be("15px");
            styles[0].FontWeight.Should().Be("400");
            styles[0].Colour.Should().Be("#2a2a2a");
            styles[19].Background.Should().Be("#f5f5f5");
            styles[19].Border.Should().Be("none");
            styles[2].MarginTop.Should().Be("40px");
            styles[2].MarginRight.Should().Be("30px");
            styles[2].MarginBottom.Should().Be("40px");
            styles[2].MarginLeft.Should().Be("30px");
            styles[19].PaddingTop.Should().Be("13px");
            styles[19].PaddingRight.Should().Be("12px");
            styles[19].PaddingBottom.Should().Be("11px");
            styles[19].PaddingLeft.Should().Be("12px");
            styles[19].CustomCss.Should().Be(
                "border-top-left-radius: 4px !important; border-top-right-radius: 4px !important; "
                + "border-bottom-left-radius: 0 !important; border-bottom-right-radius: 0 !important; outline: none; "
                + "box-shadow: none !important;");
        }
    }
}
