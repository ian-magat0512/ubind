// <copyright file="EmailComposerTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests
{
    using MimeKit;
    using Moq;
    using UBind.Application.Services.Messaging;
    using Xunit;

    public class EmailComposerTest
    {
        [Fact]
        public void EmailComposer_Returns_Expected_MailMessage()
        {
            // Arrange
            var mailMessage = new MimeMessage();
            mailMessage.From.Add(MailboxAddress.Parse("fromEmail@domain.com"));
            mailMessage.To.Add(MailboxAddress.Parse("toEmail@domain.com"));
            mailMessage.Cc.Add(MailboxAddress.Parse("ccEmail@test.com"));
            mailMessage.Subject = "subject";
            var builder = new BodyBuilder();
            builder.TextBody = "body";
            builder.HtmlBody = "body";
            mailMessage.Body = builder.ToMessageBody();

            var emailParameters = new { tenantName = "Test Tenant Name", remainingNumber = 10, productName = "Test Product Name" };
            var serviceMock = new Mock<IEmailComposer>();

            // Verify
            serviceMock
                .Setup(m => m.ComposeMailMessage("fromEmail@domain.com", "toEmail@domain.com", "ccEmail@test.com", "subject", "InvoiceNumbersInWarningThreshold.cshtml", emailParameters)) // the expected method called with provided Id
                .Returns(mailMessage)
                .Verifiable();
        }
    }
}
