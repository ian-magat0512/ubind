﻿// <auto-generated />
#pragma warning disable 1591
namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class AddSavedPaymentMethod : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SavedPaymentMethods",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        TenantId = c.Guid(nullable: false),
                        CustomerId = c.Guid(nullable: false),
                        PaymentMethodId = c.Guid(nullable: false),
                        IdentificationDataJson = c.String(),
                        AuthenticationDataJson = c.String(),
                        LastModifiedTicksSinceEpoch = c.Long(nullable: false),
                        CreatedTicksSinceEpoch = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SavedPaymentMethods");
        }
    }
}
