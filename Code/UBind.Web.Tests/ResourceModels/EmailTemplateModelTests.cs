// <copyright file="EmailTemplateModelTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Tests.ResourceModels
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using UBind.Web.ResourceModels;
    using Xunit;

    /// <summary>
    /// Defines the <see cref="EmailTemplateModelTests" />.
    /// </summary>
    public class EmailTemplateModelTests
    {
        /// <summary>
        /// The SmtpServerHost_IsJudgedInvalid_WhenItHasInvalidValue.
        /// </summary>
        /// <param name="hostValue">The hostValue<see cref="string"/>.</param>
        [Theory]
        [InlineData("   ")]
        [InlineData("foo bar")]
        public void SmtpServerHost_IsJudgedInvalid_WhenItHasInvalidValue(string hostValue)
        {
            // Act
            var isValid = this.ValidateProperty(nameof(EmailTemplateResponseModel.SmtpServerHost), hostValue);

            // Assert
            Assert.False(isValid);
        }

        /// <summary>
        /// The SmtpServerHost_IsJudgedValid_WhenItHasValidValue.
        /// </summary>
        /// <param name="hostValue">The hostValue<see cref="string"/>.</param>
        [Theory]
        [InlineData("foo")]
        [InlineData("foo.bar")]
        public void SmtpServerHost_IsJudgedValid_WhenItHasValidValue(string hostValue)
        {
            // Act
            var isValid = this.ValidateProperty(nameof(EmailTemplateResponseModel.SmtpServerHost), hostValue);

            // Assert
            Assert.True(isValid);
        }

        /// <summary>
        /// The HtmlBody_IsJudgedValid_WhenItHasValidValue.
        /// </summary>
        /// <param name="htmlValue">The htmlValue<see cref="string"/>.</param>
        [Theory]
        [InlineData("foo")]
        [InlineData("{{foo}}")]
        [InlineData("foo {{bar}}")]
        [InlineData("{{foo}} {{foo}}")]
        [InlineData("{{{}}}")]
        public void HtmlBody_IsJudgedValid_WhenItHasValidValue(string htmlValue)
        {
            // Act
            var isValid = this.ValidateProperty(nameof(EmailTemplateResponseModel.HtmlBody), htmlValue);

            // Assert
            Assert.True(isValid);
        }

        /// <summary>
        /// The HtmlBody_IsJudgedInValid_WhenItHasInValidValue.
        /// </summary>
        /// <param name="htmlValue">The htmlValue<see cref="string"/>.</param>
        [Theory]
        [InlineData("{{")]
        [InlineData("{{foo")]
        [InlineData("{{foo}} {{")]
        public void HtmlBody_IsJudgedInValid_WhenItHasInValidValue(string htmlValue)
        {
            // Act
            var isValid = this.ValidateProperty(nameof(EmailTemplateResponseModel.HtmlBody), htmlValue);

            // Assert
            Assert.False(isValid);
        }

        /// <summary>
        /// The Name_IsJudgedValid_WhenItHasValidValue.
        /// </summary>
        /// <param name="iconValue">The iconValue<see cref="string"/>.</param>
        [Theory]
        [InlineData("foo")]
        [InlineData("foo bar")]
        public void Name_IsJudgedValid_WhenItHasValidValue(string iconValue)
        {
            // Act
            var isValid = this.ValidateProperty(nameof(EmailTemplateResponseModel.Name), iconValue);

            // Assert
            Assert.True(isValid);
        }

        /// <summary>
        /// The Name_IsJudgedInValid_WhenItHasInValidValue.
        /// </summary>
        /// <param name="iconValue">The iconValue<see cref="string"/>.</param>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("  ")]
        public void Name_IsJudgedInValid_WhenItHasInValidValue(string iconValue)
        {
            // Act
            var isValid = this.ValidateProperty(nameof(EmailTemplateResponseModel.Name), iconValue);

            // Assert
            Assert.False(isValid);
        }

        /// <summary>
        /// The Cc_IsJudgedValid_WhenItHasValidValue.
        /// </summary>
        /// <param name="validCcValue">The validCcValue<see cref="string"/>.</param>
        [Theory]
        [InlineData("a@a.com")]
        [InlineData("a@a.com, a@a.com")]
        [InlineData("a@a")]
        [InlineData("a@a, a@a")]
        [InlineData("a@a, a@a, a@a")]
        public void Cc_IsJudgedValid_WhenItHasValidValue(string validCcValue)
        {
            // Act
            var isValid = this.ValidateProperty(nameof(EmailTemplateResponseModel.Cc), validCcValue);

            // Assert
            Assert.True(isValid);
        }

        /// <summary>
        /// The Cc_IsJudgedInvalid_WhenItHasInvalidValue.
        /// </summary>
        /// <param name="invalidCcValue">The invalidCcValue<see cref="string"/>.</param>
        [Theory]
        [InlineData("a@")]
        [InlineData("a@a, a@")]
        [InlineData("a@a, a@a, a@")]
        [InlineData("a@a, a@a, 1")]
        public void Cc_IsJudgedInvalid_WhenItHasInvalidValue(string invalidCcValue)
        {
            // Act
            var isValid = this.ValidateProperty(nameof(EmailTemplateResponseModel.Cc), invalidCcValue);

            // Assert
            Assert.False(isValid);
        }

        /// <summary>
        /// The Bcc_IsJudgedValid_WhenItHasValidValue.
        /// </summary>
        /// <param name="validCcValue">The validCcValue<see cref="string"/>.</param>
        [Theory]
        [InlineData("a@a.com")]
        [InlineData("a@a.com, a@a.com")]
        [InlineData("a@a")]
        [InlineData("a@a, a@a")]
        [InlineData("a@a, a@a, a@a")]
        public void Bcc_IsJudgedValid_WhenItHasValidValue(string validCcValue)
        {
            // Act
            var isValid = this.ValidateProperty(nameof(EmailTemplateResponseModel.Bcc), validCcValue);

            // Assert
            Assert.True(isValid);
        }

        /// <summary>
        /// The ToAddress_IsJudgedValid_WhenItHasValidValue.
        /// </summary>
        /// <param name="validToAddressValue">The validToAddressValue<see cref="string"/>.</param>
        [Theory]
        [InlineData("foo")]
        [InlineData("{{foo}}")]
        [InlineData("foo {{bar}}")]
        [InlineData("{{foo}} {{foo}}")]
        [InlineData("{{{}}}")]
        public void ToAddress_IsJudgedValid_WhenItHasValidValue(string validToAddressValue)
        {
            // Act
            var isValid = this.ValidateProperty(nameof(EmailTemplateResponseModel.ToAddress), validToAddressValue);

            // Assert
            Assert.True(isValid);
        }

        /// <summary>
        /// The ToAddress_IsJudgedInvalid_WhenItHasInvalidValue.
        /// </summary>
        /// <param name="invalidToAddressValue">The invalidToAddressValue<see cref="string"/>.</param>
        [Theory]
        [InlineData("{{")]
        [InlineData("{{foo")]
        [InlineData("{{foo}} {{")]
        public void ToAddress_IsJudgedInvalid_WhenItHasInvalidValue(string invalidToAddressValue)
        {
            // Act
            var isValid = this.ValidateProperty(nameof(EmailTemplateResponseModel.ToAddress), invalidToAddressValue);

            // Assert
            Assert.False(isValid);
        }

        /// <summary>
        /// The Subject_IsJudgedValid_WhenItHasValidValue.
        /// </summary>
        /// <param name="subjectValue">The subjectValue<see cref="string"/>.</param>
        [Theory]
        [InlineData("foo")]
        [InlineData("{{foo}}")]
        [InlineData("foo {{bar}}")]
        [InlineData("{{foo}} {{foo}}")]
        [InlineData("{{{}}}")]
        public void Subject_IsJudgedValid_WhenItHasValidValue(string subjectValue)
        {
            // Act
            var isValid = this.ValidateProperty(nameof(EmailTemplateResponseModel.Subject), subjectValue);

            // Assert
            Assert.True(isValid);
        }

        /// <summary>
        /// The ToEmailTemplateSetting_CreatesNullSmtpServerHost_WhenModelSmtpServerHostIsWhitespace.
        /// </summary>
        [Fact]
        public void ToEmailTemplateSetting_CreatesNullSmtpServerHost_WhenModelSmtpServerHostIsWhitespace()
        {
            // Arrange
            var sut = new EmailTemplateResponseModel
            {
                SmtpServerHost = "  ",
            };

            // Act
            var emailTemplateData = sut.GetData();

            // Assert
            Assert.Null(emailTemplateData.SmtpServerHost);
        }

        /// <summary>
        /// The SmtpServerPort_IsJudgedValid_WhenItHasValidValue.
        /// </summary>
        /// <param name="portValue">The portValue<see cref="int"/>.</param>
        [Theory]
        [InlineData(1)]
        [InlineData(65535)]
        [InlineData(80)]
        [InlineData(8080)]
        [InlineData(81)]
        public void SmtpServerPort_IsJudgedValid_WhenItHasValidValue(int portValue)
        {
            // Act
            var isValid = this.ValidateProperty(nameof(EmailTemplateResponseModel.SmtpServerPort), portValue);

            // Assert
            Assert.True(isValid);
        }

        /// <summary>
        /// The SmtpServerPort_IsJudgedInvalid_WhenItHasInvalidValue.
        /// </summary>
        /// <param name="hostValue">The hostValue<see cref="int"/>.</param>
        [Theory]
        [InlineData(0)]
        [InlineData(65536)]
        [InlineData(-12)]
        public void SmtpServerPort_IsJudgedInvalid_WhenItHasInvalidValue(int hostValue)
        {
            // Act
            var isValid = this.ValidateProperty(nameof(EmailTemplateResponseModel.SmtpServerPort), hostValue);

            // Assert
            Assert.False(isValid);
        }

        /// <summary>
        /// The ValidateProperty.
        /// </summary>
        /// <typeparam name="T">.</typeparam>
        /// <param name="propertyName">The propertyName<see cref="string"/>.</param>
        /// <param name="value">The value<see cref="bool"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private bool ValidateProperty<T>(string propertyName, T value)
        {
            var sut = new EmailTemplateResponseModel();
            var validationContext = new ValidationContext(sut) { MemberName = propertyName };
            var validationResults = new List<ValidationResult>();
            return Validator.TryValidateProperty(value, validationContext, validationResults);
        }
    }
}
