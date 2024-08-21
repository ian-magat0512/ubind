// <copyright file="IUpdaterJobFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ThirdPartyDataSets
{
    using System;

    /// <summary>
    /// Provides the contract to be use for the updater job factory to resolve different updater job implementation like RedBook, Gnaf and Glass's Guide.
    /// </summary>
    public interface IUpdaterJobFactory
    {
        /// <summary>
        /// Retrieves the updater job by job type.
        /// </summary>
        /// <param name="updaterJobType">The updater job concrete type.</param>
        /// <returns>Return the updater job instance from the service collection.</returns>
        IUpdaterJob GetUpdaterJob(Type updaterJobType);
    }
}
