// <copyright file="RenameColumnOrDropOldColumnIfNewColumnExistsOperation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations.Extensions
{
    using System.Data.Entity.Migrations.Model;

    public class RenameColumnOrDropOldColumnIfNewColumnExistsOperation : MigrationOperation
    {
        public RenameColumnOrDropOldColumnIfNewColumnExistsOperation(
            string tableName,
            string oldColumnName,
            string newColumnName,
            object anonymousArguments)
            : base(anonymousArguments)
        {
            this.TableName = tableName;
            this.OldColumnName = oldColumnName;
            this.NewColumnName = newColumnName;
        }

        public string TableName { get; }

        public string OldColumnName { get; }

        public string NewColumnName { get; }

        /// <inheritdoc/>
        public override bool IsDestructiveChange => true;

        /// <inheritdoc/>
        public override MigrationOperation Inverse => new RenameColumnOrDropOldColumnIfNewColumnExistsOperation(
            this.TableName,
            this.NewColumnName,
            this.OldColumnName,
            anonymousArguments: null);
    }
}
