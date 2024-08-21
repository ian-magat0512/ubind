// <copyright file="GetUsersMatchingEmailAddressIncludingPlusAddressingQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.User;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// Command handler for getting users that matches an email address, including plus addressing.
    /// So if the email address passed is "person@domain", then the user with email address
    /// person+test@domain will also be matched.
    /// </summary>
    public class GetUsersMatchingEmailAddressIncludingPlusAddressingQueryHandler
        : IQueryHandler<GetUsersMatchingEmailAddressIncludingPlusAddressingQuery, IEnumerable<UserModel>>
    {
        private readonly IUserReadModelRepository userReadModelRepository;

        public GetUsersMatchingEmailAddressIncludingPlusAddressingQueryHandler(
            IUserReadModelRepository userReadModelRepository)
        {
            this.userReadModelRepository = userReadModelRepository;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<UserModel>> Handle(GetUsersMatchingEmailAddressIncludingPlusAddressingQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var users = this.GetUsersByEmail(query.TenantId, query.EmailAddress, query.Blocked);
            if (query.OrganisationIds != null && query.OrganisationIds.Any())
            {
                users = users.Where(u => query.OrganisationIds.Contains(u.OrganisationId));
            }

            var userModels = users.Select(user => new UserModel(user));
            return await Task.FromResult(userModels.ToList());
        }

        private IEnumerable<UserReadModel> GetUsersByEmail(Guid? tenantId, string email, bool? blocked)
        {
            IEnumerable<UserReadModel> userModels;
            if (!tenantId.HasValue)
            {
                userModels = this.userReadModelRepository.GetUsersMatchingEmailAddressIncludingPlusAddressingForAllTenants(email);
            }
            else
            {
                userModels = this.userReadModelRepository.GetUsersMatchingEmailAddressIncludingPlusAddressing(tenantId.Value, email);
            }

            return blocked != null ? userModels.Where(user => user.IsDisabled == blocked) : userModels;
        }
    }
}
