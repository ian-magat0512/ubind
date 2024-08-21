// <copyright file="DbMigrationWrapper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Migrations.Extensions
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Builders;
    using System.Data.Entity.Migrations.Model;
    using System.Dynamic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Wraps DbMigration to make available operations such as AddColumn etc, which are protected, but this makes them public.
    /// This allows the DbMigration instance to be passed around to helper classes, so that they can invoke the migrations as they
    /// see fit, and also allows us to create re-usable code.
    /// </summary>
    public class DbMigrationWrapper : DynamicObject
    {
        private DbMigration migration;

        public DbMigrationWrapper(DbMigration migration)
        {
            this.migration = migration;
        }

        // Summary:
        //     Adds an operation to alter the definition of an existing column. Entity Framework
        //     Migrations APIs are not designed to accept input provided by untrusted sources
        //     (such as the end user of an application). If input is accepted from such sources
        //     it should be validated before being passed to these APIs to protect against SQL
        //     injection attacks etc.
        //
        // Parameters:
        //   table:
        //     The name of the table the column exists in. Schema name is optional, if no schema
        //     is specified then dbo is assumed.
        //
        //   name:
        //     The name of the column to be changed.
        //
        //   columnAction:
        //     An action that specifies the new definition for the column. i.e. c => c.String(nullable:
        //     false, defaultValue: "none")
        //
        //   anonymousArguments:
        //     Additional arguments that may be processed by providers. Use anonymous type syntax
        //     to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        public void AlterColumn(string table, string name, Func<ColumnBuilder, ColumnModel> columnAction, object anonymousArguments = null)
        {
            object[] args = { table, name, columnAction, anonymousArguments };
            Type[] types = MethodBase.GetCurrentMethod().GetParameters().ToList().Select(p => p.ParameterType).ToArray();
            this.DoInvokeMember(
                MethodBase.GetCurrentMethod().Name,
                types,
                args,
                out object result);
        }

        // Summary:
        //     Adds an operation to execute a SQL command or set of SQL commands. Entity Framework
        //     Migrations APIs are not designed to accept input provided by untrusted sources
        //     (such as the end user of an application). If input is accepted from such sources
        //     it should be validated before being passed to these APIs to protect against SQL
        //     injection attacks etc.
        //
        // Parameters:
        //   sql:
        //     The SQL to be executed.
        //
        //   suppressTransaction:
        //     A value indicating if the SQL should be executed outside of the transaction being
        //     used for the migration process. If no value is supplied the SQL will be executed
        //     within the transaction.
        //
        //   anonymousArguments:
        //     Additional arguments that may be processed by providers. Use anonymous type syntax
        //     to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        public void Sql(string sql, bool suppressTransaction = false, object anonymousArguments = null)
        {
            object[] args = { sql, suppressTransaction, anonymousArguments };
            Type[] types = MethodBase.GetCurrentMethod().GetParameters().ToList().Select(p => p.ParameterType).ToArray();
            this.DoInvokeMember(
                MethodBase.GetCurrentMethod().Name,
                types,
                args,
                out object result);
        }

        private bool DoInvokeMember(string name, Type[] types, object[] args, out object result)
        {
            MethodInfo methodInfo = this.migration.GetType().GetMethod(
                name,
                BindingFlags.Instance | BindingFlags.NonPublic,
                Type.DefaultBinder,
                types,
                null);
            if (methodInfo == null)
            {
                // TODO: search for a matching extension method
                throw new TargetInvocationException($"Could not find a method with the name \"{name}\" on the class DbMigration.", null);
            }

            result = methodInfo.Invoke(this.migration, args);
            return true;
        }
    }
}
