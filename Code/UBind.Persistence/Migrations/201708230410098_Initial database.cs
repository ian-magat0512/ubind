// <auto-generated />
#pragma warning disable 1591

namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Initialdatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Applications",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ProductId = c.String(),
                        Environment = c.Int(nullable: false),
                        CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CalculationResults",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        FormDataId = c.Guid(nullable: false),
                        Json = c.String(),
                        CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                        Application_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Applications", t => t.Application_Id)
                .Index(t => t.Application_Id);
            
            CreateTable(
                "dbo.FormDatas",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Json = c.String(),
                        CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                        Application_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Applications", t => t.Application_Id)
                .Index(t => t.Application_Id);
            
            CreateTable(
                "dbo.Deployments",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Enivronment = c.Int(nullable: false),
                        CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                        Release_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Releases", t => t.Release_Id)
                .Index(t => t.Release_Id);
            
            CreateTable(
                "dbo.Releases",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ProductId = c.String(),
                        SpreadsheetPath = c.String(),
                        CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProductDetails",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                        Product_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Products", t => t.Product_Id, cascadeDelete: true)
                .Index(t => t.Product_Id);
            
            CreateTable(
                "dbo.ProductEvents",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Type = c.Int(nullable: false),
                        CreationTimeInTicksSinceEpoch = c.Long(nullable: false),
                        Product_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Products", t => t.Product_Id)
                .Index(t => t.Product_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProductEvents", "Product_Id", "dbo.Products");
            DropForeignKey("dbo.ProductDetails", "Product_Id", "dbo.Products");
            DropForeignKey("dbo.Deployments", "Release_Id", "dbo.Releases");
            DropForeignKey("dbo.FormDatas", "Application_Id", "dbo.Applications");
            DropForeignKey("dbo.CalculationResults", "Application_Id", "dbo.Applications");
            DropIndex("dbo.ProductEvents", new[] { "Product_Id" });
            DropIndex("dbo.ProductDetails", new[] { "Product_Id" });
            DropIndex("dbo.Deployments", new[] { "Release_Id" });
            DropIndex("dbo.FormDatas", new[] { "Application_Id" });
            DropIndex("dbo.CalculationResults", new[] { "Application_Id" });
            DropTable("dbo.ProductEvents");
            DropTable("dbo.ProductDetails");
            DropTable("dbo.Products");
            DropTable("dbo.Releases");
            DropTable("dbo.Deployments");
            DropTable("dbo.FormDatas");
            DropTable("dbo.CalculationResults");
            DropTable("dbo.Applications");
        }
    }
}
