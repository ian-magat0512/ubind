// <copyright file="GemBoxCharacterFormatModelTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.FileHandling
{
    using FluentAssertions;
    using GemBox.Document;
    using UBind.Application.FileHandling.GemBoxServices.Models;
    using UBind.Application.FileHandling.Template_Provider;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="GenericJObjectParser"/>.
    /// </summary>
    public class GemBoxCharacterFormatModelTests
    {
        [Fact]
        public void Constructor_CreatesInInstanceWithNullProperties_WhenNoArgumentIsProvided()
        {
            // Arrange/Act
            var sut = new GemBoxCharacterFormatModel();

            // Assert
            sut.AllCaps.Should().BeNull();
            sut.BackgroundColor.Should().BeNull();
            sut.Bold.Should().BeNull();
            sut.Border.Should().BeNull();
            sut.FontColor.Should().BeNull();
            sut.FontName.Should().BeNull();
            sut.Hidden.Should().BeNull();
            sut.HighlightColor.Should().BeNull();
            sut.Italic.Should().BeNull();
            sut.Size.Should().BeNull();
            sut.SmallCaps.Should().BeNull();
            sut.Spacing.Should().BeNull();
            sut.Strikethrough.Should().BeNull();
            sut.Subscript.Should().BeNull();
            sut.Superscript.Should().BeNull();
            sut.UnderlineColor.Should().BeNull();
            sut.UnderlineStyle.Should().BeNull();
        }

        [Fact]
        public void IsDefault_ShouldReturnTrue_WhenAllPropertiesAreNull()
        {
            // Arrange
            var model = new GemBoxCharacterFormatModel();

            // Act
            var result = model.IsDefault();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsDefault_ShouldReturnFalse_WhenAtLeastOnePropertyIsNotNull()
        {
            // Arrange
            var model = new GemBoxCharacterFormatModel
            {
                AllCaps = true,
            };

            // Act
            var result = model.IsDefault();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void FromCharacterFormatDifference_ShouldReturnTheDifferenceOfTwoCharacterFormats_WhenThereAreAny()
        {
            // Arrange
            var format1 = this.CreateCharacterFormat();
            var format2 = this.CreateCharacterFormat();
            format2.AllCaps = true;
            format2.FontName = "Calibri";
            format2.Bold = true;

            // Act
            var result = GemBoxCharacterFormatModel.FromCharacterFormatDifference(format1, format2);

            // Assert
            result.IsDefault().Should().BeFalse();

            // these properties should not be null since they are different
            result.AllCaps.Should().BeTrue();
            result.FontName.Should().Be("Calibri");
            result.Bold.Should().BeTrue();

            // the rest of these properties should be null since they are the same
            result.BackgroundColor.Should().BeNull();
            result.Border.Should().BeNull();
            result.FontColor.Should().BeNull();
            result.Hidden.Should().BeNull();
            result.HighlightColor.Should().BeNull();
            result.Italic.Should().BeNull();
            result.Size.Should().BeNull();
            result.SmallCaps.Should().BeNull();
            result.Spacing.Should().BeNull();
            result.Strikethrough.Should().BeNull();
            result.Subscript.Should().BeNull();
            result.Superscript.Should().BeNull();
            result.UnderlineColor.Should().BeNull();
            result.UnderlineStyle.Should().BeNull();
        }

        [Fact]
        public void FromCharacterFormatDifference_ShouldReturnADefaultModel_WhenThereAreNoDifferences()
        {
            // Arrange
            var format1 = this.CreateCharacterFormat();
            var format2 = this.CreateCharacterFormat();

            // Act
            var result = GemBoxCharacterFormatModel.FromCharacterFormatDifference(format1, format2);

            // Assert
            result.IsDefault().Should().BeTrue();
        }

        [Fact]
        public void ApplyToCharacterFormat_ShouldApplyThePropertiesOfTheModelToTheCharacterFormat_WhenTheModelIsNotDefault()
        {
            // Arrange
            var format = new CharacterFormat();
            format.UnderlineStyle = UnderlineType.None;
            format.Italic = false;
            format.Strikethrough = false;
            var model = new GemBoxCharacterFormatModel
            {
                UnderlineStyle = UnderlineType.Single,
                Italic = true,
                Strikethrough = true,
            };

            // Act
            model.ApplyToCharacterFormat(format);

            // Assert
            format.UnderlineStyle.Should().Be(UnderlineType.Single);
            format.Italic.Should().BeTrue();
            format.Strikethrough.Should().BeTrue();
        }

        private CharacterFormat CreateCharacterFormat()
        {
            return new CharacterFormat
            {
                AllCaps = false,
                BackgroundColor = Color.Red,
                Bold = false,
                FontColor = Color.Green,
                FontName = "Arial",
                Hidden = false,
                Italic = false,
                Size = 12.0,
                SmallCaps = false,
                Spacing = 1.0,
                Strikethrough = false,
                Subscript = false,
                Superscript = false,
                UnderlineColor = Color.Black,
                UnderlineStyle = UnderlineType.None,
            };
        }
    }
}
