// <copyright file="StartupJobRunnerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.StartupJobs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;
    using Microsoft.Extensions.Logging;
    using Moq;
    using UBind.Application.Person;
    using UBind.Application.Services.Email;
    using UBind.Application.Services.Search;
    using UBind.Application.StartupJobs;
    using UBind.Application.Tests.Fakes;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Search;
    using Xunit;

    public class StartupJobRunnerTests
    {
        private readonly string keyExecuteMethodByName = "ExecuteStartupJobAndMarkCompleted";
        private readonly string keyMethodName = "startupJobAlias";

        private readonly Mock<IStartupJobRepository> startupJobRepository;
        private readonly Mock<IBackgroundJobClient> backgroundJobClient;
        private readonly Mock<ICqrsMediator> mediator;
        private readonly Mock<ILogger<StartupJobRunner>> logger;
        private readonly Mock<IPersonService> personService;
        private readonly Mock<IFileContentRepository> fileContentRepository;
        private readonly Mock<ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters>> policySearchService;
        private readonly Mock<IErrorNotificationService> mockErrorNotificationService;
        private readonly Mock<IRecurringJobManager> mockRecurringJobManager;
        private readonly IStartupJobRegistry startupJobRegistry;
        private readonly Mock<IUBindDbContext> dbContextMock;
        private readonly Mock<ICachingResolver> cachingResolverMock;
        private IStartupJobEnvironmentConfiguration startupJobConfiguration;

        public StartupJobRunnerTests()
        {
            this.startupJobRepository = new Mock<IStartupJobRepository>();
            this.backgroundJobClient = new Mock<IBackgroundJobClient>();
            this.backgroundJobClient.Setup(s => s.Create(It.IsAny<Job>(), It.IsAny<IState>())).Returns("TESTJOBID");
            this.mediator = new Mock<ICqrsMediator>();
            this.logger = new Mock<ILogger<StartupJobRunner>>();
            this.personService = new Mock<IPersonService>();
            this.fileContentRepository = new Mock<IFileContentRepository>();
            this.policySearchService = new Mock<ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters>>();
            this.mockErrorNotificationService = new Mock<IErrorNotificationService>();
            this.mockRecurringJobManager = new Mock<IRecurringJobManager>();
            this.dbContextMock = new Mock<IUBindDbContext>();
            this.cachingResolverMock = new Mock<ICachingResolver>();
            this.startupJobRegistry = new StartupJobRegistry(
                this.personService.Object,
                this.fileContentRepository.Object,
                this.policySearchService.Object,
                this.mediator.Object,
                this.dbContextMock.Object,
                this.cachingResolverMock.Object);
        }

        [Fact]
        public async Task RunJobs_ShouldOnlyRunJobsThatAreConfiguredToRunAutomaticallyWhenInMultiNode()
        {
            // Arrange
            List<StartupJob> jobs = new List<StartupJob>
            {
                new StartupJob("RegeneratePersonReadModels", false, true),
                new StartupJob("UpdateFileContentsAndHashCodes", false, false),
                new StartupJob("ApplyNewIdToMultipleAggregates", false, true),
            };

            this.startupJobRepository.Setup(s => s.GetIncompleteStartupJobs()).Returns(jobs);
            this.startupJobRepository.Setup(s => s.GetIncompleteByAlias("RegeneratePersonReadModels")).Returns(jobs[0]);
            this.startupJobRepository.Setup(s => s.GetIncompleteByAlias("UpdateFileContentsAndHashCodes")).Returns(jobs[1]);
            this.startupJobRepository.Setup(s => s.GetStartupJobByAlias("UpdateFileContentsAndHashCodes")).Returns(jobs[1]);
            this.startupJobRepository.Setup(s => s.GetIncompleteByAlias("ApplyNewIdToMultipleAggregates")).Returns(jobs[2]);
            this.startupJobConfiguration = new DefaultStartupJobEnvironmentConfiguration
            {
                MultiNodeEnvironment = true,
            };
            var startupJobRunner = new StartupJobRunner(
                this.startupJobRepository.Object,
                this.backgroundJobClient.Object,
                this.logger.Object,
                this.startupJobConfiguration,
                this.mockErrorNotificationService.Object,
                this.mockRecurringJobManager.Object,
                this.startupJobRegistry);

            // Act
            await startupJobRunner.RunJobs();

            // Assert
            this.backgroundJobClient.Verify(
               x => x.Create(
                    It.Is<Job>(job => job.Method.Name == this.keyExecuteMethodByName
                        && job.Method.GetParameters()[0].Name == this.keyMethodName),
                    It.IsAny<IState>()), Times.Once); // called once
        }

        [Fact]
        public async Task RunJobs_ShouldRunInBackgroundIfNotBlockingAndNotInMultiNodeEnvironment()
        {
            // Arrange
            List<StartupJob> jobs = new List<StartupJob>
            {
                new StartupJob("RegeneratePersonReadModels", false, false),
            };

            this.startupJobRepository.Setup(s => s.GetIncompleteStartupJobs()).Returns(jobs);
            this.startupJobRepository.Setup(s => s.GetStartupJobByAlias(It.IsAny<string>())).Returns(jobs[0]);
            this.startupJobConfiguration = new DefaultStartupJobEnvironmentConfiguration();
            var startupJobRunner = new StartupJobRunner(
                this.startupJobRepository.Object,
                this.backgroundJobClient.Object,
                this.logger.Object,
                this.startupJobConfiguration,
                this.mockErrorNotificationService.Object,
                this.mockRecurringJobManager.Object,
                this.startupJobRegistry);

            // Act
            await startupJobRunner.RunJobs();

            // Assert
            this.backgroundJobClient.Verify(
               x => x.Create(
                    It.Is<Job>(job => job.Method.Name == this.keyExecuteMethodByName
                        && job.Method.GetParameters()[0].Name == this.keyMethodName),
                    It.IsAny<IState>()), Times.Once); // called once
        }

        [Fact]
        public async Task RunJobs_ShouldNotRunInBackgroundIfBlocking()
        {
            // arrange
            List<StartupJob> jobs = new List<StartupJob>
            {
                new StartupJob("RegeneratePersonReadModels", true),
            };

            this.startupJobRepository.Setup(s => s.GetIncompleteStartupJobs()).Returns(jobs);
            this.startupJobRepository.Setup(s => s.GetStartupJobByAlias(It.IsAny<string>())).Returns(jobs[0]);
            this.startupJobRepository.Setup(s => s.CompleteJobByAlias("RegeneratePersonReadModels")).Verifiable();
            this.startupJobConfiguration = new DefaultStartupJobEnvironmentConfiguration();
            var startupJobRunner = new StartupJobRunner(
                this.startupJobRepository.Object,
                this.backgroundJobClient.Object,
                this.logger.Object,
                this.startupJobConfiguration,
                this.mockErrorNotificationService.Object,
                this.mockRecurringJobManager.Object,
                this.startupJobRegistry);

            // Act
            await startupJobRunner.RunJobs();

            // verify
            this.backgroundJobClient.Verify(
               x => x.Create(
                    It.Is<Job>(job => job.Method.Name == this.keyExecuteMethodByName
                        && job.Method.GetParameters()[0].Name == this.keyMethodName),
                    It.IsAny<IState>()), Times.Never); // never called
        }

        [Fact]
        public async Task RunJobs_ShouldNotReturnAnErrorException_WhenValidJobAliasAsync()
        {
            // Arrange
            List<StartupJob> jobs = new List<StartupJob>
            {
                new StartupJob("MyJobAlias", true),
            };

            this.startupJobRepository.Setup(s => s.GetIncompleteStartupJobs()).Returns(jobs);
            this.startupJobRepository.Setup(s => s.GetStartupJobByAlias(It.IsAny<string>())).Returns(jobs[0]);
            this.startupJobConfiguration = new DefaultStartupJobEnvironmentConfiguration();
            StartupJobRunner startupJobRunner = new StartupJobRunner(
                this.startupJobRepository.Object,
                this.backgroundJobClient.Object,
                this.logger.Object,
                this.startupJobConfiguration,
                this.mockErrorNotificationService.Object,
                this.mockRecurringJobManager.Object,
                this.startupJobRegistry);

            // Act
            Func<Task> func = async () => await startupJobRunner.RunJobs();

            // Assert
            await func.Should().NotThrowAsync<ErrorException>();
        }

        [Fact]
        public async Task RunJobs_ShouldNotRunIfInMultiNodeIsTrueAndRunManuallyInMultiNodeIsTrue()
        {
            // Arrange
            List<StartupJob> jobs = new List<StartupJob>
            {
                new StartupJob("RegeneratePersonReadModels", false, true),
            };

            this.startupJobRepository.Setup(s => s.GetIncompleteStartupJobs()).Returns(jobs);
            this.startupJobRepository.Setup(s => s.GetStartupJobByAlias(It.IsAny<string>())).Returns(jobs[0]);
            var startupJobConfigTest = new DefaultStartupJobEnvironmentConfiguration();
            startupJobConfigTest.ToggleMultiNodeEnvironment();
            var startupJobRunner = new StartupJobRunner(
                this.startupJobRepository.Object,
                this.backgroundJobClient.Object,
                this.logger.Object,
                startupJobConfigTest,
                this.mockErrorNotificationService.Object,
                this.mockRecurringJobManager.Object,
                this.startupJobRegistry);

            // Act
            await startupJobRunner.RunJobs();

            // verify
            // Note :  Extension methods (here: BackgroundJobClientExtensions.Enqueue) may not be used in setup / verification expressions.
            // used instead the Create method which is being called after Enqueue
            this.backgroundJobClient.Verify(
               x => x.Create(
                    It.Is<Job>(job => job.Method.Name == this.keyExecuteMethodByName
                        && job.Method.GetParameters()[0].Name == this.keyMethodName),
                    It.IsAny<IState>()), Times.Never);
        }

        [Fact]
        public async Task RunJobByAlias_ShouldNotRunIfPrecedingJobsHaveNotRunAsync()
        {
            // Arrange
            List<StartupJob> jobs = new List<StartupJob>
            {
                new StartupJob("ContextJob", true, true, new List<string> { "PrecedingJob1", "PrecedingJob2" }),
                new StartupJob("PrecedingJob1", true, true),
                new StartupJob("PrecedingJob2", true, true),
            };

            this.startupJobRepository.Setup(s => s.GetIncompleteStartupJobs()).Returns(jobs);
            this.startupJobRepository.Setup(s => s.GetStartupJobByAlias(It.IsAny<string>())).Returns(jobs[0]);
            var startupJobConfigTest = new DefaultStartupJobEnvironmentConfiguration();
            var startupJobRunner = new StartupJobRunner(
                this.startupJobRepository.Object,
                this.backgroundJobClient.Object,
                this.logger.Object,
                startupJobConfigTest,
                this.mockErrorNotificationService.Object,
                this.mockRecurringJobManager.Object,
                this.startupJobRegistry);

            // Act
            Func<Task> func = async () => await startupJobRunner.RunJobByAlias("ContextJob");

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("preceding.startup.jobs.not.completed");
        }

        [Fact]
        public async Task RunJobs_ShouldSkipJobsWhenPrecedingJobsHaveNotBeenCompleted()
        {
            // Arrange
            List<StartupJob> jobs = new List<StartupJob>
            {
                new StartupJob("ContextJob", true, false, new List<string> { "PrecedingJob1", "PrecedingJob2" }),
                new StartupJob("PrecedingJob2", true, false),
            };

            this.startupJobRepository.Setup(s => s.GetIncompleteStartupJobs()).Returns(jobs);
            this.startupJobRepository.Setup(s => s.GetStartupJobByAlias("ContextJob")).Returns(jobs[0]);
            this.startupJobRepository.Setup(s => s.GetStartupJobByAlias("PrecedingJob1"))
                .Returns(new StartupJob("PrecedingJob1", false, true));
            this.startupJobRepository.Setup(s => s.GetStartupJobByAlias("PrecedingJob2")).Returns(jobs[1]);
            this.startupJobRepository.Setup(s => s.CompleteJobByAlias(It.IsAny<string>())).Callback((string alias) =>
            {
                var job = jobs.Single(j => j.Alias == alias);
                job.CompleteJob();
            });
            var startupJobConfigTest = new DefaultStartupJobEnvironmentConfiguration();
            var startupJobRunner = new StartupJobRunner(
                this.startupJobRepository.Object,
                this.backgroundJobClient.Object,
                this.logger.Object,
                startupJobConfigTest,
                this.mockErrorNotificationService.Object,
                this.mockRecurringJobManager.Object,
                this.startupJobRegistry);

            // Act
            await startupJobRunner.RunJobs();

            // Assert
            jobs[0].Complete.Should().BeFalse();
            jobs[1].Complete.Should().BeTrue();
        }

        [Fact]
        public async Task RunJobs_ShouldOrderJobsByPrecedingAndThenRunThem()
        {
            // Arrange
            List<StartupJob> jobs = new List<StartupJob>
            {
                new StartupJob("ContextJob", true, false, new List<string> { "PrecedingJob1" }),
                new StartupJob("PrecedingJob1", true, false, new List<string> { "PrecedingJob2" }),
                new StartupJob("PrecedingJob2", true, false),
            };

            this.startupJobRepository.Setup(s => s.GetIncompleteStartupJobs()).Returns(jobs);
            this.startupJobRepository.Setup(s => s.GetStartupJobByAlias("ContextJob")).Returns(jobs[0]);
            this.startupJobRepository.Setup(s => s.GetStartupJobByAlias("PrecedingJob1")).Returns(jobs[1]);
            this.startupJobRepository.Setup(s => s.GetStartupJobByAlias("PrecedingJob2")).Returns(jobs[2]);
            this.startupJobRepository.Setup(s => s.CompleteJobByAlias(It.IsAny<string>())).Callback((string alias) =>
            {
                var job = jobs.Single(j => j.Alias == alias);
                job.CompleteJob();
            });
            var startupJobConfigTest = new DefaultStartupJobEnvironmentConfiguration();
            var startupJobRunner = new StartupJobRunner(
                this.startupJobRepository.Object,
                this.backgroundJobClient.Object,
                this.logger.Object,
                startupJobConfigTest,
                this.mockErrorNotificationService.Object,
                this.mockRecurringJobManager.Object,
                this.startupJobRegistry);

            // Act
            await startupJobRunner.RunJobs();

            // Assert
            // Assert
            jobs[2].Complete.Should().BeTrue();
            jobs[1].Complete.Should().BeTrue();
            jobs[0].Complete.Should().BeTrue();
        }

        [Fact]
        public async Task ExecuteBlockingStartupJobAndMarkCompleted_ShouldNotThrowException_WhenMethodNotFoundAsync()
        {
            // Arrange
            var startupJobRepository = new Mock<IStartupJobRepository>();
            startupJobRepository.Setup(s => s.CompleteJobByAlias(It.IsAny<string>())).Verifiable();
            var startupJobConfigTest = new DefaultStartupJobEnvironmentConfiguration();
            var startupJobRunner = new StartupJobRunner(
                startupJobRepository.Object,
                this.backgroundJobClient.Object,
                this.logger.Object,
                startupJobConfigTest,
                this.mockErrorNotificationService.Object,
                this.mockRecurringJobManager.Object,
                this.startupJobRegistry);

            // Act
            Func<Task> func = async () => await startupJobRunner.ExecuteBlockingStartupJobAndMarkCompleted("UnknownJobThatDefinitelyDoesNotExist");

            // Assert
            await func.Should().NotThrowAsync<ErrorException>();
            startupJobRepository.Verify(s => s.CompleteJobByAlias(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteStartupJobAndMarkCompleted_ShouldThrowException_WhenMethodNotFoundAsync()
        {
            // Arrange
            var startupJobRepository = new Mock<IStartupJobRepository>();
            startupJobRepository.Setup(s => s.CompleteJobByAlias(It.IsAny<string>())).Verifiable();
            var startupJobConfigTest = new DefaultStartupJobEnvironmentConfiguration();
            var startupJobRunner = new StartupJobRunner(
                startupJobRepository.Object,
                this.backgroundJobClient.Object,
                this.logger.Object,
                startupJobConfigTest,
                this.mockErrorNotificationService.Object,
                this.mockRecurringJobManager.Object,
                this.startupJobRegistry);

            // Act
            Func<Task> func = async () => await startupJobRunner.ExecuteStartupJobAndMarkCompleted("UnknownJobThatDefinitelyDoesNotExist", CancellationToken.None);

            // Assert
            (await func.Should().ThrowAsync<ErrorException>()).Which.Error.Code.Should().Be("startupjob.method.name.not.found");
            startupJobRepository.Verify(s => s.CompleteJobByAlias(It.IsAny<string>()), Times.Never);
        }
    }
}
