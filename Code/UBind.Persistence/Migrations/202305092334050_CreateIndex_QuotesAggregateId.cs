// <copyright file="202305092334050_CreateIndex_QuotesAggregateId.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class CreateIndex_QuotesAggregateId : DbMigration
    {
        public override void Up()
        {
            var sql = "CREATE NONCLUSTERED INDEX [IX_Quotes_AggregateId] ON [dbo].[Quotes] ([AggregateId]) INCLUDE([QuoteNumber])";
            this.Sql(sql);
        }

        public override void Down()
        {
            this.Sql("DROP INDEX [IX_Quotes_AggregateId] ON [dbo].[Quotes]");
        }
    }
}
