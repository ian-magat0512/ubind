// <copyright file="202006280843256_RenameUserIsBlockedToIsDisabled.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1600 // Elements should be documented

namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Renames the column "isBlocked" in the UserReadModels table to "isDisabled" to avoid
    /// confusion with account locking from multiple failed login attempts which is a different thing.
    /// </summary>
    public partial class RenameUserIsBlockedToIsDisabled : DbMigration
    {
        public override void Up()
        {
            this.RenameColumn("dbo.UserReadModels", "IsBlocked", "IsDisabled");
        }

        public override void Down()
        {
            this.RenameColumn("dbo.UserReadModels", "IsDisabled", "IsBlocked");
        }
    }
}
