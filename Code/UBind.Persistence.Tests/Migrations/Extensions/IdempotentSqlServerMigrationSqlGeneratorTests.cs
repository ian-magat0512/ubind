// <copyright file="IdempotentSqlServerMigrationSqlGeneratorTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Migrations.Extensions
{
    using System;
    using System.Data.Entity.Migrations.Builders;
    using System.Data.Entity.Migrations.Model;
    using System.Linq;
    using FluentAssertions;
    using UBind.Persistence.Migrations.Extensions;
    using Xunit;

    public class IdempotentSqlServerMigrationSqlGeneratorTests
    {
        private IdempotentSqlServerMigrationSqlGenerator migrationSqlGenerator;

        public IdempotentSqlServerMigrationSqlGeneratorTests()
        {
            this.migrationSqlGenerator = new IdempotentSqlServerMigrationSqlGenerator();
        }

        [Fact]
        public void IdempotentSqlServerMigrationSqlGenerator_Generate_can_output_add_column_statement_for_GUID_and_uses_newid()
        {
            // Arrange
            Func<ColumnBuilder, ColumnModel> action = c => c.Guid(nullable: false, identity: true, name: "Bar");
            var addColumnOperation = new AddColumnIfNotExistsOperation("Foo", "bar", action, null);

            // Act
            var sql = string.Join(Environment.NewLine, this.migrationSqlGenerator.Generate(new[] { addColumnOperation }, "2005")
                .Select(s => s.Sql));

            // Assert
            sql.Should().Contain("IF NOT EXISTS(SELECT 1 FROM sys.columns");
            sql.Should().Contain("WHERE Name = N\'bar\'");
            sql.Should().Contain("AND Object_ID = Object_ID(N\'[Foo]\'))");
            sql.Should().Contain("BEGIN");
            sql.Should().Contain("ALTER TABLE");
            sql.Should().Contain("[Foo]");
            sql.Should().Contain("ADD [bar] [uniqueidentifier] NOT NULL DEFAULT newsequentialid() END");
        }

        [Fact]
        public void IdempotentSqlServerMigrationSqlGenerator_Generate_can_output_create_unique_index()
        {
            // Arrange
            var operation = new CreateIndexDropFirstIfExistsOperation("FooTable", new[] { "BarColumn", "BazColumn", "ShazColumn" }, unique: true, name: "MazIndex");

            // Act
            var sql = string.Join(Environment.NewLine, this.migrationSqlGenerator.Generate(new[] { operation }, "2005")
                .Select(s => s.Sql));

            // Assert
            sql.Should().Contain("IF EXISTS(SELECT 1 FROM sys.indexes");
            sql.Should().Contain("WHERE Name = N'MazIndex'");
            sql.Should().Contain("AND Object_ID = Object_ID(N'[FooTable]'))");
            sql.Should().Contain("DROP INDEX MazIndex ON [FooTable];");
            sql.Should().Contain("CREATE UNIQUE  INDEX MazIndex");
            sql.Should().Contain("ON [FooTable]");
            sql.Should().Contain("(BarColumn, BazColumn, ShazColumn);");
        }
    }
}
