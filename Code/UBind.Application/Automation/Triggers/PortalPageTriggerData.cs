// <copyright file="PortalPageTriggerData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using Newtonsoft.Json;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Providers.File;
    using UBind.Domain;

    public class PortalPageTriggerData : TriggerData
    {
        public PortalPageTriggerData(string triggerAlias, EntityType entityType, PageType pageType, string tab)
            : base(TriggerType.PortalPageTrigger)
        {
            this.TriggerAlias = triggerAlias;
            this.EntityType = entityType;
            this.PageType = pageType;
            this.Tab = tab;
        }

        [JsonConstructor]
        public PortalPageTriggerData()
            : base(TriggerType.PortalPageTrigger)
        {
        }

        [JsonProperty("entityType")]
        public EntityType EntityType { get; set; }

        [JsonProperty("pageType")]
        public PageType PageType { get; set; }

        [JsonProperty("tab")]
        public string Tab { get; set; }

        [JsonProperty("downloadFile")]
        public FileInfo DownloadFile { get; set; }

        [JsonProperty("successSnackbarText")]
        public string SuccessSnackbarText { get; set; }
    }
}
