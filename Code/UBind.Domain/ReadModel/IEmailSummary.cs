// <copyright file="IEmailSummary.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel
{
    using System;

    /// <summary>
    /// Data transfer object for Email Read Model.
    /// Used for email listing queries.
    /// </summary>
    public interface IEmailSummary : IEntityReadModel<Guid>
    {
        /// <summary>
        /// Gets the email's recipient.
        /// </summary>
        string Recipient { get; }

        /// <summary>
        /// Gets the email's subject.
        /// </summary>
        string Subject { get; }

        /// <summary>
        /// Gets a value indicating whether the email has an attachment.
        /// </summary>
        bool HasAttachment { get; }
    }
}
