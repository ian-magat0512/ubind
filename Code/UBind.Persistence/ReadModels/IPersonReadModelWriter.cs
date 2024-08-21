// <copyright file="IPersonReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;

    /// <summary>
    /// Reponsible for updating the person read model.
    /// </summary>
    public interface IPersonReadModelWriter : IPersonEventObserver, IUserEventObserver, ICustomerEventObserver, IReadModelWriter
    {
    }
}
