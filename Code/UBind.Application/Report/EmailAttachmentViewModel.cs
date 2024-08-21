// <copyright file="EmailAttachmentViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Report
{
    using DotLiquid;
    using Humanizer;
    using UBind.Domain;

    /// <summary>
    /// Email attachment view model for dot liquid template.
    /// </summary>
    public class EmailAttachmentViewModel : Drop
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailAttachmentViewModel"/> class.
        /// </summary>
        /// <param name="emailAttachment">The email attachment.</param>
        /// <param name="invitationLinkHost">The email invitation link host value in app setting.</param>
        public EmailAttachmentViewModel(EmailAttachment emailAttachment, string invitationLinkHost)
        {
            this.FileName = emailAttachment.DocumentFile.Name;
            this.FileSizeBytes = emailAttachment.DocumentFile.FileContent.Content.Length.ToString();
            this.FileSizeString = emailAttachment.DocumentFile.FileContent.Content.Length.Bytes().Humanize("#.##");
            this.DownloadUrl = $"{invitationLinkHost}/api/v1/email/{emailAttachment.EmailId}/attachment/{emailAttachment.Id}";
        }

        /// <summary>
        /// Gets the filename of attachment.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the size in bytes of the attachment.
        /// </summary>
        public string FileSizeBytes { get; }

        /// <summary>
        /// Gets the size in string of the attachment.
        /// </summary>
        public string FileSizeString { get; }

        /// <summary>
        /// Gets the download url of the attachment.
        /// </summary>
        public string DownloadUrl { get; }
    }
}
