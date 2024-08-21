// <copyright file="RegeneratePolicyLuceneIndexCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.LuceneIndex
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using UBind.Application.Services.Search;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Search;

    /// <summary>
    /// This is the command handler to regenerate the policy lucene indexes.
    /// We need to regenerate the lucene index so that the index folder will be clear
    /// because every generation of index will create the del file so it will increase the
    /// size of our storage. the regeneration of index will run every week.
    /// </summary>
    public class RegeneratePolicyLuceneIndexCommandHandler : ICommandHandler<RegeneratePolicyLuceneIndexCommand, Unit>
    {
        private readonly ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters> policySearchIndexService;
        private readonly ICachingResolver cachingResolver;
        private readonly ILogger<RegeneratePolicyLuceneIndexCommandHandler> logger;

        public RegeneratePolicyLuceneIndexCommandHandler(
            ISearchableEntityService<IPolicySearchResultItemReadModel, PolicyReadModelFilters> policySearchIndexService,
            ICachingResolver cachingResolver,
            ILogger<RegeneratePolicyLuceneIndexCommandHandler> logger)
        {
            this.policySearchIndexService = policySearchIndexService;
            this.cachingResolver = cachingResolver;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(RegeneratePolicyLuceneIndexCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.logger.LogInformation("RegeneratePolicyLuceneIndexCommandHandler: Regenerating policy lucene index");
            var activeProducts = await this.cachingResolver.GetActiveProducts();
            this.policySearchIndexService.RegenerateLuceneIndexes(
                command.Environment,
                activeProducts,
                cancellationToken,
                command.Tenants);
            return Unit.Value;
        }
    }
}
