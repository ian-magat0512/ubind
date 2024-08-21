// <copyright file="IMsExcelEngineService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FileHandling
{
    /// <summary>
    /// Defines the required contract for implementing the MS Excel template service.
    /// </summary>
    public interface IMsExcelEngineService
    {
        /// <summary>
        /// Gets or sets the datasource that mediates the engine service’s data model for content generation.
        /// </summary>
        IMsExcelEngineDatasource Datasource { get; set; }

        /// <summary>
        /// Generate content.
        /// </summary>
        /// <param name="sourceFilename">The source filename.</param>
        /// <param name="outputFilename">The output filename.</param>
        /// <returns>Output bytes.</returns>
        byte[] GenerateContent(string sourceFilename, string outputFilename);
    }
}
