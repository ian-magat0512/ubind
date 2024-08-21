// <copyright file="IStartupJobRegistry.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.StartupJobs;

using System.Threading.Tasks;

/// <summary>
/// A registry of startup jobs, whereby each method represents a job to be executed when the app starts up.
/// When naming your method, please append the date it was created in the format "_YYYYMMDD".
/// </summary>
public interface IStartupJobRegistry
{
    /// <summary>
    /// Regenerates person read models.
    /// </summary>
    void RegeneratePersonReadModels();

    /// <summary>
    /// Apply tenant and product guid new ids to multiple aggregates.
    /// </summary>
    void ApplyNewIdToMultipleAggregates();

    /// <summary>
    /// Update file contents and hash codes.
    /// </summary>
    void UpdateFileContentsAndHashCodes();

    /// <summary>
    /// Populate file contents for quote file attachments.
    /// </summary>
    Task PopulateFileContentsForQuoteFileAttachments();

    /// <summary>
    /// Remove file content from event json.
    /// </summary>
    Task RemoveEventJsonFileContent();

    /// <summary>
    /// Set default organisations to existing quotes.
    /// </summary>
    Task AssignDefaultOrganisationToExistingQuotes_20210616();

    /// <summary>
    /// Set default organisations to existing claims.
    /// </summary>
    Task AssignDefaultOrganisationToExistingClaims_20210630();

    /// <summary>
    /// Set default organisations to existing reports.
    /// </summary>
    Task AssignDefaultOrganisationToExistingReports_20210624();

    /// <summary>
    /// Creates the new Organisation Admin role for all tenants.
    /// </summary>
    Task CreateOrganisationAdminRoleForAllTenants_20210902();

    /// <summary>
    /// Set default organisation to existing customers.
    /// </summary>
    Task AssignDefaultOrganisationToExistingCustomers_20210911();

    /// <summary>
    /// Set default organisation to existing users.
    /// </summary>
    Task AssignDefaultOrganisationToExistingUsers_20210912();

    /// <summary>
    /// Sets the tenant's default organanisation id on portals.
    /// This is needed because we are addding organisation ID to portals
    /// and so we need to set an initial value. Since all portals were only
    /// against the tenant previously, it's fine to set the organisation ID
    /// to the default.
    /// </summary>
    Task SetDefaultOrganisationIdOnPortals_20211001();

    /// <summary>
    /// Update message management settings.
    /// </summary>
    Task UpdateMessageManagementSettings_20220628();

    /// <summary>
    /// Update serialized permission for message.
    /// </summary>
    Task UpdateSerializedPermissionsForMessages_20220628();

    /// <summary>
    /// Rollback message management setting.
    /// </summary>
    Task RollbackMessageManagementSettings_20220628();

    /// <summary>
    /// Revert the modified time affected by organisation migration for customers.
    /// </summary>
    Task RevertCustomerModifiedTimeAffectedByOrganisationMigration_20210927();

    /// <summary>
    /// Revert the modified time affected by organisation migration for users.
    /// </summary>
    Task RevertUserModifiedTimeAffectedByOrganisationMigration_20210927();

    /// <summary>
    /// Add missing user or customer ID to the person.
    /// </summary>
    Task AddMissingIdsToPersonReadModel_20210920();

    /// <summary>
    /// Sets tenant new ids to claim file attachments.
    /// </summary>
    void SetTenantNewIdToClaimAttachments_20210922();

    /// <summary>
    /// Delete all policies that are not issued.
    /// </summary>
    void DeleteUnIssuedPolicyRecords_20211027();

    /// <summary>
    /// Replace empty guid with null n customer Id and Person Id columns.
    /// </summary>
    void ReplaceEmptyGuidWithNullValue_20211102();

    /// <summary>
    /// Update user aggregate Customer Id.
    /// </summary>
    Task UpdateUserAggregateCustomerId_20220831();

    /// <summary>
    /// Sets tenant new ids to Portals.
    /// </summary>
    void SetTenantNewIdToPortals_20210922();

