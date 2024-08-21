// <copyright file="EmailMetadataFactoryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using NodaTime;
    using UBind.Application.Services.SystemEmail;
    using UBind.Domain.Factory;
    using UBind.Domain.Quote;
    using UBind.Domain.Repositories;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class EmailMetadataFactoryTests
    {
        [Fact]
        public void PasswordResetCreateForUser_ReturnsCorrectMetadata()
        {
            // Arrange
            var expectedTags = new List<string>() { DefaultEmailTags.PasswordReset, DefaultEmailTags.Invitation };
            var userId = Guid.NewGuid();
            var personId = Guid.NewGuid();
            var senderUserId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var emailType = EmailType.Admin;
            var environment = DeploymentEnvironment.Development;
            Instant timestamp = SystemClock.Instance.GetCurrentInstant();
            var emailModel = new EmailModel(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                environment,
                "noreply@ubind.io",
                "john.talavera@ubind.io",
                "Test subject",
                "some text plain text body",
                "some html body");

            // Act
            var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForUser(
                emailModel,
                userId,
                personId,
                organisationId,
                emailType,
                timestamp,
                new Mock<IFileContentRepository>().Object);

            var emailRecipient = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.MessageRecipient);
            var emailSender = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.MessageSender);

            // Assert
            Assert.Equal(emailRecipient.ToEntityId, personId);
            Assert.Equal(emailSender.ToEntityId, organisationId);
            Assert.Contains(metadata.Tags, x => expectedTags.Contains(x.Value));
            Assert.NotNull(metadata.Tags.FirstOrDefault(x => x.TagType == TagType.EmailType && x.Value == emailType.ToString()));
            Assert.NotNull(metadata.Tags.FirstOrDefault(x => x.TagType == TagType.Environment && x.Value == environment.ToString()));
        }

        [Fact]
        public void PasswordResetCreateForCustomer_ReturnsCorrectMetadata()
        {
            // Arrange
            var expectedTags = new List<string>() { DefaultEmailTags.PasswordReset, DefaultEmailTags.Invitation };
            var customerId = Guid.NewGuid();
            var senderUserId = Guid.NewGuid();
            var personId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var emailType = EmailType.Admin;
            var environment = DeploymentEnvironment.Development;
            Instant timestamp = SystemClock.Instance.GetCurrentInstant();
            var emailModel = new EmailModel(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                environment,
                "noreply@ubind.io",
                "john.talavera@ubind.io",
                "Test subject",
                "some text plain text body",
                "some html body");

            // Act
            var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForCustomer(
                emailModel,
                customerId,
                personId,
                organisationId,
                emailType,
                timestamp,
                new Mock<IFileContentRepository>().Object);

            var emailRecipient = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.MessageRecipient);
            var customerEmail = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.CustomerMessage);
            var emailSender = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.MessageSender);

            // Assert
            Assert.Equal(emailRecipient.ToEntityId, personId);
            Assert.Equal(customerEmail.FromEntityId, customerId);
            Assert.Equal(emailSender.ToEntityId, organisationId);
            Assert.Contains(metadata.Tags, x => expectedTags.Contains(x.Value));
            Assert.NotNull(metadata.Tags.FirstOrDefault(x => x.TagType == TagType.EmailType && x.Value == emailType.ToString()));
            Assert.NotNull(metadata.Tags.FirstOrDefault(x => x.TagType == TagType.Environment && x.Value == environment.ToString()));
        }

        [Fact]
        public void AccountActivationCreateForUser_ReturnsCorrectMetadata()
        {
            // Arrange
            var expectedTags = new List<string>() { DefaultEmailTags.AccountActivation, DefaultEmailTags.Invitation };
            var userId = Guid.NewGuid();
            var personId = Guid.NewGuid();
            var senderUserId = Guid.NewGuid();
            var organisationId = Guid.NewGuid();
            var emailType = EmailType.Admin;
            var environment = DeploymentEnvironment.Development;
            Instant timestamp = SystemClock.Instance.GetCurrentInstant();
            var emailModel = new EmailModel(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                environment,
                "noreply@ubind.io",
                "john.talavera@ubind.io",
                "Test subject",
                "some text plain text body",
                "some html body");

            // Act
            var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForUser(
                emailModel,
                userId,
                personId,
                organisationId,
                emailType,
                timestamp,
                new Mock<IFileContentRepository>().Object);

            var emailRecipient = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.MessageRecipient);
            var emailSender = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.MessageSender);

            // Assert
            Assert.Equal(emailRecipient.ToEntityId, personId);
            Assert.Equal(emailSender.ToEntityId, organisationId);
            Assert.Contains(metadata.Tags, x => expectedTags.Contains(x.Value));
            Assert.NotNull(metadata.Tags.FirstOrDefault(x => x.TagType == TagType.EmailType && x.Value == emailType.ToString()));
            Assert.NotNull(metadata.Tags.FirstOrDefault(x => x.TagType == TagType.Environment && x.Value == environment.ToString()));
        }

        [Fact]
        public void AccountActivationCreateForCustomer_ReturnsCorrectMetadata()
        {
            // Arrange
            var expectedTags = new List<string>() { DefaultEmailTags.AccountActivation, DefaultEmailTags.Invitation };
            var customerId = Guid.NewGuid();
            var senderUserId = Guid.NewGuid();
            var personId = Guid.NewGuid();
            var emailType = EmailType.Admin;
            var environment = DeploymentEnvironment.Development;
            Instant timestamp = SystemClock.Instance.GetCurrentInstant();
            var emailModel = new EmailModel(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                environment,
                "noreply@ubind.io",
                "john.talavera@ubind.io",
                "Test subject",
                "some text plain text body",
                "some html body");

            // Act
            var organisationId = Guid.NewGuid();
            var metadata = SystemEmailMetadataFactory.PasswordReset.CreateForCustomer(
                emailModel,
                customerId,
                personId,
                organisationId,
                emailType,
                timestamp,
                new Mock<IFileContentRepository>().Object);

            var emailRecipient = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.MessageRecipient);
            var customerEmail = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.CustomerMessage);
            var emailSender = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.MessageSender);

            // Assert
            Assert.Equal(emailRecipient.ToEntityId, personId);
            Assert.Equal(customerEmail.FromEntityId, customerId);
            Assert.Equal(emailSender.ToEntityId, organisationId);
            Assert.Contains(metadata.Tags, x => expectedTags.Contains(x.Value));
            Assert.NotNull(metadata.Tags.FirstOrDefault(x => x.TagType == TagType.EmailType && x.Value == emailType.ToString()));
            Assert.NotNull(metadata.Tags.FirstOrDefault(x => x.TagType == TagType.Environment && x.Value == environment.ToString()));
        }

        [Fact]
        public void IntegrationEventCreateForQuote_ReturnsCorrectMetadata()
        {
            // Arrange
            var expectedTags = new List<string>() { DefaultEmailTags.Quote };
            var customerId = Guid.NewGuid();
            var personId = Guid.NewGuid();
            var senderUserId = Guid.NewGuid();
            var quoteId = Guid.NewGuid();
            var policyId = Guid.NewGuid();
            var emailType = EmailType.Admin;
            var environment = DeploymentEnvironment.Development;
            Instant timestamp = SystemClock.Instance.GetCurrentInstant();
            var emailModel = new EmailModel(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                environment,
                "noreply@ubind.io",
                "john.talavera@ubind.io",
                "Test subject",
                "some text plain text body",
                "some html body");

            // Act
            var metadata = IntegrationEmailMetadataFactory.CreateForQuote(
                emailModel, policyId, quoteId, default, senderUserId, customerId, personId, emailType, timestamp, new Mock<IFileContentRepository>().Object);

            var quote = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.QuoteMessage);
            var policy = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.PolicyMessage);
            var emailRecipient = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.MessageRecipient);
            var customerEmail = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.CustomerMessage);
            var emailSender = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.MessageSender);

            // Assert
            Assert.Equal(quote.FromEntityId, quoteId);
            Assert.Equal(policy.FromEntityId, policyId);
            Assert.Equal(emailRecipient.ToEntityId, personId);
            Assert.Equal(customerEmail.FromEntityId, customerId);
            Assert.Equal(emailSender.ToEntityId, senderUserId);
            Assert.Contains(metadata.Tags, x => expectedTags.Contains(x.Value));
            Assert.NotNull(metadata.Tags.FirstOrDefault(x => x.TagType == TagType.EmailType && x.Value == emailType.ToString()));
            Assert.NotNull(metadata.Tags.FirstOrDefault(x => x.TagType == TagType.Environment && x.Value == environment.ToString()));
        }

        [Fact]
        public void IntegrationEventCreateForPolicy_ReturnsCorrectMetadata()
        {
            // Arrange
            var eventTypeTag = DefaultEmailTags.Purchase;
            var expectedTags = new List<string>() { DefaultEmailTags.Policy, eventTypeTag };
            var customerId = Guid.NewGuid();
            var personId = Guid.NewGuid();
            var senderUserId = Guid.NewGuid();
            var quoteId = Guid.NewGuid();
            var policyId = Guid.NewGuid();
            var policyTransactionId = Guid.NewGuid();
            var emailType = EmailType.Admin;
            var environment = DeploymentEnvironment.Development;
            Instant timestamp = SystemClock.Instance.GetCurrentInstant();
            var emailModel = new EmailModel(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                environment,
                "noreply@ubind.io",
                "john.talavera@ubind.io",
                "Test subject",
                "some text plain text body",
                "some html body");

            // Act
            var metadata = IntegrationEmailMetadataFactory.CreateForPolicy(
                emailModel, policyId, quoteId, policyTransactionId, default, senderUserId, customerId, personId, ApplicationEventType.PolicyIssued, emailType, timestamp, new Mock<IFileContentRepository>().Object);

            var quote = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.QuoteMessage);
            var policy = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.PolicyMessage);
            var policyTransaction = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.PolicyTransactionMessage);
            var emailRecipient = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.MessageRecipient);
            var customerEmail = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.CustomerMessage);
            var emailSender = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.MessageSender);

            // Assert
            Assert.Equal(policyTransaction.FromEntityId, policyTransactionId);
            Assert.Equal(quote.FromEntityId, quoteId);
            Assert.Equal(policy.FromEntityId, policyId);
            Assert.Equal(emailRecipient.ToEntityId, personId);
            Assert.Equal(customerEmail.FromEntityId, customerId);
            Assert.Equal(emailSender.ToEntityId, senderUserId);
            Assert.Contains(metadata.Tags, x => expectedTags.Contains(x.Value));
            Assert.NotNull(metadata.Tags.FirstOrDefault(x => x.TagType == TagType.EmailType && x.Value == emailType.ToString()));
            Assert.NotNull(metadata.Tags.FirstOrDefault(x => x.TagType == TagType.Environment && x.Value == environment.ToString()));
        }

        [Fact]
        public void IntegrationEventCreateForRenewalInvitation_ReturnsCorrectMetadata()
        {
            // Arrange
            var expectedTags = new List<string>() { DefaultEmailTags.Policy, DefaultEmailTags.Renewal, DefaultEmailTags.Invitation };
            var customerId = Guid.NewGuid();
            var personId = Guid.NewGuid();
            var senderUserId = Guid.NewGuid();
            var quoteId = Guid.NewGuid();
            var policyId = Guid.NewGuid();
            var policyTransactionId = Guid.NewGuid();
            var emailType = EmailType.Admin;
            var environment = DeploymentEnvironment.Development;
            Instant timestamp = SystemClock.Instance.GetCurrentInstant();
            var emailModel = new EmailModel(
                TenantFactory.DefaultId,
                Guid.NewGuid(),
                ProductFactory.DefaultId,
                environment,
                "noreply@ubind.io",
                "john.talavera@ubind.io",
                "Test subject",
                "some text plain text body",
                "some html body");

            // Act
            var metadata = IntegrationEmailMetadataFactory.CreateForRenewalInvitation(
                emailModel, policyId, quoteId, policyTransactionId, senderUserId, customerId, personId, emailType, timestamp, new Mock<IFileContentRepository>().Object);

            var quote = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.QuoteMessage);
            var policy = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.PolicyMessage);
            var policyTransaction = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.PolicyTransactionMessage);
            var emailRecipient = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.MessageRecipient);
            var customerEmail = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.CustomerMessage);
            var emailSender = metadata.Relationships.FirstOrDefault(x => x.Type == RelationshipType.MessageSender);

            // Assert
            Assert.Equal(policyTransaction.FromEntityId, policyTransactionId);
            Assert.Equal(quote.FromEntityId, quoteId);
            Assert.Equal(policy.FromEntityId, policyId);
            Assert.Equal(emailRecipient.ToEntityId, personId);
            Assert.Equal(customerEmail.FromEntityId, customerId);
            Assert.Equal(emailSender.ToEntityId, senderUserId);
            Assert.Contains(metadata.Tags, x => expectedTags.Contains(x.Value));
            Assert.NotNull(metadata.Tags.FirstOrDefault(x => x.TagType == TagType.EmailType && x.Value == emailType.ToString()));
            Assert.NotNull(metadata.Tags.FirstOrDefault(x => x.TagType == TagType.Environment && x.Value == environment.ToString()));
        }
    }
}
