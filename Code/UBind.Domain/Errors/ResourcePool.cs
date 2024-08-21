// <copyright file="ResourcePool.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain;

using System.Net;

public static partial class Errors
{
    public static class ResourcePool
    {
        public static Error ObjectDisposed() =>
        new Error(
            "resource.pool.object.disposed",
            "Resource pool has been disposed",
            "The resource pool has been disposed. This could happen if it was closed or finished being used. "
            + "Please try again later or contact customer support if you continue to experience issues.",
            HttpStatusCode.ServiceUnavailable);

        public static Error MaxRetriesReached(int retryCount) =>
            new Error(
                "resource.pool.max.retries.reached",
                "Maximum retries reached",
                $"We're having difficulty getting the resource you requested after {retryCount} attempts. "
                + "This could be due to high demand or a temporary issue. Please try again in a moment, "
                + "and if the problem persists, feel free to reach out to our support team for assistance.",
                HttpStatusCode.RequestTimeout);

        public static Error FailedToCreateResource(Guid releaseId, WebFormAppType webFormAppType, IEnumerable<string> details) =>
            new Error(
                "resource.pool.failed.to.create",
                "Failed to create resource",
                $"Failed to create a resource for release ID {releaseId} and WebFormAppType {webFormAppType}. "
                + "Please check the logs for more information or contact customer support.",
                HttpStatusCode.InternalServerError,
                details);

        public static Error FailedToCreateResource(IEnumerable<string> details) =>
            new Error(
                "resource.pool.failed.to.create",
                "Failed to create resource",
                $"Failed to create a resource. "
                + "Please check the logs for more information or contact customer support.",
                HttpStatusCode.InternalServerError,
                details);
    }
}
