// <copyright file="RedbookUpdaterSchedulerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Scheduler
{
    using Hangfire;
    using Hangfire.Common;
    using Microsoft.Extensions.Logging;
    using Moq;
    using UBind.Application.Scheduler.Jobs;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;
    using Xunit;

    public class RedbookUpdaterSchedulerTests
    {
        private readonly Mock<ICqrsMediator> mediator;
        private readonly Mock<ILogger<RedbookUpdaterScheduler>> logger;
        private readonly Mock<IRecurringJobManager> recurringJobManager;
        private readonly Mock<ITenantRepository> tenantRepository;
        private readonly Mock<Hangfire.JobStorage> jobStorage;

        public RedbookUpdaterSchedulerTests()
        {
            this.mediator = new Mock<ICqrsMediator>();
            this.recurringJobManager = new Mock<IRecurringJobManager>();
            this.logger = new Mock<ILogger<RedbookUpdaterScheduler>>();
            this.tenantRepository = new Mock<ITenantRepository>();
            this.jobStorage = new Mock<Hangfire.JobStorage>();
        }

        [Fact]
        public void RegisterStateChangeJob_ShouldRegisterRecurringJob()
        {
            // Assert
            var redbookScheduler = new RedbookUpdaterScheduler(
                this.tenantRepository.Object,
                null,
                this.recurringJobManager.Object,
                this.logger.Object,
                this.mediator.Object);

            // Act
            redbookScheduler.RegisterStateChangeJob();

            // Assert
            this.recurringJobManager.Verify(
                c => c.AddOrUpdate(
                    redbookScheduler.GetRecurringJobId(),
                    It.IsAny<Job>(),
                    RedbookUpdaterScheduler.RedbookUpdaterCronSchedule,
                    It.IsAny<RecurringJobOptions>()));
        }
    }
}
