// <copyright file="GetRoleByIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Role;

using System.Threading;
using System.Threading.Tasks;
using UBind.Domain.Entities;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Repositories;

public class GetRoleByIdQueryHandler : IQueryHandler<GetRoleByIdQuery, Domain.Entities.Role>
{
    private readonly IRoleRepository roleRepository;

    public GetRoleByIdQueryHandler(IRoleRepository roleRepository)
    {
        this.roleRepository = roleRepository;
    }

    public Task<Role> Handle(GetRoleByIdQuery query, CancellationToken cancellationToken)
    {
        var role = this.roleRepository.GetRoleById(query.TenantId, query.RoleId);
        EntityHelper.ThrowIfNotFound(role, query.RoleId, "ID");
        return Task.FromResult(role);
    }
}
