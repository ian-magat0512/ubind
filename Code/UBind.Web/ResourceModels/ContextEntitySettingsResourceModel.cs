// <copyright file="ContextEntitySettingsResourceModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using UBind.Domain.Configuration;

    public class ContextEntitySettingsResourceModel
    {
        public ContextEntitySettingsResourceModel(IContextEntitySettings searchResult)
        {
            this.IncludeContextEntities = searchResult.IncludeContextEntities;
            this.ReloadIntervalSeconds = searchResult?.ReloadIntervalSeconds;
            this.ReloadWithOperations = searchResult?.ReloadWithOperations;
        }

        public string[] IncludeContextEntities { get; }

        public int? ReloadIntervalSeconds { get; }

        public string[] ReloadWithOperations { get; }
    }
}
