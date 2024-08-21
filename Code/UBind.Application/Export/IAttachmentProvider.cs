// <copyright file="IAttachmentProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System.Threading.Tasks;
    using MimeKit;
    using UBind.Domain;

    /// <summary>
    /// For providing attachments for emails.
    /// </summary>
    public interface IAttachmentProvider
    {
        /// <summary>
        /// Creates an attachment for an email in response to an application event.
        /// </summary>
        /// <param name="applicationEvent">The application event.</param>
        /// <returns>A new attachment.</returns>
        Task<MimeEntity> Invoke(ApplicationEvent applicationEvent);

        /// <summary>
        /// Gets the value whether this attachment is included or not.
        /// </summary>
        /// <param name="applicationEvent">The application event.</param>
        /// <returns>Whether the attachment is included or not.</returns>
        Task<bool> IsIncluded(ApplicationEvent applicationEvent);
    }
}
