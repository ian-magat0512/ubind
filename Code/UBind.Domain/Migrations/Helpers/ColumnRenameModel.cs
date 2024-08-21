// <copyright file="ColumnRenameModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations.Helpers
{
    /// <summary>
    /// Model representing the rename of a column in a table.
    /// </summary>
    public class ColumnRenameModel
    {
        public ColumnRenameModel(string tableName, string oldColumnName, string newColumnName, bool nullable = false)
        {
            this.TableName = tableName;
            this.OldColumnName = oldColumnName;
            this.NewColumnName = newColumnName;
            this.NewNullable = nullable;
        }

        public string TableName { get; set; }

        public string OldColumnName { get; set; }

        public string NewColumnName { get; set; }

        public bool NewNullable { get; set; }

        public string OldStoreType { get; set; }

        public string NewStoreType { get; set; }

        public int OldPrecision { get; set; }

        public int NewPrecision { get; set; }
    }
}
