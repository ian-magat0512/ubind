// <copyright file="IFileContentMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.Migration
{
    public interface IFileContentMigration
    {
        /// <summary>
        /// Populate file contents for quote file attachments called from startup job.
        /// </summary>
        void PopulateFileContentsForQuoteFileAttachments();

        /// <summary>
        /// Populate asset file contents.
        /// </summary>
        void PopulateAssetFileContents();

        /// <summary>
        /// Populate email attachment file contents from document files.
        /// </summary>
        void PopulateFileContentsFromDocumentFiles();

        /// <summary>
        /// Process batch called when the job fails and retry is needed.
        /// </summary>
        /// <param name="batch">The batch number.</param>
        void ProcessBatch(int batch);

        /// <summary>
        /// Remove file content from event json.
        /// </summary>
        void RemoveEventJsonFileContent();

        /// <summary>
        /// Process event JSON batch called when the job fails and retry is needed.
        /// </summary>
        /// <param name="batch">The batch number.</param>
        void ProcessEventJsonBatch(int batch);

        /// <summary>
        /// Remove new event JSON contents that were added while R1 was running.
        /// </summary>
        void RemoveEventJsonFileContentCleanup();

        /// <summary>
        /// Cleanup document file contents after migration of file contents.
        /// </summary>
        void CleanupDocumentFileContents();
    }
}
