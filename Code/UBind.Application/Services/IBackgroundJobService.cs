// <copyright file="IBackgroundJobService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Domain.Dto;

    /// <summary>
    /// An interface for hangfire service.
    /// </summary>
    public interface IBackgroundJobService
    {
        /// <summary>
        /// Acknowledge a hangfire job.
        /// </summary>
        /// <param name="tenantId">The tenant id.</param>
        /// <param name="hangFireJobId">The hangfire Job ID.</param>
        /// <param name="performingUserId">The performing user id.</param>
        /// <param name="ticketId">The Ticket Id.</param>
        /// <param name="acknowledgementMessage">The acknowledgment message.</param>
        Task Acknowledge(Guid tenantId, string hangFireJobId, Guid performingUserId, string ticketId, string acknowledgementMessage);

        /// <summary>
        /// Get list of Failed jobs (even if they are being retried) regardless if it was marked as acknowledged or not.
        /// </summary>
        /// <param name="tenantId">The users tenant Id.</param>
        /// <param name="filterEnvironment">Will only include jobs where an environment parameter has been set.</param>
        /// <param name="filterTenant">Will only include jobs where a tenant parameter has been set.</param>
        /// <param name="filterProduct">Will only include jobs where a product parameter has been set.</param>
        /// <param name="environment">(optional) if not set, match jobs which do not have an environment specified. If non-empty, match jobs with the exact environment.</param>
        /// <param name="tenantAlias">(optional) if not set, match jobs which do not have a tenant specified. If non-empty, match jobs with the exact tenant.</param>
        /// <param name="productAlias">(optional) if not set, match jobs which do not have a product specified. If non-empty, match jobs with the exact product.</param>
        /// <param name="isAcknowledged">(optional) will only include jobs whethere acknowledged or not.</param>
        /// <returns>Text indicating the number of failed jobs.</returns>
        Task<List<BackgroundJobDto>> GetFailedAndRetryingJobs(Guid tenantId, bool filterEnvironment = false, bool filterTenant = false, bool filterProduct = false, string environment = null, string tenantAlias = null, string productAlias = null, bool? isAcknowledged = null);

        /// <summary>
        /// Counts the number of failed and retrying hangfire jobs.
        /// </summary>
        /// <param name="filterEnvironment">Will only include jobs where an environment parameter has been set.</param>
        /// <param name="filterTenant">Will only include jobs where a tenant parameter has been set.</param>
        /// <param name="filterProduct">Will only include jobs where a product parameter has been set.</param>
        /// <param name="environment">(optional) if not set, match jobs which do not have an environment specified. If non-empty, match jobs with the exact environment.</param>
        /// <param name="tenantAlias">(optional) if not set, match jobs which do not have a tenant specified. If non-empty, match jobs with the exact tenant.</param>
        /// <param name="productAlias">(optional) if not set, match jobs which do not have a product specified. If non-empty, match jobs with the exact product.</param>
        /// <param name="isAcknowledged">(optional) will only include jobs whethere acknowledged or not.</param>
        /// <returns>Text indicating the number of failed jobs.</returns>
        Task<int> GetFailedAndRetryingJobsCount(bool filterEnvironment = false, bool filterTenant = false, bool filterProduct = false, string environment = null, string tenantAlias = null, string productAlias = null, bool? isAcknowledged = null);

        /// <summary>
        /// Expires a job automatically given that passed job ID exists.
        /// </summary>
        /// <param name="jobId">The Id of the job to be expired.</param>
        void ExpireJob(string jobId);

        /// <summary>
        /// Create a Test Failed Job.
        /// </summary>
        /// <param name="tenantId">The users tenant Id.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="tenantAlias">The tenant alias.</param>
        /// <param name="productAlias">The product alias.</param>
        /// <param name="markAsFailAfterFirstFailed">The indicator if the job will be marked as failed upon the first failure if the indicator is set.
        /// Otherwise, the job will only be marked as failed after 10 unsuccessful attempts.</param>
        /// <returns>Return the Id of Failed Test Job.</returns>
        Task<string> CreateTestFailedJob(Guid? tenantId, string environment, string tenantAlias, string productAlias, bool markAsFailAfterFirstFailed = false);
    }
}
