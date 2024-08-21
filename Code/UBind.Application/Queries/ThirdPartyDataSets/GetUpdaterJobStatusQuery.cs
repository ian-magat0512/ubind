// <copyright file="GetUpdaterJobStatusQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.ThirdPartyDataSets;

using System;
using UBind.Application.ThirdPartyDataSets.ViewModel;
using UBind.Domain.Patterns.Cqrs;

/// <summary>
/// Represents the command to obtain the list of updater job statuses.
/// </summary>
public class GetUpdaterJobStatusQuery : IQuery<JobStatusResponse>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetUpdaterJobStatusQuery"/> class.
    /// </summary>
    /// <param name="updaterJobType">The updater job type.</param>
    /// <param name="updateJobId">The updater job id.</param>
    public GetUpdaterJobStatusQuery(Type updaterJobType, Guid updateJobId)
    {
        this.UpdateJobId = updateJobId;
        this.UpdaterJobType = updaterJobType;
    }

    /// <summary>
    /// Gets the updater job data type.
    /// </summary>
    public Type UpdaterJobType { get; }

    /// <summary>
    /// Gets The updater job id.
    /// </summary>
    public Guid UpdateJobId { get; }
}
