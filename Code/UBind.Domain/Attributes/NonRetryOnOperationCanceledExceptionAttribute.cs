// <copyright file="NonRetryOnOperationCanceledExceptionAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Attributes
{
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.Server;

    public class NonRetryOnOperationCanceledExceptionAttribute : JobFilterAttribute, IServerFilter
    {
        public void OnPerforming(PerformingContext filterContext)
        {
            // No action needed before performing the job
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            // Check if the job has failed and the exception is OperationCanceledException
            if (filterContext?.Exception?.InnerException is OperationCanceledException)
            {
                // Delete the job to mark it as canceled
                BackgroundJob.Delete(filterContext.BackgroundJob.Id);
            }
        }
    }
}
