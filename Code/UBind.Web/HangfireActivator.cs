// <copyright file="HangfireActivator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web
{
    using System;
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Domain.Attributes;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Hangfire Job Activator that uses a service provider to instantiate services.
    /// </summary>
    public class HangfireActivator : Hangfire.JobActivator
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireActivator"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use.</param>
        public HangfireActivator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public override object ActivateJob(Type jobType)
        {
            // see if the RequestIntent attribute is present on the job type or the ExecuteJob method
            RequestIntentAttribute requestIntentAttribute = jobType.GetCustomAttribute<RequestIntentAttribute>()
                        ?? jobType.GetMethod("ExecuteJob").GetCustomAttribute<RequestIntentAttribute>();

            if (requestIntentAttribute != null)
            {
                ICqrsRequestContext cqrsContext = this.serviceProvider.GetService<ICqrsRequestContext>();
                if (cqrsContext != null)
                {
                    cqrsContext.RequestIntent = requestIntentAttribute.RequestIntent;
                }
            }

            return this.serviceProvider.GetService(jobType);
        }
    }
}
