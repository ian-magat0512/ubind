// <copyright file="QuartzHostedService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Quartz;
    using Quartz.Spi;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Quartz hosted service.
    /// </summary>
    public class QuartzHostedService : IHostedService
    {
        private readonly ISchedulerFactory schedulerFactory;
        private readonly IJobFactory jobFactory;
        private readonly IEnumerable<JobSchedule> jobSchedules;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuartzHostedService"/> class.
        /// </summary>
        /// <param name="schedulerFactory">The scheduler factory.</param>
        /// <param name="jobFactory">The job factory.</param>
        /// <param name="jobSchedules">The job schedules.</param>
        /// <param name="mediator">The mediator.</param>
        public QuartzHostedService(
            ISchedulerFactory schedulerFactory,
            IJobFactory jobFactory,
            IEnumerable<JobSchedule> jobSchedules,
            ICqrsMediator mediator)
        {
            this.schedulerFactory = schedulerFactory;
            this.jobSchedules = jobSchedules;
            this.jobFactory = jobFactory;
        }

        /// <summary>
        /// Gets or sets the scheduler.
        /// </summary>
        public IScheduler Scheduler { get; set; }

        /// <summary>
        /// Start the scheduled jobs.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.Scheduler = await this.schedulerFactory.GetScheduler();
            this.Scheduler.JobFactory = this.jobFactory;
            foreach (var jobSchedule in this.jobSchedules)
            {
                // Added this line as an extra precaution if the cancellation is sent midway the execution
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                var job = CreateJob(jobSchedule);
                var trigger = CreateTrigger(jobSchedule);
                await this.Scheduler.ScheduleJob(job, trigger);
            }

            await this.Scheduler.Start();
        }

        /// <summary>
        /// Shut the scheduler down.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await this.Scheduler.Shutdown();
        }

        private static IJobDetail CreateJob(JobSchedule schedule)
        {
            var jobType = schedule.JobType;
            var identityBuilder = new StringBuilder();
            identityBuilder.Append(jobType.FullName);

            var builder = JobBuilder
                .Create(jobType);

            if (schedule.JobDataMap != null)
            {
                foreach (var parameter in schedule.JobDataMap)
                {
                    builder.UsingJobData(parameter.Key, parameter.Value);
                    identityBuilder.Append("_" + parameter.Value);
                }
            }

            builder.WithIdentity($"{identityBuilder}")
                .WithDescription(jobType.Name);

            return builder.Build();
        }

        private static ITrigger CreateTrigger(JobSchedule schedule)
        {
            var trigger = TriggerBuilder
                .Create()
                .WithIdentity($"{schedule.JobType.FullName}-{Guid.NewGuid():N}.trigger");

            if (!string.IsNullOrEmpty(schedule.CronExpression))
            {
                trigger.WithCronSchedule(schedule.CronExpression)
                .WithDescription(schedule.CronExpression);
            }

            if (schedule.DelayTimeInSeconds < 1)
            {
                trigger.StartNow();
            }
            else
            {
                trigger.StartAt(DateTime.Now.AddSeconds(schedule.DelayTimeInSeconds));
            }

            if (schedule.JobDataMap != null)
            {
                foreach (var parameter in schedule.JobDataMap)
                {
                    trigger.UsingJobData(parameter.Key, parameter.Value);
                }
            }

            return trigger.Build();
        }
    }
}
