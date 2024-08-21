// <copyright file="IRecreateReadModelsOfEventsMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.Migration
{
    using System.Threading.Tasks;
    using Hangfire.Server;

    public interface IRecreateReadModelsOfEventsMigration
    {
        /// <summary>
        /// Assigns the correct tenant ID to all events.
        /// </summary>
        void Process();

        /// <summary>
        /// Performs the migration for the person-related events by batch.
        /// </summary>
        /// <param name="batch">The batch number for logging purposes.</param>
        /// <param name="context">The context of the performing background job.</param>
        Task ProcessBatch(
            int batch,
            PerformContext context);
    }
}
