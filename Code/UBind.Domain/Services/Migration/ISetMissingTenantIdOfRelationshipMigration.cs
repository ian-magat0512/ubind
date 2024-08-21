// <copyright file="ISetMissingTenantIdOfRelationshipMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.Migration
{
    using Hangfire.Server;

    public interface ISetMissingTenantIdOfRelationshipMigration
    {
        /// <summary>
        /// Sets the missing tenantId of relationships.
        /// </summary>
        void SetMissingTenantIdfOfRelationships();

        /// <summary>
        /// Performs the migration by batches.
        /// </summary>
        /// <param name="batch">The batch number for logging purposes.</param>
        /// <param name="context">The context of the performing background job.</param>
        void ProcessBatch(string entityTypeName, int batch, PerformContext context);
    }
}
