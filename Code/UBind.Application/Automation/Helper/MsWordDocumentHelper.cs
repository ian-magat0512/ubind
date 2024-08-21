// <copyright file="MsWordDocumentHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Helper
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Helper class for ms word document related functions.
    /// </summary>
    public static class MsWordDocumentHelper
    {
        /// <summary>
        /// Gets the supported msword document extensions by the platform.
        /// </summary>
        /// <returns>List of supported extensions.</returns>
        public static List<string> GetSupportedDocumentExtensionByThePlatform()
        {
            var validExtensions = new List<string> { ".doc", ".docx", ".docm", ".dotx", ".dot", ".dotm" };
            return validExtensions;
        }

        /// <summary>
        /// Generate a description like this '.docx','.dotx'.
        /// </summary>
        /// <param name="supportedExtensions">List of extensions.</param>
        /// <returns>String in a specific format.</returns>
        public static string GenerateDescriptionUsingSingleQuoteAndCommaSeparationForEachItem(this List<string> supportedExtensions)
        {
            var supportedExtensionDescription = string.Join(",", supportedExtensions.Select(ve => $"'{ve}'"));
            return supportedExtensionDescription;
        }
    }
}
