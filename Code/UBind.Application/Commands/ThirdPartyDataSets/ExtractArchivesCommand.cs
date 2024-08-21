﻿// <copyright file="ExtractArchivesCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.ThirdPartyDataSets
{
    using System;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Represents the command to extract the archive files.
    /// </summary>
    public class ExtractArchivesCommand : ICommand<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractArchivesCommand"/> class.
        /// </summary>
        /// <param name="updaterJobManifest">The updater job manifest.</param>
        /// <param name="jobId">The updater job id.</param>
        public ExtractArchivesCommand(IUpdaterJobManifest updaterJobManifest, Guid jobId)
        {
            this.UpdaterJobManifest = updaterJobManifest;
            this.UpdaterJobId = jobId;
        }

        /// <summary>
        /// Gets the updater job manifest.
        /// </summary>
        public IUpdaterJobManifest UpdaterJobManifest { get; }

        /// <summary>
        /// Gets the updater job id .
        /// </summary>
        public Guid UpdaterJobId { get; }
    }
}
