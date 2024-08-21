// <copyright file="IdempotentSqlServerMigrationSqlGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations.Extensions
{
    using System;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.SqlServer;
    using System.Globalization;
    using FluentAssertions;
    using MoreLinq;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Idempotent SQL server migration generator which generates SQL for migration operations that are idempotent,
    /// meaning you can run them more than once and they will still achieve the same result.
    /// </summary>
    public class IdempotentSqlServerMigrationSqlGenerator : SqlServerMigrationSqlGenerator
    {
        /// <inheritdoc/>
        protected override void Generate(MigrationOperation migrationOperation)
        {
            this.Generate(migrationOperation as dynamic);
        }

        /// <summary>
        /// Generates the SQL to add a column if it doesn't exist, and adds it as a statement to be
        /// executed on the database.
        /// </summary>
        /// <param name="operation">The AddColumnIfNotExistsOperation to generate SQL for.</param>
        protected void Generate(AddColumnIfNotExistsOperation operation)
        {
            using (var writer = Writer())
            {
                writer.WriteLine("IF NOT EXISTS(SELECT 1 FROM sys.columns");
                writer.WriteLine($"    WHERE Name = N'{operation.Name}'");
                writer.WriteLine($"    AND Object_ID = Object_ID(N'{this.Name(operation.Table)}'))");
                writer.WriteLine("BEGIN");
                writer.WriteLine("ALTER TABLE ");
                writer.WriteLine(this.Name(operation.Table));
                writer.Write(" ADD ");

                var column = operation.ColumnModel;
                this.Generate(column, writer);

                if (column.IsNullable != null
                    && !column.IsNullable.Value
                    && (column.DefaultValue == null)
                    && string.IsNullOrWhiteSpace(column.DefaultValueSql)
                    && !column.IsIdentity
                    && !column.IsTimestamp
                    && (column.StoreType == null
                        || (!column.StoreType.EqualsIgnoreCase("rowversion")
                            && !column.StoreType.EqualsIgnoreCase("timestamp"))))
                {
                    writer.Write(" DEFAULT ");
                    if (column.Type == PrimitiveTypeKind.DateTime)
                    {
                        writer.Write(this.Generate(DateTime.Parse("1900-01-01 00:00:00", CultureInfo.InvariantCulture)));
                    }
                    else
                    {
                        writer.Write(this.Generate((dynamic)column.ClrDefaultValue));
                    }
                }

                writer.WriteLine(" END");
                this.Statement(writer);
            }
        }

        protected void Generate(DropColumnIfExistsOperation operation)
        {
            string tableName = DatabaseName.Parse(operation.TableName).Name;
            string columnName = operation.ColumnName;

            using (var writer = Writer())
            {
                writer.WriteLine("IF EXISTS(SELECT 1 FROM sys.columns");
                writer.Indent++;
                writer.WriteLine($"WHERE Name = N'{operation.ColumnName}'");
                writer.Indent++;
                writer.WriteLine($"AND Object_ID = Object_ID(N'{operation.TableName}'))");
                writer.Indent--;
                writer.Indent--;
                writer.WriteLine("BEGIN");
                writer.Indent++;
                writer.WriteLine("-- Column Exists");
                writer.WriteLine("DECLARE @ConstraintName nvarchar(200)");
                writer.WriteLine("SELECT @ConstraintName = Name FROM SYS.DEFAULT_CONSTRAINTS");
                writer.WriteLine($"WHERE PARENT_OBJECT_ID = OBJECT_ID('{tableName}')");
                writer.WriteLine("AND PARENT_COLUMN_ID = (SELECT column_id FROM sys.columns");
                writer.WriteLine($"                        WHERE NAME = N'{columnName}'");
                writer.WriteLine($"                        AND object_id = OBJECT_ID(N'{tableName}'))");
                writer.WriteLine("IF @ConstraintName IS NOT NULL");
                writer.Indent++;
                writer.WriteLine($"EXEC('ALTER TABLE {tableName} DROP CONSTRAINT ' + @ConstraintName)");
                writer.Indent--;
                writer.WriteLine($"EXEC('ALTER TABLE {tableName} DROP COLUMN {columnName}')");
                writer.Indent--;
                writer.WriteLine("END");

                this.Statement(writer);
            }
        }

        /// <summary>
        /// Generates SQL for creating and index but dropping it first if it exists.
        /// </summary>
        /// <param name="operation">The CreateIndexDropFirstIfExistsOperation.</param>
        protected void Generate(CreateIndexDropFirstIfExistsOperation operation)
        {
            string unique = operation.Unique ? "UNIQUE" : string.Empty;
            string clustered = operation.Clustered ? "CLUSTERED" : string.Empty;

            using (var writer = Writer())
            {
                writer.WriteLine("IF EXISTS(SELECT 1 FROM sys.indexes");
                writer.WriteLine($"    WHERE Name = N'{operation.Name}'");
                writer.WriteLine($"    AND Object_ID = Object_ID(N'{this.Name(operation.Table)}'))");
                writer.WriteLine($"DROP INDEX {operation.Name} ON {this.Name(operation.Table)};");
                writer.WriteLine($"CREATE {unique} {clustered} INDEX {operation.Name}");
                writer.WriteLine($"    ON {this.Name(operation.Table)}");
                writer.WriteLine($"        ({string.Join(", ", operation.Columns)});");
                this.Statement(writer);
            }
        }

        protected void Generate(RenameColumnOrDropOldColumnIfNewColumnExistsOperation operation)
        {
            string tableName = DatabaseName.Parse(operation.TableName).Name;
            string oldColumnName = operation.OldColumnName;

            using (var writer = Writer())
            {
                writer.WriteLine("IF EXISTS(SELECT 1 FROM sys.columns");
                writer.Indent++;
                writer.WriteLine($"WHERE Name = N'{operation.NewColumnName}'");
                writer.Indent++;
                writer.WriteLine($"AND Object_ID = Object_ID(N'{operation.TableName}'))");
                writer.Indent--;
                writer.Indent--;
                writer.WriteLine("BEGIN");
                writer.Indent++;
                writer.WriteLine("-- Column Exists");
                writer.WriteLine("DECLARE @ConstraintName nvarchar(200)");
                writer.WriteLine("SELECT @ConstraintName = Name FROM SYS.DEFAULT_CONSTRAINTS");
                writer.WriteLine($"WHERE PARENT_OBJECT_ID = OBJECT_ID('{tableName}')");
                writer.WriteLine("AND PARENT_COLUMN_ID = (SELECT column_id FROM sys.columns");
                writer.WriteLine($"                        WHERE NAME = N'{oldColumnName}'");
                writer.WriteLine($"                        AND object_id = OBJECT_ID(N'{tableName}'))");
                writer.WriteLine("IF @ConstraintName IS NOT NULL");
                writer.Indent++;
                writer.WriteLine($"EXEC('ALTER TABLE {tableName} DROP CONSTRAINT ' + @ConstraintName)");
                writer.Indent--;
                writer.WriteLine($"EXEC('ALTER TABLE {tableName} DROP COLUMN {oldColumnName}')");
                writer.Indent--;
                writer.WriteLine("END");
                writer.WriteLine("ELSE");
                writer.WriteLine("BEGIN");
                writer.Indent++;
                writer.Write("EXECUTE sp_rename @objname = N'");
                writer.Write(operation.TableName);
                writer.Write(".");
                writer.Write(operation.OldColumnName);
                writer.Write("', @newname = N'");
                writer.Write(operation.NewColumnName);
                writer.WriteLine("', @objtype = N'COLUMN'");
                writer.Indent--;
                writer.WriteLine("END");

                this.Statement(writer);
            }
        }

        /// <summary>
        /// Generates SQL for a <see cref="CreateTableIfNotExistsOperation" />.
        /// </summary>
        /// <param name="createTableOperation"> The operation to produce SQL for. </param>
        protected virtual void Generate(CreateTableIfNotExistsOperation createTableOperation)
        {
            createTableOperation.Should().NotBeNull();
            this.WriteCreateTableIfNotExists(createTableOperation.Operation);
        }

        /// <summary>
        /// Generates SQL for a <see cref="CreateTableOperation" />.
        /// Generated SQL should be added using the Statement method.
        /// </summary>
        /// <param name="createTableOperation"> The operation to produce SQL for. </param>
        protected virtual void WriteCreateTableIfNotExists(CreateTableOperation createTableOperation)
        {
            string tableName = DatabaseName.Parse(createTableOperation.Name).Name;
            using (var writer = Writer())
            {
                writer.WriteLine(
                    $"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{tableName}' and xtype='U')");
                writer.WriteLine("BEGIN");
                writer.Indent++;
                writer.WriteLine("CREATE TABLE " + this.Name(createTableOperation.Name) + " (");
                writer.Indent++;

                createTableOperation.Columns.ForEach(
                    (c, i) =>
                    {
                        this.Generate(c, writer);

                        if (i < createTableOperation.Columns.Count - 1)
                        {
                            writer.WriteLine(",");
                        }
                    });

                if (createTableOperation.PrimaryKey != null)
                {
                    writer.WriteLine(",");
                    writer.Write("CONSTRAINT ");
                    writer.Write(this.Quote(createTableOperation.PrimaryKey.Name));
                    writer.Write(" PRIMARY KEY ");

                    if (!createTableOperation.PrimaryKey.IsClustered)
                    {
                        writer.Write("NONCLUSTERED ");
                    }

                    writer.Write("(");
                    writer.Write(createTableOperation.PrimaryKey.Columns.Join(this.Quote));
                    writer.WriteLine(")");
                }
                else
                {
                    writer.WriteLine();
                }

                writer.Indent--;
                writer.WriteLine(")");
                writer.Indent--;
                writer.WriteLine("END");
                this.Statement(writer);
            }
        }
    }
}
