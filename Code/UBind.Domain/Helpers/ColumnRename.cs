// <copyright file="ColumnRename.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using Humanizer;
    using UBind.Persistence.Migrations.Helpers;

    public static partial class SqlHelper
    {
        public static class ColumnRename
        {
            private enum RowOperation
            {
                [Description("UPDATE")]
                Update = 0,

                [Description("INSERT")]
                Insert = 1,
            }

            public static string CopyColumnValueForRenameQuery(string tableName, string oldColumnName, string newColumnName)
            {
                return $"UPDATE dbo.{tableName} SET {newColumnName} = {oldColumnName}";
            }

            public static string CopyColumnValueForRenameFilterByGreaterThanCreationDateQuery(
                string tableName,
                string oldColumnName,
                string newColumnName,
                long createdTimeTicksSinceEpoch,
                int batchCount = 0)
            {
                var result = $"UPDATE " + (batchCount > 0 ? $"TOP ({batchCount})" : string.Empty) + $" dbo.{tableName}\r\n"
                    + $"    SET {newColumnName} = {oldColumnName}\r\n"
                    + $"    WHERE {newColumnName} <> {oldColumnName} and CreationTimeInTicksSinceEpoch > {createdTimeTicksSinceEpoch}";
                return result;
            }

            public static string CopyColumnValueForRenameQuery(
                string tableName,
                string oldColumnName,
                string newColumnName,
                int batchCount = 0)
            {
                var result = $"UPDATE " + (batchCount > 0 ? $"TOP ({batchCount})" : string.Empty) + $" dbo.{tableName}\r\n"
                    + $"    SET {newColumnName} = {oldColumnName}\r\n"
                    + $"    WHERE {newColumnName} <> {oldColumnName}";
                return result;
            }

            /// <summary>
            /// Generates the SQL source for set of triggers which will copy the value from one column to the other.
            /// This is used when a column needs to be renamed, so as part of a zero dowtime deployment, we need
            /// both columns to exist whilst the deployment happens so that nodes which have the old code can continue
            /// to work.
            /// </summary>
            /// <param name="tableName">The name of the table.</param>
            /// <param name="oldColumnName">The old column name.</param>
            /// <param name="newColumnName">The new column name.</param>
            /// <param name="defaultValue">The default value, so we know that it was set by the db and not upon insert.</param>
            /// <param name="idColumnName">The name of the ID column, so we can identify the row we need to update.</param>
            /// <returns>SQL source code to create the triggers.</returns>
            public static string CopyOnInsertTriggerForNonNullableColumnRename(string tableName, string oldColumnName, string newColumnName, string defaultValue, string idColumnName = "Id")
            {
                string triggerName = GenerateTriggerName(tableName, oldColumnName, newColumnName, RowOperation.Insert);
                string defaultValueCheck = $"<> {defaultValue}";
                var sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations", "Sql", "ColumnRenameRowInsertedTrigger.sqlt");
                string sql = File.ReadAllText(sqlFile);
                return string.Format(sql, triggerName, tableName, oldColumnName, newColumnName, defaultValueCheck, idColumnName);
            }

            public static string CopyOnInsertTriggerForNullableColumnRename(string tableName, string oldColumnName, string newColumnName, string idColumnName = "Id")
            {
                string triggerName = GenerateTriggerName(tableName, oldColumnName, newColumnName, RowOperation.Insert);
                string nullCheck = " IS NOT NULL";
                var sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations", "Sql", "ColumnRenameRowInsertedTrigger.sqlt");
                string sql = File.ReadAllText(sqlFile);
                return string.Format(sql, triggerName, tableName, oldColumnName, newColumnName, nullCheck, idColumnName);
            }

            public static string CopyOnUpdateTriggerForColumnRename(string tableName, string oldColumnName, string newColumnName, string idColumnName = "Id")
            {
                string triggerName = GenerateTriggerName(tableName, oldColumnName, newColumnName, RowOperation.Update);
                var sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Migrations", "Sql", "ColumnRenameRowUpdatedTrigger.sqlt");
                string sql = File.ReadAllText(sqlFile);
                return string.Format(sql, triggerName, tableName, oldColumnName, newColumnName, idColumnName);
            }

            public static string DeleteCopyOnInsertTriggerForColumnRename(string tableName, string oldColumnName, string newColumnName)
            {
                string triggerName = GenerateTriggerName(tableName, oldColumnName, newColumnName, RowOperation.Insert);
                return $"DROP TRIGGER IF EXISTS {triggerName}";
            }

            public static List<string> CreateTriggerQueriesForTimestampColumnRename(ColumnRenameModel columnRename)
            {
                List<string> triggers = new List<string>();

                if (columnRename.NewNullable)
                {
                    triggers.Add(
                       SqlHelper.ColumnRename.CopyOnInsertTriggerForNullableColumnRename(
                    columnRename.TableName, columnRename.OldColumnName, columnRename.NewColumnName));
                }
                else
                {
                    triggers.Add(
                       SqlHelper.ColumnRename.CopyOnInsertTriggerForNonNullableColumnRename(
                    columnRename.TableName, columnRename.OldColumnName, columnRename.NewColumnName, "0"));
                }

                triggers.Add(
                   SqlHelper.ColumnRename.CopyOnUpdateTriggerForColumnRename(
                columnRename.TableName, columnRename.OldColumnName, columnRename.NewColumnName));

                return triggers;
            }

            public static List<string> CreateTriggerQueriesForDateTimeColumnRename(ColumnRenameModel columnRename)
            {
                List<string> triggers = new List<string>();

                if (columnRename.NewNullable)
                {
                    triggers.Add(
                         SqlHelper.ColumnRename.CopyOnInsertTriggerForNullableColumnRename(
                        columnRename.TableName, columnRename.OldColumnName, columnRename.NewColumnName));
                }
                else
                {
                    triggers.Add(
                        SqlHelper.ColumnRename.CopyOnInsertTriggerForNonNullableColumnRename(
                        columnRename.TableName, columnRename.OldColumnName, columnRename.NewColumnName, "'1900-01-01 00:00:00'"));
                }

                triggers.Add(
                    SqlHelper.ColumnRename.CopyOnUpdateTriggerForColumnRename(
                    columnRename.TableName, columnRename.OldColumnName, columnRename.NewColumnName));

                return triggers;
            }

            public static string DeleteCopyOnUpdateTriggerForColumnRename(string tableName, string oldColumnName, string newColumnName)
            {
                string triggerName = GenerateTriggerName(tableName, oldColumnName, newColumnName, RowOperation.Update);
                return $"DROP TRIGGER IF EXISTS {triggerName}";
            }

            private static string GenerateTriggerName(string tableName, string oldColumnName, string newColumnName, RowOperation rowOperation)
            {
                return $"{tableName}_RENAME_{oldColumnName}_TO_{newColumnName}_ON_{rowOperation.Humanize()}";
            }
        }
    }
}
