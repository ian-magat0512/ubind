// <copyright file="DatabaseFixture.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests
{
    using System;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using System.Transactions;
    using Hangfire;
    using Hangfire.SqlServer;
    using Hangfire.Storage;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using Polly;
    using ServiceStack;
    using UBind.Domain.Entities;
    using UBind.Domain.Permissions;
    using Xunit;

    public class DatabaseFixture : IAsyncLifetime
    {
        public const string TestConnectionStringName = "UBindTestDatabase";

        // This shouldn't be needed. It should get the config from UBind.Persistence.Tests/appsettings.test.json
        /*
        public const string TestDatabaseName = "UBind-Test";
        */

        // Change to false for manual test runs requiring concurrent db updates.
#pragma warning disable CS0649 // Field 'DatabaseFixture.useTransaction' is never assigned to, and will always have its default value false
        private readonly bool useTransaction;
#pragma warning restore CS0649 // Field 'DatabaseFixture.useTransaction' is never assigned to, and will always have its default value false
        private TransactionScope transactionScope;

        public static string TestConnectionString { get; private set; }

        public async Task InitializeAsync()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.test.json").Build();
            TestConnectionString = config.GetConnectionString(TestConnectionStringName);
            if (TestConnectionString.IsNullOrEmpty())
            {
                throw new Exception($"Connection string '{TestConnectionStringName}' not found in appsettings.test.json");
            }

            var defaultRolePermissionsRegistry = new DefaultRolePermissionsRegistry();
            var defaultRoleNameRegistry = new DefaultRoleNameRegistry();
            var roleTypePermissionsRegistry = new RoleTypePermissionsRegistry();
            Role.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            Role.SetDefaultRoleNameRegistry(defaultRoleNameRegistry);
            PermissionExtensions.SetDefaultRolePermissionsRegistry(defaultRolePermissionsRegistry);
            PermissionExtensions.SetRoleTypePermissionsRegistry(roleTypePermissionsRegistry);

            var initializer = new MigrateDatabaseToLatestVersion<UBindDbContext, Persistence.Migrations.Configuration>(true);
            Database.SetInitializer(initializer);
            await this.EnsureDbCleanUp(TestConnectionString);
            using (var dbContext = new UBindDbContext(TestConnectionString))
            {
                dbContext.Database.Initialize(false);
            }

            // Hangfire JobStorage
            var mockSqlServerStorage = new Mock<SqlServerStorage>(TestConnectionString);
            var mockIStorageConnection = new Mock<IStorageConnection>();
            mockSqlServerStorage.Setup(x => x.GetConnection()).Returns(mockIStorageConnection.Object);
            JobStorage.Current = mockSqlServerStorage.Object;

            this.CommonSetup();

            await Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            if (this.useTransaction)
            {
                // Uncomment the line below, to allow persistance of changes for debugging purposes.
                //// this.transactionScope.Complete();
                this.transactionScope.Dispose();
            }

            return Task.CompletedTask;
        }

        private async Task EnsureDbCleanUp(string testConnectionString)
        {
            var policy = Policy
                .Handle<Exception>()
                .OrResult<bool>(isDbExist => isDbExist == true)
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(4),
                    TimeSpan.FromSeconds(5),
                });

            await policy.ExecuteAsync(() =>
            {
                Database.Delete(testConnectionString);
                return Task.FromResult(Database.Exists(testConnectionString));
            });
        }

        private void CommonSetup()
        {
            if (this.useTransaction)
            {
                var transactionOptions = new TransactionOptions
                {
                    IsolationLevel = IsolationLevel.ReadCommitted,
                    Timeout = new System.TimeSpan(0, 30, 0),
                };
                this.transactionScope = new TransactionScope(
                    TransactionScopeOption.RequiresNew,
                    transactionOptions,
                    TransactionScopeAsyncFlowOption.Enabled);
            }
        }
    }
}
