// <copyright file="GetOrganisationByIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Organisation
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Gets an org.
    /// </summary>
    public class GetOrganisationByIdQueryHandler : IQueryHandler<GetOrganisationByIdQuery, OrganisationReadModel>
    {
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetOrganisationByIdQueryHandler"/> class.
        /// </summary>
        /// <param name="organisationReadModelRepository">The org read model repo.</param>
        public GetOrganisationByIdQueryHandler(
            IOrganisationReadModelRepository organisationReadModelRepository)
        {
            this.organisationReadModelRepository = organisationReadModelRepository;
        }

        /// <inheritdoc/>
        public async Task<OrganisationReadModel> Handle(GetOrganisationByIdQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var organisation = this.organisationReadModelRepository.Get(request.TenantId, request.OrganisationId);
            return await Task.FromResult(organisation);
        }
    }
}
