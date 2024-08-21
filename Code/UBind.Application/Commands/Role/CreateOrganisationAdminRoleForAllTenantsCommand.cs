// <copyright file="CreateOrganisationAdminRoleForAllTenantsCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Role
{
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command which is run as a startup job which creates the organisation admin role for all tenants.
    /// </summary>
    public class CreateOrganisationAdminRoleForAllTenantsCommand : ICommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateOrganisationAdminRoleForAllTenantsCommand"/> class.
        /// </summary>
        public CreateOrganisationAdminRoleForAllTenantsCommand()
        {
        }
    }
}
