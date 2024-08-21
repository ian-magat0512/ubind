// <copyright file="PopulateFeatureSettingsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.FeatureSettings
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// This is run whenever a new feature setting is added and we want it added to each tenant.
    /// </summary>
    public class PopulateFeatureSettingsCommandHandler : ICommandHandler<PopulateFeatureSettingsCommand, Unit>
    {
        private readonly IClock clock;
        private readonly IFeatureSettingRepository featureSettingRepository;

        public PopulateFeatureSettingsCommandHandler(
            IClock clock,
            IFeatureSettingRepository featureSettingRepository)
        {
            this.clock = clock;
            this.featureSettingRepository = featureSettingRepository;
        }

        public Task<Unit> Handle(PopulateFeatureSettingsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var now = this.clock.GetCurrentInstant();
            var settings = new List<Setting>();
            var policySetting = new Setting("policy-mgmt", Feature.PolicyManagement.Humanize(), "shield", 3, now, IconLibrary.AngularMaterial);
            settings.Add(new Setting("customer-mgmt", Feature.CustomerManagement.Humanize(), "contact", 1, now, IconLibrary.IonicV4));
            settings.Add(new Setting("quote-mgmt", Feature.QuoteManagement.Humanize(), "calculator", 2, now, IconLibrary.IonicV4));
            settings.Add(policySetting);
            settings.Add(new Setting("claims-mgmt", Feature.ClaimsManagement.Humanize(), "clipboard", 4, now, IconLibrary.AngularMaterial));
            settings.Add(new Setting("user-mgmt", Feature.UserManagement.Humanize(), "people", 5, now, IconLibrary.IonicV4));
            settings.Add(new Setting("email-mgmt", Feature.MessageManagement.Humanize(), "mail", 6, now, IconLibrary.IonicV4));
            settings.Add(new Setting("reporting", Feature.Reporting.Humanize(), "pie", 7, now, IconLibrary.IonicV4));
            settings.Add(new Setting("product-management", Feature.ProductManagement.Humanize(), "cube", 9, now, IconLibrary.IonicV4));
            settings.Add(new Setting("organisation-management", Feature.OrganisationManagement.Humanize(), "business", 10, now, IconLibrary.IonicV4));
            settings.Add(new Setting("portal-management", Feature.PortalManagement.Humanize(), "browsers", 11, now, IconLibrary.IonicV4));

            var portalSetting = new List<PortalSettings>();
            portalSetting.Add(new PortalSettings("Update", now));
            portalSetting.Add(new PortalSettings("Cancel", now));
            portalSetting.Add(new PortalSettings("Renew", now));
            portalSetting.ForEach(ps => { policySetting.AddPortalSettings(ps); });

            this.featureSettingRepository.PopulateForAllTenants(settings);
            this.featureSettingRepository.SaveChanges();

            return Task.FromResult(Unit.Value);
        }
    }
}
