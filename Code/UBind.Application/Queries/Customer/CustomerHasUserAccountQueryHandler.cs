// <copyright file="CustomerHasUserAccountQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Customer
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    public class CustomerHasUserAccountQueryHandler : IQueryHandler<CustomerHasUserAccountQuery, bool>
    {
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly IUserReadModelRepository userReadModelRepository;

        public CustomerHasUserAccountQueryHandler(
            IPersonReadModelRepository personReadModelRepository,
            IUserReadModelRepository userReadModelRepository)
        {
            this.personReadModelRepository = personReadModelRepository;
            this.userReadModelRepository = userReadModelRepository;
        }

        public Task<bool> Handle(CustomerHasUserAccountQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var persons = this.personReadModelRepository.GetPersonsAssociatedWithUsersByCustomerId(request.TenantId, request.CustomerId);

            // Just check if any of these persons has a record in user read model repository.
            var customerHasUserAccount
                = persons.Any(p => this.userReadModelRepository.GetUser(((IEntityReadModel<Guid>)p).TenantId, p.UserId.Value) != null);
            return Task.FromResult(customerHasUserAccount);
        }
    }
}
