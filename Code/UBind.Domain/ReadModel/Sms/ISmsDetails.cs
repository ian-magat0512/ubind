// <copyright file="ISmsDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Sms
{
    using System;
    using System.Collections.Generic;
    using NodaTime;
    using UBind.Domain.ReadModel.Email;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// Presentation model for sms and its related records.
    /// </summary>
    public interface ISmsDetails
    {
        /// <summary>
        /// Gets the sms Id.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the tenant Id of the sms.
        /// </summary>
        Guid TenantId { get; }

        /// <summary>
        /// Gets the Organisation Id of the sms.
        /// </summary>
        Guid OrganisationId { get; }

        /// <summary>
        /// Gets the product Id of the sms.
        /// </summary>
        Guid ProductId { get; }

        /// <summary>
        /// Gets the sms recipient.
        /// </summary>
        string To { get; }

        /// <summary>
        /// Gets the sms sender.
        /// </summary>
        string From { get; }

        /// <summary>
        /// Gets the sms message.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Gets the sms's customer record.
        /// </summary>
        CustomerData Customer { get; }

        /// <summary>
        /// Gets the sms's policy record.
        /// </summary>
        PolicyData Policy { get; }

        /// <summary>
        /// Gets the sms's quote record.
        /// </summary>
        QuoteData Quote { get; }

        /// <summary>
        /// Gets the sms's claim record.
        /// </summary>
        ClaimData Claim { get; }

        /// <summary>
        /// Gets the sms's policy transaction record.
        /// </summary>
        PolicyTransactionData PolicyTransaction { get; }

        /// <summary>
        /// Gets the sms's user record.
        /// </summary>
        UserData User { get; }

        /// <summary>
        /// Gets the sms's organisation record.
        /// </summary>
        OrganisationData Organisation { get; }

        /// <summary>
        /// Gets the date the quote sms was created.
        /// </summary>
        Instant CreatedTimestamp { get; }

        /// <summary>
        /// Gets the associated tags.
        /// </summary>
        IEnumerable<Tag> Tags { get; }

        /// <summary>
        /// Gets the associated relationships.
        /// </summary>
        IEnumerable<Relationship> Relationships { get; }
    }
}
