// <copyright file="NumberPoolRepositoryBase.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Transactions;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Product;
    using UBind.Domain.Repositories;

    /// <summary>
    /// base of reference number repositories.
    /// </summary>
    /// <typeparam name="TNumber">the type of number.</typeparam>
    public abstract class NumberPoolRepositoryBase<TNumber> : INumberPoolRepository
        where TNumber : class, IReferenceNumber, new()
    {
        private readonly IUBindDbContext dbContext;
        private readonly IClock clock;
        private readonly IConnectionConfiguration connectionConfiguration;
        private DbSet<TNumber> dbSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberPoolRepositoryBase{T}"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="connectionConfiguration">SQL connection configuration.</param>
        /// <param name="clock">Clock for obtaining the current time.</param>
        public NumberPoolRepositoryBase(
            IUBindDbContext dbContext,
            IConnectionConfiguration connectionConfiguration,
            IClock clock)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<TNumber>();
            this.clock = clock;
            this.connectionConfiguration = connectionConfiguration;
        }

        /// <summary>
        /// Gets field of prefix for seeding purposes.
        /// </summary>
        public abstract string Prefix { get; }

        /// <summary>
        /// seed default data.
        /// </summary>
        /// <param name="tenantId">tenant id.</param>
        /// <param name="productId">product id.</param>
        /// <param name="environment">environment.</param>
        public void Seed(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            List<string> numbers = new List<string>();
            for (int i = 0; i < 1000; i++)
            {
                numbers.Add(this.Prefix + i.ToString("D4"));
            }

            this.LoadForProduct(tenantId, productId, environment, numbers);
        }

        /// <inheritdoc/>
        public string? ConsumeForProduct(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            var availableNumber = this.dbSet
                .Where(n => n.TenantId == tenantId)
                .Where(n => n.ProductId == productId)
                .Where(n => n.Environment == environment)
                .Where(n => n.IsAssigned == false)
                .OrderBy(n => n.Number)
                .FirstOrDefault();

            if (availableNumber == null)
            {
                throw new ReferenceNumberUnavailableException(
                    Errors.NumberPool.NoneAvailable(
                        tenantId.ToString(),
                        productId.ToString(),
                        typeof(TNumber).Name.Replace("Number", string.Empty)));
            }

            var numberValue = availableNumber.Consume();
            return numberValue;
        }

        /// <inheritdoc/>
        public void Unconsume(IProductContext productContext, string number)
        {
            // get the record from the database.
            var record = this.dbSet
            .Where(n => n.TenantId == productContext.TenantId)
            .Where(n => n.ProductId == productContext.ProductId)
            .Where(n => n.Environment == productContext.Environment)
            .Where(x => x.Number == number)
            .Where(n => n.IsAssigned)
            .FirstOrDefault();

            if (record != null)
            {
                record.UnConsume();
            }
        }

        /// <inheritdoc/>
        public string? ConsumeAndSave(IProductContext productContext)
        {
            using (var scope = new TransactionScope())
            {
                string tableName = typeof(TNumber).Name + "s"; // e.g. PolicyNumbers
                using (var connection = new SqlConnection(this.connectionConfiguration.UBind))
                {
                    connection.Open();
                    var environment = (int)productContext.Environment;

                    // retrieve first usable reference number
                    var selectQuery = $@"SELECT TOP 1 Number
                        FROM {tableName} WITH (UPDLOCK, ROWLOCK)
                        WHERE TenantId = '{productContext.TenantId}'
                        AND ProductId = '{productContext.ProductId}'
                        AND Environment = {environment}
                        AND IsAssigned = 0
                        ORDER BY Number;";

                    using (var selectCommand = new SqlCommand(selectQuery, connection))
                    {
                        var numberObj = selectCommand.ExecuteScalar();
                        if (numberObj == null)
                        {
                            throw new ReferenceNumberUnavailableException(
                                Errors.NumberPool.NoneAvailable(
                                    productContext.TenantId.ToString(),
                                    productContext.ProductId.ToString(),
                                    typeof(TNumber).Name.Replace("Number", string.Empty)));
                        }

                        string number = (string)numberObj;

                        // assign it as used.
                        var updateQuery =
                            $@"UPDATE {tableName} WITH (ROWLOCK)
                            SET IsAssigned = 1
                            WHERE TenantId = '{productContext.TenantId}'
                            AND ProductId = '{productContext.ProductId}'
                            AND Environment = {environment}
                            AND Number = '{number}'";

                        using (var updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.ExecuteNonQuery();
                        }

                        // Commit the transaction
                        scope.Complete();
                        return number;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void UnconsumeAndSave(IProductContext productContext, string number)
        {
            using (var scope = new TransactionScope())
            {
                var environment = (int)productContext.Environment;
                string tableName = typeof(TNumber).Name + "s"; // e.g. PolicyNumbers
                using (var connection = new SqlConnection(this.connectionConfiguration.UBind))
                {
                    connection.Open();

                    // unassign the number
                    var updateQuery =
                        $@"UPDATE {tableName} WITH (ROWLOCK)
                        SET IsAssigned = 0
                        WHERE TenantId = '{productContext.TenantId}'
                        AND ProductId = '{productContext.ProductId}'
                        AND Environment = {environment}
                        AND Number = '{number}'";

                    using (var updateCommand = new SqlCommand(updateQuery, connection))
                    {
                        updateCommand.ExecuteNonQuery();
                    }

                    scope.Complete();
                }
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetAvailableForProduct(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            return this.dbSet.Where(
                number =>
                number.TenantId == tenantId &&
                number.ProductId == productId &&
                number.Environment == environment &&
                !number.IsAssigned)
            .Select(number => number.Number)
            .ToList();
        }

        /// <inheritdoc/>
        public int GetAvailableReferenceNumbersCount(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            return this.dbSet.Where(
                number =>
                number.TenantId == tenantId &&
                number.ProductId == productId &&
                number.Environment == environment &&
                !number.IsAssigned)
            .Count();
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetAllForProduct(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            return this.dbSet.Where(number => number.TenantId == tenantId &&
            number.ProductId == productId &&
            number.Environment == environment)
                .Select(number => number.Number)
                .ToList();
        }

        /// <inheritdoc/>
        public NumberPoolAddResult LoadForProduct(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            IEnumerable<string> numbers)
        {
            var duplicateNumbers = this.GetDuplicateNumbers(tenantId, productId, environment, numbers);
            var addedNumbers = numbers.Except(duplicateNumbers);
            var createdTimestamp = this.clock.GetCurrentInstant();
            string tableName = typeof(TNumber).Name + "s"; // e.g. PolicyNumbers
            System.Data.DataTable dataTable = new System.Data.DataTable(tableName);
            dataTable.Columns.Add(new DataColumn("Id", typeof(Guid)));
            dataTable.Columns.Add(new DataColumn("TenantId", typeof(Guid)));
            dataTable.Columns.Add(new DataColumn("ProductId", typeof(Guid)));
            dataTable.Columns.Add(new DataColumn("Environment", typeof(int)));
            dataTable.Columns.Add(new DataColumn("IsAssigned", typeof(bool)));
            dataTable.Columns.Add(new DataColumn("Number", typeof(string)));
            dataTable.Columns.Add(new DataColumn("CreatedTicksSinceEpoch", typeof(long)));
            foreach (var number in addedNumbers)
            {
                DataRow row = dataTable.NewRow();
                row["Id"] = Guid.NewGuid();
                row["TenantId"] = tenantId;
                row["ProductId"] = productId;
                row["Environment"] = environment;
                row["IsAssigned"] = false;
                row["Number"] = number;
                row["CreatedTicksSinceEpoch"] = this.clock.GetCurrentInstant().ToUnixTimeTicks();
                dataTable.Rows.Add(row);
            }

            using (var connection = new SqlConnection(this.connectionConfiguration.UBind))
            {
                SqlTransaction transaction = null;
                connection.Open();
                transaction = connection.BeginTransaction();
                using (var sqlBulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.TableLock, transaction))
                {
                    sqlBulkCopy.DestinationTableName = tableName;
                    sqlBulkCopy.ColumnMappings.Add("Id", "Id");
                    sqlBulkCopy.ColumnMappings.Add("TenantId", "TenantId");
                    sqlBulkCopy.ColumnMappings.Add("ProductId", "ProductId");
                    sqlBulkCopy.ColumnMappings.Add("Environment", "Environment");
                    sqlBulkCopy.ColumnMappings.Add("IsAssigned", "IsAssigned");
                    sqlBulkCopy.ColumnMappings.Add("Number", "Number");
                    sqlBulkCopy.ColumnMappings.Add("CreatedTicksSinceEpoch", "CreatedTicksSinceEpoch");
                    sqlBulkCopy.WriteToServer(dataTable);
                }

                transaction.Commit();
            }

            return new NumberPoolAddResult(addedNumbers, duplicateNumbers);
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> DeleteForProduct(Guid tenantId, Guid productId, DeploymentEnvironment environment, IEnumerable<string> numbers)
        {
            var entities = this.dbSet
                .Where(n => n.TenantId == tenantId && n.ProductId == productId)
                .Where(n => n.Environment == environment)
                .Where(n => n.IsAssigned == false)
                .Where(n => numbers.Contains(n.Number));
            var deletedNumbers = entities.Select(number => number.Number).ToList();
            this.dbSet.RemoveRange(entities);
            this.dbContext.SaveChanges();
            return deletedNumbers;
        }

        /// <inheritdoc/>
        public void PurgeForProduct(Guid tenantId, Guid productId, DeploymentEnvironment environment)
        {
            var entities = this.dbSet
                .Where(n => n.TenantId == tenantId)
                .Where(n => n.ProductId == productId)
                .Where(n => n.Environment == environment);
            this.dbSet.RemoveRange(entities);
            this.dbContext.SaveChanges();
        }

        /// <summary>
        /// Get duplicate reference numbers.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant the numbers are for.</param>
        /// <param name="productId">The ID of the product the numbers are for.</param>
        /// <param name="environment">The deployment environment of the reference numbers are for.</param>
        /// <param name="numbers">The reference numbers to delete.</param>
        /// <returns>A collection of reference numbers without duplicate.</returns>
        private IReadOnlyList<string> GetDuplicateNumbers(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            IEnumerable<string> numbers)
        {
            return this.dbSet
                .Where(n => n.TenantId == tenantId)
                .Where(n => n.ProductId == productId)
                .Where(n => n.Environment == environment)
                .Where(n => numbers.Contains(n.Number))
                .Select(number => number.Number)
                .ToList();
        }
    }
}
