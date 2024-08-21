// <copyright file="IData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation
{
    using System;

    /// <summary>
    /// Interface for representing data values in automations.
    /// </summary>
    public interface IData
    {
        /// <summary>
        /// Obtain value on runtime when type for base-class is unreachable.
        /// </summary>
        /// <returns>The value set as dynamic.</returns>
        dynamic GetValueFromGeneric();

        /// <summary>
        /// Gets the type of the actual data that's been wrapped.
        /// </summary>
        /// <returns>The Type of the data that's held by this wrapper.</returns>
        Type GetInnerType();
    }
}
