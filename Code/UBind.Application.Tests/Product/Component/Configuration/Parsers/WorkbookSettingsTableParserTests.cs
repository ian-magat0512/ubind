// <copyright file="WorkbookSettingsTableParserTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Product.Component.Configuration.Parsers
{
    using FluentAssertions;
    using UBind.Application.Product.Component.Configuration.Parsers;
    using UBind.Domain.Attributes;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;
    using Xunit;

    [Collection(WorkbookParserCollection.Name)]
    public class WorkbookSettingsTableParserTests
    {
        private WorkbookParserFixture fixture;
        private IAttributeObjectPropertyMapRegistry<WorkbookTableSectionPropertyNameAttribute> registry;

        public WorkbookSettingsTableParserTests(WorkbookParserFixture fixture)
            : base()
        {
            this.fixture = fixture;
            this.registry = new AttributeObjectPropertyMapRegistry<WorkbookTableSectionPropertyNameAttribute>();
        }

        [Fact]
        public void Parse_ParsesTableCorrectly_WhenStructureIsValid()
        {
            // Arrange
            var parser = new WorkbookSettingsTableParser(this.fixture.Workbook, this.registry);
            Component component = new Component();
            component.Form = new Form();

            // Act
            parser.Parse(component);

            // Assert
            var theme = component.Form.Theme;
            theme.IncludeQuoteReferenceInSidebar.Should().BeTrue();
            theme.TooltipIcon.Should().Be("fa fa-question-circle");
            theme.ShowPaymentOptionsInSidebar.Should().BeTrue();
            theme.FontAwesomeKitId.Should().Be("9ebe268e4c");
            theme.ExternalStyleSheetUrlExpressions.Should().HaveCount(2);
            theme.ExternalStyleSheetUrlExpressions[0].Should().Be("https://app.ubind.com.au/%{ myVar }%.css");
            theme.ExternalStyleSheetUrlExpressions[1].Should().Be("https://app.ubind.com.au/%{ myVar2 }%.css");
            component.Form.DefaultCurrencyCode.Should().Be("AUD");
        }
    }
}
