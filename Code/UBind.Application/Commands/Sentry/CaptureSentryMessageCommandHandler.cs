// <copyright file="CaptureSentryMessageCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Sentry
{
    using System.Threading;
    using System.Threading.Tasks;
    using global::Sentry;
    using MediatR;
    using UBind.Application.Services.Imports;
    using UBind.Application.Services.MachineInformation;
    using UBind.Domain.Patterns.Cqrs;

    public class CaptureSentryMessageCommandHandler : ICommandHandler<CaptureSentryMessageCommand>
    {
        private readonly IHub sentryHub;
        private readonly IInternalUrlConfiguration internalUrlConfig;
        private readonly IMachineInformationService machineInformation;

        public CaptureSentryMessageCommandHandler(
            IHub sentryHub,
            IInternalUrlConfiguration internalUrlConfig,
            IMachineInformationService machineInformation)
        {
            this.sentryHub = sentryHub;
            this.internalUrlConfig = internalUrlConfig;
            this.machineInformation = machineInformation;
        }

        public Task<Unit> Handle(CaptureSentryMessageCommand command, CancellationToken cancellationToken)
        {
            this.sentryHub.CaptureMessage(command.Message, scope =>
            {
                scope.SetTag("Base Url: ", this.internalUrlConfig.BaseApi);
                scope.SetTag("IP Address: ", this.machineInformation.GetIPAddress());
                scope.Environment = !string.IsNullOrEmpty(command.Environment?.ToString())
                    ? command.Environment?.ToString().ToLower()
                    : null;

                if (command.Tags != null)
                {
                    foreach (var tag in command.Tags)
                    {
                        scope.SetTag(tag.Key, tag.Value);
                    }
                }
            });

            return Task.FromResult(Unit.Value);
        }
    }
}
