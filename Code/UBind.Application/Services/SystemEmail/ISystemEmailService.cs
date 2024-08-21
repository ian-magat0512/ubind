// <copyright file="ISystemEmailService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Services.SystemEmail
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Entities;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.ReadWriteModel.Email;

    /// <summary>
    /// Interface for mail provider.
    /// </summary>
    public interface ISystemEmailService
    {
        /// <summary>
        /// Sends the mail message.
        /// </summary>
        /// <param name="model">The required values for sending the mail.</param>
        /// <param name="environment">The deployment environment.</param>
        [DisplayName("Send System Email | TENANT: {0}, ORGANISATION, {1}, TYPE: {2}, EMAIL ADDRESS: {3}")]
        Task SendMessage(
            string tenantAlias,
            string organisationAlias,
            string emailType,
            string emailAddress,
            EmailDrop emailDrop,
            DeploymentEnvironment? environment = null);

        /// <summary>
        /// Create, send and persist a password reset invitation email.
        /// </summary>
        /// <param name="emailDrop">The view model for templating the email.</param>
        /// <param name="userAggregate">The user the email is for.</param>
        /// <returns>The email that was persisted.</returns>
        Email SendAndPersistPasswordResetInvitationEmail(
            EmailDrop emailDrop,
            UserAggregate userAggregate);

        /// <summary>
        /// Create, send and persist an account activation invitation email.
        /// </summary>
        /// <param name="emailDrop">The view model for the email.</param>
        /// <param name="userAggregate">The user the email is for.</param>
        /// <returns>The email that was persisted.</returns>
        Email SendAndPersistAccountActivationInvitationEmail(
            EmailDrop emailDrop,
            UserAggregate userAggregate);

        /// <summary>
        /// Create, send and persist an account already activated email.
        /// </summary>
        /// <param name="emailDrop">The view model for the email.</param>
        /// <param name="userAggregate">The user the email is for.</param>
        /// <returns>The email that was persisted.</returns>
        Email SendAndPersistAccountAlreadyActivatedEmail(
            EmailDrop emailDrop,
            UserAggregate userAggregate);

        /// <summary>
        /// Create quote association invitation email.
        /// </summary>
        /// <param name="emailDrop">The view model for the email.</param>
        /// <param name="userAggregate">The user the email is for.</param>
        /// <param name="quoteId">The quote ID.</param>
        /// <returns>The email that was persisted.</returns>
        Email SendAndPersistQuoteAssociationInvitationEmail(
            EmailDrop emailDrop,
            UserAggregate userAggregate,
            Guid quoteId);

        /// <summary>
        /// Create, send and persist a policy renewal invitation email.
        /// </summary>
        /// <param name="emailDrop">The view model for the email.</param>
        /// <param name="quoteAggregate">The quote aggregate the renewal belongs to.</param>
        /// <param name="customerPersonId">The customerPersonId the renewal belongs to.</param>
        /// <param name="policyTransaction">The latest policy transaction.</param>
        /// <param name="quote">The quote related to the latest policy transaction.</param>
        /// <returns>The email that was persisted.</returns>
        Email SendAndPersistPolicyRenewalEmail(
            EmailDrop emailDrop,
            QuoteAggregate quoteAggregate,
            Guid customerPersonId,
            PolicyTransaction policyTransaction,
            Quote quote);
    }
}
