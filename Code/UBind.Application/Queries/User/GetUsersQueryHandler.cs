// <copyright file="GetUsersQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;

    public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, List<UserReadModel>>
    {
        private readonly IUserReadModelRepository userReadModelRepository;

        public GetUsersQueryHandler(
            IUserReadModelRepository userReadModelRepository)
        {
            this.userReadModelRepository = userReadModelRepository;
        }

        public Task<List<UserReadModel>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.userReadModelRepository.GetUsers(query.TenantId, query.Filters));
        }
    }
}
