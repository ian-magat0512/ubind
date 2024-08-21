// <copyright file="MigrationHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations
{
    /// <summary>
    /// Custom migration code for unique index where not null for account emails.
    /// </summary>
    public class MigrationHelper
    {
        /// <summary>
        /// Generate the SQL to set a unique index on a pair of columns where one is not null.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="nonNullableColumnName">The name of the column that will not be null.</param>
        /// <param name="nullableColumnName">The name of the column that may have multiple nulls.</param>
        /// <param name="indexName">A name for the index.</param>
        /// <returns>Text for the SQL command.</returns>
        public static string GenerateUniqueIndexOnTwoColumnsAllowingMultipleNullInSecondColumn(string tableName, string nonNullableColumnName, string nullableColumnName, string indexName)
        {
            return $@"CREATE UNIQUE NONCLUSTERED INDEX {indexName}
ON {tableName}({nonNullableColumnName}, {nullableColumnName})
WHERE {nullableColumnName} IS NOT NULL;";
        }
    }
}
