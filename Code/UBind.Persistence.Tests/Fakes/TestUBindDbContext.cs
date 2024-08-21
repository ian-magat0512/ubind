// <copyright file="TestUBindDbContext.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Tests.Fakes
{
    using System;

    /// <summary>
    /// Modified DbContext for testing, that allows us to inject conflictin updates and trigger concurrency exceptions.
    /// </summary>
    public class TestUBindDbContext : UBindDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestUBindDbContext"/> class with a particular connection string.
        /// </summary>
        /// <param name="connectionString">The connection string or name of connection string in settings.</param>
        /// <param name="logMethod">The logging method.</param>
        public TestUBindDbContext(string connectionString, Action<string> logMethod = null)
            : base(connectionString)
        {
            if (logMethod != null)
            {
                this.Database.Log = logMethod;
            }
        }

        public event Action<UBindDbContext> BeforeSaveChanges = _ => { };

        public override int SaveChanges()
        {
            this.BeforeSaveChanges(this);
            return base.SaveChanges();
        }
    }
}
