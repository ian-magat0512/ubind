// <copyright file="ResultModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Generic
{
    using System;

    /// <summary>
    /// standardized Result model. use if you want to send a message and a success value.
    /// </summary>
    [Serializable]
    public class ResultModel
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the ResultType.
        /// </summary>
        public string ResultType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether successful.
        /// </summary>
        public bool Successful { get; set; }
    }
}
