// <copyright file="GetCustomerSummaryByPersonIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Customer
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Customer;

    public class GetCustomerSummaryByPersonIdQueryHandler
        : IQueryHandler<GetCustomerSummaryByPersonIdQuery, ICustomerReadModelSummary>
    {
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly ICustomerReadModelRepository customerReadModelRepository;

        public GetCustomerSummaryByPersonIdQueryHandler(
            IPersonReadModelRepository personReadModelRepository,
            ICustomerReadModelRepository customerReadModelRepository)
        {
            this.personReadModelRepository = personReadModelRepository;
            this.customerReadModelRepository = customerReadModelRepository;
        }

        public Task<ICustomerReadModelSummary> Handle(
            GetCustomerSummaryByPersonIdQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var person = this.personReadModelRepository.GetPersonById(request.TenantId, request.PersonId);
            if (person == null)
            {
                throw new ErrorException(Errors.Person.NotFound(request.PersonId));
            }
            else if (!person.CustomerId.HasValue)
            {
                throw new ErrorException(Errors.Person.CustomerNotFound(request.PersonId));
            }

            ICustomerReadModelSummary details
                = this.customerReadModelRepository.GetCustomerById(person.TenantId, person.CustomerId.Value);
            return Task.FromResult(details);
        }
    }
}
