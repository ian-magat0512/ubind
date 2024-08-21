// <copyright file="AddColumnIfNotExistsSqlGenerator.cs" company="uBind">
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
    using UBind.Domain.Extensions;

    /// <summary>
    /// Generates SQL for the AddColumnIfNotExistsOperation used in EF Migrations.
    /// </summary>
    public class AddColumnIfNotExistsSqlGenerator : SqlServerMigrationSqlGenerator
    {
        /// <inheritdoc/>
        protected override void Generate(MigrationOperation migrationOperation)
        {
            var operation = migrationOperation as AddColumnIfNotExistsOperation;
            if (operation == null)
            {
                return;
            }

            using (var writer = Writer())
            {
                writer.WriteLine("IF NOT EXISTS(SELECT 1 FROM sys.columns");
                writer.WriteLine($"WHERE Name = N'{operation.Name}' AND Object_ID = Object_ID(N'{this.Name(operation.Table)}'))");
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
                    && !column.StoreType.EqualsIgnoreCase("rowversion")
                    && !column.StoreType.EqualsIgnoreCase("timestamp"))
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
    }
}
