﻿// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using UBind.Persistence.Helpers;
    using UBind.Persistence.Migrations.Extensions;

    public partial class ConsolidatePadlockMigrations : DbMigration
    {
        private const string RemoveApplicationPasswords = "RemoveApplicationPasswordsTable_20230725";
        private const string CreatePortalAggregatesAlias = "CreatePortalAggregates_20230401";
        private const string AddDataEnvironmentAccessToCustomerRoleAlias = "AddDataEnvironmentAccessToCustomerRole_20230407";
        private const string CreateDefaultPortalsAlias = "CreateDefaultPortals_20230408";
        private const string SetDefaultOrganisationsStartupJobAlias = "SetDefaultOrganisations_20230516";
        private const string CreateMasterSupportAgentRoleStartupJobAlias = "CreateMasterSupportAgentRole_20230516";
        private const string AuthMethodStartupJobAlias = "CreateLocalAuthenticationMethodForOrgsWithPortalsCommand_20230529";

        public override void Up()
        {
            //RemoveApplicationPasswordsAndAddedPolicyTransactionEffectiveTicksSinceEpochAndPolicyExpiryTicksSinceEpoch
            this.Sql(StartupJobRunnerQueryHelper
                .GenerateInsertQueryForStartupJob(RemoveApplicationPasswords, runManuallyIfInMultiNode: true));
            this.AddColumnIfNotExists("dbo.Quotes", "PolicyTransactionEffectiveTicksSinceEpoch", c => c.Long());
            this.AddColumnIfNotExists("dbo.Quotes", "PolicyExpiryTicksSinceEpoch", c => c.Long());

            //UsePortalAggregateAndReadModel 
            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(
            CreatePortalAggregatesAlias, blocking: true));
            CreateTable(
                "dbo.PortalReadModels",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    StyleSheetUrl = c.String(),
                    Styles = c.String(),
                    ProductionUrl = c.String(),
                    StagingUrl = c.String(),
                    DevelopmentUrl = c.String(),
                    Name = c.String(),
                    Alias = c.String(),
                    Title = c.String(),
                    UserType = c.Int(nullable: false),
                    OrganisationId = c.Guid(nullable: false),
                    Disabled = c.Boolean(nullable: false),
                    Deleted = c.Boolean(nullable: false),
                    IsDefault = c.Boolean(nullable: false),
                    TenantId = c.Guid(nullable: false),
                    CreatedTicksSinceEpoch = c.Long(nullable: false),
                    LastModifiedTicksSinceEpoch = c.Long(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            //AddDataEnvironmentAccessToCustomerRole
            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(
                AddDataEnvironmentAccessToCustomerRoleAlias, blocking: true));

            //CreateDefaultPortals
            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(
                CreateDefaultPortalsAlias, blocking: true));
            AddColumn("dbo.OrganisationReadModels", "DefaultPortalId", c => c.Guid());
            AddColumn("dbo.TenantDetails", "DefaultPortalId", c => c.Guid(nullable: false));

            //MigrateOrganisationPermissions
            var sql1 = @"
                UPDATE dbo.Roles
                SET SerializedPermissions
                    = REPLACE(SerializedPermissions, 'ViewUsersFromAllOrganisations', 'ViewUsersFromOtherOrganisations')";
            this.Sql(sql1);
            var sql2 = @"
                UPDATE dbo.Roles
                SET SerializedPermissions
                    = REPLACE(SerializedPermissions, 'ManageUsersForAllOrganisations', 'ManageUsersForOtherOrganisations')";
            this.Sql(sql2);
            AddColumn("dbo.OrganisationReadModels", "IsDefault", c => c.Boolean(nullable: false));
            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(
                SetDefaultOrganisationsStartupJobAlias, blocking: true));
            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(
                CreateMasterSupportAgentRoleStartupJobAlias, blocking: true));

            //ManagingOrganisations
            AddColumn("dbo.OrganisationReadModels", "ManagingOrganisationId", c => c.Guid());

            //AuthenticationMethods
            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(
                AuthMethodStartupJobAlias, blocking: true));
            CreateTable(
                "dbo.AuthenticationMethods",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    OrganisationId = c.Guid(nullable: false),
                    Name = c.String(),
                    TypeName = c.String(),
                    CanCustomersSignIn = c.Boolean(nullable: false),
                    CanAgentsSignIn = c.Boolean(nullable: false),
                    IncludeSignInButtonOnPortalLoginPage = c.Boolean(nullable: false),
                    SignInButtonBackgroundColor = c.String(),
                    SignInButtonIconUrl = c.String(),
                    SignInButtonLabel = c.String(),
                    Disabled = c.Boolean(nullable: false),
                    TenantId = c.Guid(nullable: false),
                    CreatedTicksSinceEpoch = c.Long(nullable: false),
                    LastModifiedTicksSinceEpoch = c.Long(nullable: false),
                })
                .PrimaryKey(t => t.Id);
            CreateTable(
                "dbo.OrganisationLinkedIdentityReadModels",
                c => new
                {
                    OrganisationId = c.Guid(nullable: false),
                    AuthenticationMethodId = c.Guid(nullable: false),
                    TenantId = c.Guid(nullable: false),
                    AuthenticationMethodName = c.String(),
                    AuthenticationMethodTypeName = c.String(),
                    UniqueId = c.String(),
                })
                .PrimaryKey(t => new { t.OrganisationId, t.AuthenticationMethodId })
                .ForeignKey("dbo.OrganisationReadModels", t => t.OrganisationId, cascadeDelete: true)
                .Index(t => t.OrganisationId);
            CreateTable(
                "dbo.PortalSignInMethods",
                c => new
                {
                    PortalId = c.Guid(nullable: false),
                    AuthenticationMethodId = c.Guid(nullable: false),
                    TenantId = c.Guid(nullable: false),
                    SortOrder = c.Int(nullable: false),
                    IsEnabled = c.Boolean(nullable: false),
                    Name = c.String(),
                    TypeName = c.String(),
                })
                .PrimaryKey(t => new { t.PortalId, t.AuthenticationMethodId });
            CreateTable(
                "dbo.UserLinkedIdentityReadModels",
                c => new
                {
                    UserId = c.Guid(nullable: false),
                    AuthenticationMethodId = c.Guid(nullable: false),
                    TenantId = c.Guid(nullable: false),
                    AuthenticationMethodName = c.String(),
                    AuthenticationMethodTypeName = c.String(),
                    UniqueId = c.String(),
                })
                .PrimaryKey(t => new { t.UserId, t.AuthenticationMethodId })
                .ForeignKey("dbo.UserReadModels", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            CreateTable(
                "dbo.SamlAuthenticationMethods",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    IdentityProviderEntityIdentifier = c.String(),
                    IdentityProviderSingleSignOnServiceUrl = c.String(),
                    IdentityProviderSingleLogoutServiceUrl = c.String(),
                    IdentityProviderArtifactResolutionServiceUrl = c.String(),
                    IdentityProviderCertificate = c.String(),
                    MustSignAuthenticationRequests = c.Boolean(nullable: false),
                    ShouldLinkExistingCustomerWithSameEmailAddress = c.Boolean(),
                    CanCustomerAccountsBeAutoProvisioned = c.Boolean(nullable: false),
                    CanCustomerDetailsBeAutoUpdated = c.Boolean(),
                    ShouldLinkExistingAgentWithSameEmailAddress = c.Boolean(),
                    CanAgentAccountsBeAutoProvisioned = c.Boolean(nullable: false),
                    CanAgentDetailsBeAutoUpdated = c.Boolean(),
                    CanUsersOfManagedOrganisationsSignIn = c.Boolean(nullable: false),
                    ShouldLinkExistingOrganisationWithSameAlias = c.Boolean(),
                    CanOrganisationsBeAutoProvisioned = c.Boolean(),
                    CanOrganisationDetailsBeAutoUpdated = c.Boolean(),
                    NameIdFormat = c.String(),
                    UseNameIdAsUniqueIdentifier = c.Boolean(nullable: false),
                    UseNameIdAsEmailAddress = c.Boolean(),
                    UniqueIdentifierAttributeName = c.String(),
                    FirstNameAttributeName = c.String(),
                    LastNameAttributeName = c.String(),
                    EmailAddressAttributeName = c.String(),
                    PhoneNumberAttributeName = c.String(),
                    MobileNumberAttributeName = c.String(),
                    UserTypeAttributeName = c.String(),
                    OrganisationUniqueIdentifierAttributeName = c.String(),
                    OrganisationNameAttributeName = c.String(),
                    OrganisationAliasAttributeName = c.String(),
                    RoleAttributeName = c.String(),
                    RoleAttributeValueDelimiter = c.String(),
                    DefaultAgentRoleId = c.Guid(),
                    RoleMapJson = c.String(),
                    AreRolesManagedExclusivelyByThisIdentityProvider = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AuthenticationMethods", t => t.Id)
                .Index(t => t.Id);
            CreateTable(
                "dbo.LocalAccountAuthenticationMethods",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    AllowCustomerSelfRegistration = c.Boolean(nullable: false),
                    AllowAgentSelfRegistration = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AuthenticationMethods", t => t.Id)
                .Index(t => t.Id);



            //RotatingJWTKeys
            CreateTable(
                "dbo.JwtKeys",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    KeyBase64 = c.String(),
                    IsRotated = c.Boolean(nullable: false),
                    IsExpired = c.Boolean(nullable: false),
                    CreatedTicksSinceEpoch = c.Long(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            //AddProductReleaseIdOnQuoteReadmodelAndPolicyTransaction
            AddColumn("dbo.PolicyTransactions", "ProductReleaseId", c => c.Guid());
            AddColumn("dbo.Quotes", "ProductReleaseId", c => c.Guid());
        }

        public override void Down()
        {
            //RemoveApplicationPasswordsAndAddedPolicyTransactionEffectiveTicksSinceEpochAndPolicyExpiryTicksSinceEpoch
            this.Sql(StartupJobRunnerQueryHelper.GenerateDeleteQueryForStartupJob(RemoveApplicationPasswords));
            CreateTable(
                "dbo.ApplicationPasswords",
                c => new
                {
                    Id = c.Guid(nullable: false),
                    ApplicationId = c.Guid(nullable: false),
                    SaltedHashedPasswordWithAlgorithmAndSalt = c.String(),
                    CreatedTicksSinceEpoch = c.Long(nullable: false),
                })
                .PrimaryKey(t => t.Id);
            DropColumn("dbo.Quotes", "PolicyExpiryTicksSinceEpoch");
            DropColumn("dbo.Quotes", "PolicyTransactionEffectiveTicksSinceEpoch");

            //UsePortalAggregateAndReadModel
            this.Sql(StartupJobRunnerQueryHelper.GenerateDeleteQueryForStartupJob(CreatePortalAggregatesAlias));
            DropTable("dbo.PortalReadModels");

            //AddDataEnvironmentAccessToCustomerRole
            this.Sql(StartupJobRunnerQueryHelper.GenerateDeleteQueryForStartupJob(AddDataEnvironmentAccessToCustomerRoleAlias));

            //CreateDefaultPortals
            this.Sql(StartupJobRunnerQueryHelper.GenerateDeleteQueryForStartupJob(CreateDefaultPortalsAlias));
            DropColumn("dbo.TenantDetails", "DefaultPortalId");
            DropColumn("dbo.OrganisationReadModels", "DefaultPortalId");

            //MigrateOrganisationPermissions
            this.Sql(StartupJobRunnerQueryHelper.GenerateDeleteQueryForStartupJob(CreateMasterSupportAgentRoleStartupJobAlias));
            this.Sql(StartupJobRunnerQueryHelper.GenerateDeleteQueryForStartupJob(SetDefaultOrganisationsStartupJobAlias));

            DropColumn("dbo.OrganisationReadModels", "IsDefault");

            var sql2 = @"
                UPDATE dbo.Roles
                SET SerializedPermissions
                    = REPLACE(SerializedPermissions, 'ManageUsersForOtherOrganisations', 'ManageUsersForAllOrganisations')";
            this.Sql(sql2);
            var sql1 = @"
                UPDATE dbo.Roles
                SET SerializedPermissions
                    = REPLACE(SerializedPermissions, 'ViewUsersFromOtherOrganisations', 'ViewUsersFromAllOrganisations')";
            this.Sql(sql1);

            //ManagingOrganisations
            DropColumn("dbo.OrganisationReadModels", "ManagingOrganisationId");

            //AuthenticationMethods

            DropForeignKey("dbo.LocalAccountAuthenticationMethods", "Id", "dbo.AuthenticationMethods");
            DropForeignKey("dbo.SamlAuthenticationMethods", "Id", "dbo.AuthenticationMethods");
            DropForeignKey("dbo.UserLinkedIdentityReadModels", "UserId", "dbo.UserReadModels");
            DropForeignKey("dbo.OrganisationLinkedIdentityReadModels", "OrganisationId", "dbo.OrganisationReadModels");
            DropIndex("dbo.LocalAccountAuthenticationMethods", new[] { "Id" });
            DropIndex("dbo.SamlAuthenticationMethods", new[] { "Id" });
            DropIndex("dbo.UserLinkedIdentityReadModels", new[] { "UserId" });
            DropIndex("dbo.OrganisationLinkedIdentityReadModels", new[] { "OrganisationId" });
            DropTable("dbo.LocalAccountAuthenticationMethods");
            DropTable("dbo.SamlAuthenticationMethods");
            DropTable("dbo.UserLinkedIdentityReadModels");
            DropTable("dbo.PortalSignInMethods");
            DropTable("dbo.OrganisationLinkedIdentityReadModels");
            DropTable("dbo.AuthenticationMethods");

            //RotatingJwtKeys
            DropTable("dbo.JwtKeys");

            //AddProductReleaseIdOnQuoteReadmodelAndPolicyTransaction
            DropColumn("dbo.Quotes", "ProductReleaseId");
            DropColumn("dbo.PolicyTransactions", "ProductReleaseId");
        }
    }
}
