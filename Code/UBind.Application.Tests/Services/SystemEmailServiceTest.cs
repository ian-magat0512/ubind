// <copyright file="SystemEmailServiceTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DotLiquid;
    using DotLiquid.NamingConventions;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using MimeKit;
    using Moq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Application.Export;
    using UBind.Application.Services;
    using UBind.Application.Services.Imports.MappingObjects;
    using UBind.Application.Services.SystemEmail;
    using UBind.Application.Tests.Services.Import;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Entities;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Extensions;
    using UBind.Domain.Imports;
    using UBind.Domain.Json;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Processing;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;
    using UBind.Domain.Tests.Fakes;
    using Xunit;
    using static UBind.Domain.Aggregates.Quote.QuoteAggregate;

    public class SystemEmailServiceTest
    {
        private const string DefaultSmtpHost = "defaultSmtpHost";
        private const int DefaultSmtpPort = 99;
        private const string Preferredname = null;
        private const string FullName = "Umberto";
        private const string NamePrefix = "Dr";
        private const string FirstName = "Umberto";
        private const string MiddleNames = "Salazar";
        private const string LastName = "Parker";
        private const string NameSuffix = "Jr";
        private const string GreetingName = "GreetingName Umberto";
        private const string Company = "Marvel";
        private const string Title = "Hero";
        private const string Environment = "development";
        private const string UserEmail = "user@example.com";
        private const string MobilePhone = "042555211";
        private const string WorkPhone = "042555211";
        private const string HomePhone = "042555211";
        private const string AlternativeEmail = "user@example.com";
        private static Tenant tenant = TenantFactory.Create();
        private static SystemEmailTemplateData templateDataWithTwoBodies = new SystemEmailTemplateData(
            "Activation email from {{{Tenant.Name}}",
            "from@example.com",
            "{{User.Email}}",
            "cc1@example.com, cc2@example.com",
            "bcc1@example.com, bcc2@example.com",
            "<p>Yo {{User.PreferredName}}!</p>",
            "Good day to you, {{User.PreferredName}}!",
            "my-mail-server",
            666);

        private static SystemEmailTemplateData templateDataWithPlainTextBodyOnly = new SystemEmailTemplateData(
            "Activation email from {{{Tenant.Name}}",
            "from@example.com",
            "{{User.Email}}",
            "cc1@example.com, cc2@example.com",
            "bcc1@example.com, bcc2@example.com",
            null,
            "Good day to you, {{User.PreferredName}}!",
            "my-mail-server",
            666);

        private static SystemEmailTemplateData templateDataWithHtmlBodyOnly = new SystemEmailTemplateData(
            "Activation email from {{{Tenant.Name}}",
            "from@example.com",
            "{{User.Email}}",
            "cc1@example.com, cc2@example.com",
            "bcc1@example.com, bcc2@example.com",
            "<p>Yo {{User.PreferredName}}!</p>",
            null,
            "my-mail-server",
            666);

        private Mock<ISmtpClientConfiguration> mockSmtpclientConfiguration;
        private Mock<IEmailTemplateService> mockEmailTemplateService;
        private Mock<IMailClientFactory> mockMailClientFactory;
        private Mock<IEmailService> mockEmailService;
        private Mock<IMailClient> mockMailClient;
        private Mock<IJobClient> jobClient;
        private Mock<ICqrsMediator> mediator;
        private Mock<ILogger<SystemEmailService>> mockLogger;
        private IClock clock;
        private Mock<IFileContentRepository> fileContentRepository;

        private Guid productId = Guid.NewGuid();
        private List<string> capturedSmtpHosts = new List<string>();
        private List<int> capturedSmtpPorts = new List<int>();
        private List<MimeMessage> capturedMailMessages = new List<MimeMessage>();

        public SystemEmailServiceTest()
        {
            this.mockLogger = new Mock<ILogger<SystemEmailService>>();
            this.mockSmtpclientConfiguration = new Mock<ISmtpClientConfiguration>();
            this.mockEmailService = new Mock<IEmailService>();
            this.mockSmtpclientConfiguration.Setup(c => c.Host).Returns(DefaultSmtpHost);
            this.mockSmtpclientConfiguration.Setup(c => c.Port).Returns(DefaultSmtpPort);
            this.mockEmailTemplateService = new Mock<IEmailTemplateService>();
            this.mockMailClient = new Mock<IMailClient>();
            this.mockMailClient.Setup(c => c.Send(Capture.In(this.capturedMailMessages)));
            this.mockMailClientFactory = new Mock<IMailClientFactory>();
            this.mediator = new Mock<ICqrsMediator>();
            this.mockMailClientFactory
                .Setup(f => f.Invoke(Capture.In(this.capturedSmtpHosts), string.Empty, string.Empty, Capture.In(this.capturedSmtpPorts)))
                .Returns(this.mockMailClient.Object);
            this.jobClient = new Mock<IJobClient>();
            this.clock = SystemClock.Instance;
            this.fileContentRepository = new Mock<IFileContentRepository>();
            Template.NamingConvention = new CSharpNamingConvention();
            SystemEmailService.RegisterTemplateSafeTypes();
        }

        [Fact]
        public async Task SendMessage_RendersToFieldCorrectlyAsync()
        {
            // Arrange
            this.mockEmailTemplateService
                .Setup(s => s.GenerateTemplateData(
                    It.IsAny<Guid>(),
                    SystemEmailType.AccountActivationInvitation,
                    It.IsAny<EmailDrop>()))
                .Returns(templateDataWithTwoBodies);
            var sut = new SystemEmailService(
                this.mockEmailService.Object,
                this.mockSmtpclientConfiguration.Object,
                this.mockEmailTemplateService.Object,
                this.mockMailClientFactory.Object,
                this.jobClient.Object,
                this.mockLogger.Object,
                this.mediator.Object,
                this.clock,
                this.fileContentRepository.Object);

            UserDrop userDrop = new UserDrop(
                tenant.Id,
                Environment,
                Guid.NewGuid(),
                UserEmail,
                AlternativeEmail,
                Preferredname ?? GreetingName,
                FullName,
                NamePrefix,
                FirstName,
                MiddleNames,
                LastName,
                NameSuffix,
                GreetingName,
                Company,
                Title,
                MobilePhone,
                WorkPhone,
                HomePhone,
                false,
                this.clock.GetCurrentInstant());

            TenantDrop tenantDrop = new TenantDrop(Guid.NewGuid(), "Umberto", "umberto");
            EmailInvitationDrop passwordResetDrop = new EmailInvitationDrop("link", "12121");
            OrganisationDrop organisationDrop = new OrganisationDrop(Guid.NewGuid(), "umbertos-organisation", "umbertos-organisation");

            var model = EmailDrop.CreateUserActivationInvitation(
                tenant.Id,
                null,
                userDrop,
                tenantDrop,
                organisationDrop,
                passwordResetDrop);

            // Act
            await sut.SendMessage(tenant.Details.Alias, organisationDrop.Alias, model.EmailType.ToString(), userDrop.Email, model);

            // Assert
            Assert.Equal("my-mail-server", this.capturedSmtpHosts.Single());
            Assert.Equal(666, this.capturedSmtpPorts.Single());
            Assert.NotNull(this.capturedMailMessages.SingleOrDefault());
            var mailMessage = this.capturedMailMessages.Single();
            Assert.Equal("Activation email from Umberto", this.capturedMailMessages.Single().Subject);
            Assert.Equal("from@example.com", mailMessage.From.ToString());
            Assert.Equal("user@example.com", mailMessage.To.ToString());

            Assert.Contains(mailMessage.Cc, address => address.ToString() == "cc1@example.com");

            Assert.Contains(mailMessage.Cc, address => address.ToString() == "cc2@example.com");
            Assert.Contains(mailMessage.Bcc, address => address.ToString() == "bcc1@example.com");
            Assert.Contains(mailMessage.Bcc, address => address.ToString() == "bcc2@example.com");
            Assert.Equal("Good day to you, GreetingName Umberto!", mailMessage.TextBody);
        }

        [Fact]
        public async Task SendMessage_UsesHtmlBodyAsBody_WhenOnlyHtmlBodyAvailableAsync()
        {
            // Arrange
            this.mockEmailTemplateService
                .Setup(s => s.GenerateTemplateData(
                    It.IsAny<Guid>(),
                    SystemEmailType.AccountActivationInvitation,
                    It.IsAny<EmailDrop>()))
            .Returns(templateDataWithHtmlBodyOnly);

            var sut = new SystemEmailService(
                this.mockEmailService.Object,
                this.mockSmtpclientConfiguration.Object,
                this.mockEmailTemplateService.Object,
                this.mockMailClientFactory.Object,
                this.jobClient.Object,
                this.mockLogger.Object,
                this.mediator.Object,
                this.clock,
                this.fileContentRepository.Object);

            UserDrop userDrop = new UserDrop(
                tenant.Id,
                Environment,
                Guid.NewGuid(),
                UserEmail,
                AlternativeEmail,
                Preferredname,
                FullName,
                NamePrefix,
                FirstName,
                MiddleNames,
                LastName,
                NameSuffix,
                GreetingName,
                Company,
                Title,
                MobilePhone,
                WorkPhone,
                HomePhone,
                false,
                this.clock.GetCurrentInstant());

            TenantDrop tenantDrop = new TenantDrop(Guid.NewGuid(), "Umberto", "umberto");
            EmailInvitationDrop passwordResetDrop = new EmailInvitationDrop("link", "12121");
            OrganisationDrop organisationDrop =
                new OrganisationDrop(Guid.NewGuid(), "umbertos-organisation", "umbertos-organisation");

            var model = EmailDrop.CreateUserActivationInvitation(
                tenant.Id,
                null,
                userDrop,
                tenantDrop,
                organisationDrop,
                passwordResetDrop);

            // Act
            await sut.SendMessage(tenant.Details.Alias, organisationDrop.Alias, model.EmailType.ToString(), userDrop.Email, model);

            // Assert
            Assert.NotNull(this.capturedMailMessages.SingleOrDefault());
            var mailMessage = this.capturedMailMessages.Single();
            Assert.Equal("<p>Yo GreetingName Umberto!</p>", mailMessage.HtmlBody);
        }

        [Fact]
        public async Task SendMessage_UsesPlainTextBodyAsBody_WhenOnlyPlainTextBodyAvailableAsync()
        {
            // Arrange
            var emailDrop = this.CreateUserActivationEmailDrop();
            this.mockEmailTemplateService
                .Setup(s => s.GenerateTemplateData(
                    It.IsAny<Guid>(),
                    SystemEmailType.AccountActivationInvitation,
                    It.IsAny<EmailDrop>()))
                .Returns(templateDataWithPlainTextBodyOnly);

            var sut = new SystemEmailService(
                this.mockEmailService.Object,
                this.mockSmtpclientConfiguration.Object,
                this.mockEmailTemplateService.Object,
                this.mockMailClientFactory.Object,
                this.jobClient.Object,
                this.mockLogger.Object,
                this.mediator.Object,
                this.clock,
                this.fileContentRepository.Object);

            // Act
            await sut.SendMessage(
                tenant.Details.Alias,
                emailDrop.Organisation.Alias,
                emailDrop.EmailType.ToString(),
                emailDrop.User.Email,
                emailDrop);

            // Assert
            var mailMessage = this.capturedMailMessages.Single();
            Assert.NotNull(this.capturedMailMessages.SingleOrDefault());
            Assert.Equal("Good day to you, GreetingName Umberto!", mailMessage.TextBody);
            Assert.Null(mailMessage.HtmlBody);
            Assert.Empty(mailMessage.Attachments);
        }

        [Fact]
        public void CreatePasswordResetInvitationEmail_ValidEmailType_ShouldSucceed()
        {
            // Arrange
            var emailDrop = this.CreatePasswordResetEmailDrop();
            this.mockEmailTemplateService
                .Setup(s => s.GenerateTemplateData(
                    It.IsAny<Guid>(),
                    SystemEmailType.PasswordResetInvitation,
                    It.IsAny<EmailDrop>()))
                .Returns(templateDataWithTwoBodies);

            var systemEmailService = new SystemEmailService(
                this.mockEmailService.Object,
                this.mockSmtpclientConfiguration.Object,
                this.mockEmailTemplateService.Object,
                this.mockMailClientFactory.Object,
                this.jobClient.Object,
                this.mockLogger.Object,
                this.mediator.Object,
                this.clock,
                this.fileContentRepository.Object);

            var personAggregate = PersonAggregate.CreateImportedPerson(
                tenant.Id, tenant.Details.DefaultOrganisationId, new Domain.Imports.CustomerImportData(), Guid.NewGuid(), this.clock.GetCurrentInstant());

            var userAggregate =
                UserAggregate.CreateUser(
                    tenant.Id, Guid.NewGuid(), UserType.Client, personAggregate, Guid.NewGuid(), null, this.clock.GetCurrentInstant());

            // Act
            var email = systemEmailService.SendAndPersistPasswordResetInvitationEmail(emailDrop, userAggregate);

            // Assert
            email.TenantId.Should().Be(emailDrop.TenantId);
            email.From.Should().Be(templateDataWithTwoBodies.FromAddress);
            email.To.Should().Be(emailDrop.User.Email);
            email.Subject.Should()
                .StartWith("Activation email from")
                .And.EndWith(emailDrop.Tenant.Name);
            email.HtmlBody.Should()
                .StartWith("<p>Yo")
                .And.Contain(emailDrop.User.PreferredName);
        }

        [Fact]
        public void CreateQuoteAssociationInvitationEmail_ValidEmailType_ShouldSucceed()
        {
            // Arrange
            var systemEmailService = new SystemEmailService(
                this.mockEmailService.Object,
                this.mockSmtpclientConfiguration.Object,
                this.mockEmailTemplateService.Object,
                this.mockMailClientFactory.Object,
                this.jobClient.Object,
                this.mockLogger.Object,
                this.mediator.Object,
                this.clock,
                this.fileContentRepository.Object);

            var personAggregate = PersonAggregate.CreateImportedPerson(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                new Domain.Imports.CustomerImportData(),
                Guid.NewGuid(),
                this.clock.GetCurrentInstant());

            var userAggregate =
                UserAggregate.CreateUser(
                    tenant.Id, Guid.NewGuid(), UserType.Client, personAggregate, Guid.NewGuid(), null, this.clock.GetCurrentInstant());

            var emailDrop = this.CreateQuoteActivationEmailDrop();
            this.mockEmailTemplateService
                .Setup(s => s.GenerateTemplateData(
                    It.IsAny<Guid>(),
                    SystemEmailType.QuoteAssociationInvitation,
                    It.IsAny<EmailDrop>()))
                .Returns(templateDataWithTwoBodies);

            // Act
            var email = systemEmailService.SendAndPersistQuoteAssociationInvitationEmail(
                emailDrop, userAggregate, Guid.NewGuid());

            // Assert
            email.TenantId.Should().Be(tenant.Id);
            email.From.Should().Be(templateDataWithTwoBodies.FromAddress);
            email.To.Should().Be(emailDrop.User.Email);
            email.Subject.Should()
                .StartWith("Activation email from")
                .And.EndWith(emailDrop.Tenant.Name);
            email.HtmlBody.Should()
                .StartWith("<p>Yo")
                .And.Contain(emailDrop.User.PreferredName);
        }

        [Fact]
        public void SendAndPersistAccountActivationInvitationEmail_ValidEmail_ShouldGenerateAccountActivationInvitationTemplate()
        {
            // Arrange
            var systemEmailService = new SystemEmailService(
                this.mockEmailService.Object,
                this.mockSmtpclientConfiguration.Object,
                this.mockEmailTemplateService.Object,
                this.mockMailClientFactory.Object,
                this.jobClient.Object,
                this.mockLogger.Object,
                this.mediator.Object,
                this.clock,
                this.fileContentRepository.Object);

            var emailDrop = this.CreateQuoteActivationEmailDrop();
            var personAggregate = PersonAggregate.CreateImportedPerson(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                new Domain.Imports.CustomerImportData(),
                Guid.NewGuid(),
                this.clock.GetCurrentInstant());
            var userAggregate = UserAggregate.CreateUser(
                tenant.Id, Guid.NewGuid(), UserType.Client, personAggregate, Guid.NewGuid(), null, this.clock.GetCurrentInstant());

            this.mockEmailTemplateService
                .Setup(s => s.GenerateTemplateData(
                    It.IsAny<Guid>(),
                    SystemEmailType.AccountActivationInvitation,
                    It.IsAny<EmailDrop>()))
                .Returns(templateDataWithTwoBodies);

            // Act
            var email = systemEmailService.SendAndPersistAccountActivationInvitationEmail(
                emailDrop, userAggregate);

            // Assert
            email.TenantId.Should().Be(tenant.Id);
            email.ProductId.Should().Be(this.productId);
            this.mockEmailTemplateService.Verify(
                x => x.GenerateTemplateData(
                    It.IsAny<Guid>(),
                    SystemEmailType.AccountActivationInvitation,
                    It.IsAny<EmailDrop>()));
        }

        [Fact]
        public void SendAndPersistPasswordResetInvitationEmail_ValidEmail_ShouldGeneratePasswordResetInvitationTemplate()
        {
            // Arrange
            var systemEmailService = new SystemEmailService(
                this.mockEmailService.Object,
                this.mockSmtpclientConfiguration.Object,
                this.mockEmailTemplateService.Object,
                this.mockMailClientFactory.Object,
                this.jobClient.Object,
                this.mockLogger.Object,
                this.mediator.Object,
                this.clock,
                this.fileContentRepository.Object);

            var emailDrop = this.CreatePasswordResetEmailDrop();
            var personAggregate = PersonAggregate.CreateImportedPerson(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                new Domain.Imports.CustomerImportData(),
                Guid.NewGuid(),
                this.clock.GetCurrentInstant());
            var userAggregate = UserAggregate.CreateUser(
                tenant.Id, Guid.NewGuid(), UserType.Client, personAggregate, Guid.NewGuid(), null, this.clock.GetCurrentInstant());

            this.mockEmailTemplateService
                .Setup(s => s.GenerateTemplateData(
                    It.IsAny<Guid>(),
                    It.IsAny<SystemEmailType>(),
                    It.IsAny<EmailDrop>()))
                .Returns(templateDataWithTwoBodies);

            // Act
            var email = systemEmailService.SendAndPersistPasswordResetInvitationEmail(emailDrop, userAggregate);

            // Assert
            email.TenantId.Should().Be(tenant.Id);
            this.mockEmailTemplateService.Verify(
                x => x.GenerateTemplateData(
                    It.IsAny<Guid>(),
                    SystemEmailType.PasswordResetInvitation,
                    It.IsAny<EmailDrop>()));
        }

        [Fact]
        public void SendAndPersistQuoteAssociationInvitationEmail_ValidEmail_ShouldGenerateQuoteAssociationInvitationTemplate()
        {
            // Arrange
            var systemEmailService = new SystemEmailService(
                this.mockEmailService.Object,
                this.mockSmtpclientConfiguration.Object,
                this.mockEmailTemplateService.Object,
                this.mockMailClientFactory.Object,
                this.jobClient.Object,
                this.mockLogger.Object,
                this.mediator.Object,
                this.clock,
                this.fileContentRepository.Object);

            var emailDrop = this.CreateQuoteActivationEmailDrop();
            var personAggregate = PersonAggregate.CreateImportedPerson(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                new Domain.Imports.CustomerImportData(),
                Guid.NewGuid(),
                this.clock.GetCurrentInstant());
            var userAggregate = UserAggregate.CreateUser(
                tenant.Id, Guid.NewGuid(), UserType.Client, personAggregate, Guid.NewGuid(), null, this.clock.GetCurrentInstant());

            this.mockEmailTemplateService
                .Setup(s => s.GenerateTemplateData(
                    It.IsAny<Guid>(),
                    It.IsAny<SystemEmailType>(),
                    It.IsAny<EmailDrop>()))
                .Returns(templateDataWithTwoBodies);

            // Act
            var email = systemEmailService.SendAndPersistQuoteAssociationInvitationEmail(
                emailDrop, userAggregate, Guid.NewGuid());

            // Assert
            email.TenantId.Should().Be(tenant.Id);
            email.ProductId.Should().Be(this.productId);
            this.mockEmailTemplateService.Verify(
                x => x.GenerateTemplateData(
                    It.IsAny<Guid>(),
                    SystemEmailType.QuoteAssociationInvitation,
                    It.IsAny<EmailDrop>()));
        }

        [Fact]
        public void SendAndPersistPolicyRenewalEmailEmail_ValidEmail_ShouldGenerateRenewalInvitationTemplate()
        {
            // Arrange
            var systemEmailService = new SystemEmailService(
                this.mockEmailService.Object,
                this.mockSmtpclientConfiguration.Object,
                this.mockEmailTemplateService.Object,
                this.mockMailClientFactory.Object,
                this.jobClient.Object,
                this.mockLogger.Object,
                this.mediator.Object,
                this.clock,
                this.fileContentRepository.Object);

            var personAggregate = PersonAggregate.CreateImportedPerson(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                new Domain.Imports.CustomerImportData(),
                Guid.NewGuid(),
                this.clock.GetCurrentInstant());
            var emailDrop = this.CreatePolicyRenawalEmailDrop(personAggregate);
            var userAggregate = UserAggregate.CreateUser(
                tenant.Id, Guid.NewGuid(), UserType.Client, personAggregate, Guid.NewGuid(), null, this.clock.GetCurrentInstant());

            this.mockEmailTemplateService
                .Setup(s => s.GenerateTemplateData(
                    It.IsAny<Guid>(),
                    It.IsAny<SystemEmailType>(),
                    It.IsAny<EmailDrop>()))
                .Returns(templateDataWithTwoBodies);

            var json = ImportTestData.GeneratePolicyCompleteImportJson();
            var data = JsonConvert.DeserializeObject<ImportData>(json);
            var config = JsonConvert.DeserializeObject<ImportConfiguration>(json);
            var customerId = Guid.NewGuid();
            var policyImportData = new PolicyImportData((JObject)data.Data[0], config.PolicyMapping);
            var aggregate = QuoteAggregate.CreateImportedPolicy(
                    tenant.Id,
                    tenant.Details.DefaultOrganisationId,
                    this.productId,
                    DeploymentEnvironment.Development,
                    Guid.NewGuid(),
                    customerId,
                    It.IsAny<IPersonalDetails>(),
                    policyImportData,
                    Timezones.AET,
                    new Mock<IPolicyTransactionTimeOfDayScheme>().Object,
                    Guid.NewGuid(),
                    Instant.MinValue,
                    Guid.NewGuid());
            var instant = this.clock.GetCurrentInstant();
            var quote = new NewBusinessQuote(Guid.NewGuid(), aggregate, 1, instant, Timezones.AET, false, null, Guid.NewGuid());
            var quoteDataSnapshot = this.CreateQuoteSnapshot(instant);
            var policyEvent = new PolicyRenewedEvent(
                quote.Aggregate.TenantId,
                quote.Id,
                quote.Id,
                "LEON",
                instant.ToLocalDateTimeInAet(),
                instant,
                instant.ToLocalDateTimeInAet(),
                instant,
                quoteDataSnapshot,
                Guid.NewGuid(),
                instant,
                quote.ProductReleaseId);
            var policyRenewalTransaction = new RenewalPolicyTransaction(Guid.NewGuid(), policyEvent, 1);

            // Act
            var email = systemEmailService.SendAndPersistPolicyRenewalEmail(
                emailDrop, aggregate, Guid.NewGuid(), policyRenewalTransaction, quote);

            // Assert
            email.TenantId.Should().Be(tenant.Id);
            email.ProductId.Should().Be(this.productId);
            this.mockEmailTemplateService.Verify(
                x => x.GenerateTemplateData(
                    It.IsAny<Guid>(),
                    SystemEmailType.RenewalInvitation,
                    It.IsAny<EmailDrop>()));
        }

        private EmailDrop CreateUserActivationEmailDrop()
        {
            UserDrop userDrop = this.CreateUserDrop();
            TenantDrop tenantDrop = new TenantDrop(Guid.NewGuid(), "Umberto", "umberto");
            EmailInvitationDrop passwordResetDrop = new EmailInvitationDrop("link", "12121");
            OrganisationDrop organisationDrop = new OrganisationDrop(Guid.NewGuid(), "umberto-org", "umberto-org");
            return EmailDrop.CreateUserActivationInvitation(
                tenant.Id,
                null,
                userDrop,
                tenantDrop,
                organisationDrop,
                passwordResetDrop);
        }

        private EmailDrop CreatePolicyRenawalEmailDrop(PersonAggregate personAggregate)
        {
            UserDrop userDrop = this.CreateUserDrop();
            TenantDrop tenantDrop = new TenantDrop(Guid.NewGuid(), "Umberto", "umberto");
            PolicyDrop policyDrop = new PolicyDrop(Guid.NewGuid(), "21232", "21321");
            PolicyRenewalDrop policyRenewalDrop = new PolicyRenewalDrop("link", "12121", "link1", "productAlias", Guid.NewGuid());
            OrganisationDrop organisationDrop = new OrganisationDrop(Guid.NewGuid(), "umberto-org", "umberto-org");
            PersonDrop personDrop = new PersonDrop(
                personAggregate.Email,
                personAggregate.AlternativeEmail,
                personAggregate.PreferredName,
                personAggregate.FullName,
                personAggregate.NamePrefix,
                personAggregate.FirstName,
                personAggregate.MiddleNames,
                personAggregate.LastName,
                personAggregate.NameSuffix,
                personAggregate.GreetingName,
                personAggregate.Company,
                personAggregate.Title,
                personAggregate.MobilePhone,
                personAggregate.WorkPhone,
                personAggregate.HomePhone);
            return EmailDrop.CreatePolicyRenewalInvation(
                tenant.Id,
                this.productId,
                null,
                tenantDrop,
                policyDrop,
                organisationDrop,
                policyRenewalDrop,
                userDrop,
                personDrop);
        }

        private EmailDrop CreateQuoteActivationEmailDrop()
        {
            UserDrop userDrop = this.CreateUserDrop();
            TenantDrop tenantDrop = new TenantDrop(Guid.NewGuid(), "Umberto", "umberto");
            QuoteCustomerAssociationDrop associationDrop = new QuoteCustomerAssociationDrop("link", "12121");
            OrganisationDrop organisationDrop = new OrganisationDrop(Guid.NewGuid(), "umberto-org", "umberto-org");
            return EmailDrop.CreateQuoteAssociationInvitation(
                tenant.Id,
                this.productId,
                null,
                userDrop,
                tenantDrop,
                organisationDrop,
                associationDrop);
        }

        private EmailDrop CreatePasswordResetEmailDrop()
        {
            UserDrop userDrop = this.CreateUserDrop();
            TenantDrop tenantDrop = new TenantDrop(Guid.NewGuid(), "Umberto", "umberto");
            EmailInvitationDrop invitationDrop = new EmailInvitationDrop("link", "12121");
            OrganisationDrop organisationDrop = new OrganisationDrop(Guid.NewGuid(), "umberto-org", "umberto-org");
            return EmailDrop.CreatePasswordResetInvition(
                tenant.Id,
                null,
                userDrop,
                tenantDrop,
                organisationDrop,
                invitationDrop);
        }

        private UserDrop CreateUserDrop() =>
            new UserDrop(
                tenant.Id,
                Environment,
                Guid.NewGuid(),
                UserEmail,
                AlternativeEmail,
                Preferredname ?? GreetingName,
                FullName,
                NamePrefix,
                FirstName,
                MiddleNames,
                LastName,
                NameSuffix,
                GreetingName,
                Company,
                Title,
                MobilePhone,
                WorkPhone,
                HomePhone,
                false,
                this.clock.GetCurrentInstant());

        private QuoteDataSnapshot CreateQuoteSnapshot(Instant timestamp)
        {
            var formDataJson = FormDataJsonFactory.GetSampleWithStartAndEndDates();
            var calculationResultJson = CalculationResultJsonFactory.Create();
            var quoteData = QuoteFactory.QuoteDataRetriever(
                new CachingJObjectWrapper(formDataJson), new CachingJObjectWrapper(calculationResultJson));
            var calculationResult = Domain.ReadWriteModel.CalculationResult.CreateForNewPolicy(
                new CachingJObjectWrapper(calculationResultJson), quoteData);
            return new QuoteDataSnapshot(
                    new QuoteDataUpdate<Domain.Aggregates.Quote.FormData>(
                        Guid.NewGuid(), new Domain.Aggregates.Quote.FormData(formDataJson), timestamp),
                    new QuoteDataUpdate<Domain.ReadWriteModel.CalculationResult>(
                        Guid.NewGuid(), calculationResult, timestamp),
                    new QuoteDataUpdate<IPersonalDetails>(
                        Guid.NewGuid(), new FakePersonalDetails(), timestamp));
        }
    }
}
