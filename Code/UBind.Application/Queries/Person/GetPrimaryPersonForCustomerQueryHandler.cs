// <copyright file="GetPrimaryPersonForCustomerQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.Person
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Query handler for getting primary person record by Id.
    /// </summary>
    public class GetPrimaryPersonForCustomerQueryHandler
        : IQueryHandler<GetPrimaryPersonForCustomerQuery, IPersonReadModelSummary?>
    {
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly ICustomerReadModelRepository customerReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPrimaryPersonForCustomerQueryHandler"/> class.
        /// </summary>
        /// <param name="personReadModelRepository">The person read model repository.</param>
        /// <param name="customerReadModelRepository">The customer read model repository.</param>
        public GetPrimaryPersonForCustomerQueryHandler(
            IPersonReadModelRepository personReadModelRepository,
            ICustomerReadModelRepository customerReadModelRepository)
        {
            this.personReadModelRepository = personReadModelRepository;
            this.customerReadModelRepository = customerReadModelRepository;
        }

        /// <inheritdoc/>
        public Task<IPersonReadModelSummary?> Handle(
            GetPrimaryPersonForCustomerQuery query,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var customer = this.customerReadModelRepository.GetCustomerById(query.TenantId, query.CustomerId)
                ?? throw new ErrorException(Errors.Customer.NotFound(query.CustomerId));

            var primaryPerson
                = this.personReadModelRepository.GetPersonSummaryById(customer.TenantId, customer.PrimaryPersonId);

            return Task.FromResult(primaryPerson);
        }
    }
}
