// <copyright file="WorkbookTextTableParserTests.cs" company="uBind">
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
    public class WorkbookTextTableParserTests
    {
        private WorkbookParserFixture fixture;

        public WorkbookTextTableParserTests(WorkbookParserFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void Parse_ParsesTableCorrectly_WhenStructureIsValid()
        {
            // Arrange
            var parser = new WorkbookTextTableParser(this.fixture.Workbook, this.fixture.ColumnToPropertyMapRegistry);

            // Act
            List<TextElement> textElements = parser.ParseForTextElements();

            // Assert
            textElements.Count.Should().Be(686 - 5 - 55 - 49);
            textElements[0].Category.Should().Be("Organisation");
            textElements[0].Subcategory.Should().BeNull();
            textElements[0].Name.Should().Be("Name");
            textElements[0].Text.Should().Be("Company Co Pty Ltd");
            textElements[10].Category.Should().Be("Product");
            textElements[10].Name.Should().Be("Title");
            textElements[10].Text.Should().Be("Special Insurance");
            textElements[14].Category.Should().Be("Sidebar");
            textElements[14].Name.Should().Be("Incomplete Header");
            textElements[14].Text.Should().Be("Answer a few simple questions to get an instant quote online.");
            textElements[280].Category.Should().Be("Workflow");
            textElements[280].Subcategory.Should().Be("Purchase Quote");
            textElements[280].Name.Should().Be("Close Button Label");
            textElements[280].Text.Should().Be("Cancel");
            textElements[287].Category.Should().Be("Workflow");
            textElements[287].Subcategory.Should().Be("Purchase Quote");
            textElements[287].Name.Should().Be("Enquiry Button");
            textElements[287].Icon.Should().Be("fa fa-phone");
            textElements[287].Text.Should().Be("Enquire");
            textElements[294].Category.Should().Be("Workflow");
            textElements[294].Subcategory.Should().Be("Purchase Details");
            textElements[294].Name.Should().Be("Tab Label");
            textElements[294].Icon.Should().BeNull();
            textElements[294].Text.Should().Be("Details");
        }
    }
}
