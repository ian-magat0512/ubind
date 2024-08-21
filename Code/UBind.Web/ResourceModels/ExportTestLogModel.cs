// <copyright file="ExportTestLogModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    /// <summary>
    /// Model for serving export test logs.
    /// </summary>
    public class ExportTestLogModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportTestLogModel"/> class.
        /// </summary>
        /// <param name="timestampTicks">The log timestamp in ticks since epoch.</param>
        /// <param name="log">The log text.</param>
        public ExportTestLogModel(long timestampTicks, string log)
        {
            this.TimestampTicks = timestampTicks;
            this.Log = log;
        }

        /// <summary>
        /// Gets or sets the log timestamp in ticks since epoch.
        /// </summary>
        /// <remarks>
        /// public setter for JSON serializer.
        /// .</remarks>
        public long TimestampTicks { get; set; }

        /// <summary>
        /// Gets or sets the log text.
        /// </summary>
        /// <remarks>
        /// public setter for JSON serializer.
        /// .</remarks>
        public string Log { get; set; }
    }
}
