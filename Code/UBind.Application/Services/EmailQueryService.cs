// <copyright file="EmailQueryService.cs" company="uBind">
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
    using UBind.Domain;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Email;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class EmailQueryService : IEmailQueryService
    {
        private readonly IEmailRepository emailRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailQueryService"/> class.
        /// </summary>
        /// <param name="emailRepository">The quote email model repository.</param>
        public EmailQueryService(
            IEmailRepository emailRepository)
        {
            Contract.Assert(emailRepository != null);

            this.emailRepository = emailRepository;
        }

        /// <inheritdoc/>
        public IEmailDetails GetDetails(Guid tenantId, Guid id)
        {
            var emailDetails = this.emailRepository.GetEmailDetailsWithAttachments(tenantId, id);
            return emailDetails;
        }

        /// <inheritdoc/>
        public Domain.ReadWriteModel.Email.Email GetById(Guid tenantId, Guid emailId)
        {
            return this.emailRepository.GetById(tenantId, emailId);
        }

        /// <inheritdoc/>
        public Domain.ReadWriteModel.Email.Email GetWithFilesById(Guid tenantId, Guid emailId)
        {
            return this.emailRepository.GetWithFilesById(tenantId, emailId);
        }

        /// <inheritdoc/>
        public IEnumerable<Domain.ReadWriteModel.Email.Email> GetByPolicyId(
            Guid tenantId, Guid policyId, EntityListFilters filters)
        {
            return this.emailRepository.GetByPolicyId(tenantId, policyId, filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Domain.ReadWriteModel.Email.Email> GetByQuoteId(
            Guid tenantId, Guid quoteId, EntityListFilters filters)
        {
            return this.emailRepository.GetByQuoteId(tenantId, quoteId, filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Domain.ReadWriteModel.Email.Email> GetByClaimId(
            Guid tenantId, Guid claimId, EntityListFilters filters)
        {
            return this.emailRepository.GetByClaimId(tenantId, claimId, filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Domain.ReadWriteModel.Email.Email> GetByPolicyTransactionId(
            Guid tenantId,
            Guid policyTransactionId,
            EntityListFilters filters)
        {
            return this.emailRepository.GetByPolicyTransactionId(tenantId, policyTransactionId, filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Domain.ReadWriteModel.Email.Email> GetByQuoteVersionId(
            Guid tenantId,
            Guid quoteVersionId,
            EntityListFilters filters)
        {
            return this.emailRepository.GetByQuoteVersionId(tenantId, quoteVersionId, filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Domain.ReadWriteModel.Email.Email> GetEmailsForCustomer(
            Guid tenantId, Guid customerId, EntityListFilters filters)
        {
            var emails = this.emailRepository.GetEmailsForCustomer(tenantId, customerId, filters);
            return emails;
        }

        /// <inheritdoc/>
        public IEnumerable<Domain.ReadWriteModel.Email.Email> GetEmailsForUser(
            Guid tenantId, Guid userId, EntityListFilters filters)
        {
            return this.emailRepository.GetByUserId(tenantId, userId, filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Domain.ReadWriteModel.Email.Email> GetAll(
            Guid tenantId, EntityListFilters filters)
        {
            if (filters.EntityType != null)
            {
                switch (filters.EntityType)
                {
                    case EntityType.Quote:
                    case EntityType.Policy:
                    case EntityType.Customer:
                    case EntityType.PolicyTransaction:
                    case EntityType.Claim:
                        return this.emailRepository.GetAll(tenantId, filters, false);
                    case EntityType.User:
                    default:
                        break;
                }
            }

            return this.emailRepository.GetAll(tenantId, filters);
        }

        /// <inheritdoc/>
        public IFileContentReadModel GetEmailAttachment(
            Guid tenantId,
            Guid emailId,
            Guid attachmentId)
        {
            var email = this.GetDetails(tenantId, emailId);
            var attachment = email?.EmailAttachments?.FirstOrDefault(x => x.Id == attachmentId);
            if (attachment == null)
            {
                return null;
            }

            return new FileContentReadModel
            {
                FileContent = attachment.DocumentFile.FileContent.Content,
                ContentType = attachment.DocumentFile.Type,
            };
        }
    }
}
