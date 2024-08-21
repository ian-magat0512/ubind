﻿// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// A customer must always have an environment specified, because the quote the customer 
    /// creates is in a particular environment. However users which are not customers 
    /// (e.g. Client Admins, Agents, and Master tenancy admins) should not be storing the environment
    /// against there user account, because this is irrelevant information. Furthermore, it causes
    /// problems with logic in the portal where we restrict access to certain environments. If, for
    /// example the user account has "Production" associated with it, then that user will only be able
    /// to access the production environment, which doesn't make sense for a Client Admin who may
    /// be testing a potential product release.
    /// 
    /// In this migration, we will be setting the environment to null for all users who are not Customers.
    /// To do this we must make the Environment field nullable in the database.
    /// </summary>    
    public partial class MakeEnvironmentNullForNonCustomerUsers : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.UserReadModels", "Environment", c => c.Int());
            
            Sql("UPDATE dbo.UserReadModels SET Environment = NULL"
                + " WHERE UserType != 'Customer'");
        }

        public override void Down()
        {
            Sql("UPDATE dbo.UserReadModels SET Environment = '3'"
                + " WHERE UserType != 'Customer'");

            AlterColumn("dbo.UserReadModels", "Environment", c => c.Int(nullable: false));
        }
    }
}
