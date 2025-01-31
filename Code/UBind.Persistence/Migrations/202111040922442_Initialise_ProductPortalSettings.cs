﻿// <auto-generated />

namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;
    using NodaTime;
    using UBind.Domain.Extensions;

    public partial class Initialise_ProductPortalSettings : DbMigration
    {
        private const string tableName = "ProductPortalSettings";

        public override void Up()
        {
            // All Products in the "Allow New Quotes For Products" section should be enabled for existing portals
            var timestamp = SystemClock.Instance.Now().ToUnixTimeTicks();
            var sql =
                $"INSERT INTO {tableName} (Id, TenantId, PortalId, ProductNewId, IsNewQuotesAllowed, CreationTimeInTicksSinceEpoch) " +
                $"SELECT NEWID(), prd.TenantNewId, ptl.Id, prd.NewId, 1, {timestamp} " +
                "FROM Portals ptl " +
                "INNER JOIN Products prd on prd.TenantId = ptl.Tenant_Id " +
                $"WHERE NOT EXISTS(SELECT 1 FROM {tableName} WHERE PortalId = ptl.Id AND ProductNewId = prd.NewId)";

            this.Sql(sql);
        }

        public override void Down()
        {
            this.Sql($"DELETE FROM {tableName}");
        }
    }
}
