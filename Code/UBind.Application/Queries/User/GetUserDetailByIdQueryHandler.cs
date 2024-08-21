// <copyright file="GetUserDetailByIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;

    public class GetUserDetailByIdQueryHandler : IQueryHandler<GetUserDetailByIdQuery, UserReadModelDetail>
    {
        private readonly IUserReadModelRepository userReadModelRepository;

        public GetUserDetailByIdQueryHandler(IUserReadModelRepository userReadModelRepository)
        {
            this.userReadModelRepository = userReadModelRepository;
        }

        public Task<UserReadModelDetail> Handle(GetUserDetailByIdQuery query, CancellationToken cancellationToken)
        {
            var user = this.userReadModelRepository.GetUserDetail(query.TenantId, query.UserId);
            return Task.FromResult(user);
        }
    }
}
