// <copyright file="QuoteNumberSequenceGeneratorTestsSlow.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using MoreLinq;
    using UBind.Domain;
    using UBind.Domain.NumberGenerators;
    using Xunit;

    public class QuoteNumberSequenceGeneratorTestsSlow : IDisposable
    {
        private const DeploymentEnvironment Environment = DeploymentEnvironment.Staging;
        private readonly Guid tenantId = Guid.NewGuid();
        private readonly Guid productId = Guid.NewGuid();

        public void Dispose()
        {
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var entities = dbContext
                    .Set<ReferenceNumberSequence>()
                    .Where(qns => qns.TenantId == this.tenantId)
                    .Where(qns => qns.ProductId == this.productId)
                    .Where(qns => qns.Environment == Environment);
                dbContext
                    .Set<ReferenceNumberSequence>()
                    .RemoveRange(entities);
            }
        }

        [Fact(Skip = "Very slow test. Only run when required.")]
        public void GenerateQuoteNumber_GeneratesUniqueCode_ForAllSeedsFromZeroTo308915775()
        {
            // Arrange
            const string TableName = "QuoteNumberTests";
            using (var sqlConnection = new SqlConnection(DatabaseFixture.TestConnectionString))
            {
                sqlConnection.Open();
                using (var dropTableCommand = new SqlCommand($"IF OBJECT_ID('dbo.{TableName}', 'U') IS NOT NULL DROP TABLE dbo.{TableName}; ", sqlConnection))
                {
                    dropTableCommand.ExecuteNonQuery();
                }

                using (var createTableCommand = new SqlCommand($"CREATE TABLE {TableName}(Number char(6), Seed int NOT NULL UNIQUE, PRIMARY KEY (Number));", sqlConnection))
                {
                    createTableCommand.ExecuteNonQuery();
                }
            }

            int batchSize = 10 * 1000;
            var total = 308915775;
            var seeds = Enumerable.Range(0, total);
            var dataTable = new DataTable(TableName);
            dataTable.Columns.Add("Number", typeof(string));
            dataTable.Columns.Add("Seed", typeof(int));

            // Act
            foreach (var batch in seeds.Batch(batchSize))
            {
                foreach (var seed in batch)
                {
                    var sut = new QuoteReferenceNumberGenerator(
                        new UniqueNumberSequenceGenerator(DatabaseFixture.TestConnectionString));

                    sut.SetProperties(this.tenantId, this.productId, Environment);
                    var number = sut.Generate();
                    var row = dataTable.NewRow();
                    row[0] = number;
                    row[1] = seed;
                    dataTable.Rows.Add(row);
                }

                using (var sqlConnection2 = new SqlConnection(DatabaseFixture.TestConnectionString))
                {
                    sqlConnection2.Open();
                    using (var bulkCopy = new SqlBulkCopy(sqlConnection2, SqlBulkCopyOptions.Default, null))
                    {
                        bulkCopy.DestinationTableName = TableName;
                        bulkCopy.WriteToServer(dataTable);
                    }
                }

                dataTable.Clear();
            }

            // No Assert: If no exception is thrown then 308,915,775 unique numbers have been generated.
        }
    }
}
