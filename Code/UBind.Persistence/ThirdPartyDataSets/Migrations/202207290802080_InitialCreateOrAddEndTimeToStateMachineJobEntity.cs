﻿// <auto-generated />

namespace UBind.Persistence.ThirdPartyDataSets
{
    using System;
    using System.Data.Entity.Migrations;
    using System.IO;

    public partial class InitialCreateOrAddEndTimeToStateMachineJobEntity : DbMigration
    {
        public override void Up()
        {
            var createStateMachineJobTableSqlFile = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "ThirdPartyDataSets", "Migrations", @"202207290802080_CreateStateMachineJobTable.sql");
            this.Sql(File.ReadAllText(createStateMachineJobTableSqlFile));
        }

        public override void Down()
        {
            DropTable("JobScheduler.StateMachineJob");
        }
    }
}

