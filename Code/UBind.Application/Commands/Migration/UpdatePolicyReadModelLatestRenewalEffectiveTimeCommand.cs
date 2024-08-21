// <copyright file="UpdatePolicyReadModelLatestRenewalEffectiveTimeCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// This is to update the policy read model with latest renewal effective date.
    /// getting the latest creation date on policy transaction where the transaction type is renewal.
    /// </summary>
    public class UpdatePolicyReadModelLatestRenewalEffectiveTimeCommand : ICommand
    {
    }
}
