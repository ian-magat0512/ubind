// <copyright file="PerilsService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Clients.DVA.Perils.Services
{
    using UBind.Domain.Clients.DVA.Perils.Entities;
    using UBind.Domain.Clients.DVA.Perils.Interfaces;

    /// <inheritdoc/>
    public class PerilsService : IPerilsService
    {
        private readonly IPerilsRepository perilsRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerilsService"/> class.
        /// </summary>
        /// <param name="perilsRepository">The perils database repository.</param>
        public PerilsService(IPerilsRepository perilsRepository)
        {
            this.perilsRepository = perilsRepository;
        }

        /// <inheritdoc/>
        public Peril GetDetailsByPropertyId(string propertyId)
        {
            return this.perilsRepository.GetDetailsByPropertyId(propertyId);
        }
    }
}
