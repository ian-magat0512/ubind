// <copyright file="CreateTenantCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Tenant
{
    using System;
    using UBind.Domain.Patterns.Cqrs;

    public class CreateTenantCommand : ICommand<Guid>
    {
        public CreateTenantCommand(string name, string alias, string customDomain)
        {
            this.Name = name;
            this.Alias = alias;
            this.CustomDomain = customDomain;
        }

        public string Name { get; }

        public string Alias { get; }

        public string CustomDomain { get; }
    }
}
