// <copyright file="GuidIdMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Dapper;
    using Microsoft.Extensions.Logging;
    using UBind.Domain;
    using UBind.Domain.Helpers;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class GuidIdMigration : Domain.IGuidIdMigration
    {
        private readonly IConnectionConfiguration connection;
        private readonly IUBindDbContext dbContext;
        private readonly ILogger<GuidIdMigration> logger;
        private readonly int pageCount = 100;
        private List<Tenant> tenants = new List<Tenant>();
        private List<UBind.Domain.Product.Product> products = new List<UBind.Domain.Product.Product>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidIdMigration"/> class.
        /// </summary>
        /// <param name="dbContext">The db context.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="connection">The connection configuration.</param>
        public GuidIdMigration(
            IUBindDbContext dbContext,
            ILogger<GuidIdMigration> logger,
            IConnectionConfiguration connection)
        {
            this.connection = connection;
            this.dbContext = dbContext;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public void ApplyNewIdsOnMultipleRecords()
        {
            this.logger.LogInformation($"Sql NewIdMigration.sql executing.");
            var directory = "Migrations/AddNewIdForTenantAndProductAndOtherEntitiesQueries";
            var sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directory, @"NewIdMigration.sql");
            var setupSql = System.IO.File.ReadAllText(sqlFile);

            using (var connection = new SqlConnection(this.connection.UBind))
            {
                var setupRowsAffected = connection.Execute(setupSql, null, null, 1200, System.Data.CommandType.Text);
                this.logger.LogInformation($"sql NewIdMigration.sql executed.");
            }

            this.tenants = this.dbContext.Tenants.ToList();
            this.products = this.dbContext.Products.ToList();

            this.StartMigrationForTable("ClaimNumbers");
            this.StartMigrationForTable("InvoiceNumbers");
            this.StartMigrationForTable("PolicyNumbers");
            this.StartMigrationForTable("CreditNoteNumbers");
            this.StartMigrationForTable("Deployments");
            this.StartMigrationForTable("DevReleases");
            this.StartMigrationForTable("Releases");
            this.StartMigrationForTable("PasswordResetRecords");
            this.StartMigrationForTable("LoginAttemptResults");
            this.StartMigrationForTable("ReferenceNumberSequences");
            this.StartMigrationForTable("Roles");
            this.StartMigrationForTable("SystemAlerts");
            this.StartMigrationForTable("SystemEmailTemplates");
            this.StartMigrationForTable("SettingDetails");
            this.StartMigrationForTable("TokenSessions");
            this.StartMigrationForTable("SystemEvents");
            this.StartMigrationForTable("UniqueIdentifiers");
            this.StartMigrationForTable("UserLoginEmails");
            this.StartMigrationForTable("Emails");
            this.StartMigrationForTable("EmailAddressBlockingEvents");
            this.StartMigrationForTable("ProductFeatureSettings");
        }

        private void StartMigrationForTable(string tableName)
        {
            this.logger.LogInformation("Starting " + tableName + " Migration...");
            void Retry()
            {
                foreach (var tenant in this.tenants)
                {
                    var tenantsProducts = this.products.Where(x => x.TenantId == tenant.Id).ToList();

                    if (!tenantsProducts.Any())
                    {
                        int rowsAffected = 1;
                        while (rowsAffected != 0)
                        {
                            rowsAffected = this.ExecuteUpdateQuery(tableName, tenant);

                            if (rowsAffected == 0)
                            {
                                break;
                            }

                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        foreach (var tenantsProduct in tenantsProducts)
                        {
                            int rowsAffected = 1;
                            while (rowsAffected != 0)
                            {
                                rowsAffected = this.ExecuteUpdateQuery(tableName, tenant, tenantsProduct);

                                if (rowsAffected == 0)
                                {
                                    break;
                                }

                                Thread.Sleep(1000);
                            }
                        }
                    }
                }
            }

            RetryPolicyHelper.Execute<Exception>(() => Retry(), minJitter: 500, maxJitter: 3000);
        }

        private int ExecuteUpdateQuery(string tableName, Tenant tenant, UBind.Domain.Product.Product product = null)
        {
            using (var connection = new SqlConnection(this.connection.UBind))
            {
                var p = new DynamicParameters();
                int rowsAffected = 0;
                p.Add("@tableName", tableName);
                p.Add("@pageSize", this.pageCount);
                p.Add("@tmpTenantId", tenant.Id);
                p.Add("@tmpTenantNewId", tenant.Id);
                rowsAffected = connection.Execute("SP_UpdateTenantNewId", p, commandType: CommandType.StoredProcedure);
                this.logger.LogInformation($"SP SP_UpdateTenantNewId on '{tableName}' table executed with ({rowsAffected}) rows affected.");

                if (product != null)
                {
                    p = new DynamicParameters();
                    p.Add("@tableName", tableName);
                    p.Add("@pageSize", this.pageCount);
                    p.Add("@tmpTenantId", tenant.Id);
                    p.Add("@tmpProductId", product.Id);
                    p.Add("@tmpProductNewId", product.Id);
                    var productRowsAffected = connection.Execute("SP_UpdateProductNewId", p, commandType: CommandType.StoredProcedure);
                    this.logger.LogInformation($"SP SP_UpdateProductNewId '{tableName}' table executed with ({productRowsAffected}) rows affected.");

                    rowsAffected = productRowsAffected > rowsAffected ? productRowsAffected : rowsAffected;
                }

                return rowsAffected;
            }
        }
    }
}
