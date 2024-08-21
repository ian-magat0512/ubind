// <copyright file="GraphUrlProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph
{
    using Flurl;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Provides URLs for Microsoft Graph usage.
    /// </summary>
    public class GraphUrlProvider : IGraphUrlProvider
    {
        private const string OneDrivePathRoot = "v1.0/me/drive/root";

        private const string OneDriveItemRoot = "v1.0/me/drive/items";

        private const string OneDriveBtachRequestPathRoot = "/me/drive/root";

        private const string CreateSessionPathSegment = "workbook/createSession";

        private const string WorksheetTableColumnPathTemplate = "workbook/worksheets('{0}')/tables('{1}')/columns('{2}')";

        private const string WorksheetTableTextPathTemplate = "workbook/worksheets('{0}')/tables('{1}')/range";

        private const string WorksheetRangePathSegmentTemplate = "workbook/worksheets('{0}')/range(address='{1}')";

        /// <inheritdoc/>
        public Url GraphOrigin => new Url("https://graph.microsoft.com");

        /// <inheritdoc/>
        public Url AuthProviderOrigin => new Url("https://login.windows.net");

        /// <inheritdoc/>
        public Url GraphBatchOrigin => new Url("https://graph.microsoft.com/v1.0/$batch");

        /// <inheritdoc/>
        public Url SubscriptionsUrl => this.GraphOrigin.AppendPathSegments(OneDrivePathRoot, "Subscriptions");

        /// <inheritdoc/>
        public string OAuthTokenPathSegment => "oauth2/token";

        /// <inheritdoc/>
        public Url GetCreateSessionUrl(string workbookPath)
        {
            var url = Url.Combine(
                this.GraphOrigin,
                PathHelper.AppendColon(OneDrivePathRoot),
                PathHelper.AppendColon(workbookPath),
                CreateSessionPathSegment);
            return url;
        }

        /// <inheritdoc/>
        public Url GetWorksheetTableColumnUrl(
            string workbookPath,
            string worksheetName,
            string tableName,
            string columnName)
        {
            var worksheetTableColumnPath = string.Format(
                WorksheetTableColumnPathTemplate,
                worksheetName,
                tableName,
                columnName);
            var url = Url.Combine(
                this.GraphOrigin,
                PathHelper.AppendColon(OneDrivePathRoot),
                PathHelper.AppendColon(workbookPath),
                worksheetTableColumnPath);
            return url;
        }

        /// <inheritdoc/>
        public Url GetWorksheetTableTextUrl(
            string workbookPath,
            string worksheetName,
            string tableName)
        {
            var worksheetTableTextPath = string.Format(
                WorksheetTableTextPathTemplate,
                worksheetName,
                tableName);
            var url = Url.Combine(
                this.GraphOrigin,
                PathHelper.AppendColon(OneDrivePathRoot),
                PathHelper.AppendColon(workbookPath),
                worksheetTableTextPath);
            return url;
        }

        /// <inheritdoc/>
        public Url GetWorksheetAddressUrl(
            string workbookPath,
            string worksheetName,
            string address)
        {
            var worksheetRangePath = string.Format(
                WorksheetRangePathSegmentTemplate,
                worksheetName,
                address);
            var url = Url.Combine(
                this.GraphOrigin,
                PathHelper.AppendColon(OneDrivePathRoot),
                PathHelper.AppendColon(workbookPath),
                worksheetRangePath);
            return url;
        }

        /// <inheritdoc/>
        public Url GetCreateFolderUrl(string parentFolderPath = null)
        {
            var url = parentFolderPath != null
                ? Url.Combine(
                this.GraphOrigin,
                PathHelper.AppendColon(OneDrivePathRoot),
                PathHelper.AppendColon(parentFolderPath),
                "children")
                : Url.Combine(
                this.GraphOrigin,
                OneDrivePathRoot,
                "children");
            return url;
        }

        /// <inheritdoc/>
        public Url GetUpdateItemUrl(string itemPath)
        {
            var url = Url.Combine(
                this.GraphOrigin,
                PathHelper.AppendColon(OneDrivePathRoot),
                itemPath);
            return url;
        }

        /// <inheritdoc/>
        public Url GetFolderPath(string folderPath)
        {
            return Url.Combine(
                this.GraphOrigin,
                PathHelper.AppendColon(OneDrivePathRoot),
                PathHelper.AppendColon(folderPath));
        }

        /// <inheritdoc/>
        public Url GetFileContentsPath(string filePath)
        {
            var url = Url.Combine(
                this.GraphOrigin,
                PathHelper.AppendColon(OneDrivePathRoot),
                PathHelper.AppendColon(filePath),
                "content");
            return url;
        }

        public Url GetFileMetadataPath(string filePath)
        {
            var url = Url.Combine(
                this.GraphOrigin,
                PathHelper.AppendColon(OneDrivePathRoot),
                PathHelper.AppendColon(filePath));
            return url;
        }

        /// <inheritdoc/>
        public Url GetCopyItemUrl(string path)
        {
            var url = Url.Combine(
                this.GraphOrigin,
                PathHelper.AppendColon(OneDrivePathRoot),
                PathHelper.AppendColon(path),
                "copy");
            return url;
        }

        /// <inheritdoc/>
        public Url GetUploadFileUrl(string destinationPath)
        {
            var url = Url.Combine(
                this.GraphOrigin,
                PathHelper.AppendColon(OneDrivePathRoot),
                PathHelper.AppendColon(destinationPath),
                "content");
            return url;
        }

        /// <inheritdoc/>
        public Url GetFolderChildrenPath(string folderPath)
        {
            var url = Url.Combine(
                this.GraphOrigin,
                PathHelper.AppendColon(OneDrivePathRoot),
                PathHelper.AppendColon(folderPath),
                "children");
            return url;
        }

        /// <inheritdoc/>
        public Url GetBatchRequestFolderChildrenUrl(string folderPath)
        {
            var url = Url.Combine(
                PathHelper.AppendColon(OneDriveBtachRequestPathRoot),
                PathHelper.AppendColon(folderPath),
                "children");
            return url;
        }

        /// <inheritdoc/>
        public Url GetCreateLinkFileUrl(string item)
        {
            var url = Url.Combine(
                this.GraphOrigin,
                OneDriveItemRoot,
                item,
                "createLink");
            return url;
        }
    }
}
