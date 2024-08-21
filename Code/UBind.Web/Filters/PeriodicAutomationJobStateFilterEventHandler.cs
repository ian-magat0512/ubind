// <copyright file="PeriodicAutomationJobStateFilterEventHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using Hangfire.Common;
    using Hangfire.States;
    using UBind.Application.Automation;
    using UBind.Application.Services;

    /// <summary>
    /// This EventHandler will create logs during the deletion and the initialisation
    /// of all automation periodic trigger.
    /// </summary>
    public class PeriodicAutomationJobStateFilterEventHandler : IElectStateFilter
    {
        private readonly string registerJobTypeName;
        private readonly string triggerJobTypeName;
        private readonly ILogger<PeriodicAutomationJobStateFilterEventHandler> logger;

        public PeriodicAutomationJobStateFilterEventHandler(ILogger<PeriodicAutomationJobStateFilterEventHandler> logger)
        {
            this.logger = logger;
            this.registerJobTypeName = $"{typeof(IAutomationPeriodicTriggerScheduler).Name}.{nameof(IAutomationPeriodicTriggerScheduler.RegisterPeriodicTriggerJobs)}";
            this.triggerJobTypeName = $"{typeof(IAutomationService).Name}.{nameof(IAutomationService.TriggerPeriodicAutomation)}";
        }

        /// <inheritdoc />
        public void OnStateElection(ElectStateContext context)
        {
            // Stop any logging process if any of the object defined is not available
            if (context == null || context?.BackgroundJob == null || context?.BackgroundJob.Job == null)
            {
                return;
            }

            string jobId = context.BackgroundJob.Id;
            string jobTypeName = context.BackgroundJob.Job.ToString();

            // Detect if the event contains exception during state change regardless from what type it is
            if (context.BackgroundJob.Job.Args.OfType<Exception>().Any())
            {
                this.logger.LogError($"\nJob {jobTypeName} with JobId: {jobId} contains errors on status change execution. "
                    + $"Args: {SerializationHelper.Serialize(context.BackgroundJob.Job.Args)}");
                return;
            }

            if (this.triggerJobTypeName.Contains(jobTypeName, StringComparison.OrdinalIgnoreCase))
            {
                if (context.CandidateState.Name == DeletedState.StateName)
                {
                    // This is to detect whenever a periodic job is deleted
                    this.logger.LogDebug($"Job {jobTypeName} with JobId: {jobId} deleted");
                }
                else
                {
                    this.logger.LogDebug($"Job {jobTypeName} with JobId: {jobId} updated");
                }

                return;
            }

            // This is to detect if the automation periodic triggers has been recreated
            if (this.registerJobTypeName.Contains(jobTypeName, StringComparison.OrdinalIgnoreCase))
            {
                this.logger.LogDebug($"Job {jobTypeName} with JobId: {jobId}. "
                    + $"All automation scheduled jobs will be removed and registered again.");
            }
        }
    }
}
