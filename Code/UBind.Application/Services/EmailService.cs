// <copyright file="EmailService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using MimeKit;
    using NodaTime;
    using UBind.Application.Automation.Entities;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Quote;
    using UBind.Domain.ReadWriteModel.Email;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class EmailService : IEmailService
    {
        private readonly IEmailRepository emailRepository;
        private readonly IClock clock;
        private readonly IFileContentRepository fileContentRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailService"/> class.
        /// </summary>
        /// <param name="emailRepository">The quote email model repository.</param>
        /// <param name="clock">The system clock.</param>
        public EmailService(
            IEmailRepository emailRepository,
            IFileContentRepository fileContentRepository,
            IClock clock)
        {
            Contract.Assert(emailRepository != null);
            this.emailRepository = emailRepository;
            this.fileContentRepository = fileContentRepository;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public void InsertEmailAndMetadata(EmailAndMetadata metadata)
        {
            this.emailRepository.InsertEmailAndMetadata(metadata);
        }

        /// <inheritdoc/>
        public void SaveMailMessage(
            Guid tenantId,
            MimeMessage mailMessage,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment environment,
            IEnumerable<string> tags,
            IEnumerable<Relationship> relationships)
        {
            var emailModel =
                this.ConvertMailMessageToEmailModel(
                    tenantId,
                    mailMessage,
                    organisationId,
                    productId,
                    environment);
            var timestamp = this.clock.Now();
            var metadata = new EmailAndMetadata(emailModel, timestamp, this.fileContentRepository);

            if (relationships != null && relationships.Any())
            {
                foreach (var relationship in relationships)
                {
                    var targetEntity = relationship.TargetEntity;
                    var sourceEntity = relationship.SourceEntity;
                    var relationshipType = relationship.RelationshipType;

                    if (targetEntity != null)
                    {
                        var entityType = (EntityType)Enum.Parse(typeof(EntityType), targetEntity.GetType().Name);
                        var entityId = targetEntity.Id; //// new Guid(targetEntity.Id);
                        metadata.CreateRelationshipFromEmailToEntity(relationshipType, entityType, entityId);
                        metadata.CreateTagFromEntityType(entityType);
                    }

                    if (sourceEntity != null)
                    {
                        var entityType = (EntityType)Enum.Parse(typeof(EntityType), sourceEntity.GetType().Name);
                        var entityId = sourceEntity.Id; ////new Guid();
                        metadata.CreateRelationshipFromEntityToEmail(entityType, entityId, relationshipType);
                    }
                }
            }

            if (tags != null && tags.Any())
            {
                metadata.CreateUserDefinedTags(tags);
            }

            metadata.CreateTagFromEnvironment(environment);
            this.InsertEmailAndMetadata(metadata);
        }

        private EmailModel ConvertMailMessageToEmailModel(
            Guid tenantId,
            MimeMessage mailMessage,
            Guid organisationId,
            Guid? productId,
            DeploymentEnvironment environment)
        {
            var fromAddress = mailMessage.From.ToString();
            var toAddress = string.Join(",", mailMessage.To);
            var ccAddress = mailMessage.Cc;
            var bccAddress = mailMessage.Bcc;
            var subject = mailMessage.Subject;
            var replyToAddress = mailMessage.ResentReplyTo;

            var emailModel =
                new EmailModel(
                    tenantId,
                    organisationId,
                    productId,
                    environment,
                    fromAddress,
                    toAddress,
                    subject,
                    mailMessage.TextBody,
                    mailMessage.HtmlBody,
                    string.Join(",", ccAddress),
                    string.Join(",", bccAddress),
                    string.Join(",", replyToAddress));
            if (mailMessage.Attachments != null)
            {
                emailModel.Attachments = mailMessage.Attachments.ToList();
            }

            return emailModel;
        }
    }
}
