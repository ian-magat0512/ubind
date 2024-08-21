// <copyright file="IESystemAlertConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Messaging
{
    /// <summary>
    /// Configuration for System Alert.
    /// </summary>
    public interface IESystemAlertConfiguration
    {
        /// <summary>
        /// Gets the email from value.
        /// </summary>
        string From { get; }

        /// <summary>
        /// Gets the email to value.
        /// </summary>
        string To { get; }

        /// <summary>
        /// Gets the email cc value.
        /// </summary>
        string CC { get; }
    }
}
