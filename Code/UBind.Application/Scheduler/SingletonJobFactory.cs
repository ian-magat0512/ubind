// <copyright file="SingletonJobFactory.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Scheduler
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Quartz;
    using Quartz.Spi;

    /// <summary>
    /// Quartz .NET singleton Job Factory.
    /// </summary>
    public class SingletonJobFactory : IJobFactory
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingletonJobFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public SingletonJobFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Create a new job.
        /// </summary>
        /// <param name="bundle">trigger fired bundle.</param>
        /// <param name="scheduler">The scheduler.</param>
        /// <returns>The job.</returns>
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return this.serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
        }

        /// <summary>
        /// Return the job provided.
        /// </summary>
        /// <param name="job">The job.</param>
        public void ReturnJob(IJob job)
        {
        }
    }
}
