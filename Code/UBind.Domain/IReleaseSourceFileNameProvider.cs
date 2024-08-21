// <copyright file="IReleaseSourceFileNameProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Default File names.
    /// </summary>
    public interface IReleaseSourceFileNameProvider
    {
        /// <summary>
        /// Gets the name of the development folder in the UBind folder.
        /// </summary>
        string DevelopmentFolderName { get; }

        /// <summary>
        /// Gets the name of the release folder in the UBind folder.
        /// </summary>
        string ReleasesFolderName { get; }

        /// <summary>
        /// Gets the name of the misc files folder in a product's development folder.
        /// </summary>
        string MiscFilesFolderName { get; }

        /// <summary>
        /// Gets the name of the folder for storing example workbook files.
        /// </summary>
        string TemplatesFolderName { get; }

        /// <summary>
        /// Gets the name of the default workbook in the templates folder.
        /// </summary>
        string DefaultWorkbookName { get; }

        /// <summary>
        /// Gets the name used for workflow files.
        /// </summary>
        string WorkflowFileName { get; }

        /// <summary>
        /// Gets the name used for payment form files.
        /// </summary>
        string PaymentFormFileName { get; }

        /// <summary>
        /// Gets the name used for form model files.
        /// </summary>
        string FormModelFileName { get; }

        /// <summary>
        /// Gets the name used for integrations files.
        /// </summary>
        string IntegrationsFileName { get; }

        /// <summary>
        /// Gets the name used for payment config files.
        /// </summary>
        string PaymentFileName { get; }

        /// <summary>
        /// Gets the name used for premium funding config files.
        /// </summary>
        string FundingFileName { get; }

        /// <summary>
        /// Gets the name used for product configuration config files.
        /// </summary>
        string ProductConfigFileName { get; }

        /// <summary>
        /// Gets the name of the folder for storing asset files.
        /// </summary>
        string AssetFilesFolderName { get; }
    }
}
