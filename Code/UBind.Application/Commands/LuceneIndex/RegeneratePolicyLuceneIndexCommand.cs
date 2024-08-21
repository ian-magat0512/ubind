// <copyright file="RegeneratePolicyLuceneIndexCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.LuceneIndex
{
    using System.Collections.Generic;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// This is the command handler to regenerate the policy lucene indexes.
    /// We need to regenerate the lucene index so that the index folder will be clear
    /// because every generation of index will create the del file so it will increase the
    /// size of our storage. the regeneration of index will run every week.
    /// </summary>
    public class RegeneratePolicyLuceneIndexCommand : ICommand
    {
        public RegeneratePolicyLuceneIndexCommand(DeploymentEnvironment environment, IEnumerable<Tenant> tenants = null)
        {
            this.Environment = environment;
            this.Tenants = tenants;
        }

        public DeploymentEnvironment Environment { get; }

        public IEnumerable<Tenant> Tenants { get; }
    }
}
