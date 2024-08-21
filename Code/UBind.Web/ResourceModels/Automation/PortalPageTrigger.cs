// <copyright file="PortalPageTrigger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels.Automation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain;

    /// <summary>
    /// A resource model which represents the configuration of a portal page trigger.
    /// This is used to render additional actions in the portal, as defined by an automation.
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class PortalPageTrigger
    {
        public PortalPageTrigger(
            Guid tenantId,
            Guid? productId,
            DeploymentEnvironment environment,
            string automationAlias,
            Application.Automation.Triggers.PortalPageTrigger trigger)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.Environment = environment.ToString();
            this.AutomationAlias = automationAlias;
            this.TriggerAlias = trigger.Alias;
            this.Pages = trigger.Pages.Select(p =>
            {
                return new Page
                {
                    EntityType = p.EntityType.ToString(),
                    PageType = p.PageType.ToString(),
                    Tabs = p.Tabs,
                };
            });
            this.ActionName = trigger.ActionName;
            this.ActionIcon = trigger.ActionIcon;
            this.ActionIconLibrary = trigger.ActionIconLibrary;
            this.ActionButtonLabel = trigger.ActionButtonLabel;
            this.ActionButtonPrimary = trigger.ActionButtonPrimary;
            this.IncludeInMenu = trigger.IncludeInMenu;
            this.SpinnerAlertText = trigger.SpinnerAlertText;
        }

        [JsonProperty("tenantId")]
        public Guid TenantId { get; set; }

        [JsonProperty("productId")]
        public Guid? ProductId { get; set; }

        [JsonProperty("environment")]
        public string Environment { get; set; }

        [JsonProperty("automationAlias")]
        public string AutomationAlias { get; set; }

        [JsonProperty("triggerAlias")]
        public string TriggerAlias { get; set; }

        [JsonProperty("pages")]
        public IEnumerable<Page> Pages { get; set; }

        [JsonProperty("actionName")]
        public string ActionName { get; set; }

        [JsonProperty("actionIcon")]
        public string ActionIcon { get; set; }

        [JsonProperty("actionIconLibrary")]
        public string ActionIconLibrary { get; set; }

        [JsonProperty("actionButtonLabel")]
        public string ActionButtonLabel { get; set; }

        [JsonProperty("actionButtonPrimary")]
        public bool ActionButtonPrimary { get; set; }

        [JsonProperty("includeInMenu")]
        public bool IncludeInMenu { get; set; }

        [JsonProperty("spinnerAlertText")]
        public string SpinnerAlertText { get; set; }

        public class Page
        {
            [JsonProperty("entityType")]
            public string EntityType { get; set; }

            [JsonProperty("pageType")]
            public string PageType { get; set; }

            [JsonProperty("tabs")]
            public IEnumerable<string> Tabs { get; set; }
        }
    }
}
