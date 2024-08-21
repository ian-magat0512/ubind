// <copyright file="PortalPageTrigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Triggers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;

    /// <summary>
    /// Represents a trigger that is invoked by clicking an action item or popover menu item in the portal.
    /// </summary>
    public class PortalPageTrigger : Trigger
    {
        public PortalPageTrigger(
            string name,
            string alias,
            string description,
            List<Page> pages,
            string actionIcon,
            string actionIconLibrary,
            string actionName,
            string actionButtonLabel,
            bool actionButtonPrimary,
            string spinnerAlertText,
            IProvider<Data<string>> successSnackbarText,
            IProvider<Data<FileInfo>>? downloadFile,
            bool includeInMenu = true)
            : base(name, alias, description)
        {
            this.Pages = pages;
            this.ActionIcon = actionIcon;
            this.ActionIconLibrary = actionIconLibrary;
            this.ActionName = actionName;
            this.ActionButtonLabel = actionButtonLabel;
            this.ActionButtonPrimary = actionButtonPrimary;
            this.IncludeInMenu = includeInMenu;
            this.SpinnerAlertText = spinnerAlertText;
            this.SuccessSnackbarText = successSnackbarText;
            this.DownloadFile = downloadFile;
        }

        public List<Page> Pages { get; private set; }

        public string ActionIcon { get; private set; }

        public string ActionIconLibrary { get; private set; }

        public string ActionName { get; private set; }

        public string ActionButtonLabel { get; private set; }

        public bool ActionButtonPrimary { get; private set; }

        public bool IncludeInMenu { get; private set; }

        public string SpinnerAlertText { get; private set; }

        public IProvider<Data<string>> SuccessSnackbarText { get; private set; }

        public IProvider<Data<FileInfo>>? DownloadFile { get; private set; }

        public override Task<bool> DoesMatch(AutomationData dataContext)
        {
            if (!(dataContext.Trigger is PortalPageTriggerData trigger))
            {
                return Task.FromResult(false);
            }

            if (this.Alias != dataContext.Trigger.TriggerAlias)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(this.Pages.Any(
                p => p.EntityType == trigger.EntityType && p.PageType == trigger.PageType));
        }

        public override async Task GenerateCompletionResponse(IProviderContext providerContext)
        {
            var automationData = providerContext.AutomationData;
            var fileInfo = (await this.DownloadFile.ResolveValueIfNotNull(providerContext))?.DataValue;
            if (fileInfo != null)
            {
                ((PortalPageTriggerData)automationData.Trigger!).DownloadFile = fileInfo;
            }

            string successMessage = (await this.SuccessSnackbarText.Resolve(providerContext)).GetValueOrThrowIfFailed();
            ((PortalPageTriggerData)automationData.Trigger!).SuccessSnackbarText = successMessage;
        }
    }
}