    /// <summary>
    /// Configure seed data to initialize database with defaults.
    /// </summary>
    Task InitialDataSeeder();

    /// <summary>
    /// Apply the new user types to all user read model.
    /// </summary>
    Task AssignNewUserTypeToUserReadModels_20211012();

    /// <summary>
    /// New permissions will now be applied to default roles.
    /// As implementation for UB-4685.
    /// </summary>
    Task AssignNewPermissionsToDefaultRoles_20211012();

    Task RemoveFilesEntity_20230626();

    /// <summary>
    /// Removes unused columns on person read model.
    /// </summary>
    void RemoveUnusedColumnsOnPersonReadModel_20211214();

    /// <summary>
    /// Sets product ids of report read models to Guid.
    /// </summary>
    Task SetProductIdsToGuidForReportReadModel_20220119();

    /// <summary>
    /// Adds indexes for policy read models.
    /// </summary>
    void AddNonclusteredDBIndexForPolicyReadModels_20220119();

    /// <summary>
    /// Sets the tenant id of the relationship.
    /// </summary>
    Task SetTenantIdOfRelationship_05042022();

    /// <summary>
    /// Creates index for relationships.
    /// </summary>
    void CreateIndexForRelationships_05042022();

    /// <summary>
    /// Delete index for relationships.
    /// </summary>
    void DeleteIndexOfRelationships_05042022();

    /// <summary>
    /// Deletes the duplicate indexes of assets table.
    /// </summary>
    void DeleteAssetsTableDuplicateIndexes_11032024();

    /// <summary>
    /// Set the proper value of user Id and customer Id for person and customer read models.
    /// </summary>
    /// <remarks>
    /// Must only execute if deployed in all nodes.
    /// </remarks>
    void CorrectUserIdAndCustomerIdFromReadModels_20220707();

    /// <summary>
    /// Remove all text text additional property with empty value.
    /// </summary>
    Task RemoveEmptyTextAdditionalPropertyValueReadModels_20220823();

    /// <summary>
    /// Update to additional property entity type column value.
    /// </summary>
    Task UpdateAdditionalPropertyEntityTypeColumnValue_20220823();

    /// <summary>
    /// Revert update to additional property entity type column value.
    /// </summary>
    Task RevertUpdateAdditionalPropertyEntityTypeColumnValue_20220823();

    /// <summary>
    /// Creates tenant id index for event guid table.
    /// </summary>
    void CreateTenantIdIndexForEventGuidTable_20220516();

    /// <summary>
    /// Recreate ReadModels of events.
    /// </summary>
    Task RecreateReadModelsOfEvents_20220524();

    /// <summary>
    /// Populate asset file contents.
    /// </summary>
    Task PopulateAssetFileContents();

    /// <summary>
    /// Pascalize status of selected entities like quotes/claims/policies.
    /// </summary>
    Task PascalizeStatus_20220524();

    /// <summary>
    /// Update customer read models' owner user id from default guid to null.
    /// </summary>
    Task UpdateCustomerOwnerUserIdFromDefaultGuidToNull_20221205();

    /// <summary>
    /// Set tenantId of relationships.
    /// </summary>
    Task SetTenantIdOfRelationships_20221006();

    /// <summary>
    /// Set all system events to emitted.
    /// </summary>
    Task SetAllSystemEventsToEmitted_20230127();

    /// <summary>
    /// Insert quote and claim file contents to distinguish FileContents by newly added TenantId.
    /// </summary>
    Task InsertQuoteAndClaimFileContentsByTenant();

    /// <summary>
    /// Cleanup asset and file contents.
    /// </summary>
    Task CleanupAssetsAndFileContents();

    /// <summary>
    /// Assign a credit note for all cancellation and adjustment quotes whose
    /// total payable is less than zero and credit note is not issued.
    /// </summary>
    Task AssignCreditNoteNumber_20230505();

