// <copyright file="202407120101391_CreateMemoryCachingEnabledColumnForDataTableDefinition.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;
    using UBind.Persistence.Migrations.Extensions;

    public partial class CreateMemoryCachingEnabledColumnForDataTableDefinition : DbMigration
    {
        public override void Up()
        {
            this.AddColumnIfNotExists("dbo.DataTableDefinitions", "MemoryCachingEnabled", c => c.Boolean(nullable: false));
            this.AddColumnIfNotExists("dbo.DataTableDefinitions", "CacheExpiryInSeconds", c => c.Int(nullable: false));
        }

        public override void Down()
        {
            this.DropColumnIfExists("dbo.DataTableDefinitions", "CacheExpiryInSeconds");
            this.DropColumnIfExists("dbo.DataTableDefinitions", "MemoryCachingEnabled");
        }
    }
}

