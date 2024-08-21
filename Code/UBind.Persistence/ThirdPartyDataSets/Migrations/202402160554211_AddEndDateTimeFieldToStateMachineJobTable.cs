﻿// <auto-generated />
#pragma warning disable 1591
namespace UBind.Persistence.ThirdPartyDataSets;

using System.Data.Entity.Migrations;

public partial class AddEndDateTimeFieldToStateMachineJobTable : DbMigration
{
    public override void Up()
    {
        AddColumn("JobScheduler.StateMachineJob", "EndTicksSinceEpoch", c => c.Long());
    }

    public override void Down()
    {
        DropColumn("JobScheduler.StateMachineJob", "EndTicksSinceEpoch");
    }
}