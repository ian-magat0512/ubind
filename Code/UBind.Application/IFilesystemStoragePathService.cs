// <copyright file="IFilesystemStoragePathService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using UBind.Domain;

    /// <summary>
    /// For obtaining paths to files and folders in One Drive.
    /// </summary>
    public interface IFilesystemStoragePathService : IReleaseSourceFileNameProvider
    {
        /// <summary>
        /// Gets the name of the UBind folder in the application's user account's One Drive root folder.
        /// </summary>
        string UBindFolderName { get; }

        /// <summary>
        /// Gets the name used for the automations json file.
        /// </summary>
        string AutomationsFileName { get; }

        /// <summary>
        /// Gets the name used for outbound email servers configuration file.
        /// </summary>
        string OutboundEmailServersFileName { get; }

        /// <summary>
        /// Gets the path of the development folder in application's user account's One Drive root folder.
        /// </summary>
        string DevelopmentFolderPath { get; }

        /// <summary>
        /// Gets the path of the releases folder in application's user account's One Drive root folder.
        /// </summary>
        string ReleasesFolderPath { get; }

        /// <summary>
        /// Gets the name of the folder for a given type of web form app.
        /// </summary>
        /// <param name="type">THe type of the web form app.</param>
        /// <returns>The name of the web form app's folder.</returns>
        string GetWebFormAppFolderName(WebFormAppType type);

        /// <summary>
        /// Gets the path to a tenant's development folder.
        /// </summary>
        /// <param name="tenantAlias">The alias of the tenant.</param>
        /// <returns>The path to a tenant's development folder.</returns>
        string GetTenantDevelopmentFolder(string tenantAlias);

        /// <summary>
        /// Gets the path to a tenant's product development folder.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <returns>The path to a tenant's development folder.</returns>
        string GetTenantProductDevelopmentFolder(string tenantAlias, string productAlias);

        /// <summary>
        /// Gets the path to a tenant's release folder.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the tenant.</param>
        /// <returns>The path to a tenant's release folder.</returns>
        string GetTenantReleaseDevelopmentFolder(string tenantAlias);

        /// <summary>
        /// Gets the path to a product's dev workbook in OneDrive.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name="webFormAppType">The type of web form app the file is for.</param>
        /// <returns>The path to the product's dev workbook in OneDrive.</returns>
        string GetDevWorkbookPath(string tenantAlias, string productAlias, WebFormAppType webFormAppType);

        /// <summary>
        /// Gets the path to a product's dev workflow file in OneDrive.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name = "webFormAppType" >The type of web form app the file is for.</param>
        /// <returns>The path to the product's dev workflow file in OneDrive.</returns>
        string GetDevWorkflowFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType);

        /// <summary>
        /// Gets the path to a product's dev payment form file in OneDrive.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name = "webFormAppType" >The type of web form app the file is for.</param>
        /// <returns>The path to the product's dev payment form file in OneDrive.</returns>
        string GetDevPaymentFormFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType);

        /// <summary>
        /// Gets the path to a product's named miscellaneous file in OneDrive.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name="filename">The name of the file to get the path of.</param>
        /// <param name = "webFormAppType" >The type of web form app the file is for.</param>
        /// <returns>The path to the file in OneDrive.</returns>
        string GetPrivateFilePath(string tenantAlias, string productAlias, string filename, WebFormAppType webFormAppType);

        /// <summary>
        /// Gets the path of an asset file.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name="filename">The name of the file to get the path of.</param>
        /// <param name = "webFormAppType" >The type of web form app the file is for.</param>
        /// <returns>The path to the asset file in OneDrive.</returns>
        string GetPublicAssetFilePath(string tenantAlias, string productAlias, string filename, WebFormAppType webFormAppType);

        /// <summary>
        /// Gets the path to a product's dev form model file in OneDrive.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name = "webFormAppType" >The type of web form app the file is for.</param>
        /// <returns>The path to the product's dev form model file in OneDrive.</returns>
        string GetDevFormModelFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType);

        /// <summary>
        /// Gets the path to a product's dev integrations file in OneDrive.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name = "webFormAppType" >The type of web form app the file is for.</param>
        /// <returns>The path to the product's dev integrations file in OneDrive.</returns>
        string GetDevIntegrationsFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType);

        /// <summary>
        /// Gets the path to a product's dev automations file in OneDrive.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name = "webFormAppType" >The type of web form app the file is for.</param>
        /// <returns>The path to the product's dev automations file in OneDrive.</returns>
        string GetDevAutomationsFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType);

        /// <summary>
        /// Gets the path to a product's dev outbound-email-servers.json file in OneDrive.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name = "webFormAppType" >The type of web form app the file is for.</param>
        /// <returns>The path to the product's dev outbound-email-servers.json file in OneDrive.</returns>
        string GetDevOutboundEmailServersFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType);

        /// <summary>
        /// Gets the path to a product's payment file in OneDrive.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name = "webFormAppType" >The type of web form app the file is for.</param>
        /// <returns>The path to the product's payment configuration file in OneDrive.</returns>
        string GetDevPaymentFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType);

        /// <summary>
        /// Gets the path to a product's dev premium finding configuration file.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name="webFormAppType" >The type of web form app the file is for.</param>
        /// <returns>The path to the product's premium funding configuration file in OneDrive.</returns>
        string GetDevFundingFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType);

        /// <summary>
        /// Gets the path to a product's dev configurable product workflow configuration file.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name="webFormAppType" >The type of web form app the file is for.</param>
        /// <returns>The path to the product's configurable product configuration file in OneDrive.</returns>
        string GetDevProductConfigFilePath(string tenantAlias, string productAlias, WebFormAppType webFormAppType);

        /// <summary>
        /// Gets the path to a given product's app development folder in OneDrive.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name="webFormAppType" >The type of web form app the folder is for.</param>
        /// <returns>The path to the product's development folder in OneDrive.</returns>
        string GetProductDevelopmentAppFolder(string tenantAlias, string productAlias, WebFormAppType webFormAppType);

        /// <summary>
        /// Gets the path to a given product's development folder in OneDrive.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <returns>The path to the product's development folder in OneDrive.</returns>
        string GetProductDevelopmentFolder(string tenantAlias, string productAlias);

        /// <summary>
        /// Gets the path to a product's miscellaneous files folder in OneDrive.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name="webFormAppType" >The type of web form app the file is for.</param>
        /// <returns>The path to the product's product's miscellaneous files folder in OneDrive.</returns>
        string GetPrivateFilesFolder(string tenantAlias, string productAlias, WebFormAppType webFormAppType);

        /// <summary>
        /// Gets the path to a product's release folder (which will contain individual release folders) in OneDrive.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <returns>The path to the product release's folder in OneDrive.</returns>
        string GetProductReleasesFolder(string tenantAlias, string productAlias);

        /// <summary>
        /// Gets the path to a product release's folder in OneDrive.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name="releaseId">The Alias of the release.</param>
        /// <returns>The path to the product release's folder in OneDrive.</returns>
        string GetSpecificReleaseFolder(string tenantAlias, string productAlias, Guid releaseId);

        /// <summary>
        /// Get the path to assets folder.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name="webFormAppType" >The type of web form app the file is for.</param>
        /// <returns>The path to assets folder.</returns>
        string GetPublicAssetFolder(string tenantAlias, string productAlias, WebFormAppType webFormAppType);

        /// <summary>
        /// Gets the file name for the workbook for a given product.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the tenant the product belongs to.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <returns>The file name.</returns>
        /// <remarks>Each product has a unique file name to allow opening multiple workbooks in Excel at the same time.</remarks>
        string GetProductWorkbookName(string tenantAlias, string productAlias);

        /// <summary>
        /// Gets the path to the workbook for a particular release of a product in OneDrive.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name="releaseId">The Alias of the release.</param>
        /// <returns>The path to the workbook for the product release in OneDrive.</returns>
        string GetReleaseWorkbookPath(string tenantAlias, string productAlias, Guid releaseId);

        /// <summary>
        /// Gets the path to the sample workbook to copy for a new product.
        /// </summary>
        /// <returns>The path to the sample workbook to copy for a new product.</returns>
        string GetSampleWorkbookPath();

        /// <summary>
        /// Gets the path to the sample workflow file to copy for a new product.
        /// </summary>
        /// <returns>The path to the sample workflow file to copy for a new product.</returns>
        string GetSampleWorkflowFilePath();

        /// <summary>
        /// Gets the path to the release folder's miscellaneous files.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name="releaseId">The Alias of the release.</param>
        /// <returns>The path to the release's miscellaneous files folder.</returns>
        string GetReleaseMiscFilesFolder(string tenantAlias, string productAlias, Guid releaseId);

        /// <summary>
        /// Gets the path to the release asset folder.
        /// </summary>
        /// <param name="tenantAlias">The Alias of the product's tenant.</param>
        /// <param name="productAlias">The Alias of the product.</param>
        /// <param name="releaseId">The Alias of the release.</param>
        /// <returns>The path to the release's miscellaneous files folder.</returns>
        string GetReleaseAssetFilesFolder(string tenantAlias, string productAlias, Guid releaseId);
    }
}
