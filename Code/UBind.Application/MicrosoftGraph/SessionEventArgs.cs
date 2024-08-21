// <copyright file="SessionEventArgs.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.MicrosoftGraph
{
    using System;

    /// <summary>
    /// Events for sesison manager to handle.
    /// </summary>
    public class SessionEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionEventArgs"/> class.
        /// </summary>
        /// <param name="workbookPath">The path of the workbook the sesion is for.</param>
        public SessionEventArgs(string workbookPath)
        {
            this.WorkbookPath = workbookPath;
        }

        /// <summary>
        /// Gets the path of the workbook the session is for.
        /// </summary>
        public string WorkbookPath { get; }
    }
}
