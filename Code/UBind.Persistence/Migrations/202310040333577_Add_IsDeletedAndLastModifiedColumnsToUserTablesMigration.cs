// <copyright file="202310040333577_Add_IsDeletedAndLastModifiedColumnsToUserTablesMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations;

using System.Data.Entity.Migrations;
using UBind.Persistence.Helpers;

public partial class Add_IsDeletedAndLastModifiedColumnsToUserTablesMigration : DbMigration
{
    private const string AddUserDeletedEventsAlias = "AddUserDeletedEvents";

    public override void Up()
    {
        this.AddColumn("dbo.UserReadModels", "IsDeleted", c => c.Boolean(nullable: false));
        this.AddColumn("dbo.UserLoginEmails", "IsDeleted", c => c.Boolean(nullable: false));
        this.AddColumn("dbo.UserLoginEmails", "LastModifiedTicksSinceEpoch", c => c.Long(nullable: false));

        // Copy values from CreatedTicksSinceEpoch to LastModifiedTicksSinceEpoch
        this.Sql("UPDATE dbo.UserLoginEmails SET LastModifiedTicksSinceEpoch = CreatedTicksSinceEpoch");

        // Delete duplicates, a Person can only have 1 user account. Also delete the related aggregate events
        this.Sql(
@"SELECT PersonId, Id, HasBeenActivated, ROW_NUMBER() OVER (PARTITION BY PersonId ORDER BY HasBeenActivated) AS RowNum
INTO #TempTable FROM UserReadModels;

SELECT Id INTO #TempIds FROM UserReadModels
WHERE HasBeenActivated = 0 AND PersonId IN (SELECT PersonId FROM #TempTable WHERE RowNum > 1);

DELETE FROM EventRecordWithGuidIds WHERE EXISTS (SELECT Id FROM #TempIds WHERE Id=AggregateId);

DELETE u FROM UserReadModels u WHERE EXISTS (SELECT Id FROM #TempIds WHERE Id=u.Id);

DROP TABLE #TempTable;
DROP TABLE #TempIds;");

        this.Sql(
            StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(AddUserDeletedEventsAlias));
    }

    public override void Down()
    {
        this.DropColumn("dbo.UserLoginEmails", "LastModifiedTicksSinceEpoch");
        this.DropColumn("dbo.UserLoginEmails", "IsDeleted");
        this.DropColumn("dbo.UserReadModels", "IsDeleted");

        this.Sql(
            StartupJobRunnerQueryHelper.GenerateDeleteQueryForStartupJob(AddUserDeletedEventsAlias));
    }
}
