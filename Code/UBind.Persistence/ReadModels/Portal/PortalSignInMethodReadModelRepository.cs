// <copyright file="PortalSignInMethodReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.Portal
{
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.Repositories;

    public class PortalSignInMethodReadModelRepository : IPortalSignInMethodReadModelRepository
    {
        private readonly IUBindDbContext dbContext;

        public PortalSignInMethodReadModelRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public PortalSignInMethodReadModel? Get(Guid tenantId, Guid portalId, Guid authenticationMethodId)
        {
            return this.dbContext.PortalSignInMethods
                .Where(p => p.TenantId == tenantId
                    && p.PortalId == portalId
                    && p.Id == authenticationMethodId)
                .SingleOrDefault();
        }

        public IList<PortalSignInMethodReadModel> GetAll(Guid tenantId, Guid portalId)
        {
            return this.dbContext.PortalSignInMethods
                .Where(p => p.TenantId == tenantId && p.PortalId == portalId)
                .ToList();
        }
    }
}
