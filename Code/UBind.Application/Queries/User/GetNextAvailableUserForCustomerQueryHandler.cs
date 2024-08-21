// <copyright file="GetNextAvailableUserForCustomerQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;

    public class GetNextAvailableUserForCustomerQueryHandler
        : IQueryHandler<GetNextAvailableUserForCustomerQuery, UserReadModel>
    {
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly IUserReadModelRepository userReadModelRepository;

        public GetNextAvailableUserForCustomerQueryHandler(
            IPersonReadModelRepository personReadModelRepository,
            IUserReadModelRepository userReadModelRepository)
        {
            this.personReadModelRepository = personReadModelRepository;
            this.userReadModelRepository = userReadModelRepository;
        }

        public Task<UserReadModel> Handle(
            GetNextAvailableUserForCustomerQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var persons = this.personReadModelRepository.GetPersonsAssociatedWithUsersByCustomerId(request.TenantId, request.CustomerId)
                .OrderBy(p => p.CreatedTimestamp);

            foreach (var person in persons)
            {
                var user = this.userReadModelRepository.GetUser(((IReadModel<Guid>)person).TenantId, person.UserId.Value);
                if (user != null)
                {
                    return Task.FromResult(user);
                }
            }

            return Task.FromResult<UserReadModel>(null);
        }
    }
}
