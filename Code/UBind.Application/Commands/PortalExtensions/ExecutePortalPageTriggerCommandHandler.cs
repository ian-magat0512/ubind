// <copyright file="ExecutePortalPageTriggerCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.PortalExtensions
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.Services;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Command handler to execute portal page trigger.
    /// </summary>
    public class ExecutePortalPageTriggerCommandHandler : ICommandHandler<ExecutePortalPageTriggerCommand, (FileInfo file, string successMesssage)>
    {
        private readonly IAutomationService automationService;

        public ExecutePortalPageTriggerCommandHandler(IAutomationService automationService)
        {
            this.automationService = automationService;
        }

        public Task<(FileInfo, string)> Handle(ExecutePortalPageTriggerCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = this.automationService.TriggerPortalPageAutomation(
                request.TenantId,
                request.OrganisationId,
                request.ProductId,
                request.Environment,
                request.AutomationAlias,
                request.TriggerAlias,
                request.EntityType,
                request.PageType,
                request.Tab,
                request.User,
                request.Filters,
                request.EntityId,
                cancellationToken);

            return result;
        }
    }
}
