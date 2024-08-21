// <copyright file="RollbackMessageManagementSettingsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.FeatureSettings
{
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Patterns.Cqrs;

    internal class RollbackMessageManagementSettingsCommandHandler : ICommandHandler<RollbackMessageManagementSettingsCommand, Unit>
    {
        private readonly IClock clock;
        private readonly IFeatureSettingRepository featureSettingRepository;

        public RollbackMessageManagementSettingsCommandHandler(
            IClock clock,
            IFeatureSettingRepository featureSettingRepository)
        {
            this.clock = clock;
            this.featureSettingRepository = featureSettingRepository;
        }

        public Task<Unit> Handle(RollbackMessageManagementSettingsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var setting = new Setting("email-mgmt", Feature.MessageManagement.Humanize(), "mail", 6, this.clock.GetCurrentInstant(), IconLibrary.IonicV4);
            this.featureSettingRepository.Upsert(setting);
            this.featureSettingRepository.SaveChanges();

            return Task.FromResult(Unit.Value);
        }
    }
}
