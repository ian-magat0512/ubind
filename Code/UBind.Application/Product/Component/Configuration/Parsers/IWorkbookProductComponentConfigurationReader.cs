// <copyright file="IWorkbookProductComponentConfigurationReader.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Product.Component.Configuration.Parsers
{
    using UBind.Application.FlexCel;
    using UBind.Domain.Product.Component;

    /// <summary>
    /// Reads product configuration from a standard UBind workbook.
    /// </summary>
    public interface IWorkbookProductComponentConfigurationReader
    {
        /// <summary>
        /// Generates the configuration by reading data from the workbook.
        /// </summary>
        /// <param name="workbook">An instance of the workbook.</param>
        /// <returns>A JObject with the configuration.</returns>
        Component Read(FlexCelWorkbook workbook);
    }
}
