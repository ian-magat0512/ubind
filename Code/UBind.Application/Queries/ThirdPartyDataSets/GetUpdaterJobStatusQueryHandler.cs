// <copyright file="GetUpdaterJobStatusQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets;

using System.Threading;
using System.Threading.Tasks;
using UBind.Application.ThirdPartyDataSets;
using UBind.Application.ThirdPartyDataSets.ViewModel;
using UBind.Domain.Patterns.Cqrs;

/// <summary>
/// Represents the handler to obtain the list of updater job statuses.
/// </summary>
public class GetUpdaterJobStatusQueryHandler : IQueryHandler<GetUpdaterJobStatusQuery, JobStatusResponse>
{
    private readonly IUpdaterJobFactory updaterJobFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetUpdaterJobStatusQueryHandler"/> class.
    /// </summary>
    /// <param name="updaterJobFactory">The updater job factory.</param>
    public GetUpdaterJobStatusQueryHandler(IUpdaterJobFactory updaterJobFactory)
    {
        this.updaterJobFactory = updaterJobFactory;
    }

    /// <inheritdoc/>
    public async Task<JobStatusResponse> Handle(GetUpdaterJobStatusQuery query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var updaterStateMachine = this.updaterJobFactory.GetUpdaterJob(query.UpdaterJobType);
        return await Task.FromResult(updaterStateMachine.GetUpdaterJobStatus(query.UpdateJobId));
    }
}