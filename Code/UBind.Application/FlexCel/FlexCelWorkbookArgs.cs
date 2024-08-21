// <copyright file="FlexCelWorkbookArgs.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.FlexCel
{
    /// <summary>
    /// Events for session manager to handle.
    /// </summary>
    public class FlexCelWorkbookArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlexCelWorkbookArgs"/> class.
        /// </summary>
        /// <param name="releaseId">The release ID the sesion is for.</param>
        /// <param name="flexCelWorkbookData">The workbook binary data.</param>
        public FlexCelWorkbookArgs(string releaseId, byte[] flexCelWorkbookData)
        {
            this.ReleaseId = releaseId;
            this.FlexCelWorkbookData = flexCelWorkbookData;
        }

        /// <summary>
        /// Gets or sets gets the path of the workbook the session is for.
        /// </summary>
        public string ReleaseId { get; set; }

        /// <summary>
        /// Gets or sets gets the path of the workbook the session is for.
        /// </summary>
        public byte[] FlexCelWorkbookData { get; set; }
    }
}
