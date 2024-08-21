// <copyright file="Page.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Triggers
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UBind.Application.Automation.Enums;
    using UBind.Domain;

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Page
    {
        public Page(EntityType entityType, PageType pageType, List<string> tabs = null)
        {
            this.EntityType = entityType;
            this.PageType = pageType;
            if (tabs != null)
            {
                this.Tabs = tabs;
            }
        }

        [JsonProperty("entityType")]
        public EntityType EntityType { get; set; }

        [JsonProperty("pageType")]
        public PageType PageType { get; set; }

        [JsonProperty("tabs")]
        public IList<string> Tabs { get; set; }
    }
}
