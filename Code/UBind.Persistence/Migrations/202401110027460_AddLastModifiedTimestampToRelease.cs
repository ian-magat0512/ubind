// <copyright file="202401110027460_AddLastModifiedTimestampToRelease.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations;

using System.Data.Entity.Migrations;

public partial class AddLastModifiedTimestampToRelease : DbMigration
{
    public override void Up()
    {
        this.AddColumn("dbo.Releases", "LastModifiedTicksSinceEpoch", c => c.Long(nullable: false));
        this.AddColumn("dbo.DevReleases", "LastModifiedTicksSinceEpoch", c => c.Long(nullable: false));
    }

    public override void Down()
    {
        this.DropColumn("dbo.DevReleases", "LastModifiedTicksSinceEpoch");
        this.DropColumn("dbo.Releases", "LastModifiedTicksSinceEpoch");
    }
}
