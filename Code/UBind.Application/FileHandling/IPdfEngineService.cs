// <copyright file="IPdfEngineService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Providers.File;

    /// <summary>
    /// Contracts for Pdf Engine Service that handles pdf related request.
    /// </summary>
    public interface IPdfEngineService
    {
        /// <summary>
        /// Outputs the bytes of source file to pdf bytes.
        /// </summary>
        /// <param name="sourceFileInfo">FileInfo of source file.</param>
        /// <param name="errorData">Operational details.</param>
        /// <returns>Byte array of converted temporary pdf file.</returns>
        byte[] OutputSourceFileBytesToPdfBytes(
            FileInfo sourceFileInfo,
            JObject errorData);
    }
}
