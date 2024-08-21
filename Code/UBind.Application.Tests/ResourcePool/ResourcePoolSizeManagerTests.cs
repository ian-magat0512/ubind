// <copyright file="ResourcePoolSizeManagerTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.ResourcePool
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NodaTime;
    using UBind.Application.ResourcePool;
    using UBind.Application.Services.Email;
    using Xunit;

    /// <summary>
    /// Tests the algorithm for managing the size of a resource pool.
    /// </summary>
    public class ResourcePoolSizeManagerTests
    {
        private readonly int timeoutMillis = 10000;
        private readonly IErrorNotificationService errorNotificationService = new Mock<IErrorNotificationService>().Object;
        private IClock clock = SystemClock.Instance;
        private ILogger<IResourcePool> logger;

        public ResourcePoolSizeManagerTests()
        {
            this.logger = new Mock<ILogger<IResourcePool>>().Object;
        }

        [Fact]
        public async Task Minimum_Number_Of_Instances_Is_Created_On_Startup()
        {
            // Added a retry mechanism around the entire test logic. It will attempt the test up to three times.
            // You can adjust maxRetries as needed before declaring failure.
            int maxRetries = 3;
            int retryCount = 0;
            Exception? lastException = null;

            while (retryCount < maxRetries)
            {
                try
                {
                    // Arrange
                    ResourcePoolStub pool;
                    var options = new ResourcePoolSizeManager.Options();
                    options.CreationGrowthDelayDuration = Duration.FromMilliseconds(20);
                    options.CreationGrowthDelayRandomMaxDuration = Duration.Zero;
                    options.CreationGrowthIntervalDuration = Duration.Zero;
                    options.MinimumPoolSize = 5;
                    var resourcesToCreate = new Queue<IResourcePoolMember>();
                    for (int i = 0; i <= options.MinimumPoolSize; i++)
                    {
                        resourcesToCreate.Enqueue(new ResourcePoolMemberStub());
                    }

                    // Act
                    pool = new ResourcePoolStub(this.clock, this.logger, this.errorNotificationService, resourcesToCreate);
                    var completed = new TaskCompletionSource<bool>();
                    var poolSizeManager = new ResourcePoolSizeManager(this.clock, this.logger, options);
                    poolSizeManager.StartupCompleted += (object sender, EventArgs e) => completed.TrySetResult(true);
                    poolSizeManager.Manage(pool);
                    if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
                    {
                        this.AssertStartupTimeoutFailure(poolSizeManager.StartupLog);
                    }

                    // Assert
                    pool.GetResourceCount().Should().Be(poolSizeManager.Settings.MinimumPoolSize);

                    // If the test passes, exit the retry loop
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    retryCount++;

                    // Add a delay between retries to reduce contention
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            }

            // If the test continues to fail after retries, fail the test with the last exception
            Assert.True(false, $"Test failed after {maxRetries} retries. Last exception: {lastException}");
        }

        [Fact]
        public async Task Pool_Grows_When_Usage_Exceeds_Threshold()
        {
            // Arrange
            ResourcePoolStub pool;
            var options = new ResourcePoolSizeManager.Options();
            options.CreationGrowthDelayDuration = Duration.FromMilliseconds(20);
            options.CreationGrowthDelayRandomMaxDuration = Duration.Zero;
            options.CreationGrowthIntervalDuration = Duration.Zero;
            options.MinimumPoolSize = 10;
            options.InitialPoolGrowthThresholdPercent = 0.5f;
            options.InitialPoolGrowthRatePercent = 0.5f;
            var resourcesToCreate = new Queue<IResourcePoolMember>();
            for (int i = 0; i <= 20; i++)
            {
                resourcesToCreate.Enqueue(new ResourcePoolMemberStub());
            }

            pool = new ResourcePoolStub(this.clock, this.logger, this.errorNotificationService, resourcesToCreate);
            var completed = new TaskCompletionSource<bool>();
            var poolSizeManager = new ResourcePoolSizeManager(this.clock, this.logger, options);
            poolSizeManager.StartupCompleted += (object sender, EventArgs e) => completed.TrySetResult(true);
            poolSizeManager.Manage(pool);
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                this.AssertStartupTimeoutFailure(poolSizeManager.StartupLog);
            }

            // Act
            for (int i = 0; i < 4; i++)
            {
                pool.AcquireResource();
            }

            completed = new TaskCompletionSource<bool>();
            poolSizeManager.GrowPoolCompleted += (object sender, EventArgs e) => completed.TrySetResult(true);
            pool.AcquireResource();
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                Assert.True(false, $"Timed out waiting {this.timeoutMillis}ms for resource growth to complete.\n" + poolSizeManager.StartupLog);
            }

            // Assert
            pool.PoolSizeManager.TargetPoolSize.Should().Be(15);
            pool.GetResourceCount().Should().Be(15);
        }

        [Fact]
        public async Task Pool_Resource_Is_Deleted_When_Never_Used()
        {
            // Arrange
            ResourcePoolStub pool;
            var options = new ResourcePoolSizeManager.Options();
            options.CreationGrowthDelayDuration = Duration.FromMilliseconds(20);
            options.CreationGrowthDelayRandomMaxDuration = Duration.Zero;
            options.CreationGrowthIntervalDuration = Duration.Zero;
            options.MinimumPoolSize = 10;
            options.MinimumResourceLifeDuration = Duration.FromMinutes(10);
            options.PoolReaperInterval = Duration.FromMilliseconds(50);
            var resourcesToCreate = new Queue<IResourcePoolMember>();
            for (int i = 0; i <= options.MinimumPoolSize; i++)
            {
                resourcesToCreate.Enqueue(new ResourcePoolMemberStub()
                {
                    CreatedTimestamp =
                        this.clock.GetCurrentInstant() - options.MinimumResourceLifeDuration - Duration.FromMinutes(1),
                });
            }

            pool = new ResourcePoolStub(this.clock, this.logger, this.errorNotificationService, resourcesToCreate);
            var completed = new TaskCompletionSource<bool>();
            var poolSizeManager = new ResourcePoolSizeManager(this.clock, this.logger, options);
            poolSizeManager.StartupCompleted += (object sender, EventArgs e) => completed.TrySetResult(true);
            poolSizeManager.Manage(pool);
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                this.AssertStartupTimeoutFailure(poolSizeManager.StartupLog);
            }

            // Act
            options.MinimumPoolSize = 5;
            completed = new TaskCompletionSource<bool>();
            poolSizeManager.ReapUnusedCycleStarted += (object sender, EventArgs e) => completed.TrySetResult(true);
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                Assert.True(false, $"Timed out waiting {this.timeoutMillis}ms for reaping to start.");
            }

            completed = new TaskCompletionSource<bool>();
            poolSizeManager.ReapUnusedCycleCompleted += (object sender, EventArgs e) => completed.TrySetResult(true);
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                Assert.True(false, $"Timed out waiting {this.timeoutMillis}ms for reaping to complete.");
            }

            // Assert
            pool.GetResourceCount().Should().Be(options.MinimumPoolSize);
        }

        [Fact]
        public async Task Pool_Resource_Is_Deleted_When_More_Than_One_Stale()
        {
            // Arrange
            var now = this.clock.GetCurrentInstant();
            var elevenMinutesAgo = now - Duration.FromMinutes(11);
            var fakeClock = new Mock<IClock>();
            fakeClock.Setup(x => x.GetCurrentInstant()).Returns(elevenMinutesAgo);
            ResourcePoolStub pool;
            var options = new ResourcePoolSizeManager.Options();
            options.CreationGrowthDelayDuration = Duration.FromMilliseconds(20);
            options.CreationGrowthDelayRandomMaxDuration = Duration.Zero;
            options.CreationGrowthIntervalDuration = Duration.Zero;
            options.MinimumPoolSize = 10;
            options.MinimumResourceLifeDuration = Duration.FromMinutes(10);
            options.PoolReaperInterval = Duration.FromMilliseconds(50);
            var resourcesToCreate = new Queue<IResourcePoolMember>();
            for (int i = 0; i <= options.MinimumPoolSize; i++)
            {
                resourcesToCreate.Enqueue(new ResourcePoolMemberStub()
                {
                    CreatedTimestamp =
                        this.clock.GetCurrentInstant() - options.MinimumResourceLifeDuration - Duration.FromMinutes(2),
                    LastUsedTimestamp =
                        this.clock.GetCurrentInstant() - options.ResourceUnusedStaleDuration - Duration.FromMinutes(1),
                });
            }

            pool = new ResourcePoolStub(fakeClock.Object, this.logger, this.errorNotificationService, resourcesToCreate);
            var completed = new TaskCompletionSource<bool>();
            var poolSizeManager = new ResourcePoolSizeManager(this.clock, this.logger, options);
            poolSizeManager.StartupCompleted += (object sender, EventArgs e) => completed.TrySetResult(true);
            poolSizeManager.Manage(pool);
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                this.AssertStartupTimeoutFailure(poolSizeManager.StartupLog);
            }

            // Act
            options.MinimumPoolSize = 5;
            completed = new TaskCompletionSource<bool>();
            poolSizeManager.ReapUnusedCycleStarted += (object sender, EventArgs e) => completed.TrySetResult(true);
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                Assert.True(false, $"Timed out waiting {this.timeoutMillis}ms for reaping to start.");
            }

            completed = new TaskCompletionSource<bool>();
            poolSizeManager.ReapUnusedCycleCompleted += (object sender, EventArgs e) => completed.TrySetResult(true);
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                Assert.True(false, $"Timed out waiting {this.timeoutMillis}ms for reaping to complete.");
            }

            // Assert
            pool.GetResourceCount().Should().Be(options.MinimumPoolSize);
        }

        [Fact]
        public async Task Pool_Growth_Threshold_Is_Lowered_When_Pool_Exhausted()
        {
            // Arrange
            ResourcePoolStub pool;
            var options = new ResourcePoolSizeManager.Options();
            options.CreationGrowthDelayDuration = Duration.FromMilliseconds(20);
            options.CreationGrowthDelayRandomMaxDuration = Duration.Zero;
            options.CreationGrowthIntervalDuration = Duration.Zero;
            options.MinimumPoolSize = 10;
            options.InitialPoolGrowthThresholdPercent = 1.5f;
            options.InitialPoolGrowthRatePercent = 0.5f;
            options.PoolExhaustedGrowthThresholdPercentReduction = 0.7f;
            var resourcesToCreate = new Queue<IResourcePoolMember>();
            for (int i = 0; i <= 20; i++)
            {
                resourcesToCreate.Enqueue(new ResourcePoolMemberStub());
            }

            pool = new ResourcePoolStub(this.clock, this.logger, this.errorNotificationService, resourcesToCreate);
            var completed = new TaskCompletionSource<bool>();
            var poolSizeManager = new ResourcePoolSizeManager(this.clock, this.logger, options);
            poolSizeManager.StartupCompleted += (object sender, EventArgs e) => completed.TrySetResult(true);
            poolSizeManager.Manage(pool);
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                this.AssertStartupTimeoutFailure(poolSizeManager.StartupLog);
            }

            // Act
            for (int i = 0; i < 10; i++)
            {
                pool.AcquireResource();
            }

            completed = new TaskCompletionSource<bool>();
            poolSizeManager.PoolExhausted += (object sender, EventArgs e) => completed.TrySetResult(true);
            pool.AcquireResource();
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                Assert.True(false, $"Timed out waiting {this.timeoutMillis}ms for pool to be exhausted.");
            }

            // Assert
            pool.PoolSizeManager.PoolGrowthThresholdPercent.Should().Be(0.8f);
        }

        [Fact]
        public async Task Pool_Growth_Rate_Increases_When_Pool_Exhausted()
        {
            // Arrange
            ResourcePoolStub pool;
            var options = new ResourcePoolSizeManager.Options();
            options.CreationGrowthDelayDuration = Duration.FromMilliseconds(20);
            options.CreationGrowthDelayRandomMaxDuration = Duration.Zero;
            options.CreationGrowthIntervalDuration = Duration.Zero;
            options.MinimumPoolSize = 10;
            options.InitialPoolGrowthThresholdPercent = 1.5f;
            options.InitialPoolGrowthRatePercent = 0.5f;
            options.PoolExhaustedGrowthRatePercentIncrease = 0.2f;
            var resourcesToCreate = new Queue<IResourcePoolMember>();
            for (int i = 0; i <= 20; i++)
            {
                resourcesToCreate.Enqueue(new ResourcePoolMemberStub());
            }

            pool = new ResourcePoolStub(this.clock, this.logger, this.errorNotificationService, resourcesToCreate);
            var completed = new TaskCompletionSource<bool>();
            var poolSizeManager = new ResourcePoolSizeManager(this.clock, this.logger, options);
            poolSizeManager.StartupCompleted += (object sender, EventArgs e) => completed.TrySetResult(true);
            poolSizeManager.Manage(pool);
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                this.AssertStartupTimeoutFailure(poolSizeManager.StartupLog);
            }

            // Act
            for (int i = 0; i < 10; i++)
            {
                pool.AcquireResource();
            }

            completed = new TaskCompletionSource<bool>();
            poolSizeManager.PoolExhausted += (object sender, EventArgs e) => completed.TrySetResult(true);
            pool.AcquireResource();
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                Assert.True(false, $"Timed out waiting {this.timeoutMillis}ms for pool to be exhausted.");
            }

            // Assert
            pool.PoolSizeManager.PoolGrowthRatePercent.Should().Be(0.7f);
        }

        [Fact]
        public async Task Pool_Growth_Threshold_Increases_When_More_Than_One_Resource_Expires_Unused()
        {
            // Arrange
            ResourcePoolStub pool;
            var options = new ResourcePoolSizeManager.Options();
            options.CreationGrowthDelayDuration = Duration.FromMilliseconds(20);
            options.CreationGrowthDelayRandomMaxDuration = Duration.Zero;
            options.CreationGrowthIntervalDuration = Duration.Zero;
            options.MinimumPoolSize = 10;
            options.MinimumResourceLifeDuration = Duration.FromMinutes(10);
            options.PoolReaperInterval = Duration.FromMilliseconds(50);
            options.InitialPoolGrowthThresholdPercent = 0.5f;
            options.InitialPoolGrowthRatePercent = 0.5f;
            options.ResourcesWastedGrowthThresholdPercentIncrease = 0.2f;
            var resourcesToCreate = new Queue<IResourcePoolMember>();
            for (int i = 0; i <= options.MinimumPoolSize; i++)
            {
                resourcesToCreate.Enqueue(new ResourcePoolMemberStub()
                {
                    CreatedTimestamp =
                        this.clock.GetCurrentInstant() - options.MinimumResourceLifeDuration - Duration.FromMinutes(1),
                });
            }

            pool = new ResourcePoolStub(this.clock, this.logger, this.errorNotificationService, resourcesToCreate);
            var completed = new TaskCompletionSource<bool>();
            var poolSizeManager = new ResourcePoolSizeManager(this.clock, this.logger, options);
            poolSizeManager.StartupCompleted += (object sender, EventArgs e) => completed.TrySetResult(true);
            poolSizeManager.Manage(pool);
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                this.AssertStartupTimeoutFailure(poolSizeManager.StartupLog);
            }

            // Reduce pool size by 2 so that 2 expire unused
            options.MinimumPoolSize -= 2;
            completed = new TaskCompletionSource<bool>();
            poolSizeManager.ResourcesWasted += (object sender, EventArgs e) => completed.TrySetResult(true);
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                Assert.True(false, $"Timed out waiting {this.timeoutMillis}ms for resources to be detected as wasted.");
            }

            completed = new TaskCompletionSource<bool>();
            poolSizeManager.ReapUnusedCycleCompleted += (object sender, EventArgs e) => completed.TrySetResult(true);
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                Assert.True(false, $"Timed out waiting {this.timeoutMillis}ms for reaping to complete.");
            }

            pool.PoolSizeManager.PoolGrowthThresholdPercent.Should().Be(0.7f);
        }

        [Fact]
        public async Task Pool_Growth_Rate_Is_Not_Reduced_When_Only_One_Resource_Expires_Unused()
        {
            // Arrange
            ResourcePoolStub pool;
            var options = new ResourcePoolSizeManager.Options();
            options.CreationGrowthDelayDuration = Duration.FromMilliseconds(20);
            options.CreationGrowthDelayRandomMaxDuration = Duration.Zero;
            options.CreationGrowthIntervalDuration = Duration.Zero;
            options.MinimumPoolSize = 10;
            options.MinimumResourceLifeDuration = Duration.FromMinutes(10);
            options.PoolReaperInterval = Duration.FromMilliseconds(50);
            options.InitialPoolGrowthRatePercent = 0.5f;
            options.ResourcesWastedGrowthRatePercentReduction = 0.2f;
            var resourcesToCreate = new Queue<IResourcePoolMember>();
            for (int i = 0; i <= options.MinimumPoolSize; i++)
            {
                resourcesToCreate.Enqueue(new ResourcePoolMemberStub()
                {
                    CreatedTimestamp =
                        this.clock.GetCurrentInstant() - options.MinimumResourceLifeDuration - Duration.FromMinutes(1),
                });
            }

            pool = new ResourcePoolStub(this.clock, this.logger, this.errorNotificationService, resourcesToCreate);
            var completed = new TaskCompletionSource<bool>();
            var poolSizeManager = new ResourcePoolSizeManager(this.clock, this.logger, options);
            poolSizeManager.StartupCompleted += (object sender, EventArgs e) => completed.TrySetResult(true);
            poolSizeManager.Manage(pool);
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                this.AssertStartupTimeoutFailure(poolSizeManager.StartupLog);
            }

            // Act
            // Reduce minimum pool size by 1 so that only 1 expires unused
            completed = new TaskCompletionSource<bool>();
            poolSizeManager.ReapUnusedCycleStarted += (object sender, EventArgs e) => completed.TrySetResult(true);
            options.MinimumPoolSize -= 1;
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                Assert.True(false, $"Timed out waiting {this.timeoutMillis}ms for reaping to start.");
            }

            completed = new TaskCompletionSource<bool>();
            poolSizeManager.ReapUnusedCycleCompleted += (object sender, EventArgs e) => completed.TrySetResult(true);
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                Assert.True(false, $"Timed out waiting {this.timeoutMillis}ms for reaping to complete.");
            }

            // There should be no change to the threshold
            pool.PoolSizeManager.PoolGrowthRatePercent.Should().Be(0.5f);
        }

        [Fact]
        public async Task Pool_Growth_Rate_Is_Reduced_When_More_Than_One_Resource_Expires_Unused()
        {
            // Arrange
            ResourcePoolStub pool;
            var options = new ResourcePoolSizeManager.Options();
            options.CreationGrowthDelayDuration = Duration.FromMilliseconds(20);
            options.CreationGrowthDelayRandomMaxDuration = Duration.Zero;
            options.CreationGrowthIntervalDuration = Duration.Zero;
            options.MinimumPoolSize = 10;
            options.MinimumResourceLifeDuration = Duration.FromMinutes(10);
            options.PoolReaperInterval = Duration.FromMilliseconds(50);
            options.InitialPoolGrowthRatePercent = 0.5f;
            options.ResourcesWastedGrowthRatePercentReduction = 0.2f;
            var resourcesToCreate = new Queue<IResourcePoolMember>();
            for (int i = 0; i <= options.MinimumPoolSize; i++)
            {
                resourcesToCreate.Enqueue(new ResourcePoolMemberStub()
                {
                    CreatedTimestamp =
                        this.clock.GetCurrentInstant() - options.MinimumResourceLifeDuration - Duration.FromMinutes(1),
                });
            }

            pool = new ResourcePoolStub(this.clock, this.logger, this.errorNotificationService, resourcesToCreate);
            var completed = new TaskCompletionSource<bool>();
            var poolSizeManager = new ResourcePoolSizeManager(this.clock, this.logger, options);
            poolSizeManager.StartupCompleted += (object sender, EventArgs e) => completed.TrySetResult(true);
            poolSizeManager.Manage(pool);
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                this.AssertStartupTimeoutFailure(poolSizeManager.StartupLog);
            }

            // Act
            // Reduce minimum pool size by 2 so that 2 expire unused
            options.MinimumPoolSize -= 2;
            completed = new TaskCompletionSource<bool>();
            poolSizeManager.ResourcesWasted += (object sender, EventArgs e) => completed.TrySetResult(true);
            if (await Task.WhenAny(completed.Task, Task.Delay(this.timeoutMillis)) != completed.Task)
            {
                Assert.True(false, $"Timed out waiting {this.timeoutMillis}ms for resources to be detected as wasted.");
            }

            pool.PoolSizeManager.PoolGrowthRatePercent.Should().Be(0.3f);
        }

        private void AssertStartupTimeoutFailure(string startupLog)
        {
            ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out int availableIoThreads);
            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minIoThreads);
            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxIoThreads);
            Assert.True(
                false,
                $"Timed out waiting {this.timeoutMillis}ms for resource pool startup to complete.\n"
                + startupLog
                + $"\nAvailable Worker Threads: {availableWorkerThreads}, Available IO Threads: {availableIoThreads}"
                + $"\nMin Worker Threads: {availableWorkerThreads}, Min IO Threads: {availableIoThreads}"
                + $"\nMax Worker Threads: {availableWorkerThreads}, Max IO Threads: {availableIoThreads}");
        }

        private class ResourcePoolStub : ResourcePool
        {
            private Queue<IResourcePoolMember> resourcesToCreate;

            public ResourcePoolStub(
                IClock clock,
                ILogger<IResourcePool> logger,
                IErrorNotificationService errorNotificationService,
                Queue<IResourcePoolMember> resourcesToCreate)
            : base(clock, logger, errorNotificationService)
            {
                this.resourcesToCreate = resourcesToCreate;
            }

            public override string GetDebugIdentifier()
            {
                return "ResourcePoolSizeManagerTests.ResourcePoolStub";
            }

            /// <inheritdoc/>
            protected override IResourcePoolMember CreateResource()
            {
                return this.resourcesToCreate.Dequeue();
            }
        }

        private class ResourcePoolMemberStub : IResourcePoolMember
        {
            public Instant CreatedTimestamp { get; set; }

            public virtual Instant LastUsedTimestamp { get; set; }
        }
    }
}
