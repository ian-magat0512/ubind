// <copyright file="IDataTableDefinitionRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Entities;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Repository for storing data table definition.
    /// </summary>
    public interface IDataTableDefinitionRepository
    {
        /// <summary>
        /// Create the data table definition entry from data table definition object.
        /// </summary>
        void CreateDataTableDefinition(DataTableDefinition dataTableDefinition);

        /// <summary>
        /// Update the database name of the data table definition
        /// </summary>
        Task UpdateDatabaseTableName(DataTableDefinition dataTableDefinition, string newDatabaseTableName);

        /// <summary>
        /// Get the data table definitions.
        /// </summary>
        /// <returns>A collection of data table definition.</returns>
        IQueryable<DataTableDefinition> GetDataTableDefinitionsQuery(EntityListFilters filters);

        /// <summary>
        /// Get the data table definition by ID.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="dataTableDefinitionId">The data table definition id.</param>
        /// <returns>The data table.</returns>
        DataTableDefinition GetDataTableDefinitionById(Guid tenantId, Guid dataTableDefinitionId);

        /// <summary>
        /// Get the data table definitions by tenant ID.
        /// </summary>
        List<DataTableDefinition> GetDataTableDefinitionsByTenantId(Guid tenantId);

        /// <summary>
        /// Get the data table definitions by organisation ID.
        /// </summary>
        List<DataTableDefinition> GetDataTableDefinitionsByOrganisationId(Guid tenantId, Guid organisationId);

        /// <summary>
        /// Get the data table definitions by organisation ID.
        /// </summary>
        List<DataTableDefinition> GetDataTableDefinitionsByProductId(Guid tenantId, Guid organisationId);

        /// <summary>
        /// Check if the data table definition name is in use.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="name">The name.</param>
        /// <returns>A value whether the name is in use.</returns>
        bool IsNameIsInUse(Guid tenantId, Guid entityId, string name);

        /// <summary>
        /// Check if the data table definition alias is in use.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="entityId">The entity ID.</param>
        /// <param name="alias">The alias.</param>
        /// <returns>A value whether the alias is in use.</returns>
        bool IsAliasIsInUse(Guid tenantId, Guid entityId, string alias);

        /// <summary>
        /// Gets data table definitions by database table name.
        /// </summary>
        IList<DataTableDefinition> GetDataTableDefinitionsByDatabaseTableName(Guid tenantId, string databaseTableName);

        DataTableDefinition GetDataTableDefinitionsByEntityAndAlias(Guid tenantId, Guid entityId, EntityType entityType, string alias);

        /// <summary>
        /// Removes the datatable definition record
        /// </summary>
        /// <param name="dataTableDefinition"></param>
        /// <returns></returns>
        Task RemoveDataTableDefinition(DataTableDefinition dataTableDefinition);
    }
}
