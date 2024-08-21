// <copyright file="GetAdditionalPropertyDefinitionByIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Queries.AdditionalPropertyDefinition
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Handler for <see cref="GetAdditionalPropertyDefinitionByIdQuery"/> class.
    /// </summary>
    public class GetAdditionalPropertyDefinitionByIdQueryHandler
        : IQueryHandler<GetAdditionalPropertyDefinitionByIdQuery,
            AdditionalPropertyDefinitionReadModel>
    {
        private readonly IAdditionalPropertyDefinitionRepository additionalPropertyRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetAdditionalPropertyDefinitionByIdQueryHandler"/> class.
        /// </summary>
        /// <param name="additionalPropertyRepository">Repository of additional property.</param>
        public GetAdditionalPropertyDefinitionByIdQueryHandler(
            IAdditionalPropertyDefinitionRepository additionalPropertyRepository)
        {
            this.additionalPropertyRepository = additionalPropertyRepository;
        }

        /// <summary>
        /// Gets the additional property by Id.
        /// </summary>
        /// <param name="query">The query to be handled. <see cref="GetAdditionalPropertyDefinitionByIdQuery"/>.</param>
        /// <param name="cancellationToken">The cancellation token to be used, if any.</param>
        /// <returns><see cref="AdditionalPropertyDefinitionReadModel"/>.</returns>
        public async Task<AdditionalPropertyDefinitionReadModel> Handle(
            GetAdditionalPropertyDefinitionByIdQuery query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await this.additionalPropertyRepository.GetById(query.TenantId, query.Id);
            return result;
        }
    }
}
