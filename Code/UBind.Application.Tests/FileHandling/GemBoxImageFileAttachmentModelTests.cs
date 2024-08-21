// <copyright file="GemBoxImageFileAttachmentModelTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.FileHandling
{
    using FluentAssertions;
    using UBind.Application.FileHandling.GemBoxServices.Models;
    using UBind.Application.FileHandling.Template_Provider;
    using UBind.Domain.Exceptions;
    using Xunit;

    /// <summary>
    /// Unit tests for the <see cref="GenericJObjectParser"/>.
    /// </summary>
    public class GemBoxImageFileAttachmentModelTests
    {
        [Theory]
        [InlineData(null, "merge.field.value.image.data.invalid")]
        [InlineData("", "merge.field.value.image.data.invalid")]
        [InlineData(
            "Test Image One.jpg:image/jpeg",
            "merge.field.value.image.data.invalid")]
        [InlineData(
            "Test Image Four.jpg:application/jpeg:DF5147EF-5EAB-4911-B86D-46FD0EAB82EF:1280:831:169845",
            "merge.field.value.image.mime.type.invalid")]
        [InlineData(
            "Test Image Five.jpg:image/jpeg::1280:831:169845",
            "merge.field.value.image.attachment.id.not.found")]
        [InlineData(
            "invalid filename Two:image/jpeg:DF5147EF-5EAB-4911-B86D-46FD0EAB82EF:1280:831:169845",
            "merge.field.value.image.file.format.invalid")]
        [InlineData(
            "Test Image Four.jpg:image/jpeg:NOT-A-VALID-GUID:1280:831:169845",
            "merge.field.value.image.attachment.id.not.found")]
        [InlineData(
            "Test Image Four.jpg:image/jpeg:DF5147EF-5EAB-4911-B86D-46FD0EAB82EF:invalid_width:831:169845",
            "merge.field.value.image.size.invalid")]
        [InlineData(
            "Test Image Four.jpg:image/jpeg:DF5147EF-5EAB-4911-B86D-46FD0EAB82EF:1280:invalid_height:169845",
            "merge.field.value.image.size.invalid")]
        public void Create_ShouldThrowError_WhenDataIsInvalid(string data, string errorCode)
        {
            // Arrange
            var sut = () => { GemBoxImageFileAttachmentModel.Create(data, "fieldName"); };

            // Act, Assert
            sut.Should().Throw<ErrorException>().Which.Error.Code.Should().Be(errorCode);
        }

        [Fact]
        public void Create_ShouldReturnAnInstanceWithCorrectPropertyValues_WhenDataIsValid()
        {
            // Arrange
            var fileName = "Test Image Four.jpg";
            var mimeType = "image/jpeg";
            var attachementId = Guid.Parse("DF5147EF-5EAB-4911-B86D-46FD0EAB82EF");
            double width = 1280;
            double height = 831;
            var stringData = $"{fileName}:{mimeType}:{attachementId}:{width}:{height}";

            stringData.Should().NotBeNull();

            // Act
            var sut = GemBoxImageFileAttachmentModel.Create(stringData, "fieldName");

            // Assert
            sut.FileName.Should().Be(fileName);
            sut.MimeType.Should().Be(mimeType);
            sut.AttachmentId.Should().Be(attachementId);
            sut.Width.Should().Be(width);
            sut.Height.Should().Be(height);
        }
    }
}