    /// <summary>
    /// Populate email attachment file contents from document files.
    /// </summary>
    Task PopulateFileContentsFromDocumentFiles();

    void RemoveSystemEventEmittedFlag_20230225();

    /// <summary>
    /// Cleanup DocumentFiles and remove Content column after migrating it to FileContents table.
    /// </summary>
    Task CleanupDocumentFileContents();

    /// <summary>
    /// Fix corrupt policy documents.
    /// </summary>
    Task FixCorruptPolicyDocuments();

    /// <summary>
    /// Asynchronously executes a migration to fix question set attachments with invalid ID.
    /// </summary>
    Task FixInvalidQuestionSetAttachments();

    Task CreatePortalAggregates_20230401();

    Task AddDataEnvironmentAccessToCustomerRole_20230407();

    Task CreateDefaultPortals_20230408();

    Task SetDefaultOrganisations_20230516();

    Task CreateMasterSupportAgentRole_20230516();

    /// <summary>
    /// Restore deleted customers.
    /// </summary>
    /// <returns></returns>
    Task RestoreDeletedCustomersWithUser_20231002();

    /// <summary>
    /// Asynchronously executes a migration to add user deleted events.
    /// </summary>
    Task AddUserDeletedEvents();

    Task DropColumnsForProductFeatureSettings_20231005();

    /// <summary>
    /// Asynchronously executes a migration that assigns the current default organisation
    /// as the managing organisation for the other sub-orgs of the same tenancy.
    /// </summary>
    Task AssignManagingOrganisationForExistingOrganisations_20231117();

    Task AddMissingUserCreatedEventForPersonAggregate_20231116();

    Task AddManagePolicyNumberAndManageClaimNumbersToClientRoles_20231118();

    Task RestructureReleaseData_20230826();

    Task DropOldReleaseRelations_20230826();

    void CreateNonClusteredIndexForSystemEvents_20231109();

    Task UpdateSystemEventsExpiryTimeStamp_20230901(CancellationToken cancellationToken);

    Task RemoveDuplicateReleaseAssets_20240104();

    Task SetTotalPayableOnExistingQuotes_20230608(CancellationToken cancellationToken);

    Task SetTotalPayableOnExistingPolicyTransaction_20230630(CancellationToken cancellationToken);

    Task SetClaimReadModelDates_20230711(CancellationToken cancellationToken);

    Task AddPasswordToUserLoginEmails_20231005();

    /// <summary>
    /// Adds the 'Type' property to PolicyTransaction Entity and add default values to it based on the value of
    /// 'Discriminator' property.
    /// </summary>
    /// <returns></returns>
    Task AddTypePropertyToPolicyTransactionEntity_20240110();

    Task ReplaceRolesNotExistingOrganisationIdByTenantDefaultOrganisationId_20240110();

    /// <summary>
    /// This migration is to add the existing policy record to have a aggregate snapshot
    /// so it can be more efficient when loading the aggregate.
    /// This to serialized the aggregate and store it in the aggregate snapshot table.
    /// </summary>
    Task AddQuoteAggregateSnapshot_20240507(CancellationToken cancellationToken);

    /// <summary>
    /// Removes all duplicate file contents from the database to free up space.
    /// This migration identifies duplicate entries by their hash codes and tenant IDs,
    /// and retains only the first occurrence while deleting the rest.
    /// </summary>
    Task RemoveDuplicateFileContents_06092024(CancellationToken cancellationToken);

    /// <summary>
    /// This migration is to regenerate all the aggregate snapshot
    /// since we need to remove some property on the formData and calculationResult class.
    /// This way can reduce the size of the aggregate snapshot in the database.
    /// </summary>
    Task RegenerateAggregateSnapshot_06242024(CancellationToken cancellationToken);

    Task AlterCustomEventAliasInSystemEventsTable_20240704(CancellationToken cancellationToken);

    Task UpdateQuoteDocumentsQuoteOrPolicyTransactionId_20240813(CancellationToken cancellationToken);
}
