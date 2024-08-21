// <copyright file="BuildAddressSearchIndexCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.ThirdPartyDataSets.Gnaf
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Represents the command to build an address search index from the Gnaf database.
    /// </summary>
    public class BuildAddressSearchIndexCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildAddressSearchIndexCommand"/> class.
        /// </summary>
        /// <param name="jobId">The updater job id.</param>
        /// <param name="pageNumber">The initial page number.</param>
        /// <param name="pageSize">The page size.</param>
        public BuildAddressSearchIndexCommand(Guid jobId, int pageNumber, int pageSize)
        {
            this.UpdaterJobId = jobId;
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
        }

        /// <summary>
        /// Gets the updater job id .
        /// </summary>
        public Guid UpdaterJobId { get; }

        /// <summary>
        /// Gets the page number.
        /// </summary>
        public int PageNumber { get; }

        /// <summary>
        /// Gets the page size.
        /// </summary>
        public int PageSize { get; }
    }
}
