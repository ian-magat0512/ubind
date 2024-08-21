// <copyright file="EmailTemplateServiceTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services.Messaging
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using Xunit;

    /// <summary>
    /// Unit test for the email template service.
    /// </summary>
    public class EmailTemplateServiceTest
    {
        private readonly IClock clock;
        private Guid tenantId = Guid.NewGuid();
        private Guid productId = Guid.NewGuid();
        private Mock<ISystemEmailTemplateRepository> emailTemplateRepository;
        private Mock<ITenantRepository> tenantRepository;
        private Mock<IProductRepository> productRepository;
        private Mock<ICachingResolver> cachingResolver;
        private Mock<ITenantSystemEventEmitter> tenantSystemEventEmitter;

        public EmailTemplateServiceTest()
        {
            this.clock = SystemClock.Instance;
            this.emailTemplateRepository = new Mock<ISystemEmailTemplateRepository>();
            this.tenantRepository = new Mock<ITenantRepository>();
            this.productRepository = new Mock<IProductRepository>();
            this.cachingResolver = new Mock<ICachingResolver>();
            this.tenantSystemEventEmitter = new Mock<ITenantSystemEventEmitter>();
        }

        /// <summary>
        /// Tests GenerateTemplateData generates correct default template data for account activation invitation.
        /// </summary>
        [Fact]
        public void GenerateTemplateData_ShouldGenerateCorrectDefaultTemplate_ForActivationInvitation()
        {
            // Arrange
            var defaultSystemEmailTemplateData = SystemEmailTemplateData.DefaultActivationData;
            this.emailTemplateRepository.Setup(e => e.GetApplicableTemplates(
                this.tenantId,
                SystemEmailType.AccountActivationInvitation,
                this.productId,
                null))
                .Returns(Enumerable.Empty<ISystemEmailTemplateSummary>());
            var emailTemplateService = new EmailTemplateService(
                this.emailTemplateRepository.Object,
                this.cachingResolver.Object,
                this.tenantSystemEventEmitter.Object,
                this.clock);

            // Act
            var emailTemplate = emailTemplateService.GenerateTemplateData(
                this.tenantId,
                SystemEmailType.AccountActivationInvitation,
                this.productId,
                null);

            // Assert
            emailTemplate.Subject.Should().Be(defaultSystemEmailTemplateData.Subject);
            emailTemplate.FromAddress.Should().Be(defaultSystemEmailTemplateData.FromAddress);
            emailTemplate.ToAddress.Should().Be(defaultSystemEmailTemplateData.ToAddress);
            emailTemplate.Cc.Should().Be(defaultSystemEmailTemplateData.Cc);
            emailTemplate.Bcc.Should().Be(defaultSystemEmailTemplateData.Bcc);
            emailTemplate.HtmlBody.Should().Be(defaultSystemEmailTemplateData.HtmlBody);
            emailTemplate.PlainTextBody.Should().Be(defaultSystemEmailTemplateData.PlainTextBody);
            emailTemplate.SmtpServerHost.Should().Be(defaultSystemEmailTemplateData.SmtpServerHost);
            emailTemplate.SmtpServerPort.Should().Be(defaultSystemEmailTemplateData.SmtpServerPort);
        }

        /// <summary>
        /// Tests GenerateTemplateData generates correct default template data for password reset.
        /// </summary>
        [Fact]
        public void GenerateTemplateData_ShouldGenerateCorrectDefaultTemplate_ForPasswordReset()
        {
            // Arrange
            var defaultSystemEmailTemplateData = SystemEmailTemplateData.DefaultPasswordResetData;
            this.emailTemplateRepository.Setup(e => e.GetApplicableTemplates(
                this.tenantId,
                SystemEmailType.PasswordResetInvitation,
                this.productId,
                null))
                .Returns(Enumerable.Empty<ISystemEmailTemplateSummary>());
            var emailTemplateService = new EmailTemplateService(
              this.emailTemplateRepository.Object,
              this.cachingResolver.Object,
              this.tenantSystemEventEmitter.Object,
              this.clock);

            // Act
            var emailTemplate = emailTemplateService.GenerateTemplateData(
                this.tenantId,
                SystemEmailType.PasswordResetInvitation,
                this.productId,
                null);

            // Assert
            emailTemplate.Subject.Should().Be(defaultSystemEmailTemplateData.Subject);
            emailTemplate.FromAddress.Should().Be(defaultSystemEmailTemplateData.FromAddress);
            emailTemplate.ToAddress.Should().Be(defaultSystemEmailTemplateData.ToAddress);
            emailTemplate.Cc.Should().Be(defaultSystemEmailTemplateData.Cc);
            emailTemplate.Bcc.Should().Be(defaultSystemEmailTemplateData.Bcc);
            emailTemplate.HtmlBody.Should().Be(defaultSystemEmailTemplateData.HtmlBody);
            emailTemplate.PlainTextBody.Should().Be(defaultSystemEmailTemplateData.PlainTextBody);
            emailTemplate.SmtpServerHost.Should().Be(defaultSystemEmailTemplateData.SmtpServerHost);
            emailTemplate.SmtpServerPort.Should().Be(defaultSystemEmailTemplateData.SmtpServerPort);
        }

        /// <summary>
        /// Tests GenerateTemplateData generates correct default template data for quote association invitation.
        /// </summary>
        [Fact]
        public void GenerateTemplateData_ShouldGenerateCorrectDefaultTemplate_ForQuoteAssociationInvitation()
        {
            // Arrange
            var defaultSystemEmailTemplateData = SystemEmailTemplateData.DefaultQuoteAssociationInvitationData;
            this.emailTemplateRepository.Setup(e => e.GetApplicableTemplates(
                this.tenantId,
                SystemEmailType.QuoteAssociationInvitation,
                this.productId,
                null))
                .Returns(Enumerable.Empty<ISystemEmailTemplateSummary>());
            var emailTemplateService = new EmailTemplateService(
              this.emailTemplateRepository.Object,
              this.cachingResolver.Object,
              this.tenantSystemEventEmitter.Object,
              this.clock);

            // Act
            var emailTemplate = emailTemplateService.GenerateTemplateData(
                this.tenantId,
                SystemEmailType.QuoteAssociationInvitation,
                this.productId,
                null);

            // Assert
            emailTemplate.Subject.Should().Be(defaultSystemEmailTemplateData.Subject);
            emailTemplate.FromAddress.Should().Be(defaultSystemEmailTemplateData.FromAddress);
            emailTemplate.ToAddress.Should().Be(defaultSystemEmailTemplateData.ToAddress);
            emailTemplate.Cc.Should().Be(defaultSystemEmailTemplateData.Cc);
            emailTemplate.Bcc.Should().Be(defaultSystemEmailTemplateData.Bcc);
            emailTemplate.HtmlBody.Should().Be(defaultSystemEmailTemplateData.HtmlBody);
            emailTemplate.PlainTextBody.Should().Be(defaultSystemEmailTemplateData.PlainTextBody);
            emailTemplate.SmtpServerHost.Should().Be(defaultSystemEmailTemplateData.SmtpServerHost);
            emailTemplate.SmtpServerPort.Should().Be(defaultSystemEmailTemplateData.SmtpServerPort);
        }

        /// <summary>
        /// Tests GenerateTemplateData generates correct default template data for renewal invitation.
        /// </summary>
        [Fact]
        public void GenerateTemplateData_ShouldGenerateCorrectDefaultTemplate_ForRenewalInvitation()
        {
            // Arrange
            var defaultSystemEmailTemplateData = SystemEmailTemplateData.DefaultRenewalInvitationData;
            this.emailTemplateRepository.Setup(e => e.GetApplicableTemplates(
                this.tenantId,
                SystemEmailType.RenewalInvitation,
                this.productId,
                null))
                .Returns(Enumerable.Empty<ISystemEmailTemplateSummary>());
            var emailTemplateService = new EmailTemplateService(
              this.emailTemplateRepository.Object,
              this.cachingResolver.Object,
              this.tenantSystemEventEmitter.Object,
              this.clock);

            // Act
            var emailTemplate = emailTemplateService.GenerateTemplateData(
                this.tenantId,
                SystemEmailType.RenewalInvitation,
                this.productId,
                null);

            // Assert
            emailTemplate.Subject.Should().Be(defaultSystemEmailTemplateData.Subject);
            emailTemplate.FromAddress.Should().Be(defaultSystemEmailTemplateData.FromAddress);
            emailTemplate.ToAddress.Should().Be(defaultSystemEmailTemplateData.ToAddress);
            emailTemplate.Cc.Should().Be(defaultSystemEmailTemplateData.Cc);
            emailTemplate.Bcc.Should().Be(defaultSystemEmailTemplateData.Bcc);
            emailTemplate.HtmlBody.Should().Be(defaultSystemEmailTemplateData.HtmlBody);
            emailTemplate.PlainTextBody.Should().Be(defaultSystemEmailTemplateData.PlainTextBody);
            emailTemplate.SmtpServerHost.Should().Be(defaultSystemEmailTemplateData.SmtpServerHost);
            emailTemplate.SmtpServerPort.Should().Be(defaultSystemEmailTemplateData.SmtpServerPort);
        }
    }
}
