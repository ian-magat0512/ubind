// <copyright file="StartupJobStatus.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Enums
{
    using System.ComponentModel;

    /// <summary>
    /// The startup job status.
    /// </summary>
    public enum StartupJobStatus
    {
        [Description("notStarted")]
        NotStarted = 0,

        [Description("started")]
        Started = 1,

        [Description("complete")]
        Complete = 2,
    }
}
