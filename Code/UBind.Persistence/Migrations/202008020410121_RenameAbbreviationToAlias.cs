// <copyright file="202008020410121_RenameAbbreviationToAlias.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Defines the <see cref="RenameAbbreviationToAlias" />.
    /// </summary>
    public partial class RenameAbbreviationToAlias : DbMigration
    {
        /// <summary>
        /// The Up.
        /// </summary>
        public override void Up()
        {
            this.RenameColumn("dbo.PortalDetails", "Abbreviation", "Alias");
            this.RenameColumn("dbo.TenantDetails", "Abbreviation", "Alias");
        }

        /// <summary>
        /// The Down.
        /// </summary>
        public override void Down()
        {
            this.RenameColumn("dbo.PortalDetails", "Alias", "Abbreviation");
            this.RenameColumn("dbo.TenantDetails", "Alias", "Abbreviation");
        }
    }
}
