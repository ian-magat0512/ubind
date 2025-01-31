﻿// <auto-generated />
namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedDataTypeOfEntityTypeContextIdAndParentContextId : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AdditionalPropertyDefinitions", "ContextId", c => c.Guid(nullable: false));
            AlterColumn("dbo.AdditionalPropertyDefinitions", "ParentContextId", c => c.Guid());
            AlterColumn("dbo.TextAdditionalPropertyValueReadModels", "EntityId", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.TextAdditionalPropertyValueReadModels", "EntityId", c => c.String());
            AlterColumn("dbo.AdditionalPropertyDefinitions", "ParentContextId", c => c.String());
            AlterColumn("dbo.AdditionalPropertyDefinitions", "ContextId", c => c.String());
        }
    }
}
