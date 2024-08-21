// <copyright file="StartupJobRunner.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.StartupJobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Hangfire;
    using Microsoft.Extensions.Logging;
    using UBind.Application.Services.Email;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <inheritdoc/>
    public partial class StartupJobRunner : IStartupJobRunner
    {
        private readonly ILogger<StartupJobRunner> logger;
        private readonly IStartupJobRepository startupJobRepository;
        private readonly IBackgroundJobClient backgroundJobClient;
        private readonly IStartupJobEnvironmentConfiguration startupJobConfiguration;
        private readonly IErrorNotificationService errorNotificationService;
        private readonly IStartupJobRegistry startupJobRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupJobRunner"/> class.
        /// </summary>
        /// <param name="startupJobRepository">Repository for startup jobs.</param>
        /// <param name="backgroundJobClient">The background client.</param>
        /// <param name="mediator">The mediator that encapsulates request/response and publishing interaction patterns.</param>
        /// <param name="logger">Logger for for startup job runner.</param>
        /// <param name="personService">Service for person read models.</param>
        /// <param name="fileContentRepository">The file content repository.</param>
        public StartupJobRunner(
            IStartupJobRepository startupJobRepository,
            IBackgroundJobClient backgroundJobClient,
            ILogger<StartupJobRunner> logger,
            IStartupJobEnvironmentConfiguration startupJobConfig,
            IErrorNotificationService errorNotificationService,
            IRecurringJobManager recurringJobManager,
            IStartupJobRegistry startupJobRegistry)
        {
            this.logger = logger;
            this.startupJobRepository = startupJobRepository;
            this.backgroundJobClient = backgroundJobClient;
            this.startupJobConfiguration = startupJobConfig;
            this.backgroundJobClient = backgroundJobClient;
            this.errorNotificationService = errorNotificationService;
            this.startupJobRegistry = startupJobRegistry;

            // Every day email someone if there are still incomplete jobs
            recurringJobManager.AddOrUpdate("incomplete-startup-jobs-notifier", () => this.NotifyIncompleteJobs(), Cron.Daily);
        }

        /// <inheritdoc/>
        public async Task RunJobs()
        {
            var startupJobs = this.GetAllIncompleteJobs();
            if (this.startupJobConfiguration.MultiNodeEnvironment)
            {
                startupJobs = startupJobs.Where(x => !x.RunManuallyInMultiNodeEnvironment).ToList();
            }

            // keep looping until we can't run any more jobs from the list.
            List<StartupJob> jobsWhichRan = new List<StartupJob>();
            bool jobRan;
            do
            {
                jobRan = false;
                foreach (var job in startupJobs.Where(j => !jobsWhichRan.Any(jr => jr == j)))
                {
                    if (this.HavePrecedingStartupJobsBeenCompleted(job))
                    {
                        await this.RunJobByAlias(job.Alias);
                        jobRan = true;
                        jobsWhichRan.Add(job);
                    }
                }
            }
            while (jobRan);

            // Log the jobs which didn't run, and why.
            var jobsWhichDidntRun = startupJobs.Where(sj => !jobsWhichRan.Any(sjr => sjr == sj));
            foreach (var jobWhichDidntRun in jobsWhichDidntRun)
            {
                this.logger.LogDebug(
                    "Startup Job {0} didn't run because it had preceding startup jobs which have not completed: {1}",
                    jobWhichDidntRun.Alias,
                    string.Join(", ", this.GetPrecedingJobAliasesThatHaveNotBeenCompleted(jobWhichDidntRun)) + ".");
            }
        }

        public async Task RunJobByAlias(string startupJobAlias, bool force = false)
        {
            // Just to make sure that the job has not been updated from different node and still in incomplete state.
            var startupJob = this.startupJobRepository.GetStartupJobByAlias(startupJobAlias);
            if (startupJob == null)
            {
                throw new ErrorException(Errors.General.NotFound("startup job", startupJobAlias, "alias"));
            }

            if (!force && startupJob.Complete)
            {
                this.logger.LogInformation("Not starting StartupJob {0} since it's already complete.", startupJobAlias);
                return;
            }

            if (!force && !startupJob.Blocking && startupJob.Started)
            {
                this.logger.LogInformation(
                    "Not starting StartupJob {0} since it's already started and it's set to run as a background job.",
                    startupJobAlias);
                return;
            }

            this.ThrowIfPrecedingStartupJobsHaveNotBeenCompleted(startupJob);

            // record that the startup job has started executing
            this.startupJobRepository.StartJobByAlias(startupJobAlias);

            if (startupJob.Blocking)
            {
                await this.ExecuteBlockingStartupJobAndMarkCompleted(startupJobAlias);
            }
            else
            {
                string hangfireJobId
                    = this.backgroundJobClient.Enqueue(() => this.ExecuteStartupJobAndMarkCompleted(startupJobAlias, CancellationToken.None));
                this.startupJobRepository.RecordHangfireJobId(startupJobAlias, hangfireJobId);
                this.backgroundJobClient.ContinueWith(hangfireJobId, () => this.RunDependentJobs(startupJobAlias));
            }
        }

        /// <inheritdoc/>
        public void EnqueueStartupJob(string startupJobAlias, bool force = false)
        {
            this.backgroundJobClient.Enqueue(() => this.ManualRunJobByAlias(startupJobAlias, force));
        }

        /// <inheritdoc/>
        [JobDisplayName("Startup Job: {0}")]
        public async Task ExecuteStartupJobAndMarkCompleted(string startupJobAlias, CancellationToken cancellationToken)
        {
            if (await this.ExecuteMethodByName(startupJobAlias, false, cancellationToken))
            {
                this.startupJobRepository.CompleteJobByAlias(startupJobAlias);
            }
        }

        public async Task ExecuteBlockingStartupJobAndMarkCompleted(string startupJobAlias)
        {
            if (await this.ExecuteMethodByName(startupJobAlias, true))
            {
                this.startupJobRepository.CompleteJobByAlias(startupJobAlias);
            }
        }

        public async Task RunDependentJobs(string startupJobAlias)
        {
            var dependentJobs = this.startupJobRepository.GetJobsDependentOn(startupJobAlias);
            if (dependentJobs.Any())
            {
                // we'll try to run them but they might not actually be runnable yet.
                await this.RunJobs();
            }
        }

        [JobDisplayName("Notify Of Incomplete Startup Jobs")]
        public void NotifyIncompleteJobs()
        {
            var incompleteJobs = this.GetAllIncompleteJobs();
            if (incompleteJobs.Any())
            {
                var message = new StringBuilder();
                message.AppendLine("The following startup jobs are incomplete:");
                incompleteJobs.ForEach(j =>
                {
                    message.Append($" - {j.Alias}");
                    if (!string.IsNullOrEmpty(j.HangfireJobId))
                    {
                        message.Append($"    Hangfire job: {j.HangfireJobId}");
                    }

                    message.AppendLine();
                });
                message.AppendLine();
                message.AppendLine(
                    "These startup jobs can be started by going to the swagger page and triggering them manually, "
                    + "entering the startup job alias. Before starting them, please check that they are not already "
                    + "running. If the incomplete job has a hangfire ID associated with it then please check the "
                    + "status of the existing hangfire job before starting another with the same alias.");
                this.errorNotificationService.SendSystemNotificationEmail(
                    "uBind: There are incomplete startup jobs", message.ToString());
            }
        }

        /// <summary>
        /// Wraps the call to <see cref="RunJobByAlias(string, bool)"/> into a hangfire job, to prevent timeouts.
        /// </summary>
        /// <param name="startupJobAlias">The startup job alias.</param>
        /// <param name="force">Indicates whether to still run this job, even it's already started.</param>
        /// <returns>The associated task.</returns>
        [JobDisplayName("Controller Startup Job: {0}, force: {1}")]
        [AutomaticRetry(Attempts = 0)]
        public async Task ManualRunJobByAlias(string startupJobAlias, bool force = false)
        {
            await this.RunJobByAlias(startupJobAlias, force);
        }

        public void ThrowIfPrecedingStartupJobsHaveNotBeenCompleted(StartupJob startupJob)
        {
            List<string> notCompletedJobAliases = this.GetPrecedingJobAliasesThatHaveNotBeenCompleted(startupJob);
            if (notCompletedJobAliases.Any())
            {
                throw new ErrorException(Errors.StartupJob.PrecedingJobsNotComplete(startupJob.Alias, startupJob.PrecedingStartupJobAliases, notCompletedJobAliases));
            }
        }

        private async Task<bool> ExecuteMethodByName(string methodName, bool blocking = false, CancellationToken? cancellationToken = null)
        {
            Type registryType = this.startupJobRegistry.GetType();
            MethodInfo theMethod = registryType.GetMethod(methodName);

            if (theMethod == null)
            {
                var errorException = new ErrorException(Errors.StartupJob.MethodNameNotFound(methodName));
                if (blocking)
                {
                    // IMPORTANT!!
                    // We are deliberately not throwing this error, because during a deployment, if the release has been
                    // deployed to node 1 but not the other nodes, and one of the other nodes is restarted, then it's
                    // startup will fail because it will find the startup job in the database, but will not have the code
                    // to execute it.
                    this.logger.LogError(errorException, errorException.Error.ToString());
                    return false;
                }
                else
                {
                    throw errorException;
                }
            }

            object result = null;
            ParameterInfo[] parameters = theMethod.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(CancellationToken) && cancellationToken.HasValue)
            {
                result = theMethod.Invoke(this.startupJobRegistry, new object[] { cancellationToken.Value });
            }
            else
            {
                result = theMethod.Invoke(this.startupJobRegistry, null);
            }

            // Check if the result is of type Task
            if (result is Task task)
            {
                // Await the task
                await task;
            }

            return true;
        }

        private List<StartupJob> GetAllIncompleteJobs()
        {
            return this.startupJobRepository.GetIncompleteStartupJobs().ToList();
        }

        private bool HavePrecedingStartupJobsBeenCompleted(StartupJob startupJob)
        {
            return this.GetPrecedingJobAliasesThatHaveNotBeenCompleted(startupJob).None();
        }

        private List<string> GetPrecedingJobAliasesThatHaveNotBeenCompleted(StartupJob startupJob)
        {
            List<string> notCompletedJobAliases = new List<string>();
            foreach (var precedingJobAlias in startupJob.PrecedingStartupJobAliases)
            {
                var precedingJob = this.startupJobRepository.GetStartupJobByAlias(precedingJobAlias);
                if (!precedingJob.Complete)
                {
                    notCompletedJobAliases.Add(precedingJob.Alias);
                }
            }

            return notCompletedJobAliases;
        }
    }
}
