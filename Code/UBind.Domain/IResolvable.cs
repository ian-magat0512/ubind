// <copyright file="IResolvable.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    /// <summary>
    /// Interface for something that is resolved when needed.
    /// </summary>
    public interface IResolvable
    {
        /// <summary>
        /// Resolves the thing and returns it.
        /// </summary>
        /// <returns>The resolved thing.</returns>
        /// <typeparam name="TResolved">The type of the thing to be resolved.</typeparam>
        TResolved Resolve<TResolved>();
    }
}
