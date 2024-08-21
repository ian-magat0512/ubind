// <copyright file="CreateTableIfNotExistsOperation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations.Extensions
{
    using System.Data.Entity.Migrations.Model;

    /// <summary>
    /// This is needed because if the deletion of a table is done in a StartupJob, and in the Down() method of the
    /// migration it's recreated, then if the startup job fails to run, when we roll back the migration, we only
    /// need to recreate the table if it was actually dropped. If the startup job fails or was never run, then it
    /// won't have dropped the table, so it doesn't need recreating.
    /// </summary>
    public class CreateTableIfNotExistsOperation : MigrationOperation
    {
        public CreateTableIfNotExistsOperation(CreateTableOperation operation)
            : base(operation.AnonymousArguments)
        {
            this.Operation = operation;
        }

        public CreateTableOperation Operation { get; }

        public override MigrationOperation Inverse => this.Operation.Inverse;

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
