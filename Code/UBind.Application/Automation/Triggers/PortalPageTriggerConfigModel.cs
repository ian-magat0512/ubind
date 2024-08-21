// <copyright file="PortalPageTriggerConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Triggers
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;

    public class PortalPageTriggerConfigModel : IBuilder<Trigger>
    {
        [JsonConstructor]
        public PortalPageTriggerConfigModel(
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
            IBuilder<IProvider<Data<string>>> successSnackbarText,
            IBuilder<IProvider<Data<FileInfo>>> downloadFile,
            bool includeInMenu)
        {
            this.Name = name;
            this.Alias = alias;
            this.Description = description;
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

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("alias")]
        public string Alias { get; private set; }

        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("pages")]
        public List<Page> Pages { get; private set; }

        [JsonProperty("actionIcon")]
        public string ActionIcon { get; private set; }

        // For backwards compatibility.
        [JsonProperty("actionItemIcon")]
        [Obsolete("Do not use. instead use actionIcon")]
        public string ActionItemIcon
        {
            set { this.ActionIcon = value; }
        }

        [JsonProperty("actionIconLibrary")]
        public string ActionIconLibrary { get; private set; }

        [JsonProperty("actionName")]
        public string ActionName { get; private set; }

        [JsonProperty("actionButtonLabel")]
        public string ActionButtonLabel { get; private set; }

        // For backwards compatibility
        [JsonProperty("actionItemLabel")]
        [Obsolete("Do not use. instead use actionButtonLabel")]
        public string ActionItemLabel
        {
            set { this.ActionButtonLabel = value; }
        }

        [JsonProperty("actionButtonPrimary")]
        public bool ActionButtonPrimary { get; private set; }

        [DefaultValue(true)]
        [JsonProperty("includeInMenu", DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool IncludeInMenu { get; private set; }

        [JsonProperty("spinnerAlertText")]
        public string SpinnerAlertText { get; private set; }

        public IBuilder<IProvider<Data<string>>> SuccessSnackbarText { get; }

        public IBuilder<IProvider<Data<FileInfo>>> DownloadFile { get; }

        public Trigger Build(IServiceProvider dependencyProvider)
        {
            return new PortalPageTrigger(
                this.Name,
                this.Alias,
                this.Description,
                this.Pages,
                this.ActionIcon,
                this.ActionIconLibrary,
                this.ActionName,
                this.ActionButtonLabel,
                this.ActionButtonPrimary,
                this.SpinnerAlertText,
                this.SuccessSnackbarText.Build(dependencyProvider),
                this.DownloadFile?.Build(dependencyProvider),
                this.IncludeInMenu);
        }
    }
}
