// <copyright file="IEmailQueryService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Email;

    /// <summary>
    /// Portal service for handling email retrieval.
    /// </summary>
    public interface IEmailQueryService
    {
        /// <summary>
        /// Retrieve a list of emails known for a given user, product and environment.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Additional filters to be applied to the result set.</param>
        /// <returns>Collectin of quote emails.</returns>
        IEnumerable<Domain.ReadWriteModel.Email.Email> GetAll(
            Guid tenantId, EntityListFilters filters);

        /// <summary>
        /// Retrieve a list of email known for a given user, product and environment.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Additional filters to be applied to the result set.</param>
        /// <returns>Collectin of quote emails.</returns>
        IEnumerable<Domain.ReadWriteModel.Email.Email> GetByPolicyId(
            Guid tenantId, Guid policyId, EntityListFilters filters);

        /// <summary>
        /// Retrieve a list of email known for a given user, product and environment.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Additional filters to be applied to the result set.</param>
        /// <returns>Collectin of quote emails.</returns>
        IEnumerable<Domain.ReadWriteModel.Email.Email> GetByQuoteId(
            Guid tenantId, Guid quoteId, EntityListFilters filters);

        /// <summary>
        /// Retrieve a list of email for a given claim Id.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Additional filters to be applied to the result set.</param>
        /// <returns>Collectin of claim emails.</returns>
        IEnumerable<Domain.ReadWriteModel.Email.Email> GetByClaimId(
            Guid tenantId, Guid claimId, EntityListFilters filters);

        /// <summary>
        /// Retrieve a list of email known for a given user, product and environment.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Additional filters to be applied to the result set.</param>
        /// <returns>Collectin of quote emails.</returns>
        IEnumerable<Domain.ReadWriteModel.Email.Email> GetByPolicyTransactionId(
            Guid tenantId,
            Guid policyTransactionId,
            EntityListFilters filters);

        /// <summary>
        /// Retrieve a list of email known for a given user, product and environment.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Additional filters to be applied to the result set.</param>
        /// <returns>Collectin of quote emails.</returns>
        IEnumerable<Domain.ReadWriteModel.Email.Email> GetByQuoteVersionId(
            Guid tenantId, Guid quoteVersionId, EntityListFilters filters);

        /// <summary>
        /// Retrieve a list of email for a given user, product and environment.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Additional filters to be applied to the result set.</param>
        /// <returns>List of quote emails as enumerable.</returns>
        IEnumerable<Domain.ReadWriteModel.Email.Email> GetEmailsForCustomer(
            Guid tenantId, Guid customerId, EntityListFilters filters);

        /// <summary>
        /// Retrieve a list of email for a given user, product and environment.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="filters">Additional filters to be applied to the result set.</param>
        /// <returns>Collectin of quote emails.</returns>
        IEnumerable<Domain.ReadWriteModel.Email.Email> GetEmailsForUser(
            Guid tenantId, Guid userId, EntityListFilters filters);

        /// <summary>
        /// Retrieve email by id.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="emailId">The email id.</param>
        /// <returns>The email entity.</returns>
        Domain.ReadWriteModel.Email.Email GetById(Guid tenantId, Guid emailId);

        /// <summary>
        /// Retrieve email by id including the files.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="emailId">The email id.</param>
        /// <returns>The email entity.</returns>
        Domain.ReadWriteModel.Email.Email GetWithFilesById(Guid tenantId, Guid emailId);

        /// <summary>
        /// retrieve email detail for the user.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="id">email id.</param>
        /// <returns>the quote details.</returns>
        IEmailDetails GetDetails(Guid tenantId, Guid id);

        /// <summary>
        /// Retrieve attachment.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="emailId">The email id.</param>
        /// <param name="attachmentId">The attachment id.</param>
        /// <returns>The file content read model.</returns>
        IFileContentReadModel GetEmailAttachment(
            Guid tenantId, Guid emailId, Guid attachmentId);
    }
}
