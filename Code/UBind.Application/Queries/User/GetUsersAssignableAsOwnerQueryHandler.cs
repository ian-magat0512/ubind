// <copyright file="GetUsersAssignableAsOwnerQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// Handle the get users that are assignable as owner query.
    /// </summary>
    public class GetUsersAssignableAsOwnerQueryHandler : IQueryHandler<GetUsersAssignableAsOwnerQuery, List<UserReadModel>>
    {
        private readonly IUserReadModelRepository userReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetUsersAssignableAsOwnerQueryHandler"/> class.
        /// </summary>
        /// <param name="userReadModelRepository">The user repository.</param>
        public GetUsersAssignableAsOwnerQueryHandler(IUserReadModelRepository userReadModelRepository)
        {
            this.userReadModelRepository = userReadModelRepository;
        }

        /// <summary>
        /// The method to handle query.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The list of users.</returns>
        public Task<List<UserReadModel>> Handle(GetUsersAssignableAsOwnerQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var permissions = new Permission[] { Permission.ManageCustomers };
            var users = this.userReadModelRepository.GetUserWithAnyOfThePermissions(request.TenantId, request.OrganisationId, permissions);
            return Task.FromResult(users.ToList());
        }
    }
}
