// <copyright file="CreateUpdaterJobCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.ThirdPartyDataSets
{
    using System;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Application.ThirdPartyDataSets.ViewModel;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Represents the command for creating the updater job.
    /// </summary>
    public class CreateUpdaterJobCommand : ICommand<UpdaterJobStatusResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateUpdaterJobCommand"/> class.
        /// </summary>
        /// <param name="updaterJobType">The updater job type.</param>
        /// <param name="updaterJobManifest">The updater job manifest.</param>
        public CreateUpdaterJobCommand(Type updaterJobType, IUpdaterJobManifest updaterJobManifest)
        {
            this.UpdaterJobManifest = updaterJobManifest;
            this.UpdaterJobType = updaterJobType;
        }

        /// <summary>
        /// Gets the updater job data type.
        /// </summary>
        public Type UpdaterJobType { get; }

        /// <summary>
        /// Gets the updater job manifest.
        /// </summary>
        public IUpdaterJobManifest UpdaterJobManifest { get; }
    }
}
