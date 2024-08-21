// <copyright file="CreateTransactionThatSavesChangesIfNoneExistsAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Patterns.Cqrs
{
    using System;

    /// <summary>
    /// When this attribute is added to a Command, if there is no current transaction associated with the DB Context,
    /// then one will be created, and upon completion of the command, DbContext.SaveChanges() will be called and the
    /// transaction will be committed.
    /// This allows nested commands to run, all within the one transaction created at the top level. Child commands
    /// will not create a new transaction or call DbContext.SaveChanges(), only the top level command will.
    /// </summary>
    public class CreateTransactionThatSavesChangesIfNoneExistsAttribute : Attribute
    {
    }
}
