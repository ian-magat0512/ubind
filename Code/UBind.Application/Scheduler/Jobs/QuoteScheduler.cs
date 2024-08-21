// <copyright file="QuoteScheduler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Scheduler.Jobs
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Hangfire;
    using Hangfire.Storage;
    using Microsoft.Extensions.Logging;
    using UBind.Application.Commands.Quote;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Attributes;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class QuoteScheduler : BaseEntityScheduler, IEntityScheduler<Quote>
    {
        private readonly ILogger<QuoteScheduler> logger;

        public QuoteScheduler(
            ITenantRepository tenantRepository,
            IStorageConnection storageConnection,
            IRecurringJobManager recurringJobManager,
            ICqrsMediator mediator,
            ILogger<QuoteScheduler> logger)
            : base(tenantRepository, storageConnection, recurringJobManager, mediator)
        {
            this.logger = logger;
        }

        public override void CreateStateChangeJob()
        {
            var tenants = this.RetrieveTenantIdsForUpdate();
            var command = new UpdateQuoteStateWhenExpiredCommand(tenants);
            this.RecurringJobManager.AddOrUpdate<QuoteScheduler>(
                        this.GetRecurringJobId(),
                        (c) => this.ExecuteQuoteStateChangeCommand(command, CancellationToken.None),
                        "*/5 * * * *");
        }

        [JobDisplayName("Quote State Updater Job")]
        [RequestIntent(RequestIntent.ReadWrite)]
        public async Task ExecuteQuoteStateChangeCommand(
            UpdateQuoteStateWhenExpiredCommand command,
            CancellationToken cancellationToken)
        {
            try
            {
                // this lock stops 2 quote state updater jobs from running at the same time
                using (JobStorage.Current.GetConnection().AcquireDistributedLock(
                    "Quote State Updater Job", TimeSpan.FromSeconds(1)))
                {
                    await this.Mediator.Send(command, cancellationToken);
                }
            }
            catch (DistributedLockTimeoutException)
            {
                // A job is already running and hasn't completed within the 5 minute lock timeout.
                // In this case, we just catch the exception and return.
                this.logger.LogInformation($"The previous Quote State Updater Job is still running, so not executing another.");
            }
        }

        public override string GetRecurringJobId()
        {
            return $"quote-state-scheduler";
        }
    }
}
