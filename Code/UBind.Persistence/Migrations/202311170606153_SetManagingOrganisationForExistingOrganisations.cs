// <copyright file="202311170606153_SetManagingOrganisationForExistingOrganisations.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations;

using System.Data.Entity.Migrations;
using UBind.Persistence.Helpers;

public partial class SetManagingOrganisationForExistingOrganisations : DbMigration
{
    private const string AssignManagingOrganisationForExistingOrganisations = "AssignManagingOrganisationForExistingOrganisations_20231117";

    public override void Up()
    {
        // Managing Organisations added for existing orgs.
        this.Sql(StartupJobRunnerQueryHelper.GenerateInsertQueryForStartupJob(
                AssignManagingOrganisationForExistingOrganisations, precedingStartupJobAliases: new List<string> { "SetDefaultOrganisations_20230516" }));
    }

    public override void Down()
    {
    }
}
