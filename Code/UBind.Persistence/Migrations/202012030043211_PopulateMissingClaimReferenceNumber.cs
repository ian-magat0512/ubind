﻿// <auto-generated />
#pragma warning disable 1591
namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.IO;

    public partial class PopulateMissingClaimReferenceNumber : DbMigration
    {
        public override void Up()
        {
            var sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations", @"populate_missing_claim_reference.sql");
            this.Sql(File.ReadAllText(sqlFile));
        }
        
        public override void Down()
        {
        }
    }
}
