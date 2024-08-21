// <copyright file="DbMigrationExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Builders;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Data.Entity.Migrations.Model;
    using System.Linq;
    using System.Reflection;
    using FluentAssertions;
    using UBind.Domain.Helpers;
    using UBind.Persistence.Migrations.Helpers;

    /// <summary>
    /// Provides extension methods for the DbMigration class.
    /// </summary>
    public static class DbMigrationExtensions
    {
        public static void AlterLongColumnWithDefaultValue(this DbMigration dbMigration, ColumnRenameModel columnRename)
        {
            var migration = new DbMigrationWrapper(dbMigration);

            // Make sure the timestamp columns that are not supposed nullable and have a default value
            // and those that are, have their nullable status updated.
            if (columnRename.NewNullable)
            {
                migration.AlterColumn(columnRename.TableName, columnRename.OldColumnName, c => c.Long(nullable: true));
            }
            else
            {
                migration.AlterColumn(columnRename.TableName, columnRename.OldColumnName, c => c.Long(nullable: false, defaultValue: 0));
            }
        }

        /// <summary>
        /// Creates trigger for the models timestamp properties and fill it with data from old to new column.
        /// </summary>
        /// <param name="dbMigration">The db migration.</param>
        /// <param name="columnRename">The column rename model.</param>
        public static void FillAndCreateTriggersForTimestampColumnRename(this DbMigration dbMigration, ColumnRenameModel columnRename)
        {
            var migration = new DbMigrationWrapper(dbMigration);

            AlterLongColumnWithDefaultValue(dbMigration, columnRename);

            migration.Sql(SqlHelper.ColumnRename.CopyColumnValueForRenameQuery(
                  columnRename.TableName, columnRename.OldColumnName, columnRename.NewColumnName));

            SqlHelper.ColumnRename.CreateTriggerQueriesForTimestampColumnRename(columnRename)
                .ForEach(trigger =>
                {
                    migration.Sql(trigger);
                });
        }

        /// <summary>
        /// Creates trigger for the models timestamp properties.
        /// </summary>
        /// <param name="dbMigration">The db migration.</param>
        /// <param name="columnRename">The column rename model.</param>
        public static void CreateTriggersForTimestampColumnRename(this DbMigration dbMigration, ColumnRenameModel columnRename)
        {
            var migration = new DbMigrationWrapper(dbMigration);

            AlterLongColumnWithDefaultValue(dbMigration, columnRename);

            SqlHelper.ColumnRename.CreateTriggerQueriesForTimestampColumnRename(columnRename)
                .ForEach(trigger =>
                {
                    migration.Sql(trigger);
                });
        }

        public static void AlterDatetimeColumnWithDefaultValue(this DbMigration dbMigration, ColumnRenameModel columnRename)
        {
            var migration = new DbMigrationWrapper(dbMigration);

            if (columnRename.OldStoreType == "datetime")
            {
                if (columnRename.NewNullable)
                {
                    migration.AlterColumn(
                        columnRename.TableName,
                        columnRename.OldColumnName,
                        c => c.DateTime(nullable: columnRename.NewNullable, storeType: "datetime", defaultValue: null));
                }
                else
                {
                    migration.AlterColumn(
                        columnRename.TableName,
                        columnRename.OldColumnName,
                        c => c.DateTime(nullable: columnRename.NewNullable, storeType: "datetime", defaultValueSql: "'1900-01-01 00:00:00'"));
                }
            }
            else
            {
                if (columnRename.NewNullable)
                {
                    migration.AlterColumn(
                        columnRename.TableName,
                        columnRename.OldColumnName,
                        c => c.DateTime(nullable: columnRename.NewNullable, precision: 7, storeType: "datetime2", defaultValue: null));
                }
                else
                {
                    migration.AlterColumn(
                        columnRename.TableName,
                        columnRename.OldColumnName,
                        c => c.DateTime(nullable: columnRename.NewNullable, precision: 7, storeType: "datetime2", defaultValueSql: "'1900-01-01 00:00:00'"));
                }
            }
        }

        /// <summary>
        /// Creates trigger for the models timestamp properties and fill it with data from old to new column.
        /// </summary>
        /// <param name="dbMigration">The db migration.</param>
        /// <param name="columnRename">The column rename model.</param>
        public static void FillAndCreateTriggersForDateTimeColumnRename(this DbMigration dbMigration, ColumnRenameModel columnRename)
        {
            var migration = new DbMigrationWrapper(dbMigration);

            AlterDatetimeColumnWithDefaultValue(dbMigration, columnRename);

            migration.Sql(SqlHelper.ColumnRename.CopyColumnValueForRenameQuery(
                columnRename.TableName, columnRename.OldColumnName, columnRename.NewColumnName));

            SqlHelper.ColumnRename.CreateTriggerQueriesForDateTimeColumnRename(columnRename)
               .ForEach(trigger =>
               {
                   migration.Sql(trigger);
               });
        }

        /// <summary>
        /// Creates trigger for the models timestamp properties.
        /// </summary>
        /// <param name="dbMigration">The db migration.</param>
        /// <param name="columnRename">The column rename model.</param>
        public static void CreateTriggersForDateTimeColumnRename(this DbMigration dbMigration, ColumnRenameModel columnRename)
        {
            var migration = new DbMigrationWrapper(dbMigration);

            AlterDatetimeColumnWithDefaultValue(dbMigration, columnRename);

            SqlHelper.ColumnRename.CreateTriggerQueriesForDateTimeColumnRename(columnRename)
               .ForEach(trigger =>
               {
                   migration.Sql(trigger);
               });
        }

        public static void DeleteTriggersForColumnRename(this DbMigration dbMigration, ColumnRenameModel columnRename)
        {
            var migration = new DbMigrationWrapper(dbMigration);
            migration.Sql(SqlHelper.ColumnRename.DeleteCopyOnInsertTriggerForColumnRename(
                columnRename.TableName, columnRename.OldColumnName, columnRename.NewColumnName));
            migration.Sql(SqlHelper.ColumnRename.DeleteCopyOnUpdateTriggerForColumnRename(
                columnRename.TableName, columnRename.OldColumnName, columnRename.NewColumnName));
        }

        /// <summary>
        /// Reverses the rename of a column.
        /// </summary>
        public static void RenameColumnReverse(this DbMigration dbMigration, ColumnRenameModel columnRename)
        {
            // Note: we are deliberately switching old and new column names here, because we are reversing
            // the rename operation.
            dbMigration.RenameColumnOrDropOldColumnIfNewColumnExists(
                columnRename.TableName, columnRename.NewColumnName, columnRename.OldColumnName);
        }

        /// <summary>
        /// Adds a column to a table only if the column doesn't already exist.
        /// </summary>
        /// <param name="migration">The DbMigration instance.</param>
        /// <param name="table">The name of the table to add the column to.</param>
        /// <param name="name">The name of the column to add.</param>
        /// <param name="columnAction">An action that specifies the column to be added. i.e. c => c.Int(nullable: false, defaultValue: 3).</param>
        /// <param name="anonymousArguments">Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.</param>
        public static void AddColumnIfNotExists(
            this DbMigration migration,
            string table,
            string name,
            Func<ColumnBuilder, ColumnModel>
            columnAction,
            object anonymousArguments = null)
        {
            ((IDbMigration)migration)
                .AddOperation(new AddColumnIfNotExistsOperation(table, name, columnAction, anonymousArguments));
        }

        public static void DropColumnIfExists(
            this DbMigration migration,
            string table,
            string name,
            object anonymousArguments = null)
        {
            ((IDbMigration)migration).AddOperation(new DropColumnIfExistsOperation(table, name, anonymousArguments));
        }

        /// <summary>
        /// Adds an index to a table only. Drops the index first if the index already exists.
        /// </summary>
        /// <param name="migration">The DbMigration instance.</param>
        /// <param name="table">The name of the table to create the index on. Schema name is optional,
        /// if no schema is specified then dbo is assumed.</param>
        /// <param name="columns">The name of the columns to create the index on.</param>
        /// <param name="unique">A value indicating if this is a unique index. If no value is supplied a
        /// non-unique index will be created.</param>
        /// <param name="name">The name to use for the index in the database. If no value is supplied a
        /// unique name will be generated.</param>
        /// <param name="clustered">A value indicating whether or not this is a clustered index.</param>
        /// <param name="anonymousArguments">Additional arguments that may be processed by providers.
        /// Use anonymous type syntax to specify arguments e.g.
        /// 'new { SampleArgument = "MyValue" }'.</param>
        public static void CreateIndexDropFirstIfExists(
            this DbMigration migration,
            string table,
            string[] columns,
            bool unique = false,
            string name = null,
            bool clustered = false,
            object anonymousArguments = null)
        {
            ((IDbMigration)migration)
                .AddOperation(new CreateIndexDropFirstIfExistsOperation(
                    table,
                    columns,
                    unique,
                    name,
                    clustered,
                    anonymousArguments));
        }

        public static void CreateIndexDropFirstIfExists(
            this DbMigration migration,
            string table,
            string column,
            bool unique = false,
            string name = null,
            bool clustered = false,
            object anonymousArguments = null)
        {
            ((IDbMigration)migration)
                .AddOperation(new CreateIndexDropFirstIfExistsOperation(
                    table,
                    new string[1] { column },
                    unique,
                    name,
                    clustered,
                    anonymousArguments));
        }

        public static TableBuilder<TColumns> CreateTableIfNotExists<TColumns>(
            this DbMigration migration,
            string name,
            Func<ColumnBuilder, TColumns> columnsAction,
            object anonymousArguments = null)
        {
            return CreateTableIfNotExists(migration, name, columnsAction, null, anonymousArguments);
        }

        public static TableBuilder<TColumns> CreateTableIfNotExists<TColumns>(
            this DbMigration migration,
            string name,
            Func<ColumnBuilder, TColumns> columnsAction,
            IDictionary<string, object> annotations,
            object anonymousArguments = null)
        {
            name.Should().NotBeNullOrEmpty();
            columnsAction.Should().NotBeNull();

            var innerOperation = new CreateTableOperation(name, annotations, anonymousArguments);
            var operation = new CreateTableIfNotExistsOperation(innerOperation);
            ((IDbMigration)migration).AddOperation(operation);
            AddColumns(columnsAction(new ColumnBuilder()), innerOperation.Columns);
            return new TableBuilder<TColumns>(innerOperation, migration);
        }

        public static void RenameColumnOrDropOldColumnIfNewColumnExists(
            this DbMigration migration,
            string tableName,
            string oldColumnName,
            string newColumnName,
            object anonymousArguments = null)
        {
            tableName.Should().NotBeNullOrEmpty();
            oldColumnName.Should().NotBeNullOrEmpty();
            newColumnName.Should().NotBeNullOrEmpty();

            var operation = new RenameColumnOrDropOldColumnIfNewColumnExistsOperation(
                tableName, oldColumnName, newColumnName, anonymousArguments);

            ((IDbMigration)migration).AddOperation(operation);
        }

        public static string Join<T>(this IEnumerable<T> ts, Func<T, string> selector = null, string separator = ", ")
        {
            selector = selector ?? (t => t.ToString());
            return string.Join(separator, ts.Where(t => !ReferenceEquals(t, null)).Select(selector));
        }

        /// <summary>
        /// This method was copied from EF6 source on github.
        /// </summary>
        private static void AddColumns<TColumns>(TColumns columns, ICollection<ColumnModel> columnModels)
        {
            columns.GetType().GetNonIndexerProperties()
                .Each(
                    (p, i) =>
                    {
                        var columnModel = p.GetValue(columns, null) as ColumnModel;

                        if (columnModel != null)
                        {
                            // we can't do this:
                            //
                            // columnModel.ApiPropertyInfo = p;
                            //
                            // because it's protected, so we use reflection:
                            PropertyInfo apiPropertyInfo = columnModel.GetType().GetProperty(
                                "ApiPropertyInfo",
                                BindingFlags.NonPublic | BindingFlags.Instance);
                            apiPropertyInfo.SetValue(columnModel, p);

                            if (string.IsNullOrWhiteSpace(columnModel.Name))
                            {
                                columnModel.Name = p.Name;
                            }

                            columnModels.Add(columnModel);
                        }
                    });
        }

        /// <summary>
        /// This method was copied from EF6 source on github.
        /// </summary>
        private static IEnumerable<PropertyInfo> GetNonIndexerProperties(this Type type)
        {
            type.Should().NotBeNull();

            return type.GetRuntimeProperties().Where(
                p => p.IsPublic()
                     && !p.GetIndexParameters().Any());
        }

        /// <summary>
        /// This method was copied from EF6 source on github.
        /// </summary>
        private static bool IsPublic(this PropertyInfo property)
        {
            property.Should().NotBeNull();

            // The MethodAttributes enum for member access has the following values:
            // 1 Private
            // 2 FamANDAssem
            // 3 Assembly
            // 4 Family
            // 5 FamORAssem
            // 6 Public
            // Starting from the bottom, Public is more permissive than anything above it--meaning that
            // if it can be accessed publically then it can be accessed by anything. Likewise,
            // FamORAssem is more permissive than anything above it. Assembly can be more permissive
            // than Family and vice versa. (However, at least in C# and VB a property setter cannot be
            // Assembly while the getter is Family or vice versa.) Since there is no real permissive winner
            // here, we will use the enum order and call Family more permissive than Assembly, but this is
            // a largely arbitrary choice. Finally, FamANDAssem is more permissive than private, which is the
            // least permissive.
            // We can therefore use this order to infer the accessibility of the property.
            var getter = property.GetMethod;
            var getterAccess = getter == null ? MethodAttributes.Private : (getter.Attributes & MethodAttributes.MemberAccessMask);

            var setter = property.SetMethod;
            var setterAccess = setter == null ? MethodAttributes.Private : (setter.Attributes & MethodAttributes.MemberAccessMask);

            var propertyAccess = getterAccess > setterAccess ? getterAccess : setterAccess;

            return propertyAccess == MethodAttributes.Public;
        }

        /// <summary>
        /// This method was copied from EF6 source on github.
        /// </summary>
        private static void Each<T>(this IEnumerable<T> ts, Action<T, int> action)
        {
            ts.Should().NotBeNull();
            action.Should().NotBeNull();

            var i = 0;
            foreach (var t in ts)
            {
                action(t, i++);
            }
        }
    }
}
