// <copyright file="IEmailService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services
{
    using System;
    using System.Collections.Generic;
    using MimeKit;
    using UBind.Application.Automation.Entities;
    using UBind.Domain;
    using UBind.Domain.ReadWriteModel.Email;

    /// <summary>
    /// Portal service for handling email persistence.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Persiste email and metadata in the database.
        /// </summary>
        /// <param name="emailAndMetadata">An email and its metadata.</param>
        void InsertEmailAndMetadata(EmailAndMetadata emailAndMetadata);

        /// <summary>
        /// Method for saving MailMessage to the database.
        /// </summary>
        /// <param name="tenantId">The tenant Id of the email entity.</param>
        /// <param name="mailMessage">The mail message that will be converted to email entity.</param>
        /// <param name="organisationId">The organisation Id of the email entity.</param>
        /// <param name="productId">The product Id of the email entity.</param>
        /// <param name="environment">The environment of the email entity.</param>
        /// <param name="tags">The tags that will be attached to the email entity.</param>
        /// <param name="relationships">The relationships that will be attached to the email entity.</param>
        void SaveMailMessage(
            Guid tenantId,
            MimeMessage mailMessage,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment environment,
            IEnumerable<string>? tags,
            IEnumerable<Relationship>? relationships);
    }
}
