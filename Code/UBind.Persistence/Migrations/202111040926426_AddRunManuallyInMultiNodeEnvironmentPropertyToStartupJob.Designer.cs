﻿// <auto-generated />
namespace UBind.Persistence.Migrations
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;
    
    [GeneratedCode("EntityFramework.Migrations", "6.4.0")]
    public sealed partial class AddRunManuallyInMultiNodeEnvironmentPropertyToStartupJob : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(AddRunManuallyInMultiNodeEnvironmentPropertyToStartupJob));
        
        string IMigrationMetadata.Id
        {
            get { return "202111040926426_AddRunManuallyInMultiNodeEnvironmentPropertyToStartupJob"; }
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
