// <copyright file="IGraphUrlProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph
{
    using Flurl;

    /// <summary>
    /// Provides URLs for Microsoft Graph usage.
    /// </summary>
    public interface IGraphUrlProvider
    {
        /// <summary>
        /// Gets the origin for Microsoft Graph.
        /// </summary>
        Url GraphOrigin { get; }

        /// <summary>
        /// Gets the origin for Microsoft Graph Batch.
        /// </summary>
        Url GraphBatchOrigin { get; }

        /// <summary>
        /// Gets the origin for the Authentication provider.
        /// </summary>
        Url AuthProviderOrigin { get; }

        /// <summary>
        /// Gets the URL for subscribing to change notifications..
        /// </summary>
        Url SubscriptionsUrl { get; }

        /// <summary>
        /// Gets the path for the OAuth token.
        /// </summary>
        string OAuthTokenPathSegment { get; }

        /// <summary>
        /// Gets the URL for creating a workbook session.
        /// </summary>
        /// <param name="workbookPath">The path of the workbook in One Drive.</param>
        /// <returns>The URL for creating a workbook session.</returns>
        Url GetCreateSessionUrl(string workbookPath);

        /// <summary>
        /// Gets the URL for copying a drive item.
        /// </summary>
        /// <param name="path">The path to the item in One Drive.</param>
        /// <returns>The URL for copying the drive item.</returns>
        Url GetCopyItemUrl(string path);

        /// <summary>
        /// Gets the URL for referencing a column in a table in a worksheet.
        /// </summary>
        /// <param name="workbookPath">The path of the workbook in One Drive.</param>
        /// <param name="worksheetName">The name of the worksheet.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column.</param>
        /// <returns>The URL for referencing a column in a table in a worksheet.</returns>
        Url GetWorksheetTableColumnUrl(
            string workbookPath,
            string worksheetName,
            string tableName,
            string columnName);

        /// <summary>
        /// Gets the URL for referencing the text in a table in a worksheet.
        /// </summary>
        /// <param name="workbookPath">The path of the workbook in One Drive.</param>
        /// <param name="worksheetName">The name of the worksheet.</param>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The URL for referencing the text in a table in a worksheet.</returns>
        Url GetWorksheetTableTextUrl(
            string workbookPath,
            string worksheetName,
            string tableName);

        /// <summary>
        /// Gets the URL for uploading a file to One Drive.
        /// </summary>
        /// <param name="destinationPath">The path relative to the One Drive root where the file will be uploaded.</param>
        /// <returns>The URL for uploading a file to One Drive.</returns>
        Url GetUploadFileUrl(string destinationPath);

        /// <summary>
        /// Gets the URL for referencing a range of cells in a worksheet.
        /// </summary>
        /// <param name="workbookPath">The path of the workbook in One Drive.</param>
        /// <param name="worksheetName">The name of the worksheet.</param>
        /// <param name="address">The address of the range of cells.</param>
        /// <returns>The URL for referencing a range of cells in a worksheet.</returns>
        Url GetWorksheetAddressUrl(
            string workbookPath,
            string worksheetName,
            string address);

        /// <summary>
        /// Gets the URL for creating a folder in One Drive.
        /// </summary>
        /// <param name="parentFolderPath">The path of the folder to create it in, or null if folder should be created in root.</param>
        /// <returns>The URL for creating a folder in One Drive.</returns>
        Url GetCreateFolderUrl(string parentFolderPath = null);

        /// <summary>
        /// Gets the URL for updating an item in One Drive.
        /// </summary>
        /// <param name="itemPath">THe path of the item to update.</param>
        /// <returns>The URL for updating the item in One Drive.</returns>
        Url GetUpdateItemUrl(string itemPath);

        /// <summary>
        /// Gets the URL given the folder path.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <returns>Url for folder path.</returns>
        Url GetFolderPath(string folderPath);

        /// <summary>
        /// Gets the URL for downloading a file's contents from OneDrive.
        /// </summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns>The URL for downloading the file's contents from OneDrive.</returns>
        Url GetFileContentsPath(string filePath);

        Url GetFileMetadataPath(string filePath);

        /// <summary>
        /// Gets the URL for getting a folder's children in OneDrive.
        /// </summary>
        /// <param name="folderPath">The path to the folder whose children are sought.</param>
        /// <returns>The URL for getting the folder's children.</returns>
        Url GetFolderChildrenPath(string folderPath);

        /// <summary>
        /// Gets the URL for getting a folder's children in OneDrive.
        /// </summary>
        /// <param name="folderPath">The path to the folder whose children are sought.</param>
        /// <returns>The URL for getting the folder's children.</returns>
        Url GetBatchRequestFolderChildrenUrl(string folderPath);

        /// <summary>
        /// Gets the URL for creating a link for a file from OneDrive.
        /// </summary>
        /// <param name="item">The item id.</param>
        /// <returns>The url for creating a link to the file.</returns>
        Url GetCreateLinkFileUrl(string item);
    }
}
