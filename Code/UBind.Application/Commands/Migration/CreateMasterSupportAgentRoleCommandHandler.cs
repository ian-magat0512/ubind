// <copyright file="CreateMasterSupportAgentRoleCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Repositories;

    public class CreateMasterSupportAgentRoleCommandHandler : ICommandHandler<CreateMasterSupportAgentRoleCommand, Unit>
    {
        private readonly IUBindDbContext dbContext;
        private readonly IRoleRepository roleRepository;
        private readonly IClock clock;

        public CreateMasterSupportAgentRoleCommandHandler(
            IUBindDbContext dbContext,
            IRoleRepository roleRepository,
            IClock clock)
        {
            this.dbContext = dbContext;
            this.roleRepository = roleRepository;
            this.clock = clock;
        }

        public Task<Unit> Handle(CreateMasterSupportAgentRoleCommand request, CancellationToken cancellationToken)
        {
            var roleInfo = DefaultRole.MasterSupportAgent.GetAttributeOfType<RoleInformationAttribute>();
            if (!this.dbContext.Roles
                .Any(r => r.TenantId == Tenant.MasterTenantId && r.Type == RoleType.Master && r.Name == roleInfo.Name))
            {
                var masterSupportAgent = new Role(
                    Tenant.MasterTenantId, default, DefaultRole.MasterSupportAgent, this.clock.Now());
                this.dbContext.Roles.Add(masterSupportAgent);
                this.dbContext.SaveChanges();
            }

            return Task.FromResult(Unit.Value);
        }
    }
}
