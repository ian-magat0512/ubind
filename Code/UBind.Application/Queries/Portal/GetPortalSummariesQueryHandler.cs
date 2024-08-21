// <copyright file="GetPortalSummariesQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Portal
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Portal;

    public class GetPortalSummariesQueryHandler
        : IQueryHandler<GetPortalSummariesQuery, IEnumerable<PortalReadModelSummary>>
    {
        private readonly IPortalReadModelRepository portalReadModelRepository;

        public GetPortalSummariesQueryHandler(IPortalReadModelRepository portalReadModelRepository)
        {
            this.portalReadModelRepository = portalReadModelRepository;
        }

        public Task<IEnumerable<PortalReadModelSummary>> Handle(GetPortalSummariesQuery query, CancellationToken cancellationToken)
        {
            var result = this.portalReadModelRepository.GetPortals(query.TenantId, query.Filters);
            return Task.FromResult(result);
        }
    }
}
