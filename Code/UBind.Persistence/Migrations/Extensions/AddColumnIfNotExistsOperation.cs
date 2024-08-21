// <copyright file="AddColumnIfNotExistsOperation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations.Extensions
{
    using System;
    using System.Data.Entity.Migrations.Builders;
    using System.Data.Entity.Migrations.Model;
    using System.Linq;
    using ArgumentValidator;

    /// <summary>
    /// To allow idempotent migrations (ones that can run more than once and still acheive the same outcome)
    /// this allows you to have a migration operation which adds a column if it doesn't exist.
    /// </summary>
    public class AddColumnIfNotExistsOperation : MigrationOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddColumnIfNotExistsOperation"/> class.
        /// </summary>
        /// <param name="table">The name of the table to add the column to.</param>
        /// <param name="name">The name of the column.</param>
        /// <param name="columnAction">An action that specifies the column to be added. i.e. c => c.Int(nullable: false, defaultValue: 3).</param>
        /// <param name="anonymousArguments">Additional arguments that may be processed by providers. Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.</param>
        public AddColumnIfNotExistsOperation(string table, string name, Func<ColumnBuilder, ColumnModel> columnAction, object anonymousArguments)
            : base(anonymousArguments)
        {
            Throw.IfNullOrEmpty(table, nameof(table));
            Throw.IfNullOrEmpty(name, nameof(name));
            Throw.IfNull(columnAction, nameof(columnAction));

            this.Table = table;
            this.Name = name;

            this.ColumnModel = columnAction(new ColumnBuilder());
            this.ColumnModel.Name = name;
        }

        /// <summary>
        /// Gets the table which the column is to be added to.
        /// </summary>
        public string Table { get; private set; }

        /// <summary>
        /// Gets the name of the column to be added.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the column model.
        /// </summary>
        public ColumnModel ColumnModel { get; private set; }

        /// <inheritdoc/>
        public override bool IsDestructiveChange => false;

        /// <inheritdoc/>
        public override MigrationOperation Inverse => new DropColumnOperation(
            this.Table,
            this.Name,
            removedAnnotations: this.ColumnModel.Annotations.ToDictionary(s => s.Key, s => (object)s.Value),
            anonymousArguments: null);
    }
}
