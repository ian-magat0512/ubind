// <copyright file="IGlassGuideUpdaterScheduler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Scheduler.Jobs;

public interface IGlassGuideUpdaterScheduler
{
    /// <summary>
    /// Handles the registration of an entity's state updater job on hangfire
    /// to trigger it hourly to update the quote state.
    /// </summary>
    void RegisterStateChangeJob();
}
