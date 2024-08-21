// <copyright file="ErrorDataKey.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Enums
{
    public struct ErrorDataKey
    {
        public static string Tenant { get; } = "tenantId";

        public static string TenantAlias { get; } = "tenantAlias";

        public static string Product { get; } = "productId";

        public static string ProductAlias { get; } = "productAlias";

        public static string Environment { get; } = "environment";

        public static string Feature { get; } = "feature";

        public static string Automation { get; } = "automation";

        public static string ActionPath { get; } = "actionPath";

        public static string ActionType { get; } = "actionType";

        public static string ActionAlias { get; } = "actionAlias";

        public static string TriggerType { get; } = "triggerType";

        public static string TriggerAlias { get; } = "triggerAlias";

        public static string ProviderType { get; } = "providerType";

        public static string ValueToParse { get; } = "valueToParse";

        public static string ErrorMessage { get; } = "errorMessage";

        public static string StackTrace { get; } = "stackTrace";

        public static string EntityId { get; } = "entityId";

        public static string EntityType { get; } = "entityType";

        public static string EntityDescriptor { get; } = "entityDescriptor";

        public static string EntityReference { get; } = "entityReference";

        public static string EntityEnvironment { get; } = "entityEnvironment";

        public static string ValueType { get; } = "valueType";

        public static string QueryPath { get; } = "queryPath";
    }
}
