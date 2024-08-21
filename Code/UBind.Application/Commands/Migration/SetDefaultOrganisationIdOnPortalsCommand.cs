// <copyright file="SetDefaultOrganisationIdOnPortalsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Sets the tenant's default organanisation id on portals.
    /// This is needed because we are addding organisation ID to portals
    /// and so we need to set an initial value. Since all portals were only
    /// against the tenant previously, it's fine to set the organisation ID
    /// to the default.
    /// </summary>
    public class SetDefaultOrganisationIdOnPortalsCommand : ICommand
    {
    }
}
