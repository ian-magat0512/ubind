// <copyright file="FilesystemStoragePathService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using UBind.Domain;

    /// <summary>
    /// Helper for obtainaing OneDrive file paths.
    /// </summary>
    public class FilesystemStoragePathService : IFilesystemStoragePathService
    {
        private static readonly IDictionary<WebFormAppType, string> WebFormAppFolders = new Dictionary<WebFormAppType, string>
        {
            { WebFormAppType.Quote, "Quote" },
            { WebFormAppType.Claim, "Claim" },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="FilesystemStoragePathService"/> class.
        /// </summary>
        /// <param name="configuration">Configuration settings.</param>
        public FilesystemStoragePathService(IFilesystemStorageConfiguration configuration)
        {
            this.UBindFolderName = configuration.UBindFolderName;
        }

        /// <inheritdoc/>
        public string DevelopmentFolderName => "Development";

        /// <inheritdoc/>
        public string ReleasesFolderName => "Releases";

        /// <inheritdoc/>
        public string MiscFilesFolderName => "Files";

        /// <inheritdoc/>
        public string AssetFilesFolderName => "Assets";

        /// <inheritdoc/>
        public string DefaultWorkbookName => "Workbook.xlsx";

        /// <inheritdoc/>
        public string TemplatesFolderName => "Templates";

        /// <inheritdoc/>
        public string WorkflowFileName => "Workflow.json";

        /// <inheritdoc/>
        public string PaymentFormFileName => "payment.form.json";

        /// <inheritdoc/>
        public string FormModelFileName => "form.model.json";

        /// <inheritdoc/>
        public string IntegrationsFileName => "integrations.json";

        /// <inheritdoc/>
        public string AutomationsFileName => "automations.json";

        /// <inheritdoc/>
        public string OutboundEmailServersFileName => "outbound-email-servers.json";

        /// <inheritdoc/>
        public string PaymentFileName => "payment.json";

        /// <inheritdoc/>
        public string FundingFileName => "funding.json";

        /// <inheritdoc/>
        public string ProductConfigFileName => "product.json";

        /// <summary>
        /// Gets the name of the UBind folder in OneDrive.
        /// </summary>
        public string UBindFolderName { get; }

        /// <summary>
        /// Gets the path to the UBind development folder.
        /// </summary>
        public string DevelopmentFolderPath => Path.Combine(this.UBindFolderName, this.DevelopmentFolderName);

        /// <summary>
        /// Gets the path to the UBind releases folder.
        /// </summary>
        public string ReleasesFolderPath => Path.Combine(this.UBindFolderName, this.ReleasesFolderName);

        /// <inheritdoc/>
        public string GetWebFormAppFolderName(WebFormAppType type)
        {
            return WebFormAppFolders[type];
        }

        /// <inheritdoc/>
        public string GetTenantDevelopmentFolder(string tenantAlias)
        {
            var path = Path.Combine(this.DevelopmentFolderPath, tenantAlias);
            return path;
        }

        /// <inheritdoc/>
        public string GetTenantProductDevelopmentFolder(string tenantAlias, string productAlias)
        {
            var path = Path.Combine(this.DevelopmentFolderPath, tenantAlias, productAlias);
            return path;
        }

        /// <inheritdoc/>
        public string GetTenantReleaseDevelopmentFolder(string tenantAlias)
        {
            var path = Path.Combine(this.ReleasesFolderPath, tenantAlias);
            return path;
        }

        /// <inheritdoc/>
        public string GetProductDevelopmentAppFolder(string tenantAlias, string productAlias, WebFormAppType webFormAppType)
        {
            string[] pathSegments = { this.DevelopmentFolderPath, tenantAlias, productAlias, WebFormAppFolders[webFormAppType] };
            var path = Path.Combine(pathSegments);
            return path;
        }

        /// <inheritdoc/>
        public string GetProductDevelopmentFolder(string tenantAlias, string productAlias)
        {
            return Path.Combine(this.DevelopmentFolderPath, tenantAlias, productAlias);
        }

        /// <inheritdoc/>
        public string GetProductReleasesFolder(string tenantAlias, string productAlias)
        {
            string[] pathSegments = { this.ReleasesFolderPath, tenantAlias, productAlias };
            var path = Path.Combine(pathSegments);
            return path;
        }

        /// <inheritdoc/>
        public string GetSpecificReleaseFolder(string tenantAlias, string productAlias, Guid releaseId)
        {
            var path = Path.Combine(this.GetProductReleasesFolder(tenantAlias, productAlias), releaseId.ToString());
            return path;
        }

        /// <inheritdoc/>
        public string GetPublicAssetFolder(string tenantAlias, string productAlias, WebFormAppType webFormAppType)
        {
            var path = Path.Combine(this.GetProductDevelopmentAppFolder(tenantAlias, productAlias, webFormAppType), this.AssetFilesFolderName);
            return path;
        }

        /// <inheritdoc/>
        public string GetProductWorkbookName(string tenantAlias, string productAlias)
        {
            return $"{tenantAlias}-{productAlias}-{this.DefaultWorkbookName}";
        }

        /// <inheritdoc/>
        public string GetReleaseWorkbookPath(string tenantAlias, string productAlias, Guid releaseId)
        {
            var filename = this.GetProductWorkbookName(tenantAlias, productAlias);
            var path = Path.Combine(this.GetSpecificReleaseFolder(tenantAlias, productAlias, releaseId), filename);
            return path;
        }

        /// <inheritdoc/>
        public string GetSampleWorkbookPath()
        {
            var path = Path.Combine(this.UBindFolderName, this.TemplatesFolderName, this.DefaultWorkbookName);
            return path;
        }

        /// <inheritdoc/>
        public string GetSampleWorkflowFilePath()
        {
            var path = Path.Combine(this.UBindFolderName, this.TemplatesFolderName, this.WorkflowFileName);
            return path;
        }

        /// <inheritdoc/>
        public string GetDevWorkbookPath(string tenantAlias, string productAlias, WebFormAppType webFormAppType)
        {
            var workbookName = this.GetProductWorkbookName(tenantAlias, productAlias);
            return this.GetDevFilePath(tenantAlias, productAlias, workbookName, webFormAppType);
        }

        /// <inheritdoc/>
        public string GetDevWorkflowFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType)
        {
            return this.GetDevFilePath(tenantAlias, productAlias, this.WorkflowFileName, webFormAppType);
        }

        /// <inheritdoc/>
        public string GetDevPaymentFormFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType)
        {
            return this.GetDevFilePath(tenantAlias, productAlias, this.PaymentFormFileName, webFormAppType);
        }

        /// <inheritdoc/>
        public string GetDevFormModelFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType)
        {
            return this.GetDevFilePath(tenantAlias, productAlias, this.FormModelFileName, webFormAppType);
        }

        /// <inheritdoc/>
        public string GetDevIntegrationsFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType)
        {
            return this.GetDevFilePath(tenantAlias, productAlias, this.IntegrationsFileName, webFormAppType);
        }

        /// <inheritdoc/>
        public string GetDevAutomationsFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType)
        {
            return this.GetDevFilePath(tenantAlias, productAlias, this.AutomationsFileName, webFormAppType);
        }

        /// <inheritdoc/>
        public string GetDevOutboundEmailServersFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType)
        {
            return this.GetDevFilePath(tenantAlias, productAlias, this.OutboundEmailServersFileName, webFormAppType);
        }

        /// <inheritdoc/>
        public string GetDevPaymentFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType)
        {
            return this.GetDevFilePath(tenantAlias, productAlias, this.PaymentFileName, webFormAppType);
        }

        /// <inheritdoc/>
        public string GetDevFundingFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType)
        {
            return this.GetDevFilePath(tenantAlias, productAlias, this.FundingFileName, webFormAppType);
        }

        /// <inheritdoc/>
        public string GetDevProductConfigFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType)
        {
            return this.GetDevFilePath(tenantAlias, productAlias, this.ProductConfigFileName, webFormAppType);
        }

        /// <inheritdoc/>
        public string GetPrivateFilePath(string tenantAlias, string productAlias, string fileName, WebFormAppType webFormAppType)
        {
            var path = Path.Combine(this.GetPrivateFilesFolder(tenantAlias, productAlias, webFormAppType), fileName);
            return path;
        }

        /// <inheritdoc/>
        public string GetPublicAssetFilePath(string tenantAlias, string productAlias, string filename, WebFormAppType webFormAppType)
        {
            var path = Path.Combine(this.GetPublicAssetFolder(tenantAlias, productAlias, webFormAppType), filename);
            return path;
        }

        /// <inheritdoc/>
        public string GetPrivateFilesFolder(string tenantAlias, string productAlias, WebFormAppType webFormAppType)
        {
            var path = Path.Combine(this.GetProductDevelopmentAppFolder(tenantAlias, productAlias, webFormAppType), this.MiscFilesFolderName);
            return path;
        }

        /// <inheritdoc/>
        public string GetReleaseMiscFilesFolder(string tenantAlias, string productAlias, Guid releaseId)
        {
            var path = Path.Combine(this.GetSpecificReleaseFolder(tenantAlias, productAlias, releaseId), this.MiscFilesFolderName);
            return path;
        }

        /// <inheritdoc/>
        public string GetReleaseAssetFilesFolder(string tenantAlias, string productAlias, Guid releaseId)
        {
            var path = Path.Combine(this.GetSpecificReleaseFolder(tenantAlias, productAlias, releaseId), this.AssetFilesFolderName);
            return path;
        }

        private string GetDevFilePath(string tenantAlias, string productAlias, string fileName, WebFormAppType webFormAppType)
        {
            var path = Path.Combine(this.GetProductDevelopmentAppFolder(tenantAlias, productAlias, webFormAppType), fileName);
            return path;
        }
    }
}
