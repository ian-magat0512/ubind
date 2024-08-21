// <copyright file="GetPersonsMatchingFiltersQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Person
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Query handler for getting person records for customer by person id.
    /// </summary>
    public class GetPersonsMatchingFiltersQueryHandler
        : IQueryHandler<GetPersonsMatchingFiltersQuery, IReadOnlyList<IPersonReadModelSummary>>
    {
        private readonly IPersonReadModelRepository personReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetPersonsMatchingFiltersQueryHandler"/> class.
        /// </summary>
        /// <param name="personReadModelRepository">The person read model repository.</param>
        public GetPersonsMatchingFiltersQueryHandler(IPersonReadModelRepository personReadModelRepository)
        {
            this.personReadModelRepository = personReadModelRepository;
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<IPersonReadModelSummary>> Handle(
            GetPersonsMatchingFiltersQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<IPersonReadModelSummary> persons
                = this.personReadModelRepository.GetPersonsMatchingFilters(request.TenantId, request.Filters).ToList();
            return Task.FromResult(persons);
        }
    }
}
