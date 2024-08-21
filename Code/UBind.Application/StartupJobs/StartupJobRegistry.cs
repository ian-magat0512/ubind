// <copyright file="StartupJobRegistry.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.StartupJobs
{
    using System;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using UBind.Application.Commands.Claim;
    using UBind.Application.Commands.Customer;
    using UBind.Application.Commands.FeatureSettings;
    using UBind.Application.Commands.Migration;
    using UBind.Application.Commands.Person;
    using UBind.Application.Commands.Quote;
    using UBind.Application.Commands.Report;
    using UBind.Application.Commands.Role;
    using UBind.Application.Commands.User;
    using UBind.Application.Person;
    using UBind.Application.Services.Search;
    using UBind.Domain;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;
    using UBind.Domain.Search;

    /// <summary>
    /// A registry of startup jobs, whereby each method represents a job to be executed when the app starts up.
    /// When naming your method, please append the date it was created in the format "_YYYYMMDD".
    /// </summary>
    public class StartupJobRegistry : IStartupJobRegistry
    {
        private readonly ICqrsMediator mediator;
        private readonly IPersonService personService;
        private readonly IFileContentRepository fileContentRepository;
        private readonly ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters> policySearchService;
        private readonly IUBindDbContext dbContext;
        private readonly ICachingResolver cachingResolver;

        public StartupJobRegistry(
            IPersonService personService,
            IFileContentRepository fileContentRepository,
            ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters> policySearchService,
            ICqrsMediator mediator,
            IUBindDbContext dbContext,
            ICachingResolver cachingResolver)
        {
            this.personService = personService;
            this.fileContentRepository = fileContentRepository;
            this.policySearchService = policySearchService;
            this.mediator = mediator;
            this.dbContext = dbContext;
            this.cachingResolver = cachingResolver;
        }

        /// <inheritdoc/>
        public void RegeneratePersonReadModels()
        {
            this.personService.RecreateExistingPeopleToPersonReadModelTable();
        }

        /// <inheritdoc/>
        public void UpdateFileContentsAndHashCodes()
        {
            this.fileContentRepository.UpdateFileContentsAndHashCodes();
        }

        /// <inheritdoc/>
        public void ApplyNewIdToMultipleAggregates()
        {
            // no-op, this was removed.
        }

        /// <inheritdoc/>
        public async Task AssignDefaultOrganisationToExistingQuotes_20210616()
        {
            var command = new SetDefaultOrganisationOnExistingQuotesAndRelatedRecordsCommand();
            await this.mediator.Send(command);
        }

        /// <inheritdoc/>
        public async Task AssignDefaultOrganisationToExistingReports_20210624()
        {
            var command = new SetDefaultOrganisationOnExistingReportsCommand();
            await this.mediator.Send(command);
        }

        /// <inheritdoc/>
        public async Task AssignDefaultOrganisationToExistingClaims_20210630()
        {
            var currentMethodName = MethodBase.GetCurrentMethod().Name;
            var command = new SetDefaultOrganisationOnExistingClaimsAndRelatedRecordsCommand();
            await this.mediator.Send(command);
        }

        /// <inheritdoc/>
        public async Task CreateOrganisationAdminRoleForAllTenants_20210902()
        {
            var command = new CreateOrganisationAdminRoleForAllTenantsCommand();
            await this.mediator.Send(command);
        }

        /// <inheritdoc/>
        public async Task AssignDefaultOrganisationToExistingCustomers_20210911()
        {
            var command = new SetDefaultOrganisationOnExistingCustomersCommand();
            await this.mediator.Send(command);
        }

        /// <inheritdoc/>
        public async Task AssignDefaultOrganisationToExistingUsers_20210912()
        {
            var command = new SetDefaultOrganisationOnExistingUsersCommand();
            await this.mediator.Send(command);
        }

        /// <inheritdoc/>
        public async Task SetDefaultOrganisationIdOnPortals_20211001()
        {
            var command = new SetDefaultOrganisationIdOnPortalsCommand();
            await this.mediator.Send(command);
        }

        public async Task PopulateFeatureSettings()
        {
            var command = new PopulateFeatureSettingsCommand();
            await this.mediator.Send(command);
        }

        /// <inheritdoc/>
        public async Task UpdateMessageManagementSettings_20220628()
        {
            var command = new UpdateMessageManagementSettingsCommand();
            await this.mediator.Send(command);
        }

        /// <inheritdoc/>
        public async Task UpdateSerializedPermissionsForMessages_20220628()
        {
            var command = new UpdateSerializedPermissionsForMessagesCommand();
            await this.mediator.Send(command);
        }

        /// <inheritdoc/>
        public async Task RollbackMessageManagementSettings_20220628()
        {
            var command = new RollbackMessageManagementSettingsCommand();
            await this.mediator.Send(command);
        }

        public async Task UpdateStateOnPolicyReadModels_20220729()
        {
            var command = new UpdatePolicyReadModelStateCommand();
            await this.mediator.Send(command);
            var activeProducts = await this.cachingResolver.GetActiveProducts();
            this.policySearchService.RegenerateLuceneIndexes(DeploymentEnvironment.Production, activeProducts, CancellationToken.None);
            this.policySearchService.RegenerateLuceneIndexes(DeploymentEnvironment.Staging, activeProducts, CancellationToken.None);
            this.policySearchService.RegenerateLuceneIndexes(DeploymentEnvironment.Development, activeProducts, CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task RevertCustomerModifiedTimeAffectedByOrganisationMigration_20210927()
        {
            var command = new RollbackCustomerModifiedTimeCommand();
            await this.mediator.Send(command);
        }

        /// <inheritdoc/>
        public async Task RevertUserModifiedTimeAffectedByOrganisationMigration_20210927()
        {
            var command = new RollbackUserModifiedTimeCommand();
            await this.mediator.Send(command);
        }

        /// <inheritdoc/>
        public async Task AddMissingIdsToPersonReadModel_20210920()
        {
            var command = new AddMissingIdsToPersonReadModelCommand();
            await this.mediator.Send(command);
        }

        /// <inheritdoc/>
        public void SetTenantNewIdToClaimAttachments_20210922()
        {
        }

        /// <inheritdoc/>
        public async Task UpdateUserAggregateCustomerId_20220831()
        {
            var command = new UpdateUserCustomerIdAndEnvironmentCommand();
            await this.mediator.Send(command);
        }

        public void SetTenantNewIdToPortals_20210922()
        {
        }

        /// <inheritdoc/>
        public async Task AddTypePropertyToPolicyTransactionEntity_20240110()
        {
            var command = new AddTypePropertyToPolicyTransactionEntityCommand();
            await this.mediator.Send(command);
        }

        /// <inheritdoc/>
        public async Task PopulateFileContentsForQuoteFileAttachments()
        {
            var command = new PopulateFileContentsForQuoteFileAttachmentsCommand();
            await this.mediator.Send(command);
        }

        /// <inheritdoc/>
        public void DeleteUnIssuedPolicyRecords_20211027()
        {
            var sqlScript = @"DELETE FROM [dbo].[PolicyReadModels] WHERE PolicyNumber IS NULL";
            this.dbContext.ExecuteSqlScript(sqlScript);
        }

        /// <inheritdoc/>
        public void ReplaceEmptyGuidWithNullValue_20211102()
        {
            var sqlScriptBuilder = new StringBuilder();
            sqlScriptBuilder.AppendLine("Update dbo.UserReadModels SET CustomerId = NULL WHERE CustomerId = '00000000-0000-0000-0000-000000000000';");
            sqlScriptBuilder.AppendLine("Update dbo.PersonReadModels SET CustomerId = NULL WHERE CustomerId = '00000000-0000-0000-0000-000000000000';");
            sqlScriptBuilder.AppendLine("Update dbo.ClaimReadModels SET CustomerId = NULL WHERE CustomerId = '00000000-0000-0000-0000-000000000000';");
            sqlScriptBuilder.AppendLine("Update dbo.ClaimReadModels SET PersonId = NULL WHERE PersonId = '00000000-0000-0000-0000-000000000000';");
            sqlScriptBuilder.AppendLine("Update dbo.ClaimVersionReadModels SET CustomerId = NULL WHERE CustomerId = '00000000-0000-0000-0000-000000000000';");
            sqlScriptBuilder.AppendLine("Update dbo.PolicyReadModels SET CustomerId = NULL WHERE CustomerId = '00000000-0000-0000-0000-000000000000';");
            sqlScriptBuilder.AppendLine("Update dbo.PolicyReadModels SET CustomerPersonId = NULL WHERE CustomerPersonId = '00000000-0000-0000-0000-000000000000';");
            sqlScriptBuilder.AppendLine("Update dbo.PolicyTransactions SET CustomerId = NULL WHERE CustomerId = '00000000-0000-0000-0000-000000000000';");
            sqlScriptBuilder.AppendLine("Update dbo.QuoteDocumentReadModels SET CustomerId = NULL WHERE CustomerId = '00000000-0000-0000-0000-000000000000';");
            sqlScriptBuilder.AppendLine("Update dbo.Quotes SET CustomerId = NULL WHERE CustomerId = '00000000-0000-0000-0000-000000000000';");
            sqlScriptBuilder.AppendLine("Update dbo.QuoteVersionReadModels SET CustomerId = NULL WHERE CustomerId = '00000000-0000-0000-0000-000000000000';");
            sqlScriptBuilder.AppendLine("Update dbo.QuoteVersionReadModels SET CustomerPersonId = NULL WHERE CustomerPersonId = '00000000-0000-0000-0000-000000000000';");
            this.dbContext.ExecuteSqlScript(sqlScriptBuilder.ToString());
        }

        public void SetSystemEmailTemplatePortalIdToBackToNullableGuid_20211105()
        {
            this.dbContext.ExecuteSqlScript(
                "ALTER TABLE dbo.SystemEmailTemplates ALTER COLUMN PortalId uniqueidentifier null");
        }

        public void RemoveUnusedColumnsOnPersonReadModel_20211214()
        {
            this.dbContext.ExecuteSqlScript(
                "IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE CONSTRAINT_SCHEMA = 'dbo' "
                + "AND TABLE_NAME = 'PersonReadModels' AND CONSTRAINT_NAME = 'Person_PersonId') "
                + "ALTER TABLE dbo.PersonReadModels DROP CONSTRAINT Person_PersonId");
            this.dbContext.ExecuteSqlScript(
                "IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS "
                + "WHERE TABLE_SCHEMA = 'dbo' "
                + "AND TABLE_NAME = 'PersonReadModels' "
                + "AND COLUMN_NAME = 'PersonId') "
                + "ALTER TABLE dbo.PersonReadModels DROP COLUMN PersonId");
            this.dbContext.ExecuteSqlScript(
                "IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS "
                + "WHERE TABLE_SCHEMA = 'dbo' "
                + "AND TABLE_NAME = 'PersonReadModels' "
                + "AND COLUMN_NAME = 'PersonId') "
                + "ALTER TABLE dbo.PersonReadModels DROP CONSTRAINT Person_Environment");
            this.dbContext.ExecuteSqlScript(
                "IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS "
                + "WHERE TABLE_SCHEMA = 'dbo' "
                + "AND TABLE_NAME = 'PersonReadModels' "
                + "AND COLUMN_NAME = 'Environment') "
                + "ALTER TABLE dbo.PersonReadModels DROP COLUMN Environment");
        }

        public void AddNonclusteredDBIndexForPolicyReadModels_20220119()
        {
            // this is not needed anymore, this causes error with new and updated database.
            // and the same indexing was done on a more recent migration.
        }

        /// <inheritdoc/>
        public async Task RemoveEventJsonFileContent()
        {
            var command = new RemoveEventJsonFileContentCommand();
            await this.mediator.Send(command);
        }

        public async Task InitialDataSeeder()
        {
            var command = new InitialDataSeederCommand();
            await this.mediator.Send(command);
        }

        public async Task AssignNewUserTypeToUserReadModels_20211012()
        {
            var command = new AssignNewUserTypeToUserReadModelsCommand();
            await this.mediator.Send(command);
        }

        public async Task AssignNewPermissionsToDefaultRoles_20211012()
        {
            var command = new AssignNewPermissionsToDefaultRolesCommand();
            await this.mediator.Send(command);
        }

        public async Task RemoveFilesEntity_20230626()
        {
            var command = new RemoveFilesEntityCommand();
            await this.mediator.Send(command);
        }

        public void AddTenantIdToPersonRelatedEvents_20220120803395()
        {
            // this is not needed anymore since
            // AddTenantIdToEvents_20220216 is the full implementation.
        }

        public async Task AddTenantIdToEvents_20220216()
        {
            var command = new AssignTenantIdToEventsCommand();
            await this.mediator.Send(command);
        }

        public void FixNullQuoteAggregateFromPolicy_20220102()
        {
            // This is not needed anymore as it was a temporary data fix
            /*
            var command = new FixNullQuoteAggregateFromPolicyCommand();
            this.mediator.Send(command).Wait();
            */
        }

        public async Task SetProductIdsToGuidForReportReadModel_20220119()
        {
            var command = new SetProductIdsToGuidForReportReadModelCommand();
            await this.mediator.Send(command);
        }

        public void CorrectUserIdAndCustomerIdFromReadModels_20220707()
        {
            // Code blocks that is responsible of resolving the multi-node issue after deploying
            // 'MigrateCustomerCommonPropertiesToPersonCommand_20220330' in the rest of the nodes
            this.dbContext.ExecuteSqlScript(
                "Update dbo.PersonReadModels SET UserId = NULL WHERE UserId = '00000000-0000-0000-0000-000000000000'");
            this.dbContext.ExecuteSqlScript(
                "UPDATE dbo.PersonReadModels SET CustomerReadModel_Id = CustomerId WHERE CustomerReadModel_Id <> CustomerId");
            this.dbContext.ExecuteSqlScript(
                @"IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
                    WHERE TABLE_NAME = 'CustomerReadModels' AND COLUMN_NAME = 'UserId')
                    BEGIN
                        EXEC('UPDATE dbo.CustomerReadModels SET UserId = NULL WHERE UserId = ''00000000-0000-0000-0000-000000000000''');
                    END");
        }

        public async Task SetTenantIdOfRelationship_05042022()
        {
            var command = new SetTenantIdOfRelationshipsCommand();
            await this.mediator.Send(command);
        }

        public void CreateIndexForRelationships_05042022()
        {
            var sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations", @"CreateIndexesForRelationships.sql");
            this.dbContext.ExecuteSqlScript(System.IO.File.ReadAllText(sqlFile), 0);

            // fill up Id column initially, SerializedPayload will be deleted in the future.
            this.dbContext.ExecuteSqlScript(
               @"IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS
                   WHERE TABLE_NAME = 'SystemEvents' AND COLUMN_NAME = 'SerializedPayload')
                    BEGIN
                        EXEC('UPDATE SystemEvents SET PayloadJson = SerializedPayload');
                   END",
               0);
        }

        public void DeleteIndexOfRelationships_05042022()
        {
            var sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations", @"DeleteIndexesForRelationships.sql");
            this.dbContext.ExecuteSqlScript(System.IO.File.ReadAllText(sqlFile), 0);
        }

        public void DeleteAssetsTableDuplicateIndexes_11032024()
        {
            var sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations", "Sql", @"DeleteAssetsTableDuplicateIndexes.sql");
            this.dbContext.ExecuteSqlScript(System.IO.File.ReadAllText(sqlFile), 0);
        }

        public async Task FixPersonRecordAssociationAndUserTypeForImportedCustomers_170822()
        {
            var sqlQueryBuilder = new StringBuilder();
            sqlQueryBuilder.AppendLine("SELECT AggregateId FROM EventRecordWithGuidIds WHERE AggregateType = 1");
            sqlQueryBuilder.AppendLine("AND AggregateId IN(SELECT Id FROM PersonReadModels WHERE CustomerId IS NOT NULL AND NOT CustomerId = '00000000-0000-0000-0000-000000000000')");
            sqlQueryBuilder.AppendLine("AND AggregateId NOT IN(SELECT AggregateId FROM EventRecordWithGuidIds WHERE EventJson LIKE '%AssociatedWithCustomerEvent%')");
            sqlQueryBuilder.AppendLine($"AND TenantId = 'A1F63C5F-5383-4F1A-AA36-C1B3AFF2A3AF'");
            var sqlQuery = sqlQueryBuilder.ToString();

            // Steadfast-IRS
            var result = this.dbContext.ExecuteSqlQuery<Guid>(sqlQuery, timeout: 350);
            var command = new FixAssociationOfPersonToCustomerCommand(Guid.Parse("A1F63C5F-5383-4F1A-AA36-C1B3AFF2A3AF"), result);
            await this.mediator.Send(command);

            // KBI
            sqlQueryBuilder.Clear();
            sqlQueryBuilder.AppendLine("SELECT AggregateId FROM EventRecordWithGuidIds WHERE AggregateType = 1");
            sqlQueryBuilder.AppendLine("AND AggregateId IN(SELECT Id FROM PersonReadModels WHERE CustomerId IS NOT NULL AND NOT CustomerId = '00000000-0000-0000-0000-000000000000')");
            sqlQueryBuilder.AppendLine("AND AggregateId NOT IN(SELECT AggregateId FROM EventRecordWithGuidIds WHERE EventJson LIKE '%AssociatedWithCustomerEvent%')");
            sqlQueryBuilder.AppendLine($"AND TenantId = 'EA00ABA8-1FF7-4DF7-9708-BB74182D7F8B'");
            sqlQuery = sqlQueryBuilder.ToString();

            result = this.dbContext.ExecuteSqlQuery<Guid>(sqlQuery, timeout: 350);
            command = new FixAssociationOfPersonToCustomerCommand(Guid.Parse("EA00ABA8-1FF7-4DF7-9708-BB74182D7F8B"), result);
            await this.mediator.Send(command);
        }

        public async Task RemoveEmptyTextAdditionalPropertyValueReadModels_20220823()
        {
            var command = new RemoveEmptyTextAdditionalPropertyValueReadModelsCommand();
            await this.mediator.Send(command);
        }

        public async Task UpdateAdditionalPropertyEntityTypeColumnValue_20220823()
        {
            var command = new IncrementAdditionPropertyEntityTypeColumnValueCommand();
            await this.mediator.Send(command);
        }

        public async Task RevertUpdateAdditionalPropertyEntityTypeColumnValue_20220823()
        {
            var command = new DecrementAdditionPropertyEntityTypeColumnValueCommand();
            await this.mediator.Send(command);
        }

        public void CreateTenantIdIndexForEventGuidTable_20220516()
        {
            var sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations", @"AddTenantIdIndexForGuidTable.sql");
            this.dbContext.ExecuteSqlScript(System.IO.File.ReadAllText(sqlFile), 0);
        }

        public async Task RecreateReadModelsOfEvents_20220524()
        {
            var command = new RecreateModelsOfEventsCommand();
            await this.mediator.Send(command);
        }

        public async Task DropColumnsForSystemEventsAndCustomers_20220826()
        {
            var command = new DropColumnsForSystemEventsAndCustomersCommand();
            await this.mediator.Send(command);
        }

        /// <summary>
        /// Execute queries for the rename date field migration.
        /// </summary>
        public async Task RenameDateFieldsCleanup_20220818()
        {
            var command = new RenameDateFieldsCleanupCommand();
            await this.mediator.Send(command);
        }

        /// <summary>
        /// Execute queries to copy old columns to new columns.
        /// </summary>
        public async Task UpdateRecordsAndCreateIndexesForDateAndTimestampRenameCommand_20220818()
        {
            var command = new UpdateRecordsAndCreateIndexesForDateAndTimestampRenameCommand();
            await this.mediator.Send(command);
        }

        public async Task SetLatestRenewalEffectiveOfPolicyReadModels_20220919()
        {
            var command = new UpdatePolicyReadModelLatestRenewalEffectiveTimeCommand();
            await this.mediator.Send(command);
        }

        public async Task PopulateAssetFileContents()
        {
            await this.ExecuteCommand(new PopulateAssetFileContentsCommand());
        }

        public async Task InsertQuoteAndClaimFileContentsByTenant()
        {
            await this.ExecuteCommand(new InsertQuoteAndClaimFileContentsByTenantCommand());
        }

        public async Task CleanupAssetsAndFileContents()
        {
            await this.ExecuteCommand(new CleanupAssetsAndFileContentsCommand());
        }

        public async Task UpdatePolicyStateFromPendingToIssued_20221202()
        {
            var command = new UpdatePolicyStateFromPendingToIssuedCommand();
            await this.mediator.Send(command);
        }

        public async Task AssignCreditNoteNumber_20230505()
        {
            var command = new AssignCreditNoteNumberCommand();
            await this.mediator.Send(command);
        }

        public async Task PascalizeStatus_20220524()
        {
            await this.Pascalize("Quotes", "QuoteState");
            await this.Pascalize("ClaimReadModels", "Status");
            await this.Pascalize("PolicyReadModels", "PolicyState");
            await this.Pascalize("QuoteVersionReadModels", "State");
        }

        public async Task UpdateCustomerOwnerUserIdFromDefaultGuidToNull_20221205()
        {
            var command = new UpdateCustomerOwnerUserIdFromDefaultGuidToNullCommand();
            await this.mediator.Send(command);
        }

        public void CreateNonClusteredIndexForSystemEvents_20231109()
        {
            var sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations", @"CreateIndexForSystemEvents.sql");
            this.dbContext.ExecuteSqlScript(System.IO.File.ReadAllText(sqlFile), 0);
        }

        public async Task UpdateSystemEventsExpiryTimeStamp_20230901(CancellationToken cancellationToken)
        {
            var command = new UpdateSystemEventsExpiryTimeStampCommand();
            await this.mediator.Send(command, cancellationToken);
        }

        public async Task AddCustomerRelationshipForQuayCommercialHullEmails_20230104()
        {
            var command = new AddCustomerRelationshipForQuayCommercialHullEmailsCommand();
            await this.mediator.Send(command);
        }

        public void PrecedingJob1()
        {
            // this is here for the unit test.
        }

        public void PrecedingJob2()
        {
            // this is here for the unit test.
        }

        public void ContextJob()
        {
            // this is here for the unit test.
        }

        public void MyJobAlias()
        {
            // this is here for the unit test.
        }

        public async Task FillEnvironmentColumnForEmail_20221028()
        {
            var command = new FillEnvironmentColumnForEmailCommand();
            await this.mediator.Send(command);
        }

        public async Task SetTenantIdOfRelationships_20221006()
        {
            var command = new SetMissingTenantIdOfRelationshipsCommand();
            await this.mediator.Send(command);
        }

        /// <inheritdoc/>
        public async Task PopulateFileContentsFromDocumentFiles()
        {
            await this.ExecuteCommand(new PopulateFileContentsFromDocumentFilesCommand());
        }

        public async Task SetAllSystemEventsToEmitted_20230127()
        {
            var command = new SetAllSystemEventsToEmittedCommand();
            await this.mediator.Send(command);
        }

        public void MigrateCustomerCommonPropertiesToPersonCommand_20220330()
        {
            // Adding an empty function here to address the issue of not finding this method
        }

        public void RemoveSystemEventEmittedFlag_20230225()
        {
            var sql = SqlHelper.DropColumnWithConstraintsIfExists("dbo.SystemEvents", "IsEmitted");
            this.dbContext.ExecuteSqlScript(sql);
        }

        public async Task CleanupDocumentFileContents()
        {
            this.dbContext.ExecuteSqlScript("DROP TABLE IF EXISTS tmpFileContentDocumentFilesTable;");
            await this.ExecuteCommand(new CleanupDocumentFileContentsCommand());
        }

        public async Task FixCorruptPolicyDocuments()
        {
            await this.ExecuteCommand(new FixCorruptPolicyDocumentsCommand());
        }

        public async Task UpdatePolicyExpiryDate_202306160025592()
        {
            await this.ExecuteCommand(new UpdatePolicyReadModelLatestExpiryDateTimeCommand());
        }

        public void RemoveApplicationPasswordsTable_20230725()
        {
            this.dbContext.ExecuteSqlScript("DROP TABLE IF EXISTS ApplicationPasswords;");
        }

        public async Task FixInvalidQuestionSetAttachments()
        {
            await this.ExecuteCommand(
                new FixInvalidQuestionSetAttachmentsCommand(
                    "fleetDescription", "reviewScheduleOfVehicles", "claimsHistoryAttachment", "extraDocuments[0].extraDocument"));
        }

        public async Task RestoreDeletedCustomersWithUser_20231002()
        {
            var command = new RestoreDeletedCustomersWithUserCommand();
            await this.mediator.Send(command);
        }

        public async Task AddUserDeletedEvents()
        {
            await this.ExecuteCommand(new AddUserDeletedEventsCommand());
        }

        public async Task CreatePortalAggregates_20230401()
        {
            await this.mediator.Send(new CreatePortalAggregatesCommand());
        }

        public async Task AddDataEnvironmentAccessToCustomerRole_20230407()
        {
            await this.mediator.Send(new AddDataEnvironmentAccessToCustomerRoleCommand());
        }

        public async Task CreateDefaultPortals_20230408()
        {
            await this.mediator.Send(new CreateDefaultPortalsCommand());
        }

        public async Task SetDefaultOrganisations_20230516()
        {
            await this.mediator.Send(new SetDefaultOrganisationsCommand());
        }

        public async Task CreateMasterSupportAgentRole_20230516()
        {
            await this.mediator.Send(new CreateMasterSupportAgentRoleCommand());
        }

        public async Task CreateLocalAuthenticationMethodForOrgsWithPortalsCommand_20230529()
        {
            await this.mediator.Send(new CreateLocalAuthenticationMethodForOrgsWithPortalsCommand());
        }

        public async Task DropColumnsForProductFeatureSettings_20231005()
        {
            await this.mediator.Send(new DropColumnsForProductFeatureSettingsCommand());
        }

        public async Task AssignManagingOrganisationForExistingOrganisations_20231117()
        {
            await this.mediator.Send(new AssignDefaultManagingOrganisationCommand());
        }

        public async Task AddMissingUserCreatedEventForPersonAggregate_20231116()
        {
            await this.mediator.Send(new AddMissingUserCreatedEventForPersonAggregateCommand());
        }

        public async Task AddManagePolicyNumberAndManageClaimNumbersToClientRoles_20231118()
        {
            await this.mediator.Send(new AddManagePolicyNumbersAndManageClaimNumbersCommand());
        }

        public async Task RestructureReleaseData_20230826()
        {
            await this.ExecuteCommand(new RestructureReleaseDataCommand());
        }

        public async Task DropOldReleaseRelations_20230826()
        {
            await this.ExecuteCommand(new DropOldReleaseRelationsCommand());
        }

        public async Task RemoveDuplicateReleaseAssets_20240104()
        {
            await this.ExecuteCommand(new RemoveDuplicateReleaseAssetsCommand());
        }

        public async Task SetTotalPayableOnExistingQuotes_20230608(CancellationToken cancellationToken)
        {
            var command = new SetTotalPayableOnExistingQuotesFromLatestCalculationResultCommand();
            await this.mediator.Send(command, cancellationToken);
        }

        public async Task SetTotalPayableOnExistingPolicyTransaction_20230630(CancellationToken cancellationToken)
        {
            var command = new SetTotalPayableOnExistingPolicyTransactionsFromLatestCalculationResultCommand();
            await this.mediator.Send(command, cancellationToken);
        }

        public async Task SetClaimReadModelDates_20230711(CancellationToken cancellationToken)
        {
            var command = new SetClaimReadModelDatesCommand();
            await this.mediator.Send(command, cancellationToken);
        }

        public async Task AddPasswordToUserLoginEmails_20231005()
        {
            await this.ExecuteCommand(new AddPasswordToUserLoginEmailsCommand());
        }

        public async Task ReplaceRolesNotExistingOrganisationIdByTenantDefaultOrganisationId_20240110()
        {
            await this.mediator.Send(new UpdateRolesWithNotExistingOrganisationCommand());
        }

        public async Task AddQuoteAggregateSnapshot_20240507(CancellationToken cancellationToken)
        {
            await this.mediator.Send(new AddQuoteAggregateSnapshotCommand(), cancellationToken);
        }

        public async Task RegenerateAggregateSnapshot_06242024(CancellationToken cancellationToken)
        {
            await this.mediator.Send(new RegenerateAggregateSnapshotCommand(), cancellationToken);
        }

        public async Task RemoveDuplicateFileContents_06092024(CancellationToken cancellationToken)
        {
            await this.mediator.Send(new RemoveDuplicateFileContentsCommand(), cancellationToken);
        }

        public async Task AlterCustomEventAliasInSystemEventsTable_20240704(CancellationToken cancellationToken)
        {
            await this.mediator.Send(new AlterCustomEventAliasColumnForSystemEventsCommand(), cancellationToken);
        }

        public async Task UpdateQuoteDocumentsQuoteOrPolicyTransactionId_20240813(CancellationToken cancellationToken)
        {
            await this.mediator.Send(new UpdateQuoteDocumentsQuoteOrPolicyTransactionIdCommand(), cancellationToken);
        }

        private async Task ExecuteCommand(ICommand command) => await this.mediator.Send(command);

        private async Task ExecuteCommand(ICommand command, CancellationToken cancellationToken)
            => await this.mediator.Send(command, cancellationToken);

        private async Task Pascalize(string tableName, string columnName)
        {
            var sql = $@"UPDATE {tableName} SET {columnName} = UPPER(LEFT({columnName}, 1)) + SUBSTRING({columnName}, 2, LEN({columnName})) WHERE {columnName} IS NOT NULL";
            this.dbContext.ExecuteSqlScript(sql, 0);
            await Task.Delay(10000);
        }
    }
}
