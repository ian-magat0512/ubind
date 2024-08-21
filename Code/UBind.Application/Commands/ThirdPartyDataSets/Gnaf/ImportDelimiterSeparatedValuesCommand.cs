// <copyright file="ImportDelimiterSeparatedValuesCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.Gnaf
{
    using System;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Represents the command to import delimiter separated values files into Gnaf database.
    /// </summary>
    public class ImportDelimiterSeparatedValuesCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportDelimiterSeparatedValuesCommand"/> class.
        /// </summary>
        /// <param name="updaterJobManifest">The updater job manifest.</param>
        /// <param name="jobId">The updater job id.</param>
        public ImportDelimiterSeparatedValuesCommand(IUpdaterJobManifest updaterJobManifest, Guid jobId)
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
