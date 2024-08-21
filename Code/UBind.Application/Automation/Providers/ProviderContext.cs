// <copyright file="ProviderContext.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Helper;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Contextual information for the provider to use when resolving it's value/result.
    /// e.g FixedObjectPathLookup so that it knows what the current action data path is
    /// for relative lookups.
    /// </summary>
    public class ProviderContext : IProviderContext
    {
        public ProviderContext(
            AutomationData automationData,
            CancellationToken cancellationToken = default(CancellationToken),
            string? currentActionDataPath = null)
        {
            this.AutomationData = automationData;
            this.CurrentActionDataPath = currentActionDataPath;
            this.CancellationToken = cancellationToken;
            this.CancellationToken.ThrowIfCancellationRequested();
        }

        public string? CurrentActionDataPath { get; private set; }

        public AutomationData AutomationData { get; }

        public CancellationToken CancellationToken { get; }

        public async Task<JObject> GetDebugContextForProviders(string providerReferenceKey)
        {
            var errorDetails = await this.GetDebugContext();
            errorDetails.Add(ErrorDataKey.ProviderType, providerReferenceKey);
            return errorDetails;
        }

        public async Task<JObject> GetDebugContext()
        {
            var tenantId = this.AutomationData.ContextManager.Tenant.Id;
            var productId = this.AutomationData.ContextManager.Product?.Id;
            var environment = this.AutomationData.System.Environment;
            if (this.AutomationData.ServiceProvider == null)
            {
                throw new ErrorException(Errors.Automation.ServiceProviderNotFound(
                    GenericErrorDataHelper.GetGeneralErrorDetails(tenantId, productId, environment)));
            }
            var cachingResolver = this.AutomationData.ServiceProvider.GetRequiredService<Domain.ICachingResolver>();
            var tenantAlias = await cachingResolver.GetTenantAliasOrThrowAsync(tenantId);
            var genericErrorDetails = new Dictionary<string, string>()
            {
                { ErrorDataKey.Tenant, tenantId.ToString() },
                { ErrorDataKey.TenantAlias, tenantAlias },
            };

            if (productId.HasValue)
            {
                var productAlias = await cachingResolver.GetProductAliasOrThrowAsync(tenantId, productId.Value);
                genericErrorDetails.Add(ErrorDataKey.Product, productId?.ToString()!);
                genericErrorDetails.Add(ErrorDataKey.ProductAlias, productAlias);
            }

            if (environment != null)
            {
                genericErrorDetails.Add(ErrorDataKey.Environment, environment.Humanize().ToCamelCase());
            }

            genericErrorDetails.Add(ErrorDataKey.Feature, "automations");

            if (!string.IsNullOrEmpty(this.CurrentActionDataPath))
            {
                var pathValue = await this.AutomationData.GetValue<dynamic>(this.CurrentActionDataPath, null);
                if (pathValue is ActionData)
                {
                    ActionData actionData = (ActionData)pathValue;
                    genericErrorDetails.Add(ErrorDataKey.ActionPath, this.CurrentActionDataPath);
                    genericErrorDetails.Add(ErrorDataKey.ActionType, actionData.Type.Humanize());
                    genericErrorDetails.Add(ErrorDataKey.ActionAlias, actionData.Alias);
                }
                else if (pathValue is Dictionary<string, ActionData>)
                {
                    var actionDatas = (Dictionary<string, ActionData>)pathValue;
                    if (actionDatas.Count == 1)
                    {
                        var actionData = actionDatas.First().Value;
                        genericErrorDetails.Add(ErrorDataKey.ActionPath, this.CurrentActionDataPath);
                        genericErrorDetails.Add(ErrorDataKey.ActionType, actionData.Type.Humanize());
                        genericErrorDetails.Add(ErrorDataKey.ActionAlias, actionData.Alias);
                    }
                    else
                    {
                        foreach (var pair in actionDatas.Select((item, index) => new { index, item }))
                        {
                            var actionData = pair.item.Value;
                            genericErrorDetails.Add(ErrorDataKey.ActionPath + $"[{pair.index}]", this.CurrentActionDataPath);
                            genericErrorDetails.Add(ErrorDataKey.ActionType + $"[{pair.index}]", actionData.Type.Humanize());
                            genericErrorDetails.Add(ErrorDataKey.ActionAlias + $"[{pair.index}]", actionData.Alias);
                        }
                    }
                }
            }
            else if (this.AutomationData.Trigger != null)
            {
                genericErrorDetails.Add(ErrorDataKey.TriggerAlias, this.AutomationData.Trigger.TriggerAlias);
                genericErrorDetails.Add(ErrorDataKey.TriggerType, this.AutomationData.Trigger.Type.Humanize());
            }

            return JObject.FromObject(genericErrorDetails);
        }
    }
}
