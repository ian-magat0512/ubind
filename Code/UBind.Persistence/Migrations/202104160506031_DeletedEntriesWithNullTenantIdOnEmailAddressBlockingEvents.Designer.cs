﻿// <auto-generated />
namespace UBind.Persistence.Migrations
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;
    
    [GeneratedCode("EntityFramework.Migrations", "6.4.0")]
    public sealed partial class DeletedEntriesWithNullTenantIdOnEmailAddressBlockingEvents : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(DeletedEntriesWithNullTenantIdOnEmailAddressBlockingEvents));
        
        string IMigrationMetadata.Id
        {
            get { return "202104160506031_DeletedEntriesWithNullTenantIdOnEmailAddressBlockingEvents"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return Resources.GetString("Target"); }
        }
    }
}
