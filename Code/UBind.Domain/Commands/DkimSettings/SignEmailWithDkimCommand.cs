// <copyright file="SignEmailWithDkimCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Commands.DkimSettings
{
    using System;
    using MediatR;
    using MimeKit;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command for signing email with DKIM.
    /// </summary>
    public class SignEmailWithDkimCommand : ICommand<Unit>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignEmailWithDkimCommand"/> class.
        /// </summary>
        /// <param name="organisationId">The organisation Id.</param>
        /// <param name="mimeMessage">The mime message to sign.</param>
        /// <param name="emailSource">The email source.</param>
        public SignEmailWithDkimCommand(Guid tenantId, Guid organisationId, MimeMessage mimeMessage, EmailSource emailSource)
        {
            this.TenantId = tenantId;
            this.OrganisationId = organisationId;
            this.MimeMessage = mimeMessage;
            this.EmailSource = emailSource;
        }

        /// <summary>
        /// Gets the mime message.
        /// </summary>
        public MimeMessage MimeMessage { get; }

        /// <summary>
        /// Gets the organisation Id.
        /// </summary>
        public Guid OrganisationId { get; }

        /// <summary>
        /// Gets the tenant Id.
        /// </summary>
        public Guid TenantId { get; }

        /// <summary>
        /// Gets the email source.
        /// </summary>
        public EmailSource EmailSource { get; }
    }
}
