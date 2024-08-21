// <copyright file="ImportDelimiterSeparatedValuesCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.Nfid
{
    using System;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command to import delimiter separated values files into NFID database.
    /// </summary>
    public class ImportDelimiterSeparatedValuesCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportDelimiterSeparatedValuesCommand"/> class.
        /// </summary>
        /// <param name="updaterJobManifest">The updater manifest.</param>
        /// <param name="updaterJobId">The updater job id.</param>
        public ImportDelimiterSeparatedValuesCommand(UpdaterJobManifest updaterJobManifest, Guid updaterJobId)
        {
            this.UpdaterJobManifest = updaterJobManifest;
            this.UpdaterJobId = updaterJobId;
        }

        /// <summary>
        /// Gets the updater job manifest.
        /// </summary>
        public UpdaterJobManifest UpdaterJobManifest { get; }

        /// <summary>
        /// Gets the updater job id.
        /// </summary>
        public Guid UpdaterJobId { get; }
    }
}
