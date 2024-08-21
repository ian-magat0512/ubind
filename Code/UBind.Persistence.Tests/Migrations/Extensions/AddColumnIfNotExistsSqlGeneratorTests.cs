// <copyright file="AddColumnIfNotExistsSqlGeneratorTests.cs" company="uBind">
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

    public class AddColumnIfNotExistsSqlGeneratorTests
    {
        [Fact]
        public void AddColumnIfNotExistsSqlGenerator_Generate_can_output_add_column_statement_for_GUID_and_uses_newid()
        {
            // Arrange
            var migrationSqlGenerator = new AddColumnIfNotExistsSqlGenerator();
            Func<ColumnBuilder, ColumnModel> action = c => c.Guid(nullable: false, identity: true, name: "Bar");
            var addColumnOperation = new AddColumnIfNotExistsOperation("Foo", "bar", action, null);

            // Act
            var sql = string.Join(Environment.NewLine, migrationSqlGenerator.Generate(new[] { addColumnOperation }, "2005")
                .Select(s => s.Sql));

            // Assert
            sql.Should().Contain("IF NOT EXISTS(SELECT 1 FROM sys.columns");
            sql.Should().Contain("WHERE Name = N\'bar\' AND Object_ID = Object_ID(N\'[Foo]\'))");
            sql.Should().Contain("BEGIN");
            sql.Should().Contain("ALTER TABLE");
            sql.Should().Contain("[Foo]");
            sql.Should().Contain("ADD [bar] [uniqueidentifier] NOT NULL DEFAULT newsequentialid() END");
        }
    }
}
