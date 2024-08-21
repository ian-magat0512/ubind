// <copyright file="SqlHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Helpers;

using System.Text;

/// <summary>
/// The Sql Helper for easy creation of queries.
/// </summary>
public static partial class SqlHelper
{
    public static string DropColumnWithConstraintsIfExists(string tableName, string columnName)
    {
        tableName = WithoutSchema(tableName);
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("DECLARE @ConstraintName nvarchar(200)");
        sb.AppendLine("SELECT @ConstraintName = Name FROM SYS.DEFAULT_CONSTRAINTS");
        sb.AppendLine($"WHERE PARENT_OBJECT_ID = OBJECT_ID('{tableName}')");
        sb.AppendLine("AND PARENT_COLUMN_ID = (SELECT column_id FROM sys.columns");
        sb.AppendLine($"                        WHERE NAME = N'{columnName}'");
        sb.AppendLine($"                        AND object_id = OBJECT_ID(N'{tableName}'))");
        sb.AppendLine("IF @ConstraintName IS NOT NULL");
        sb.AppendLine($"    EXEC('ALTER TABLE [{tableName}] DROP CONSTRAINT [' + @ConstraintName + ']')");
        sb.AppendLine($"EXEC('ALTER TABLE [{tableName}] DROP COLUMN IF EXISTS [{columnName}]')");
        return sb.ToString();
    }

    public static string MakeColumnNonNullable(string tableName, string columnName, string dataType)
    {
        return $"ALTER TABLE {tableName} ALTER COLUMN {columnName} {dataType} NOT NULL";
    }

    public static string WithSchema(string tableName)
    {
        return (tableName.StartsWith("[dbo].") || tableName.StartsWith("dbo."))
            ? tableName
            : "dbo." + tableName;
    }

    public static string WithoutSchema(string tableName)
    {
        int dotPosition = tableName.IndexOf('.');
        return dotPosition == -1
            ? tableName
            : tableName.Substring(dotPosition + 1);
    }

    public static string DropTableIfExists(string tableName)
    {
        return $"DROP TABLE IF EXISTS {tableName}";
    }

    public static string DropIndexIfExists(string tableName, string indexName)
    {
        return $"DROP INDEX IF EXISTS [{indexName}] ON [{tableName}]";
    }

    public static string DropColumnIfExists(string tableName, string columnName)
    {
        return $@"
                IF EXISTS(
                    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}'
                )
                BEGIN
                    ALTER TABLE {tableName}
                    DROP COLUMN {columnName};
                END";
    }

    public static string DropForeignKeyIfExists(string tableName, string foreignKeyName)
    {
        return $@"
                IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[{foreignKeyName}]'))
                BEGIN
                    ALTER TABLE {tableName} DROP CONSTRAINT [{foreignKeyName}];
                END";
    }
}
