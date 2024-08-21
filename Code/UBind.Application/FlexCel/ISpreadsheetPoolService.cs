// <copyright file="ISpreadsheetPoolService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FlexCel
{
    using System;
    using Newtonsoft.Json.Linq;
    using UBind.Domain;
    using UBind.Domain.Product;

    /// <summary>
    /// Reperesents a service to access pools of spreadsheet instances for use in calculating things.
    /// </summary>
    public interface ISpreadsheetPoolService
    {
        /// <summary>
        /// Retreives and existing spreadsheet pool for the given release and spreadsheet id, or null
        /// if none exists.
        /// </summary>
        /// <param name="releaseContext">The release context the pool is for.</param>
        /// <param name="webFormAppType">The app the pool is for.</param>
        /// <returns>The spreadsheet pool, if it exists.</returns>
        FlexCelWorkbookPool? GetSpreadsheetPool(ReleaseContext releaseContext, WebFormAppType webFormAppType);

        /// <summary>
        /// Creates a spreadsheet pool for the given release and spreadsheet, if it doesn't already exist.
        /// </summary>
        /// <param name="releaseContext">The release context the pool is for.</param>
        /// <param name="webFormAppType">The app the workbook is for.</param>
        /// <param name="spreadsheetBytes">The raw bytes of the spreadsheet file.</param>
        /// <param name="releaseId">The ID of the release the spreadsheet is from.</param>
        /// <param name="seedWorkbook">An optional workbook to seed the pool with.</param>
        /// <param name="staggeredStartup">Set this to true if the application is starting up so that we
        /// can stagger the startup growth so as not to overload the server.</param>
        void CreateSpreadsheetPool(
            ReleaseContext releaseContext,
            WebFormAppType webFormAppType,
            byte[] spreadsheetBytes,
            Guid releaseId,
            IFlexCelWorkbook seedWorkbook = null,
            bool staggeredStartup = false);

        /// <summary>
        /// Removes a spreadsheet pool, freeing up associated memory and resources.
        /// </summary>
        /// <param name="releaseContext">The release context the pool is for.</param>
        /// <param name="webFormAppType">The app the workbook is for.</param>
        void RemoveSpreadsheetPool(ReleaseContext releaseContext, WebFormAppType webFormAppType);

        /// <summary>
        /// Removes spreadsheet pool for a product in the given environment, freeing up associated memory and resources.
        /// </summary>
        /// <param name="releaseContext">The release context the pool is for.</param>
        /// <param name="webFormAppType">The app the workbook is for.</param>
        void RemoveSpreadsheetPools(ProductContext productContext, WebFormAppType webFormAppType);

        /// <summary>
        /// Gets a json object with memory usage information for each pool.
        /// </summary>
        /// <returns>A JObject.</returns>
        JObject GetMemoryUsageInformation();
    }
}
