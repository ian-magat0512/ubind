// <copyright file="GlassGuideUpdaterSchedulerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Scheduler;

using Hangfire;
using Hangfire.Common;
using Microsoft.Extensions.Logging;
using Moq;
using UBind.Application.Scheduler.Jobs;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;
using Xunit;

public class GlassGuideUpdaterSchedulerTests
{
    private readonly Mock<ICqrsMediator> mediator;
    private readonly Mock<ILogger<GlassGuideUpdaterScheduler>> logger;
    private readonly Mock<IRecurringJobManager> recurringJobManager;
    private readonly Mock<ITenantRepository> tenantRepository;

    public GlassGuideUpdaterSchedulerTests()
    {
        this.mediator = new Mock<ICqrsMediator>();
        this.recurringJobManager = new Mock<IRecurringJobManager>();
        this.logger = new Mock<ILogger<GlassGuideUpdaterScheduler>>();
        this.tenantRepository = new Mock<ITenantRepository>();
    }

    [Fact]
    public void RegisterStateChangeJob_ShouldRegisterRecurringJob()
    {
        // Assert
        var glassGuideScheduler = new GlassGuideUpdaterScheduler(
            this.tenantRepository.Object,
            null,
            this.recurringJobManager.Object,
            this.logger.Object,
            this.mediator.Object);

        // Act
        glassGuideScheduler.RegisterStateChangeJob();

        // Assert
        this.recurringJobManager.Verify(
            c => c.AddOrUpdate(
                glassGuideScheduler.GetRecurringJobId(),
                It.IsAny<Job>(),
                GlassGuideUpdaterScheduler.GlassGuideUpdaterCronSchedule,
                It.IsAny<RecurringJobOptions>()));
    }
}
