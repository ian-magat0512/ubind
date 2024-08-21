// <copyright file="CreateIndexDropFirstIfExistsOperation.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations.Extensions
{
    using System.Data.Entity.Migrations.Model;
    using ArgumentValidator;

    /// <summary>
    /// To allow idempotent migrations (ones that can run more than once and still acheive the same outcome)
    /// this allows you to have a migration operation which creates an index if it doesn't exist.
    /// </summary>
    public class CreateIndexDropFirstIfExistsOperation : MigrationOperation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateIndexDropFirstIfExistsOperation"/> class.
        /// </summary>
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
        public CreateIndexDropFirstIfExistsOperation(
            string table,
            string[] columns,
            bool unique = false,
            string name = null,
            bool clustered = false,
            object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Throw.IfNullOrEmpty(table, nameof(table));
            this.Table = table;
            this.Columns = columns;
            this.Unique = unique;
            this.Name = name;
            this.Clustered = clustered;
        }

        /// <summary>
        /// Gets the table which the index is to be added to.
        /// </summary>
        public string Table { get; private set; }

        /// <summary>
        /// Gets the name of the columns to create the index on.
        /// </summary>
        public string[] Columns { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this is a unique index.
        /// </summary>
        public bool Unique { get; private set; }

        /// <summary>
        /// Gets the name to use for the index in the database.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets a value indicating whether or not this is a clustered index.
        /// </summary>
        public bool Clustered { get; private set; }

        /// <inheritdoc/>
        public override bool IsDestructiveChange => false;

        /// <inheritdoc/>
        public override MigrationOperation Inverse => new DropIndexOperation()
        {
            Table = this.Table,
            Name = this.Name,
        };
    }
}
