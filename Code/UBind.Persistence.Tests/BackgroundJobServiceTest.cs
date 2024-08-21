// <copyright file="BackgroundJobServiceTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Hangfire;
    using Hangfire.Storage;
    using NodaTime;
    using UBind.Application;
    using UBind.Application.Tests;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Authentication;
    using UBind.Domain.Dto;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Tests the background job service.
    /// </summary>
    [Collection(DatabaseCollection.Name)]
    public class BackgroundJobServiceTest
    {
        private ApplicationStack stack;
        private BackgroundJobService backgroundJobService;

        public BackgroundJobServiceTest()
        {
            this.stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName, ApplicationStackConfiguration.Default);
            this.backgroundJobService = this.stack.BackgroundJobService;
            JobStorage.Current = this.stack.BackgroundJobStorageProvider;
        }

        [Fact]
        public async Task CreateTestJobSucceed()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = await this.CreateTenant(tenantId);
            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            var product = this.CreateProduct(tenantId, productId);
            this.stack.MockMediator.GetProductByIdOrAliasQuery(product);
            var userAuthenticationData = this.GetAuthenticationData(tenantId);
            var currentJobCount = await this.GetJobCount(tenantId, productId);

            // Act
            var jobId = await this.backgroundJobService.CreateTestFailedJob(
                userAuthenticationData.TenantId,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias,
                true);
            var newJobCount = await this.GetJobCount(tenantId, productId);

            // Assert
            jobId.Should().NotBeNullOrEmpty();
            newJobCount.Should().Be(currentJobCount + 1);
        }

        [Fact]
        public async Task GetJobCountReturnsCorrectCount()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = await this.CreateTenant(tenantId);
            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            var product = this.CreateProduct(tenantId, productId);
            this.stack.MockMediator.GetProductByIdOrAliasQuery(product);
            var userAuthenticationData = this.GetAuthenticationData(tenantId);
            var currentJobCount = await this.GetJobCount(tenantId, productId);

            await this.backgroundJobService.CreateTestFailedJob(
                userAuthenticationData.TenantId,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias,
                true);

            // Act
            var newJobCount = await this.backgroundJobService.GetFailedAndRetryingJobsCount(
                false,
                false,
                false,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias);

            // Assert
            newJobCount.Should().Be(currentJobCount + 1);
        }

        [Fact]
        public async Task GetJobCount_WithIsAcknowledgeSetToTrue_ReturnsCorrectCount()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = await this.CreateTenant(tenantId);
            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            var product = this.CreateProduct(tenantId, productId);
            this.stack.MockMediator.GetProductByIdOrAliasQuery(product);
            var userAuthenticationData = this.GetAuthenticationData(tenantId);
            var currentJobCount = await this.GetJobCount(tenantId, productId, true);

            await this.backgroundJobService.CreateTestFailedJob(
                userAuthenticationData.TenantId,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias);

            // Act
            var newJobCount = await this.backgroundJobService.GetFailedAndRetryingJobsCount(
                false,
                false,
                false,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias,
                true);

            // Assert
            newJobCount.Should().Be(currentJobCount);
        }

        [Fact]
        public async Task GetJobCount_WithIsAcknowledgeSetToFalse_ReturnsCorrectCount()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = await this.CreateTenant(tenantId);
            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            var product = this.CreateProduct(tenantId, productId);
            this.stack.MockMediator.GetProductByIdOrAliasQuery(product);
            var userAuthenticationData = this.GetAuthenticationData(tenantId);
            var currentJobCount = await this.GetJobCount(tenantId, productId, false);

            await this.backgroundJobService.CreateTestFailedJob(
                userAuthenticationData.TenantId,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias,
                true);

            // Act
            var newJobCount = await this.backgroundJobService.GetFailedAndRetryingJobsCount(
                false,
                false,
                false,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias,
                false);

            // Assert
            newJobCount.Should().Be(currentJobCount + 1);
        }

        [Fact]
        public async Task GetFailedAndScheduledJobsCount_ReturnsCorrectCount_WhenAuthenticationAndTenantAreNull()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = await this.CreateTenant(tenantId);
            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            var product = this.CreateProduct(tenantId, productId);
            this.stack.MockMediator.GetProductByIdOrAliasQuery(product);
            var currentJobCount = await this.GetJobCount();

            await this.backgroundJobService.CreateTestFailedJob(
                tenantId,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias,
                true);

            // Act
            var newJobCount = await this.backgroundJobService.GetFailedAndRetryingJobsCount(
                false,
                false,
                false,
                DeploymentEnvironment.Development.ToString(),
                null,
                null);

            // Assert
            newJobCount.Should().Be(currentJobCount + 1);
        }

        [Fact]
        public async Task GetFailedAndScheduledJobsReturnListOfJobs()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = await this.CreateTenant(tenantId);
            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            var product = this.CreateProduct(tenantId, productId);
            this.stack.MockMediator.GetProductByIdOrAliasQuery(product);
            var userAuthenticationData = this.GetAuthenticationData(tenantId);
            var currentFailedJobs = await this.GetListOfJob(tenantId, productId);

            await this.backgroundJobService.CreateTestFailedJob(
                userAuthenticationData.TenantId,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias,
                true);

            // Act
            var newFailedJobs = await this.backgroundJobService.GetFailedAndRetryingJobs(
                userAuthenticationData.TenantId,
                false,
                false,
                false,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias);

            // Assert
            newFailedJobs.Count.Should().Be(currentFailedJobs.Count + 1);
        }

        [Fact]
        public async Task GetFailedAndScheduledJobs_WithIsAcknowledgeSetToTrue_ReturnsListOfAcknowledgedJobs()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = await this.CreateTenant(tenantId);
            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            var product = this.CreateProduct(tenantId, productId);
            this.stack.MockMediator.GetProductByIdOrAliasQuery(product);
            var userAuthenticationData = this.GetAuthenticationData(tenantId);
            var currentFailedJobs = await this.GetListOfJob(tenantId, productId);

            await this.backgroundJobService.CreateTestFailedJob(
                userAuthenticationData.TenantId,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias);

            // Act
            var newFailedJobs = await this.backgroundJobService.GetFailedAndRetryingJobs(
                userAuthenticationData.TenantId,
                false,
                false,
                false,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias,
                true);

            // Assert
            newFailedJobs.Count.Should().Be(currentFailedJobs.Count);
        }

        [Fact]
        public async Task GetFailedAndScheduledJobs_WithIsAcknowledgeSetToFalse_ReturnsListOfUnAcknowledgedJobs()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = await this.CreateTenant(tenantId);
            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            var product = this.CreateProduct(tenantId, productId);
            this.stack.MockMediator.GetProductByIdOrAliasQuery(product);
            var userAuthenticationData = this.GetAuthenticationData(tenantId);
            var currentFailedJobs = await this.GetListOfJob(tenantId, productId);

            await this.backgroundJobService.CreateTestFailedJob(
                userAuthenticationData.TenantId,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias,
                true);

            // Act
            var newFailedJobs = await this.backgroundJobService.GetFailedAndRetryingJobs(
                userAuthenticationData.TenantId,
                false,
                false,
                false,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias,
                false);

            // Assert
            newFailedJobs.Count.Should().Be(currentFailedJobs.Count + 1);
        }

        [Fact]
        public async Task AcknowledgeFailedJobsMarkJobAsAcknowledge()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = await this.CreateTenant(tenantId);
            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            var product = this.CreateProduct(tenantId, productId);
            this.stack.MockMediator.GetProductByIdOrAliasQuery(product);
            var userAuthenticationData = this.GetAuthenticationData(tenantId);

            var jobId = await this.backgroundJobService.CreateTestFailedJob(
                userAuthenticationData.TenantId,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias,
                true);

            // Act
            await this.backgroundJobService.Acknowledge(userAuthenticationData.TenantId,
                jobId,
                userAuthenticationData.UserId,
                "1234",
                "test message");

            // Assert
            IStorageConnection connection = JobStorage.Current.GetConnection();
            JobData jobData = connection.GetJobData(jobId);
            var isAcknowledge = this.GetJobParameter(jobId, BackgroundJobParameter.IsAcknowledged, string.Empty);

            isAcknowledge.Should().Be("true");
        }

        [Fact]
        public async Task ReacknowledgeFailedJobsFail()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = await this.CreateTenant(tenantId);
            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            var product = this.CreateProduct(tenantId, productId);
            this.stack.MockMediator.GetProductByIdOrAliasQuery(product);
            var userAuthenticationData = this.GetAuthenticationData(tenantId);
            var jobId = await this.backgroundJobService.CreateTestFailedJob(
                userAuthenticationData.TenantId,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias);
            var currentJobsCount = this.GetJobCount(tenantId, productId);

            // Act
            await this.backgroundJobService.Acknowledge(userAuthenticationData.TenantId, jobId, userAuthenticationData.UserId, "1234", "test message");

            // Assert
            await this.backgroundJobService.Invoking(async y => await y.Acknowledge(
                userAuthenticationData.TenantId,
                jobId,
                userAuthenticationData.UserId,
                "1234",
                "test message")).Should().ThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task AcknowledgeFailedJobsWithOutPermissionDoesNotAllow()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = await this.CreateTenant(tenantId);
            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            var product = this.CreateProduct(tenantId, productId);
            this.stack.MockMediator.GetProductByIdOrAliasQuery(product);
            var userAuthenticationData = this.GetAuthenticationData(tenantId);
            var jobId = await this.backgroundJobService.CreateTestFailedJob(
                userAuthenticationData.TenantId,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias);

            // Act
            var otherTenantId = Guid.NewGuid();
            var otherProductId = Guid.NewGuid();
            await this.CreateTenant(otherTenantId);
            this.CreateProduct(otherTenantId, otherProductId);
            var otherUserAuthenticationData = new UserAuthenticationData(
                otherTenantId,
                Guid.NewGuid(),
                UserType.Client,
                Guid.NewGuid(),
                default);

            // Act
            await this.backgroundJobService.Invoking(async y => await y.Acknowledge(
                otherUserAuthenticationData.TenantId,
                jobId,
                otherUserAuthenticationData.UserId,
                "1234",
                "test message")).Should().ThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task AcknowledgeFaileJobsUsingMasterTenantSucceed()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var tenant = await this.CreateTenant(tenantId);
            this.stack.MockMediator.GetTenantByIdOrAliasQuery(tenant);
            var product = this.CreateProduct(tenantId, productId);
            this.stack.MockMediator.GetProductByIdOrAliasQuery(product);
            var userAuthenticationData = this.GetAuthenticationData(tenantId);
            var jobId = await this.backgroundJobService.CreateTestFailedJob(
                userAuthenticationData.TenantId,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias);

            // Act
            var ubindAdminUserAuthenticationData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Master,
                Guid.NewGuid(),
                default);

            // Assert
            await this.backgroundJobService.Invoking(y => y.Acknowledge(
                ubindAdminUserAuthenticationData.TenantId,
                jobId,
                ubindAdminUserAuthenticationData.UserId,
                "1234",
                "test message")).Should().NotThrowAsync();
        }

        private UserAuthenticationData GetAuthenticationData(Guid tenantId)
        {
            var tenant = this.stack.TenantRepository.GetTenantById(tenantId);
            var userAuthenticationData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                Guid.NewGuid(),
                default);

            return userAuthenticationData;
        }

        private async Task<int> GetJobCount()
        {
            var jobCount = await this.backgroundJobService.GetFailedAndRetryingJobsCount(
                false,
                false,
                false,
                DeploymentEnvironment.Development.ToString(),
                null,
                null);

            return jobCount;
        }

        private async Task<int> GetJobCount(Guid tenantId, Guid productId, bool? isAcknowledged = null)
        {
            var tenant = this.stack.TenantRepository.GetTenantById(tenantId);
            var product = this.stack.ProductRepository.GetProductById(tenantId, productId);
            var jobCount = await this.backgroundJobService.GetFailedAndRetryingJobsCount(
                false,
                false,
                false,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias,
                isAcknowledged);

            return jobCount;
        }

        private async Task<List<BackgroundJobDto>> GetListOfJob(Guid tenantId, Guid productId)
        {
            var tenant = this.stack.TenantRepository.GetTenantById(tenantId);
            var userAuthenticationData = new UserAuthenticationData(
                tenant.Id,
                tenant.Details.DefaultOrganisationId,
                UserType.Client,
                Guid.NewGuid(),
                default);

            var product = await this.stack.CachingResolver.GetProductOrThrow(tenant.Id, productId);
            var jobs = await this.backgroundJobService.GetFailedAndRetryingJobs(
                userAuthenticationData.TenantId,
                false,
                false,
                false,
                DeploymentEnvironment.Development.ToString(),
                tenant.Details.Alias,
                product.Details.Alias);
            return jobs;
        }

        private async Task<Tenant> CreateTenant(Guid tenantId)
        {
            var tenant = this.stack.TenantRepository.GetTenantById(tenantId);
            if (tenant == null)
            {
                tenant = TenantFactory.Create(tenantId);
                this.stack.TenantRepository.Insert(tenant);
                this.stack.DbContext.SaveChanges();

                var organisation = Organisation.CreateNewOrganisation(
                    tenant.Id,
                    tenant.Details.Alias,
                    tenant.Details.Name,
                    null, Guid.NewGuid(),
                    SystemClock.Instance.GetCurrentInstant());
                await this.stack.OrganisationAggregateRepository.Save(organisation);

                tenant.SetDefaultOrganisation(
                    organisation.Id, SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromMinutes(5)));
                this.stack.DbContext.SaveChanges();
            }

            return tenant;
        }

        private Domain.Product.Product CreateProduct(Guid tenantId, Guid productId)
        {
            var product = this.stack.ProductRepository.GetProductById(tenantId, productId);
            if (product == null)
            {
                product = ProductFactory.Create(tenantId, productId);
                this.stack.ProductRepository.Insert(product);
                this.stack.DbContext.SaveChanges();
            }

            return product;
        }

        private string GetJobParameter(string jobId, string name, string defaultValue)
        {
            var connection = JobStorage.Current.GetConnection();
            string result = connection.GetJobParameter(jobId, name);
            return result == null ? defaultValue : result;
        }
    }
}
