// <copyright file="AttachmentExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Extensions.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using MimeKit;
    using NodaTime;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Extension methods for attachment extensions.
    /// </summary>
    public static class AttachmentExtensions
    {
        /// <summary>
        /// Converts System.net.Mail.Attachments to EmailAttachment entities.
        /// </summary>
        /// <returns>The list of entity email attachment.</returns>
        public static Collection<EmailAttachment> ToEmailAttachmentEntities(
            this IList<MimeEntity> attachments, Guid tenantId, Guid emailId, Instant timestamp, IFileContentRepository fileContentRepository)
        {
            var emailAttachments = new Collection<EmailAttachment>();
            foreach (var attachment in attachments)
            {
                if (attachment == null)
                {
                    continue;
                }

                var part = (MimePart)attachment;
                var fileSizeInBytes = part.Content.Stream.Length;
                byte[] content = new byte[fileSizeInBytes];
                part.Content.Stream.Read(content, 0, (int)fileSizeInBytes);
                var fileContent = FileContent.CreateFromBytes(tenantId, Guid.NewGuid(), content);
                var existingFileContent = fileContentRepository.GetFileContentByHashCode(tenantId, fileContent.HashCode);
                if (existingFileContent == null)
                {
                    fileContentRepository.Insert(fileContent);
                }
                else
                {
                    fileContent = existingFileContent;
                }

                var documentFile = new DocumentFile(
                    attachment.ContentDisposition.FileName, attachment.ContentType.MimeType, fileContent, timestamp);
                emailAttachments.Add(new EmailAttachment(emailId, documentFile, timestamp));
            }

            return emailAttachments;
        }

        public static MimeEntity ResolveAttachment(
            this MimeEntity attachment, string fileName, byte[] content)
        {
            if (attachment is MessagePart)
            {
                string mimeType = attachment.ContentType.MimeType;
                ContentType contentType = ContentType.Parse(mimeType);
                attachment = new MimePart(contentType.MediaType, contentType.MediaSubtype)
                {
                    Content = new MimeContent(new MemoryStream(content)),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    FileName = fileName,
                };
            }
            else
            {
                attachment.ContentDisposition = new ContentDisposition(ContentDisposition.Attachment)
                {
                    FileName = fileName,
                };
            }

            return attachment;
        }
    }
}
