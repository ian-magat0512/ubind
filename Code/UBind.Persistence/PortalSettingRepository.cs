// <copyright file="PortalSettingRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class PortalSettingRepository : IPortalSettingRepository
    {
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalSettingRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The UBind database context.</param>
        public PortalSettingRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public IReadOnlyList<PortalSettings> GetAllPortalSettings()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public PortalSettings GetPortalSettingsById(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Insert(PortalSettings settings)
        {
            this.dbContext.PortalSettings.Add(settings);
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public void Update(Guid id, PortalSettingDetails details)
        {
            var setting = this.dbContext.PortalSettings.FirstOrDefault(s => s.Id == id);
            setting.Update(details);
        }
    }
}
