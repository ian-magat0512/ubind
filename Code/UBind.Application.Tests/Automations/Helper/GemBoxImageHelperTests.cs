// <copyright file="GemBoxImageHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Automations.Helper
{
    using FluentAssertions;
    using GemBox.Document;
    using UBind.Application.FileHandling.GemBoxServices.Helpers;
    using Xunit;

    public class GemBoxImageHelperTests
    {
        private readonly Random random = new Random();
        private readonly double tolerance = 0.00001;

        [Theory]

        // Samples for 3:2 aspect ratio
        [InlineData(1200, 800, 100, 100, true, false, 100, 66.666667)]
        [InlineData(1200, 800, 100, 100, false, true, 150, 100)]
        [InlineData(1200, 800, 100, 100, true, true, 100, 66.666667)]
        [InlineData(1200, 800, 100, 100, false, false, 100, 100)]

        // Samples for 4:3 aspect ratio
        [InlineData(1024, 768, 100, 100, true, false, 100, 75)]
        [InlineData(1024, 768, 100, 100, false, true, 133.333333, 100)]
        [InlineData(1024, 768, 100, 100, true, true, 100, 75)]
        [InlineData(1024, 768, 100, 100, false, false, 100, 100)]

        // Samples for 2:3 aspect ratio
        [InlineData(800, 1200, 100, 100, true, false, 100, 150)]
        [InlineData(800, 1200, 100, 100, false, true, 66.666667, 100)]
        [InlineData(800, 1200, 100, 100, true, true, 66.666667, 100)]
        [InlineData(800, 1200, 100, 100, false, false, 100, 100)]

        // Samples for 16:9 aspect ratio
        [InlineData(1920, 1080, 100, 100, true, false, 100, 56.25)]
        [InlineData(1920, 1080, 100, 100, false, true, 177.777778, 100)]
        [InlineData(1920, 1080, 100, 100, true, true, 100, 56.25)]
        [InlineData(1920, 1080, 100, 100, false, false, 100, 100)]

        // Samples for 1:1 aspect ratio
        [InlineData(1080, 1080, 100, 100, true, false, 100, 100)]
        [InlineData(1080, 1080, 100, 100, false, true, 100, 100)]
        [InlineData(1080, 1080, 100, 100, true, true, 100, 100)]
        [InlineData(1080, 1080, 100, 100, false, false, 100, 100)]

        public void CalculateSize_ShouldCalculateSize_DependingOnTheProvidedParameters(
            double originalWidth,
            double originalHeight,
            double targetWidth,
            double targetHeight,
            bool preserveWidth,
            bool preserveHeight,
            double expectedWidth,
            double expectedHeight)
        {
            // Arrange
            var originalSize = new Size(originalWidth, originalHeight);
            var targetSize = new Size(targetWidth, targetHeight);

            // Act
            var sut = GemBoxImageHelper.CalculateSize(originalSize, targetSize, preserveWidth, preserveHeight);

            // Assert
            sut.Width.Should().BeApproximately(expectedWidth, this.tolerance);
            sut.Height.Should().BeApproximately(expectedHeight, this.tolerance);
        }

        [Fact]
        public void CalculateSize_ShouldPreserveTargetWidthAndScaleTargetHeight_WhenPreserveWidthIsTrueAndPreserveHeightIsFalse()
        {
            // Arrange
            var preserveWidth = true;
            var preserveHeight = false;

            double targetWidth = this.random.NextDouble();
            double targetHeight = this.random.NextDouble();
            double originalWidth = this.random.NextDouble();
            double originalHeight = this.random.NextDouble();

            var originalSize = new Size(originalWidth, originalHeight);
            var targetSize = new Size(targetWidth, targetHeight);

            // Act
            var sut = GemBoxImageHelper.CalculateSize(originalSize, targetSize, preserveWidth, preserveHeight);

            // Assert
            sut.Width.Should().Be(targetWidth);
            sut.Height.Should().NotBe(targetHeight);
        }

        [Fact]
        public void CalculateSize_ShouldPreserveTargetHeightAndScaleTargetWidth_WhenPreserveHeightIsTrueAndPreserveWidthIsFalse()
        {
            // Arrange
            var preserveWidth = false;
            var preserveHeight = true;

            double targetWidth = this.random.NextDouble();
            double targetHeight = this.random.NextDouble();
            double originalWidth = this.random.NextDouble();
            double originalHeight = this.random.NextDouble();

            var originalSize = new Size(originalWidth, originalHeight);
            var targetSize = new Size(targetWidth, targetHeight);

            // Act
            var sut = GemBoxImageHelper.CalculateSize(originalSize, targetSize, preserveWidth, preserveHeight);

            // Assert
            sut.Height.Should().Be(targetHeight);
            sut.Width.Should().NotBe(targetWidth);
        }

        [Fact]
        public void CalculateSize_ShouldPreserveTheLargerSideAndScaleTheOther_WhenPreserveHeightIsTrueAndPreserveWidthIsTrue()
        {
            // Arrange
            var preserveWidth = true;
            var preserveHeight = true;

            double targetWidth = this.random.NextDouble();
            double targetHeight = this.random.NextDouble();

            // let's set height as the larger side so that the target size's height will be preserved
            double originalHeight = 500;
            double originalWidth = 100;

            var originalSize = new Size(originalWidth, originalHeight);
            var targetSize = new Size(targetWidth, targetHeight);

            // Act
            var sut = GemBoxImageHelper.CalculateSize(originalSize, targetSize, preserveWidth, preserveHeight);

            // Assert
            sut.Height.Should().Be(targetHeight);
            sut.Width.Should().NotBe(targetWidth);
        }

        [Fact]
        public void CalculateSize_ShouldUseTargetSize_WhenPreserveHeightIsFalseAndPreserveWidthIsFalse()
        {
            // Arrange
            var preserveWidth = false;
            var preserveHeight = false;

            double targetWidth = this.random.NextDouble();
            double targetHeight = this.random.NextDouble();
            double originalWidth = this.random.NextDouble();
            double originalHeight = this.random.NextDouble();

            var originalSize = new Size(originalWidth, originalHeight);
            var targetSize = new Size(targetWidth, targetHeight);

            // Act
            var sut = GemBoxImageHelper.CalculateSize(originalSize, targetSize, preserveWidth, preserveHeight);

            // Assert
            sut.Should().Be(targetSize);
        }

        [Fact]
        public void CalculateSize_ShouldPreserveTheOriginalAspectRatio_WhenAtLeastOneSideIsSetToBePreserved()
        {
            // Arrange
            var preserveWidth = true;
            var preserveHeight = false;

            double targetWidth = this.random.NextDouble();
            double targetHeight = this.random.NextDouble();
            double originalWidth = this.random.NextDouble();
            double originalHeight = this.random.NextDouble();

            var originalSize = new Size(originalWidth, originalHeight);
            var targetSize = new Size(targetWidth, targetHeight);

            var originalAspectRatio = originalWidth / originalHeight;

            // Act
            var newSize = GemBoxImageHelper.CalculateSize(originalSize, targetSize, preserveWidth, preserveHeight);
            var sut = newSize.Width / newSize.Height;

            // Assert
            sut.Should().BeApproximately(originalAspectRatio, this.tolerance);
        }
    }
}
