// <copyright file="GetUpdaterJobsStatusesQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ThirdPartyDataSets
{
    using System;
    using System.Collections.Generic;
    using UBind.Application.ThirdPartyDataSets.ViewModel;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Represents the command to obtain the list of updater job statuses.
    /// </summary>
    public class GetUpdaterJobsStatusesQuery : IQuery<IEnumerable<JobStatusResponse>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GetUpdaterJobsStatusesQuery"/> class.
        /// </summary>
        /// <param name="updaterJobType">The updater job type.</param>
        public GetUpdaterJobsStatusesQuery(Type updaterJobType)
        {
            this.UpdaterJobType = updaterJobType;
        }

        /// <summary>
        /// Gets the updater job data type.
        /// </summary>
        public Type UpdaterJobType { get; }
    }
}
