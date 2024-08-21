// <copyright file="JobSchedule.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Scheduler
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// DTO for Job Schedule.
    /// </summary>
    public class JobSchedule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobSchedule"/> class.
        /// </summary>
        /// <param name="jobType">The job type.</param>
        /// <param name="jobDataMap">The job parameters dictionary.</param>
        /// <param name="cronExpression">The CRON expression.</param>
        /// <param name="delayTimeInSeconds">The time in minutes of delay before the job executes.</param>
        public JobSchedule(Type jobType, IDictionary<string, string> jobDataMap, string cronExpression = null, int delayTimeInSeconds = 0)
        {
            this.JobType = jobType;
            this.CronExpression = cronExpression;
            this.JobDataMap = jobDataMap;
            this.DelayTimeInSeconds = delayTimeInSeconds;
        }

        /// <summary>
        /// Gets the job type.
        /// </summary>
        public Type JobType { get; }

        /// <summary>
        /// Gets the Cron Expression.
        /// </summary>
        public string CronExpression { get; }

        /// <summary>
        /// Gets the job parameters dictionary.
        /// </summary>
        public IDictionary<string, string> JobDataMap { get; }

        /// <summary>
        /// Gets the delay time in minutes.
        /// </summary>
        public int DelayTimeInSeconds { get; }
    }
}
