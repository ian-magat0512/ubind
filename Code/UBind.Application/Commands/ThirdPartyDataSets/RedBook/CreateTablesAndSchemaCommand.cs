// <copyright file="CreateTablesAndSchemaCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.RedBook
{
    using System;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Represents the command to create tables and schema to be used by Gnaf.
    /// </summary>
    public class CreateTablesAndSchemaCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTablesAndSchemaCommand"/> class.
        /// </summary>
        /// <param name="updaterJobManifest">The updater job manifest.</param>
        /// <param name="jobId">The updater job id.</param>
        public CreateTablesAndSchemaCommand(IUpdaterJobManifest updaterJobManifest, Guid jobId)
        {
            this.UpdaterJobManifest = updaterJobManifest;
            this.UpdaterJobId = jobId;
        }

        /// <summary>
        /// Gets the updater job manifest.
        /// </summary>
        public IUpdaterJobManifest UpdaterJobManifest { get; }

        /// <summary>
        /// Gets the updater job id.
        /// </summary>
        public Guid UpdaterJobId { get; }
    }
}