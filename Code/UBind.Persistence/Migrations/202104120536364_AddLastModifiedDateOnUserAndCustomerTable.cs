﻿// <auto-generated />
#pragma warning disable 1591
namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;
    using UBind.Persistence.Migrations.Extensions;

    public partial class AddLastModifiedDateOnUserAndCustomerTable : DbMigration
    {
        public override void Up()
        {
            this.AddColumnIfNotExists("dbo.CustomerReadModels", "LastModifiedTimeInTicksSinceEpoch", c => c.Long(nullable: false));
            this.AddColumnIfNotExists("dbo.UserReadModels", "LastModifiedTimeInTicksSinceEpoch", c => c.Long(nullable: false));

            var populateCustomerLastModifiedColumn = @"UPDATE dbo.CustomerReadModels
                            SET 
                                LastModifiedTimeInTicksSinceEpoch = CreationTimeInTicksSinceEpoch
                            WHERE LastModifiedTimeInTicksSinceEpoch = 0";

            var populateUserLastModifiedColumn = @"UPDATE dbo.UserReadModels
                            SET 
                                LastModifiedTimeInTicksSinceEpoch = CreationTimeInTicksSinceEpoch
                            WHERE LastModifiedTimeInTicksSinceEpoch = 0";

            Sql(populateCustomerLastModifiedColumn);
            Sql(populateUserLastModifiedColumn);
        }

        public override void Down()
        {
            DropColumn("dbo.UserReadModels", "LastModifiedTimeInTicksSinceEpoch");
            DropColumn("dbo.CustomerReadModels", "LastModifiedTimeInTicksSinceEpoch");
        }
    }
}
