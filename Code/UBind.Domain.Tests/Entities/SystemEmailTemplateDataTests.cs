// <copyright file="SystemEmailTemplateDataTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Entities
{
    using Xunit;

    public class SystemEmailTemplateDataTests
    {
        [Fact]
        public void Override_DoesNotOverrideWithNulls()
        {
            // Arrange
            var sut = new SystemEmailTemplateData(
                "foo",
                "bar",
                "baz",
                "qux",
                "quux",
                "quuz",
                "corge",
                "grault",
                7);

            var overrides = new SystemEmailTemplateData(null, null, null, null, null, null, null, null, null);

            // Act
            sut.Override(overrides);

            // Assert
            Assert.Equal("foo", sut.Subject);
            Assert.Equal("bar", sut.FromAddress);
            Assert.Equal("baz", sut.ToAddress);
            Assert.Equal("qux", sut.Cc);
            Assert.Equal("quux", sut.Bcc);
            Assert.Equal("quuz", sut.HtmlBody);
            Assert.Equal("corge", sut.PlainTextBody);
            Assert.Equal("grault", sut.SmtpServerHost);
            Assert.Equal(7, sut.SmtpServerPort);
        }

        [Fact]
        public void Override_OverridesWithNonNulls()
        {
            // Arrange
            var sut = new SystemEmailTemplateData(
                "foo",
                "newBar",
                "newBaz",
                "qux",
                "quux",
                "quuz",
                "corge",
                "grault",
                7);

            var overrides = new SystemEmailTemplateData(
                "newFoo",
                "newBar@domain.com",
                "newBaz@domain.com",
                "newQux",
                "newQuux",
                "newQuuz",
                "newCorge",
                "newGrault",
                8);

            // Act
            sut.Override(overrides);

            // Assert
            Assert.Equal("newFoo", sut.Subject);
            Assert.Equal("newBar@domain.com", sut.FromAddress);
            Assert.Equal("newBaz@domain.com", sut.ToAddress);
            Assert.Equal("newQux", sut.Cc);
            Assert.Equal("newQuux", sut.Bcc);
            Assert.Equal("newQuuz", sut.HtmlBody);
            Assert.Equal("newCorge", sut.PlainTextBody);
            Assert.Equal("newGrault", sut.SmtpServerHost);
            Assert.Equal(8, sut.SmtpServerPort);
        }
    }
}
