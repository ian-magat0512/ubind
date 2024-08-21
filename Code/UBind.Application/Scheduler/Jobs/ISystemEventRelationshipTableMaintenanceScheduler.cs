// <copyright file="ISystemEventRelationshipTableMaintenanceScheduler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Scheduler.Jobs
{
    /// <summary>
    /// This scheduler is used to run the job for rebuilding of index for system events
    /// and relationships table if necessary.
    /// The job is ran every night at 10pm Australia (10pm AEST/11PM AEDT).
    /// </summary>
    public interface ISystemEventRelationshipTableMaintenanceScheduler
    {
        void RegisterStateChangeJob();
    }
}
