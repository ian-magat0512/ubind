﻿// <auto-generated />
#pragma warning disable 1591
namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;
    using UBind.Persistence.Helpers;

    public partial class RemoveTenantNewIdAndProductNewId : DbMigration
    {
        private string setProductIdsToGuidForReportReadModel = "SetProductIdsToGuidForReportReadModel_20220119";
        private string addNonclusteredDBIndexForPolicyReadModels = "AddNonclusteredDBIndexForPolicyReadModels_20220119";

        public override void Up()
        {
            DropForeignKey("dbo.TenantDetails", "Tenant_Id", "dbo.Tenants");
            DropForeignKey("dbo.Portals", "Tenant_Id", "dbo.Tenants");
            DropForeignKey("dbo.SettingDetails", "Tenant_Id", "dbo.Tenants");
            DropForeignKey("dbo.ProductDetails", new[] { "Product_TenantId", "Product_Id" }, "dbo.Products");
            DropForeignKey("dbo.ProductDetails", "FK_dbo.ProductDetails_dbo.Products_Product_TenantId_Product_Id");
            DropForeignKey("dbo.ProductEvents", new[] { "Product_TenantId", "Product_Id" }, "dbo.Products");
            DropForeignKey("dbo.ReportProducts", new[] { "ProductID", "TenantId" }, "dbo.Products");
            DropIndex("dbo.ClaimNumbers", "AK_ClaimNumberTenantProductEnvironmentAndNumberIndex");
            DropIndex("dbo.Releases", "AK_ReleaseProductAndNumber");
            DropIndex("dbo.InvoiceNumbers", "AK_InvoiceNumberTenantProductEnvironmentAndNumberIndex");
            DropIndex("dbo.PolicyNumbers", "AK_PolicyNumberTenantProductEnvironmentAndNumberIndex");
            DropIndex("dbo.Portals", new[] { "Tenant_Id" });
            DropIndex("dbo.TenantDetails", new[] { "Tenant_Id" });
            DropIndex("dbo.SettingDetails", new[] { "Tenant_Id" });
            DropIndex("dbo.ProductDetails", new[] { "Product_TenantId", "Product_Id" });
            DropIndex("dbo.ProductEvents", new[] { "Product_TenantId", "Product_Id" });
            DropIndex("dbo.UniqueIdentifiers", "AK_UniqueIdentifierTypeTenantProductEnvironmentAndIdentifierIndex");
            DropIndex("dbo.UserLoginEmails", "AK_UserTenantOrganisationAndEmailIndex");
            DropIndex("dbo.ReportProducts", new[] { "ProductID", "TenantId" });
            DropIndex("dbo.PolicyReadModels", "IX_PolicyReadModels_CreationDate");
            this.Sql($"UPDATE ProductDetails Set Alias = Product_Id where Alias is NULL");
            this.Sql($"DELETE from InvoiceNumbers where TenantId is NULL");
            this.Sql($"DELETE from PolicyNumbers where TenantId is NULL");
            this.Sql($"DELETE from ClaimNumbers where TenantId is NULL");

            // Deployments.
            DropColumn("dbo.Deployments", "Product_Id");
            RenameColumn(table: "dbo.Deployments", name: "ProductNewId", newName: "ProductId");
            SwapColumns("dbo.Deployments", "TenantNewId", "TenantId");

            // Tenants
            DropPrimaryKey("dbo.Tenants");
            UpdateTenantStringToGuidId("TenantDetails", "Tenant_Id");
            UpdateTenantStringToGuidId("ProductDetails", "Product_TenantId");
            UpdateTenantStringToGuidId("ReportProducts", "ProductID");
            UpdateTenantStringToGuidId("ProductEvents", "Product_TenantId");
            SwapColumns("dbo.Tenants", "NewId", "Id");
            AddPrimaryKey("dbo.Tenants", "Id");

            // Products
            DropPrimaryKey("dbo.Products");
            UpdateProductStringToGuidId("ProductDetails", "Product_Id", "Product_TenantId");
            UpdateProductStringToGuidId("ReportProducts", "TenantId", "ProductID");
            UpdateProductStringToGuidId("ProductEvents", "Product_Id", "Product_TenantId");
            SwapColumns("dbo.Products", "NewId", "Id");
            SwapColumns("dbo.Products", "TenantNewId", "TenantId");
            AddPrimaryKey("dbo.Products", new[] { "TenantId", "Id" });

            AlterColumn("dbo.ProductDetails", "Product_TenantId", c => c.Guid(nullable: false));
            AlterColumn("dbo.ProductDetails", "Product_Id", c => c.Guid(nullable: false));
            CreateIndex("dbo.ProductDetails", new[] { "Product_TenantId", "Product_Id" });
            AddForeignKey("dbo.ProductDetails", new[] { "Product_TenantId", "Product_Id" }, "dbo.Products", new[] { "TenantId", "Id" }, cascadeDelete: false);

            DropPrimaryKey("dbo.ReferenceNumberSequences");
            DropPrimaryKey("dbo.ReportProducts");

            RenameColumn(table: "dbo.AdditionalPropertyDefinitions", name: "TenantNewId", newName: "TenantId");

            RenameColumn("dbo.ProductPortalSettings", "ProductNewId", "ProductId");

            RenameColumn("dbo.TextAdditionalPropertyValueReadModels", "TenantNewId", "TenantId");

            SwapColumns("dbo.Applications", "ProductNewId", "ProductId");

            SwapColumns("dbo.ClaimFileAttachments", "TenantNewId", "TenantId");

            SwapColumns("dbo.ClaimNumbers", "ProductNewId", "ProductId");
            SwapColumns("dbo.ClaimNumbers", "TenantNewId", "TenantId");
            CreateIndex("dbo.ClaimNumbers", new[] { "TenantId", "ProductId", "Environment", "Number" }, unique: true, name: "AK_ClaimNumberTenantProductEnvironmentAndNumberIndex");

            SwapColumns("dbo.ClaimReadModels", "TenantNewId", "TenantId");
            SwapColumns("dbo.ClaimReadModels", "ProductNewId", "ProductId");

            SwapColumns("dbo.ClaimVersionReadModels", "TenantNewId", "TenantId");
            SwapColumns("dbo.ClaimVersionReadModels", "ProductNewId", "ProductId");

            SwapColumns("dbo.CreditNoteNumbers", "TenantNewId", "TenantId");
            SwapColumns("dbo.CreditNoteNumbers", "ProductNewId", "ProductId");

            SwapColumns("dbo.CustomerReadModels", "TenantNewId", "TenantId");

            SwapColumns("dbo.Releases", "TenantNewId", "TenantId");
            SwapColumns("dbo.Releases", "ProductNewId", "ProductId");
            CreateIndex("dbo.Releases", new[] { "TenantId", "ProductId", "Number", "MinorNumber" }, unique: true, name: "AK_ReleaseProductAndNumber");

            SwapColumns("dbo.DevReleases", "TenantNewId", "TenantId");
            SwapColumns("dbo.DevReleases", "ProductNewId", "ProductId");

            AlterColumn("dbo.EmailAddressBlockingEvents", "TenantNewId", c => c.Guid(nullable: false));
            SwapColumns("dbo.EmailAddressBlockingEvents", "TenantNewId", "TenantId");

            SwapColumns("dbo.Emails", "TenantNewId", "TenantId");
            SwapColumns("dbo.Emails", "ProductNewId", "ProductId");

            SwapColumns("dbo.SystemEmailTemplates", "TenantNewId", "TenantId");
            SwapColumns("dbo.SystemEmailTemplates", "ProductNewId", "ProductId");

            SwapColumns("dbo.InvoiceNumbers", "TenantNewId", "TenantId");
            SwapColumns("dbo.InvoiceNumbers", "ProductNewId", "ProductId");
            CreateIndex("dbo.InvoiceNumbers", new[] { "TenantId", "ProductId", "Environment", "Number" }, unique: true, name: "AK_InvoiceNumberTenantProductEnvironmentAndNumberIndex");

            SwapColumns("dbo.LoginAttemptResults", "TenantNewId", "TenantId");

            SwapColumns("dbo.QuoteReadModels", "TenantNewId", "TenantId");
            SwapColumns("dbo.QuoteReadModels", "ProductNewId", "ProductId");

            SwapColumns("dbo.OrganisationReadModels", "TenantNewId", "TenantId");

            SwapColumns("dbo.PasswordResetRecords", "TenantNewId", "TenantId");

            SwapColumns("dbo.PersonReadModels", "TenantNewId", "TenantId");

            SwapColumns("dbo.PolicyReadModels", "TenantNewId", "TenantId");
            SwapColumns("dbo.PolicyReadModels", "ProductNewId", "ProductId");

            SwapColumns("dbo.PolicyNumbers", "TenantNewId", "TenantId");
            SwapColumns("dbo.PolicyNumbers", "ProductNewId", "ProductId");
            CreateIndex("dbo.PolicyNumbers", new[] { "TenantId", "ProductId", "Environment", "Number" }, unique: true, name: "AK_PolicyNumberTenantProductEnvironmentAndNumberIndex");

            SwapColumns("dbo.PolicyTransactions", "TenantNewId", "TenantId");
            SwapColumns("dbo.PolicyTransactions", "ProductNewId", "ProductId");

            SwapColumns("dbo.Portals", "TenantNewId", "Tenant_Id");
            CreateIndex("dbo.Portals", "Tenant_Id");
            AddForeignKey("dbo.Portals", "Tenant_Id", "dbo.Tenants", "Id");

            AlterColumn("dbo.TenantDetails", "Tenant_Id", c => c.Guid(nullable: false));
            CreateIndex("dbo.TenantDetails", "Tenant_Id");
            AddForeignKey("dbo.TenantDetails", "Tenant_Id", "dbo.Tenants", "Id", cascadeDelete: true);

            SwapColumns("dbo.SettingDetails", "TenantNewId", "Tenant_Id");
            AlterColumn("dbo.SettingDetails", "Tenant_Id", c => c.Guid());
            CreateIndex("dbo.SettingDetails", "Tenant_Id");
            AddForeignKey("dbo.SettingDetails", "Tenant_Id", "dbo.Tenants", "Id");

            SwapColumns("dbo.ProductFeatureSettings", "TenantNewId", "TenantId");
            SwapColumns("dbo.ProductFeatureSettings", "ProductNewId", "ProductId");

            AlterColumn("dbo.ProductEvents", "Product_TenantId", c => c.Guid());
            AlterColumn("dbo.ProductEvents", "Product_Id", c => c.Guid());
            CreateIndex("dbo.ProductEvents", new[] { "Product_TenantId", "Product_Id" });
            AddForeignKey("dbo.ProductEvents", new[] { "Product_TenantId", "Product_Id" }, "dbo.Products", new[] { "TenantId", "Id" });

            SwapColumns("dbo.ReportReadModels", "TenantNewId", "TenantId");

            SwapColumns("dbo.QuoteDocumentReadModels", "TenantNewId", "TenantId");

            SwapColumns("dbo.Quotes", "TenantNewId", "TenantId");
            SwapColumns("dbo.Quotes", "ProductNewId", "ProductId");

            SwapColumns("dbo.QuoteVersionReadModels", "TenantNewId", "TenantId");
            SwapColumns("dbo.QuoteVersionReadModels", "ProductNewId", "ProductId");

            SwapColumns("dbo.ReferenceNumberSequences", "TenantNewId", "TenantId");
            SwapColumns("dbo.ReferenceNumberSequences", "ProductNewId", "ProductId");
            AddPrimaryKey("dbo.ReferenceNumberSequences", new[] { "TenantId", "ProductId", "Environment", "Method", "Number", "UseCase" });

            SwapColumns("dbo.Roles", "TenantNewId", "TenantId");

            SwapColumns("dbo.UserReadModels", "TenantNewId", "TenantId");

            SwapColumns("dbo.SystemAlerts", "TenantNewId", "TenantId");
            SwapColumns("dbo.SystemAlerts", "ProductNewId", "ProductId");

            SwapColumns("dbo.SystemEvents", "TenantNewId", "TenantId");
            SwapColumns("dbo.SystemEvents", "ProductNewId", "ProductId");

            SwapColumns("dbo.TokenSessions", "TenantNewId", "TenantId");

            SwapColumns("dbo.UniqueIdentifiers", "TenantNewId", "TenantId");
            SwapColumns("dbo.UniqueIdentifiers", "ProductNewId", "ProductId");
            CreateIndex("dbo.UniqueIdentifiers", new[] { "Type", "TenantId", "ProductId", "Environment", "Identifier" }, unique: true, name: "AK_UniqueIdentifierTypeTenantProductEnvironmentAndIdentifierIndex");

            SwapColumns("dbo.UserLoginEmails", "TenantNewId", "TenantId");
            CreateIndex("dbo.UserLoginEmails", new[] { "TenantId", "LoginEmail", "OrganisationId" }, unique: true, name: "AK_UserTenantOrganisationAndEmailIndex");

            AlterColumn("dbo.ReportProducts", "ProductID", c => c.Guid(nullable: false));
            AlterColumn("dbo.ReportProducts", "TenantId", c => c.Guid(nullable: false));
            AddPrimaryKey("dbo.ReportProducts", new[] { "ReportID", "ProductID", "TenantId" });
            CreateIndex("dbo.ReportProducts", new[] { "ProductID", "TenantId" });
            AddForeignKey("dbo.ReportProducts", new[] { "ProductID", "TenantId" }, "dbo.Products", new[] { "TenantId", "Id" }, cascadeDelete: true);


            // create back the index.
            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJobV1(
              this.addNonclusteredDBIndexForPolicyReadModels));

            this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJobV1(
                this.setProductIdsToGuidForReportReadModel));
        }

        // this should be ran before UpdateProductStringToGuidId
        private void UpdateTenantStringToGuidId(string table, string tenantIdColumn)
        {
            // table name should not have dbo prefix.
            this.Sql($"UPDATE {table} SET {tenantIdColumn} = t.NewId FROM {table} td INNER JOIN Tenants t ON t.Id = td.{tenantIdColumn}");
        }

        // this should be ran after UpdateTenantStringToGuidId
        private void UpdateProductStringToGuidId(string table, string productIdColumn, string tenantIdColumn)
        {
            // table name should not have dbo prefix.
            this.Sql($"UPDATE {table} SET {productIdColumn} = p.NewId FROM {table} td INNER JOIN Products p ON p.Id = td.{productIdColumn} and p.TenantNewId = td.{tenantIdColumn}");
        }

        private void SwapColumns(string table, string column, string anotherColumn)
        {
            DropColumn(table, anotherColumn);
            RenameColumn(table: table, name: column, newName: anotherColumn);
        }

        public override void Down()
        {
        }
    }
}
