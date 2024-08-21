﻿// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations;

using System.Data.Entity.Migrations;
using UBind.Persistence.Helpers;

public partial class RemoveDuplicateFileContents : DbMigration
{
    private const string jobAlias = "RemoveDuplicateFileContents_06092024";

    public override void Up()
    {
        this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(jobAlias));
    }

    public override void Down()
    {
        this.Sql(StartupJobRunnerQueryHelper.GenerateDeleteQueryForStartupJob(jobAlias));
    }
}
