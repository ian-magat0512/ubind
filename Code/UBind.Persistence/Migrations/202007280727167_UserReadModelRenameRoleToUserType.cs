// <copyright file="202007280727167_UserReadModelRenameRoleToUserType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Renames the "Role" field of the UserReadModels table to "UserType"
    /// since roles are now objects in ubind with a different meaning.
    /// </summary>
    public partial class UserReadModelRenameRoleToUserType : DbMigration
    {
        /// <inheritdoc/>
        public override void Up()
        {
            this.RenameColumn("dbo.UserReadModels", "Role", "UserType");
        }

        /// <inheritdoc/>
        public override void Down()
        {
            this.RenameColumn("dbo.UserReadModels", "UserType", "Role");
        }
    }
}
