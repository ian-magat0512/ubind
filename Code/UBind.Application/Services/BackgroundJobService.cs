// <copyright file="BackgroundJobService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Hangfire;
    using Hangfire.States;
    using Hangfire.Storage;
    using Hangfire.Storage.Monitoring;
    using NodaTime;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Dto;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;

    /// <inheritdoc/>
    public class BackgroundJobService : IBackgroundJobService
    {
        private ICachingResolver cachingResolver;
        private IUserService userService;
        private IClock clock;
        private IBackgroundJobClient jobClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundJobService"/> class.
        /// </summary>
        /// <param name="userService">The User Service.</param>
        /// <param name="clock">The clock.</param>
        /// <param name="jobClient">The job client.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        public BackgroundJobService(
            IUserService userService,
            IClock clock,
            IBackgroundJobClient jobClient,
            ICachingResolver cachingResolver)
        {
            this.cachingResolver = cachingResolver;
            this.userService = userService;
            this.jobClient = jobClient;
            this.clock = clock;
        }

        /// <summary>
        /// Gets job Storage.
        /// </summary>
        public IStorageConnection JobStorageConnection => JobStorage.Current.GetConnection();

        /// <inheritdoc/>
        public async Task Acknowledge(Guid tenantId, string backgroundJobId, Guid performingUserId, string ticketId, string acknowledgementMessage)
        {
            await this.VerifyAcknowledgePermission(tenantId, backgroundJobId);
            IStorageConnection connection = JobStorage.Current.GetConnection();
            connection.SetJobParameter(backgroundJobId, BackgroundJobParameter.IsAcknowledged, "true");
            connection.SetJobParameter(backgroundJobId, BackgroundJobParameter.UserId, performingUserId.ToString());
            connection.SetJobParameter(backgroundJobId, BackgroundJobParameter.TicketId, ticketId);
            connection.SetJobParameter(backgroundJobId, BackgroundJobParameter.AcknowledgmentMessage, acknowledgementMessage);
            connection.SetJobParameter(backgroundJobId, BackgroundJobParameter.AcknowledgedTimestamp, this.clock.GetCurrentInstant().ToExtendedIso8601String());
        }

        /// <inheritdoc/>
        public async Task<List<BackgroundJobDto>> GetFailedAndRetryingJobs(
            Guid tenantId,
            bool filterEnvironment = false,
            bool filterTenant = false,
            bool filterProduct = false,
            string environment = null,
            string tenantAlias = null,
            string productAlias = null,
            bool? isAcknowledged = null)
        {
            await this.VerifyTenantAndProductExist(tenantAlias, productAlias);

            var failedJobs = await this.GetFailedJobs(
                tenantId,
                filterEnvironment,
                filterTenant,
                filterProduct,
                environment,
                tenantAlias,
                productAlias,
                isAcknowledged);

            var scheduledJobs = await this.GetScheduledJobs(
                tenantId,
                filterEnvironment,
                filterTenant,
                filterProduct,
                environment,
                tenantAlias,
                productAlias,
                isAcknowledged);

            var failedAndScheduledJobs = failedJobs.Concat(scheduledJobs).ToList();

            return failedAndScheduledJobs;
        }

        /// <inheritdoc/>
        public async Task<int> GetFailedAndRetryingJobsCount(bool filterEnvironment = false, bool filterTenant = false, bool filterProduct = false, string environment = null, string tenantAlias = null, string productAlias = null, bool? isAcknowledged = null)
        {
            var failedJobsCount = await this.GetFailedJobsCount(filterEnvironment, filterTenant, filterProduct, environment, tenantAlias, productAlias, isAcknowledged);
            var scheduledJobsCount = await this.GetScheduledJobsCount(filterEnvironment, filterTenant, filterProduct, environment, tenantAlias, productAlias, isAcknowledged);
            return failedJobsCount + scheduledJobsCount;
        }

        /// <inheritdoc/>
        public async Task<string> CreateTestFailedJob(
            Guid? tenantId,
            string environment,
            string tenantAlias,
            string productAlias,
            bool markAsFailAfterFirstFailed = false)
        {
            await this.VerifyTenantAndProductJobPermission(tenantId, tenantAlias, productAlias);
            string backgroundJobId = BackgroundJob.Enqueue<BackgroundJobService>(service => service.CreateFailingTask());
            IStorageConnection connection = JobStorage.Current.GetConnection();
            if (markAsFailAfterFirstFailed)
            {
                this.jobClient.ChangeState(backgroundJobId, new FailedState(new InvalidOperationException("This is a Test Error")));
            }

            connection.SetJobParameter(backgroundJobId, BackgroundJobParameter.Environment, environment);
            connection.SetJobParameter(backgroundJobId, BackgroundJobParameter.Tenant, tenantAlias);
            connection.SetJobParameter(backgroundJobId, BackgroundJobParameter.Product, productAlias);
            connection.SetJobParameter(backgroundJobId, BackgroundJobParameter.IsAcknowledged, "false");

            JobData jobData = connection.GetJobData(backgroundJobId);
            return backgroundJobId;
        }

        public void ExpireJob(string jobId)
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                var jobData = connection.GetJobData(jobId);
                if (jobData == null)
                {
                    throw new ErrorException(Errors.BackgroundJob.JobDoesNotExist(jobId));
                }

                using (var transaction = connection.CreateWriteTransaction())
                {
                    transaction.ExpireJob(jobId, TimeSpan.Zero);
                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Create a Test Failed Job.
        /// </summary>
        [AutomaticRetry(Attempts = 10, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public void CreateFailingTask()
        {
            throw new InvalidOperationException("This is a Test Error");
        }

        private async Task<List<BackgroundJobDto>> GetFailedJobs(
            Guid tenantId,
            bool filterEnvironment = false,
            bool filterTenant = false,
            bool filterProduct = false,
            string environment = null,
            string tenantAlias = null,
            string productAlias = null,
            bool? isAcknowledged = null)
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var filteredFailedJobs = monitoringApi.FailedJobs(0, 1000).Where(failedJobDtoPair =>
                (!filterEnvironment || this.JobParameterMatches(failedJobDtoPair.Key, BackgroundJobParameter.Environment, environment))
                && (!filterTenant || this.JobParameterMatches(failedJobDtoPair.Key, BackgroundJobParameter.Tenant, tenantAlias))
                && (!filterProduct || this.JobParameterMatches(failedJobDtoPair.Key, BackgroundJobParameter.Product, productAlias)));

            if (isAcknowledged != null)
            {
                filteredFailedJobs = filteredFailedJobs.Where(failedJobDtoPair => this.JobParameterMatches(failedJobDtoPair.Key, BackgroundJobParameter.IsAcknowledged, isAcknowledged.ToString().ToLower()));
            }

            if (tenantId != Domain.Tenant.MasterTenantId)
            {
                var userTenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
                filteredFailedJobs = filteredFailedJobs.Where(failedJobDtoPair =>
                this.JobParameterMatches(failedJobDtoPair.Key, BackgroundJobParameter.Tenant, userTenantAlias));
            }

            var backgroundJobsDto = await filteredFailedJobs.SelectAsync(async filteredFailedJob =>
               new BackgroundJobDto(
               filteredFailedJob.Key,
               filteredFailedJob.Value.Reason,
               filteredFailedJob.Value.ExceptionType,
               filteredFailedJob.Value.ExceptionMessage,
               filteredFailedJob.Value.ExceptionDetails,
               bool.Parse(this.GetJobParameter(filteredFailedJob.Key, BackgroundJobParameter.IsAcknowledged, isAcknowledged == null ? null : isAcknowledged.ToString())),
               this.GetJobParameter(filteredFailedJob.Key, BackgroundJobParameter.UserId, string.Empty),
               this.GetJobParameter(filteredFailedJob.Key, BackgroundJobParameter.TicketId, string.Empty),
               this.GetJobParameter(filteredFailedJob.Key, BackgroundJobParameter.AcknowledgmentMessage, string.Empty),
               await this.GetUserFullname(this.GetJobParameter(filteredFailedJob.Key, BackgroundJobParameter.Tenant, string.Empty), this.GetJobParameter(filteredFailedJob.Key, BackgroundJobParameter.UserId, string.Empty)),
               this.GetJobParameter(filteredFailedJob.Key, BackgroundJobParameter.Environment, string.Empty),
               this.GetJobParameter(filteredFailedJob.Key, BackgroundJobParameter.Tenant, string.Empty),
               this.GetJobParameter(filteredFailedJob.Key, BackgroundJobParameter.Product, string.Empty),
               LocalDateTime.FromDateTime(filteredFailedJob.Value.FailedAt.Value).InUtc().ToInstant().ToExtendedIso8601String(),
               this.GetJobParameter(filteredFailedJob.Key, BackgroundJobParameter.AcknowledgedTimestamp, string.Empty),
               this.GetJobParameter(filteredFailedJob.Key, BackgroundJobParameter.ExpireAt, string.Empty)));
            return backgroundJobsDto.ToList();
        }

        private async Task<List<BackgroundJobDto>> GetScheduledJobs(
            Guid tenantId,
            bool filterEnvironment = false,
            bool filterTenant = false,
            bool filterProduct = false,
            string environment = null,
            string tenantAlias = null,
            string productAlias = null,
            bool? isAcknowledged = null)
        {
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var scheduledJobs = monitoringApi.ScheduledJobs(0, 1000).Where(job =>
                (!filterEnvironment || this.JobParameterMatches(job.Key, BackgroundJobParameter.Environment, environment))
                && (!filterTenant || this.JobParameterMatches(job.Key, BackgroundJobParameter.Tenant, tenantAlias))
                && (!filterProduct || this.JobParameterMatches(job.Key, BackgroundJobParameter.Product, productAlias)));

            if (isAcknowledged != null)
            {
                scheduledJobs = scheduledJobs.Where(failedJobDtoPair => this.JobParameterMatches(failedJobDtoPair.Key, BackgroundJobParameter.IsAcknowledged, isAcknowledged.ToString().ToLower()));
            }

            if (tenantId != Domain.Tenant.MasterTenantId)
            {
                var userTenantAlias = await this.cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
                scheduledJobs = scheduledJobs.Where(failedJobDtoPair =>
                this.JobParameterMatches(failedJobDtoPair.Key, BackgroundJobParameter.Tenant, userTenantAlias));
            }

            var failedScheduledJobs = await scheduledJobs.SelectAsync(async job =>
            {
                JobDetailsDto jobDetails = monitoringApi.JobDetails(job.Key);
                var scheduledJob = jobDetails.History.LastOrDefault(x => x.StateName == "Failed");
                if (scheduledJob == null)
                {
                    return null;
                }

                string failedAtStr = scheduledJob.Data["FailedAt"];
                if (scheduledJob.Data.ContainsKey("FailedAt") && long.TryParse(failedAtStr, out long failedAtLong))
                {
                    failedAtStr = Instant.FromUnixTimeMilliseconds(failedAtLong).InUtc().ToInstant().ToExtendedIso8601String();
                }

                var backGroundJob = new BackgroundJobDto(
                    job.Key,
                    scheduledJob.Reason,
                    scheduledJob.Data["ExceptionType"],
                    scheduledJob.Data["ExceptionMessage"],
                    scheduledJob.Data["ExceptionDetails"],
                    bool.Parse(this.GetJobParameter(job.Key, BackgroundJobParameter.IsAcknowledged, isAcknowledged == null ? null : isAcknowledged.ToString())),
                    this.GetJobParameter(job.Key, BackgroundJobParameter.UserId, string.Empty),
                    this.GetJobParameter(job.Key, BackgroundJobParameter.TicketId, string.Empty),
                    this.GetJobParameter(job.Key, BackgroundJobParameter.AcknowledgmentMessage, string.Empty),
                    await this.GetUserFullname(this.GetJobParameter(job.Key, BackgroundJobParameter.Tenant, string.Empty), this.GetJobParameter(job.Key, BackgroundJobParameter.UserId, string.Empty)),
                    this.GetJobParameter(job.Key, BackgroundJobParameter.Environment, string.Empty),
                    this.GetJobParameter(job.Key, BackgroundJobParameter.Tenant, string.Empty),
                    this.GetJobParameter(job.Key, BackgroundJobParameter.Product, string.Empty),
                    failedAtStr,
                    this.GetJobParameter(job.Key, BackgroundJobParameter.AcknowledgedTimestamp, string.Empty),
                    this.GetJobParameter(job.Key, BackgroundJobParameter.ExpireAt, string.Empty));
                return backGroundJob;
            });

            return failedScheduledJobs.Where(e => e != null).ToList();
        }

        private async Task<int> GetScheduledJobsCount(bool filterEnvironment = false, bool filterTenant = false, bool filterProduct = false, string environment = null, string tenantAlias = null, string productAlias = null, bool? isAcknowledged = null)
        {
            await this.VerifyTenantAndProductExist(tenantAlias, productAlias);
            IStorageConnection connection = JobStorage.Current.GetConnection();
            var monitoringApi = JobStorage.Current.GetMonitoringApi();

            var scheduledJobs = monitoringApi.ScheduledJobs(0, 1000).Where(failedJobDtoPair =>
            (!filterEnvironment || this.JobParameterMatches(failedJobDtoPair.Key, BackgroundJobParameter.Environment, environment))
            && (!filterTenant || this.JobParameterMatches(failedJobDtoPair.Key, BackgroundJobParameter.Tenant, tenantAlias))
            && (!filterProduct || this.JobParameterMatches(failedJobDtoPair.Key, BackgroundJobParameter.Product, productAlias)));

            if (isAcknowledged != null)
            {
                scheduledJobs = scheduledJobs.Where(failedJobDtoPair => this.JobParameterMatches(failedJobDtoPair.Key, BackgroundJobParameter.IsAcknowledged, isAcknowledged.ToString().ToLower()));
            }

            var scheduledJobsWithFailedHistory = scheduledJobs.Select(filteredFailedJob =>
            {
                JobDetailsDto jobDetails = monitoringApi.JobDetails(filteredFailedJob.Key);
                var scheduledJob = jobDetails.History.FirstOrDefault(d => d.StateName == "Failed");
                return scheduledJob != null;
            });

            return scheduledJobsWithFailedHistory.Count();
        }

        private async Task<int> GetFailedJobsCount(bool filterEnvironment = false, bool filterTenant = false, bool filterProduct = false, string environment = null, string tenantAlias = null, string productAlias = null, bool? isAcknowledged = null)
        {
            await this.VerifyTenantAndProductExist(tenantAlias, productAlias);
            IStorageConnection connection = JobStorage.Current.GetConnection();
            var monitoringApi = JobStorage.Current.GetMonitoringApi();
            var filteredFailedJobs = monitoringApi.FailedJobs(0, 1000).Where(failedJobDtoPair =>
                (!filterEnvironment || this.JobParameterMatches(failedJobDtoPair.Key, BackgroundJobParameter.Environment, environment))
                && (!filterTenant || this.JobParameterMatches(failedJobDtoPair.Key, BackgroundJobParameter.Tenant, tenantAlias))
                && (!filterProduct || this.JobParameterMatches(failedJobDtoPair.Key, BackgroundJobParameter.Product, productAlias)));
            if (isAcknowledged != null)
            {
                filteredFailedJobs = filteredFailedJobs.Where(failedJobDtoPair => this.JobParameterMatches(failedJobDtoPair.Key, BackgroundJobParameter.IsAcknowledged, isAcknowledged.ToString().ToLower()));
            }

            return filteredFailedJobs.Count();
        }

        private string GetJobParameter(string jobId, string name, string defaultValue)
        {
            string result = this.JobStorageConnection.GetJobParameter(jobId, name);
            return result == null ? defaultValue : result;
        }

        private bool JobParameterMatches(string jobId, string name, string value)
        {
            string result = this.JobStorageConnection.GetJobParameter(jobId, name);
            if (value == null)
            {
                return result == null;
            }

            return result != null && result.ToLower().Equals(value.ToLower());
        }

        private async Task<string> GetUserFullname(string tenantId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return string.Empty;
            }

            var tenant = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(tenantId));

            var user = this.userService.GetUser(tenant.Id, Guid.Parse(userId));
            return user == null ? string.Empty : user.FullName;
        }

        private async Task VerifyAcknowledgePermission(Guid userTenantId, string backgroundJobId)
        {
            IStorageConnection connection = JobStorage.Current.GetConnection();
            JobData jobData = connection.GetJobData(backgroundJobId);
            if (jobData == null)
            {
                throw new ErrorException(Errors.General.NotFound("BackgroundJob", backgroundJobId));
            }

            var jobTenantId = this.GetJobParameter(backgroundJobId, BackgroundJobParameter.Tenant, string.Empty);
            var parsedJobTenant = await this.cachingResolver.GetTenantOrNull(new GuidOrAlias(jobTenantId));
            if (userTenantId != Domain.Tenant.MasterTenantId && parsedJobTenant?.Id != userTenantId)
            {
                throw new ErrorException(Errors.General.Forbidden($"access the background job with id {backgroundJobId}", "it is not your background job or you do not have permission to access this background job."));
            }

            var hasBeenAcknowledge = this.GetJobParameter(backgroundJobId, BackgroundJobParameter.IsAcknowledged, "false");
            if (Convert.ToBoolean(hasBeenAcknowledge))
            {
                throw new ErrorException(Errors.BackgroundJob.JobAlreadyAcknowledge(backgroundJobId));
            }
        }

        private async Task VerifyTenantAndProductExist(string tenantAlias, string productAlias)
        {
            if (!string.IsNullOrEmpty(tenantAlias))
            {
                await this.cachingResolver.GetTenantByAliasOrThrow(tenantAlias);
                if (!string.IsNullOrEmpty(productAlias))
                {
                    await this.cachingResolver.GetProductByAliasOThrow(tenantAlias, productAlias);
                }
            }
        }

        private async Task VerifyTenantAndProductJobPermission(Guid? userTenantId, string tenantAlias, string productAlias)
        {
            await this.VerifyTenantAndProductExist(tenantAlias, productAlias);
            var tenant = await this.cachingResolver.GetTenantByAliasOrThrow(tenantAlias);
            if (userTenantId != null &&
                userTenantId != Domain.Tenant.MasterTenantId
                && tenant.Id != userTenantId)
            {
                throw new ErrorException(
                    Errors.General.Forbidden(
                        $"access the tenant job with  tenant Id {tenantAlias}",
                        "it is not your background job or you do not have permission to access this background job."));
            }
        }
    }
}
