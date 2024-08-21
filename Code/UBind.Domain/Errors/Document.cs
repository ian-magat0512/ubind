// <copyright file="Document.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System.Net;
    using MimeKit;

    public static partial class Errors
    {
        public static class Document
        {
            public static Error DocumentStoredIncorrectly(string attachment) =>
                new Error(
                    "document.stored.incorrectly",
                    $"We couldn't find the document '{attachment}'",
                    $"The document you have requested was stored incorrectly during saving and cannot be retrieved at this time." +
                    $" Please get in touch with our support team so that we can update this document and allow you to access it.",
                    HttpStatusCode.NotFound);

            public static Error AttachmentContentLengthMismatch(MimePart mimePart, int bytesRead) =>
                new Error(
                    "attachment.content.length.mismatch",
                    "Attachment content length mismatch",
                    $"The length of the attached content '{mimePart.ContentDisposition.FileName}' " +
                    $"is expected to be {mimePart.Content.Stream.Length} bytes, but {bytesRead} bytes were read."
                    + "We apologise for the inconvenience. Please get in touch with customer support so we can help you "
                    + "resolve this issue.",
                    HttpStatusCode.ExpectationFailed);
        }
    }
}
