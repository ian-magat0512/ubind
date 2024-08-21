﻿// <auto-generated />
#pragma warning disable 1591


namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class UpdateQuotesPolicyAndClaimsCorrectOwnerUserIdFromCustomerReadModel : DbMigration
    {
        public override void Up()
        {
            Sql
            (
              @"UPDATE quote
                SET
                    quote.OwnerUserId = customer.OwnerUserId
                FROM 
                    dbo.Quotes quote
                    INNER JOIN dbo.CustomerReadModels customer
                        ON quote.CustomerId = customer.Id and quote.OwnerUserId != customer.OwnerUserId"
            );

            Sql
            (
              @"UPDATE policy
                SET
                    policy.OwnerUserId = customer.OwnerUserId
                FROM
                    dbo.PolicyReadModels policy
                    INNER JOIN dbo.CustomerReadModels customer
                        ON policy.CustomerId = customer.Id and policy.OwnerUserId != customer.OwnerUserId"
            );

            Sql
            (
              @"UPDATE claim
                SET
                    claim.OwnerUserId = customer.OwnerUserId
                FROM 
                    dbo.ClaimReadModels claim
                    INNER JOIN dbo.CustomerReadModels customer
                        ON claim.CustomerId = customer.Id and claim.OwnerUserId != customer.OwnerUserId"
            );
        }
        
        public override void Down()
        {
        }
    }
}
