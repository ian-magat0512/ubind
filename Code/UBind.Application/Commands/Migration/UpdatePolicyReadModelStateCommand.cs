// <copyright file="UpdatePolicyReadModelStateCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using MediatR;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// This is to update the policy state command for migration purpose.
    /// We used to have dynamic policy statuses, based upon date. E.g. a policy
    /// was in the Pending state, until the effective date (where a policy start date was in the future)
    /// We decided to change this, so that instead we set the policy status using a scheduled job which
    /// runs every minute (or so). This way we are storing the actual status in the database, which makes
    /// querying, indexing and searching much easier, and faster to execute, since we no longer have to
    /// compare timestamps.
    /// </summary>
    public class UpdatePolicyReadModelStateCommand : ICommand<Unit>
    {
    }
}
