// <copyright file="GetPortalByIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Portal
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel.Portal;

    public class GetPortalByIdQueryHandler : IQueryHandler<GetPortalByIdQuery, PortalReadModel>
    {
        private readonly IPortalReadModelRepository portalRepository;

        public GetPortalByIdQueryHandler(IPortalReadModelRepository portalRepository)
        {
            this.portalRepository = portalRepository;
        }

        public Task<PortalReadModel> Handle(GetPortalByIdQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var portal = this.portalRepository.GetPortalById(request.TenantId, request.PortalId);
            return Task.FromResult(portal);
        }
    }
}
