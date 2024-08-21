// <copyright file="DatabaseCollection.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

using Xunit;

[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly, DisableTestParallelization = false, MaxParallelThreads = -1)]

namespace UBind.Persistence.Tests
{
    /// <summary>
    /// A class used to put all database tests into a collection so that it prepares the database before running tests in that collection.
    /// </summary>
    [CollectionDefinition(Name)]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces, for Xunit test classes.
        public const string Name = "Database collection";
    }
}
