// <copyright file="GetUpdaterJobsStatusesQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.ThirdPartyDataSets
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.ThirdPartyDataSets;
    using UBind.Application.ThirdPartyDataSets.ViewModel;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Represents the handler to obtain the list of updater job statuses.
    /// </summary>
    public class GetUpdaterJobsStatusesQueryHandler : IQueryHandler<GetUpdaterJobsStatusesQuery, IEnumerable<JobStatusResponse>>
    {
        private readonly IUpdaterJobFactory updaterJobFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetUpdaterJobsStatusesQueryHandler"/> class.
        /// </summary>
        /// <param name="updaterJobFactory">The updater job factory.</param>
        public GetUpdaterJobsStatusesQueryHandler(IUpdaterJobFactory updaterJobFactory)
        {
            this.updaterJobFactory = updaterJobFactory;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<JobStatusResponse>> Handle(GetUpdaterJobsStatusesQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var updaterStateMachine = this.updaterJobFactory.GetUpdaterJob(request.UpdaterJobType);
            return await Task.FromResult(updaterStateMachine.GetListUpdaterJobsStatuses().ToList());
        }
    }
}
