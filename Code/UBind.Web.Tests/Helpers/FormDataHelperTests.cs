// <copyright file="FormDataHelperTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// CS1591 Missing XML comment for publicly visible type or member
// Suppress CS1591. The unit test method must be named correctly instead of adding a comment in it.
#pragma warning disable CS1591

namespace UBind.Web.Tests.Helpers
{
    using FluentAssertions;
    using Newtonsoft.Json.Linq;
    using UBind.Web.Helpers;
    using Xunit;

    public class FormDataHelperTests
    {
        [Theory]
        [InlineData("myfile.html:text/html:testFile::")]
        [InlineData("mypic.png:image/png:profileUpload:500:500")]
        public void FileDataRegex_Matches_FormFieldValuesWithFileData(string fieldValue)
        {
            FormDataHelper.FileDataRegex.IsMatch(fieldValue).Should().BeTrue();
        }

        [Theory]
        [InlineData("myRandomText:::asdf:")]
        public void FileDataRegex_DoesNotMatches_FormFieldValuesWithoutFileData(string fieldValue)
        {
            FormDataHelper.FileDataRegex.IsMatch(fieldValue).Should().BeFalse();
        }

        [Fact]
        public void GetQuestionAttachmentFieldPaths_ReturnsFieldPathsWithAttachmentData_WhenNoQuestionMetaDataExists()
        {
            // Arrange
            JObject formData = new JObject
            {
                { "myField", "myValue" },
                { "myAttachmentField", "myfile.html:text/html:testFile::" },
                {
                    "myRepeating", new JArray
                    {
                        new JObject
                        {
                            { "yourName", "Mary Frankenstein" },
                            { "myRepeatingAttachmentField", "mypic.png:image/png:profileUpload:500:500" },
                        },
                    }
                },
            };

            // Act
            var attachmentPaths = FormDataHelper.GetQuestionAttachmentFieldPaths(null, formData);

            // Assert
            attachmentPaths.Should().Contain("myAttachmentField");
            attachmentPaths.Should().Contain("myRepeating[0].myRepeatingAttachmentField");
        }
    }
}
