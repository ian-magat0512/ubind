// <copyright file="HtmlStringHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Helper
{
    using FluentAssertions;
    using UBind.Application.Automation.Helper;
    using Xunit;

    public class HtmlStringHelperTests
    {
        [Theory]
        [InlineData(
            "<p>Text not found</p>",
            "Incorrect Text",
            new string[] { })]
        [InlineData(
            "No Tags",
            "No Tags",
            new string[] { })]
        [InlineData(
            "<p class=\"test\">Simple Test</p>",
            "Simple Test",
            new string[] { "<p class=\"test\">" })]
        [InlineData(
            "<p><em>Italic</em>Word</p>",
            "Italic",
            new string[] { "<p>", "<em>" })]
        [InlineData(
            "<p><em>Italic</em> Word</p>",
            "Word",
            new string[] { "<p>" })]
        [InlineData(
            "<html><head><style></style></head><body><div>This is </div><div class=\"test\">a quite "
                + $"<div class=\"test2\">complex</div><p style=\"color: red;\"> Sample <b>Test</b> Data</p></div></body></html>",
            "This is",
            new string[] { "<html>", "<body>", "<div>" })]
        [InlineData(
            "<html><head><style></style></head><body><div>This is </div><div class=\"test\">a quite "
                + $"<div class=\"test2\">complex</div><p style=\"color: red;\"> Sample <b><br>Test</b> Data</p></div></body></html>",
            "a quite",
            new string[] { "<html>", "<body>", "<div class=\"test\">" })]
        [InlineData(
            "<html><head><style></style></head><body><div>This is </div><div class=\"test\">a quite "
                + $"<div class=\"test2\">complex</div><p style=\"color: red;\"> Sample <b>Test</b> Data</p></div></body></html>",
            "complex",
            new string[] { "<html>", "<body>", "<div class=\"test\">", "<div class=\"test2\">" })]
        [InlineData(
            "<html><head><style></style></head><body><div>This is </div><div class=\"test\">a quite "
                + $"<div class=\"test2\">complex</div><p style=\"color: red;\"> Sample <b>Test</b> Data</p></div></body></html>",
            "Sample",
            new string[] { "<html>", "<body>", "<div class=\"test\">", "<p style=\"color: red;\">" })]
        [InlineData(
            "<html><head><style></style></head><body><div>This is </div><div class=\"test\">a quite "
                + $"<div class=\"test2\">complex</div><p style=\"color: red;\"> Sample <b>Test</b> Data</p></div></body></html>",
            "Test",
            new string[] { "<html>", "<body>", "<div class=\"test\">", "<p style=\"color: red;\">", "<b>" })]
        [InlineData(
            "<html><head><style></style></head><body><div>This is </div><div class=\"test\">a quite "
                + $"<div class=\"test2\">complex</div><p style=\"color: red;\">Sample <b>Test</b> Data</p></div></body></html>",
            "Data",
            new string[] { "<html>", "<body>", "<div class=\"test\">", "<p style=\"color: red;\">" })]
        public void GetParentTagsOfText_ShouldReturnTheListOfParentTagsOfTheText(string htmlString, string text, string[] expectedResult)
        {
            // Arrange/Act
            var sut = HtmlStringHelper.GetParentTagsOfText(htmlString, text);

            // Assert
            sut.Should().Equal(expectedResult.Select(a => a).ToList());
        }

        // When there are multiple occurrence of the text within the html string, the GetParentTagsOfText method
        // will find the first occurrence of the text, as a content, and return its parent tags.
        // For Example: When text = "strong",
        // and htmlString="<p class=\"strong\" strong=\"strong\">I am<strong> {text}</strong>, but not strong enough</p>"
        // The method should return [ "<p class=\"strong\" strong=\"strong\">", "<strong>" ] since this are the parent tags
        // of the first occurrence of the word strong
        [Fact]
        public void GetParentTagsOfText_ShouldOnlyProcessTheFirstOccurenceOfTheText_WhenThereAreMultipleOccurrenceOfTextInTheHtmlString()
        {
            // Arrange

            // The text we want to process is "strong", however, there are two occurrence of the text as a content and
            // there is another occurrence as a tag prior to the first occurrence as a content.
            // What we should expect is the method should ignore all occurrence of the text
            // within the tag (attributes,attribute values, tag names, etc.), and will just search for its first occurrence
            // as a content and ignore the rest
            var text = "strong";
            var htmlString = $"<p class=\"{text}\" {text}=\"{text}\">I am<{text}> {text}</{text}>, but not {text} enough</p>";
            var expectedResult = new List<string>() { "<p class=\"strong\" strong=\"strong\">", "<strong>" };

            // Act
            var sut = HtmlStringHelper.GetParentTagsOfText(htmlString, text);

            // Assert
            sut.Should().Equal(expectedResult);
        }

        [Theory]
        [InlineData("<p class=\"no-style-attribute\">", new string[] { })]
        [InlineData("<p style=\"font-weight: bold;\">", new string[] { "font-weight" })]
        [InlineData("<p style=\"font-weight: bolder\">", new string[] { "font-weight" })]
        [InlineData("<p style=\"font-weight: bold; text-align: center;\">", new string[] { "font-weight", "text-align" })]
        [InlineData(
            "<p style=\"font-weight:bold; text-align:center;font-size:12px\">",
            new string[] { "font-weight", "text-align", "font-size" })]
        public void ExtractStyleAttributes_ShouldReturnListOfStyleAttributes_WhenATagContainsAny(string htmlTag, string[] expectedOutput)
        {
            // Arrange/Act
            var sut = HtmlStringHelper.ExtractStyleAttributes(htmlTag);

            // Assert
            sut.Should().Equal(expectedOutput);
        }

        [Theory]
        [InlineData("<p>")]
        [InlineData("<div id=\"test\">")]
        [InlineData("<span class=\"test\">")]
        public void ExtractStyleAttributes_ShouldReturnAnEmptyList_WhenATagHasNoStyleAttribute(string htmlTag)
        {
            // Arrange/Act
            var sut = HtmlStringHelper.ExtractStyleAttributes(htmlTag);

            // Assert
            sut.Should().Equal(Array.Empty<string>());
        }

        [Theory]
        [InlineData("<b>")]
        [InlineData("<bold>")]
        [InlineData("<strong id=\"sample\">")]
        [InlineData("<sup>")]
        [InlineData("<sub>")]
        [InlineData("<em class=\"italics\">")]
        [InlineData("<i>")]
        [InlineData("<u>")]
        [InlineData("<s>")]
        [InlineData("<strike>")]
        [InlineData("<del>")]
        public void IsCharacterFormattingTag_ShouldReturnTrue_WhenATagIsASupportedCharacterFormattingTag(string htmlTag)
        {
            // Arrange/Act
            var sut = HtmlStringHelper.IsCharacterFormattingTag(htmlTag);

            // Assert
            sut.Should().BeTrue();
        }

        [Theory]
        [InlineData("<div>")]
        [InlineData("<span>")]
        [InlineData("<table id=\"sample\">")]
        [InlineData("<ul>")]
        public void IsCharacterFormattingTag_ShouldReturnFalse_WhenATagIsNotASupportedCharacterFormattingTag(string htmlTag)
        {
            // Arrange/Act
            var sut = HtmlStringHelper.IsCharacterFormattingTag(htmlTag);

            // Assert
            sut.Should().BeFalse();
        }

        [Fact]
        public void IsCharacterFormattingTag_ShouldReturnFalse_WhenAStringIsNotATag()
        {
            // Arrange
            var str = "test";

            // Act
            var sut = HtmlStringHelper.IsCharacterFormattingTag(str);

            // Assert
            sut.Should().BeFalse();
        }

        [Theory]
        [InlineData("<strong id=\"test\">", "<strong>")]
        [InlineData("<div class=\"test\">", "<div>")]
        [InlineData("<p>", "<p>")]
        [InlineData("<p id=\"test\" class=\"test\" style=\"color:red; font-weight:bold; font-size:12px\">", "<p>")]
        public void RemoveTagAttributes_ShouldRemoveAnyAttributeInATag(string htmlTag, string expectedOutput)
        {
            // Arrange/Act
            var sut = HtmlStringHelper.RemoveTagAttributes(htmlTag);

            // Assert
            sut.Should().Be(expectedOutput);
        }

        [Fact]
        public void GetAttribute_ShouldReturnTheAttributeValue()
        {
            // Arrange
            var expectedOutput = "test";
            var tag = $"<p title='{expectedOutput}'>";

            // Act
            var sut = HtmlStringHelper.GetAttribute(tag, "title");

            // Assert
            sut.Should().Be(expectedOutput);
        }

        [Fact]
        public void GetAttribute_ShouldReturnNull_WhenTheAttributeDoesNotExist()
        {
            // Arrange
            var tag = $"<p title='test'>";

            // Act
            var sut = HtmlStringHelper.GetAttribute(tag, "non-existent_attribute");

            // Assert
            sut.Should().BeNull();
        }

        [Fact]
        public void GetAttribute_ShouldReturnNull_WhenTheStringIsNotATag()
        {
            // Arrange
            var tag = $"not a valid tag";

            // Act
            var sut = HtmlStringHelper.GetAttribute(tag, "non-existent_attribute");

            // Assert
            sut.Should().BeNull();
        }

        [Fact]
        public void RemoveTagAttributes_ShouldReturnAnEmptyString_WhenTheStringIsNotATag()
        {
            // Arrange
            var str = "test";

            // Act
            var sut = HtmlStringHelper.RemoveTagAttributes(str);

            // Assert
            sut.Should().BeEmpty();
        }

        [Fact]
        public void FindAllTags_ShouldReturnAllTags_WhenTagNameIsNotProvided()
        {
            // Arrange
            var htmlString = "<table class='test'></table><div><p><strong>test</strong></p></div>";
            var expectedOutput = new string[] { "<table class='test'>", "<div>", "<p>", "<strong>" };

            // Act
            var sut = HtmlStringHelper.FindAllTags(htmlString);

            // Assert
            sut.Should().Equal(expectedOutput);
        }

        [Fact]
        public void FindAllTags_ShouldReturnAllTagsThatMatchesTagName_WhenTagNameIsprovided()
        {
            // Arrange
            var htmlString = "<table class='test'></table><div><p id='yeah'><strong>test</strong></p> <p>2</p></div>";
            var expectedOutput = new string[] { "<p id='yeah'>", "<p>" };

            // Act
            var sut = HtmlStringHelper.FindAllTags(htmlString, "p");

            // Assert
            sut.Should().Equal(expectedOutput);
        }
    }
}
