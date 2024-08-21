// <copyright file="QuoteDeletionManager.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Reduction
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Text;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain.Loggers;
    using UBind.Domain.Reduction;
    using UBind.Domain.Repositories;

    /// <inheritdoc />
    public class QuoteDeletionManager : IQuoteDeletionManager
    {
        private readonly string connectionStringName;
        private readonly int connectionTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuoteDeletionManager"/> class.
        /// </summary>
        /// <param name="connectionStringName">A connection string for the database where quotes are persisted.</param>
        public QuoteDeletionManager(string connectionStringName)
        {
            this.connectionStringName = connectionStringName;
            this.connectionTimeout = 510;
        }

        /// <inheritdoc/>
        public void DeleteNascent(IProgressLogger logger, int size, int limit, Duration duration)
        {
            logger.Log(LogLevel.Information, "Starting to execute quote deletion by batch size.");
            this.Execute(logger, QuoteReductionType.Nascent, size, limit, duration: duration);
            logger.Log(LogLevel.Information, $"Finished deleting quotes.");
        }

        private void Execute(
            IProgressLogger logger, QuoteReductionType type, int size, int limit, Duration duration = default)
        {
            var deletedSoFar = 0;
            var batchSize = size;

            while (deletedSoFar < limit)
            {
                using (var dbContext = new UBindDbContext(this.connectionStringName, this.connectionTimeout))
                {
                    dbContext.Configuration.AutoDetectChangesEnabled = false;

                    if ((deletedSoFar + size) > limit)
                    {
                        batchSize = limit - deletedSoFar;
                    }

                    IEnumerable<QuoteDeletionDto> quotesToDelete = this.GetPolicyIdsToDelete(dbContext, type, batchSize, duration);
                    if (!quotesToDelete.Any())
                    {
                        logger.Log(LogLevel.Trace, "No more nascent policies to delete.");
                        return;
                    }

                    var aggregateIds = quotesToDelete.Select(c => c.AggregateId);
                    var quoteIds = quotesToDelete.Select(c => c.QuoteId);

                    logger.Log(LogLevel.Trace, $"Deleting the following Quote Ids -{string.Join(",", quoteIds)}");

                    // Strategies:
                    // Delete any existing views before creating a newer one.
                    // Recreate the views rather than altering to avoid errors in property changes in the future.
                    // Create and delete from views (saves transaction log space).
                    var views = new List<NascentDeletionView>
                    {
                        new NascentDeletionView("QuoteVersionReadModels", "AggregateId", aggregateIds),
                        new NascentDeletionView("QuoteDocumentReadModels", "PolicyId", aggregateIds),
                        new NascentDeletionView("Quotes", "[AggregateId]", aggregateIds),
                        new NascentDeletionView("TextAdditionalPropertyValueReadModels", "[EntityId]", quoteIds),
                        new NascentDeletionView("EventRecordWithGuidIds", "AggregateId", aggregateIds),
                    };
                    this.DropViewsIfExists(dbContext, views);
                    this.CreateViews(dbContext, views);
                    this.DeleteRecordsFromViews(dbContext, views);
                    this.DropViewsIfExists(dbContext, views);

                    deletedSoFar += aggregateIds.Count();
                    var progress = (double)deletedSoFar / limit * 100;
                    logger.UpdateProgress(Math.Min(progress, 100));
                    logger.Log(LogLevel.Trace, $"Number of quotes deleted: {deletedSoFar} / {limit}");
                }
            }
        }

        /// <summary>
        /// Gets the list of policy Ids to delete (nascent with no other records and should be 30 days old).
        /// </summary>
        /// <remarks>
        /// The strategy is to create a temporary table from the list of only nascent policies, and left join.
        /// </remarks>
        /// <param name="dbContext">The ubind database context to use.</param>
        /// <param name="type">The type of quote reduction to use.</param>
        /// <param name="size">The total size of policy Ids to retrieve.</param>
        /// <param name="duration">The minimum age of policy Ids to retrieve.</param>
        /// <returns>The enumerable of policy Ids in GUID format.</returns>
        private IEnumerable<QuoteDeletionDto> GetPolicyIdsToDelete(
                    IUBindDbContext dbContext, QuoteReductionType type, int size, Duration duration)
        {
            if (type == QuoteReductionType.Nascent)
            {
                var selectPolicyQuery = @"
                    WITH cte
                        AS (SELECT AggregateId FROM [dbo].[Quotes] WITH (NOLOCK)
                            GROUP BY AggregateId HAVING Count(AggregateId) > 1)
                    SELECT TOP (@size) Q.AggregateId, Q.Id As QuoteId FROM [dbo].[Quotes] AS Q WITH (NOLOCK)
                        LEFT JOIN cte
                            ON Q.AggregateId = cte.AggregateId
                        LEFT JOIN [dbo].[PolicyReadModels] PRM WITH (NOLOCK)
                            ON Q.PolicyId = PRM.Id
                        WHERE cte.AggregateId IS NULL
                            AND Q.AggregateId IS NOT NULL
                            AND Q.QuoteState = 'Nascent'
                            AND Q.Type = 0
                            AND PRM.Id IS NULL
                            AND Q.LastModifiedTicksSinceEpoch < (@beforeTicks);";
                var beforeTicks = SystemClock.Instance.GetCurrentInstant().Minus(duration).ToUnixTimeTicks();
                return dbContext.Database
                    .SqlQuery<QuoteDeletionDto>(
                        selectPolicyQuery,
                        new SqlParameter("@size", size),
                        new SqlParameter("@beforeTicks", beforeTicks))
                    .ToList();
            }

            throw new NotSupportedException();
        }

        private void CreateViews(IUBindDbContext dbContext, IEnumerable<NascentDeletionView> views)
        {
            foreach (NascentDeletionView view in views)
            {
                var query = string.Format(
                    @"CREATE VIEW [View{0}] AS(SELECT {1} FROM [dbo].[{0}] WHERE {1} IN ({2}));",
                    view.TableName,
                    view.PropertyToReturn,
                    string.Join(",", view.Data.Select(e => $"'{e}'").ToArray()).TrimEnd(new char[] { ',' }));
                dbContext.Database.ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction, query);
            }
        }

        private void DropViewsIfExists(IUBindDbContext dbContext, IEnumerable<NascentDeletionView> views)
        {
            StringBuilder sb = new StringBuilder();
            foreach (NascentDeletionView view in views)
            {
                string query = @"DROP VIEW IF EXISTS View{0}";
                sb.AppendLine(string.Format(query, view.TableName));
            }

            dbContext.Database.ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction, sb.ToString());
        }

        private void DeleteRecordsFromViews(IUBindDbContext dbContext, IEnumerable<NascentDeletionView> views)
        {
            dbContext.LoggingEnabled = false;
            foreach (NascentDeletionView view in views)
            {
                dbContext.Database.ExecuteSqlCommand(
                    TransactionalBehavior.DoNotEnsureTransaction, $"DELETE FROM View{view.TableName}");
            }
        }
    }
}
