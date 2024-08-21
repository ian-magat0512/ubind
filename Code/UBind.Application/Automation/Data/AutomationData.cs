// <copyright file="AutomationData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using MoreLinq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Actions;
    using UBind.Application.Automation.Context;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Triggers;
    using UBind.Application.JsonConverters;
    using UBind.Application.Services.Imports;
    using UBind.Domain;
    using UBind.Domain.Events;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Json;
    using UBind.Domain.Product;

    /// <summary>
    /// Represents the data context of the automation.
    /// </summary>
    public class AutomationData
    {
        public const string ProductId = "productId";
        private readonly object variablesLock = new object();

        public AutomationData(
            Guid tenantId,
            Guid? organisationId,
            Guid? productId,
            Guid? productReleaseId,
            DeploymentEnvironment environment,
            TriggerData? triggerData,
            IServiceProvider serviceProvider)
        {
            this.Trigger = triggerData;
            this.ContextManager =
                new AutomationDataContextManager(tenantId, organisationId, productId, productReleaseId, this.Context);
            this.ServiceProvider =
                serviceProvider ?? throw new ErrorException(Errors.Automation.ServiceProviderNotFound(GenericErrorDataHelper.GetGeneralErrorDetails(tenantId, productId, environment)));

            var clock = serviceProvider.GetRequiredService<IClock>();

            var internalUrlConfiguration = serviceProvider.GetRequiredService<IInternalUrlConfiguration>();
            this.System = new AutomationSystemData(
                environment, internalUrlConfiguration.BaseApi, AutomationSystemData.DefaultTimeZone, clock);

            this.Automation = new Dictionary<string, object>
            {
                { "startedTicksSinceEpoch", clock.GetCurrentInstant().ToUnixTimeTicks() },
                { "startedDateTime", clock.GetCurrentInstant().InZone(AutomationSystemData.DefaultTimeZone).LocalDateTime.ToString() },
            };
            if (productId.HasValue)
            {
                this.Automation[ProductId] = productId.Value;
            }
        }

        [JsonConstructor]
        private AutomationData()
        {
            this.ContextManager = new AutomationDataContextManager(this.Context);
        }

        [JsonIgnore]
        public AutomationDataContextManager ContextManager { get; private set; }

        /// <summary>
        /// Gets the trigger context used by the automation.
        /// </summary>
        /// <remarks>
        /// Containing the data supplied through the trigger of the automation,
        /// as well as the alias and resolved properties of that trigger after resolution.
        /// </remarks>
        [JsonProperty("trigger")]
        public TriggerData? Trigger { get; private set; }

        /// <summary>
        /// Gets the action context used by the automation.
        /// </summary>
        /// <remarks>
        /// Contains the contextual data relevant to the current automation, and should always have fully-resolved data.
        /// Where actions rely on one of these objects and a reference to one is not provided explicitly,
        /// the most approprate object will be chosen from the context automatically.
        /// </remarks>
        [JsonProperty("actions")]
        public Dictionary<string, ActionData> Actions { get; private set; } = new Dictionary<string, ActionData>();

        /// <summary>
        /// Gets the context data of the current automation.
        /// </summary>
        /// <remarks>Contains the relevant references and related properties that the automation can use.</remarks>
        [JsonProperty("context")]
        public Dictionary<string, object> Context { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets the properties of the automation being executed.
        /// </summary>
        [JsonProperty("automation")]
        public Dictionary<string, object> Automation { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets the properties inherent to the sytem executing the automation.
        /// </summary>
        [JsonProperty("system")]
        public AutomationSystemData @System { get; private set; } = null!;

        /// <summary>
        /// Gets the an object containing properties settable by an action, with dynamic values.
        /// </summary>
        [JsonProperty("variables")]
        public JObject Variables { get; private set; } = new JObject();

        /// <summary>
        /// Gets the unhandled raised error found during the course of the automation execution.
        /// </summary>
        [JsonProperty("error")]
        [JsonConverter(typeof(ErrorConverter))]
        public Domain.Error? Error { get; private set; }

        /// <summary>
        /// Gets the service provider used for lazy loading context.
        /// </summary>
        [JsonIgnore]
        public IServiceProvider? ServiceProvider { get; private set; }

        public static AutomationData CreateFromHttpRequest(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            Guid productReleaseId,
            DeploymentEnvironment environment,
            TriggerRequest triggerRequest,
            IServiceProvider serviceProvider)
        {
            var triggerData = new HttpTriggerData(triggerRequest);
            return new AutomationData(tenantId, organisationId, productId, productReleaseId, environment, triggerData, serviceProvider);
        }

        public static AutomationData CreateFromSystemEvent(
            SystemEvent @event,
            Guid? productReleaseId,
            IServiceProvider serviceProvider)
        {
            var triggerData = new EventTriggerData(@event);
            return new AutomationData(
                @event.TenantId,
                @event.OrganisationId,
                @event.ProductId,
                productReleaseId,
                @event.Environment,
                triggerData,
                serviceProvider);
        }

        public static AutomationData CreateFromPeriodicTrigger(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            Guid productReleaseId,
            DeploymentEnvironment environment,
            IServiceProvider serviceProvider)
        {
            var triggerData = new PeriodicTriggerData();
            return new AutomationData(tenantId, organisationId, productId, productReleaseId, environment, triggerData, serviceProvider);
        }

        public static AutomationData CreateFromPortalPageTriggerRequest(
            Guid tenantId,
            Guid organisationId,
            Guid? productId,
            Guid? productReleaseId,
            DeploymentEnvironment environment,
            string triggerAlias,
            IServiceProvider serviceProvider,
            EntityType entityType,
            PageType pageType,
            string tab)
        {
            var triggerData = new PortalPageTriggerData(triggerAlias, entityType, pageType, tab);
            return new AutomationData(tenantId, organisationId, productId, productReleaseId, environment, triggerData, serviceProvider);
        }

        /// <summary>
        /// Create automation data for context entities. We don't require trigger data for this.
        /// This will be used by the webFormApp to be able to access contextual information
        /// about the quote or claim that is being created or edited.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="organisationId">The organisation Id.</param>
        /// <param name="productId">The product Id.</param>
        /// <param name="environment">The deployment environment.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>Automation data for context entities.</returns>
        public static AutomationData CreateForContextEntities(
            ReleaseContext releaseContext,
            Guid organisationId,
            IServiceProvider serviceProvider)
        {
            using (MiniProfiler.Current.Step(nameof(AutomationData) + "." + nameof(CreateForContextEntities)))
            {
                var automationData = new AutomationData(
                    releaseContext.TenantId,
                    organisationId,
                    releaseContext.ProductId,
                    releaseContext.ProductReleaseId,
                    releaseContext.Environment,
                    null,
                    serviceProvider);
                return automationData;
            }
        }

        /// <summary>
        /// Returns the value at the given path, otherwise null.
        /// </summary>
        /// <param name="path">The path to the parameter, in JSON Pointer format, e.g.
        /// "/trigger/eventType".</param>
        /// <returns>The value at the given path.</returns>
        public async Task<object?> GetValue(string path, IProviderContext? providerContext = null, IEnumerable<string>? similarPaths = null)
        {
            if (!PathHelper.IsJsonPointer(path))
            {
                path = PathHelper.ToJsonPointer(path);
            }

            // Load Context Entities if needed.
            // TODO: instead of loading them every time, cache them and invalidate the cache when needed.
            if (!string.IsNullOrEmpty(path) && path.StartsWith("/context/"))
            {
                await this.ContextManager.LoadEntityAtPath(providerContext ?? new ProviderContext(this), path, similarPaths);
            }

            var debugContext = providerContext != null ? await providerContext.GetDebugContext() : null;
            var dataPathPointer = new PocoJsonPointer(path, "AutomationData.GetValue", debugContext);
            if (dataPathPointer.IsRelative)
            {
                throw new ArgumentException("Automation.GetData was called with a relative JSON Pointer, which "
                    + "is not supported. You must use a provider within an action to use a relative JSON Pointer, "
                    + "because relative paths are relative to the path of the running action.");
            }
            else
            {
                var pathlookupResult = dataPathPointer.Evaluate(this);
                if (pathlookupResult.IsFailure)
                {
                    if (pathlookupResult.Error.Code == Errors.Automation.PathNotFound(null!, null!, null!, null!, null!).Code)
                    {
                        return null;
                    }

                    return pathlookupResult.GetValueOrThrowIfFailed();
                }
                else
                {
                    return pathlookupResult.Value;
                }
            }
        }

        /// <summary>
        /// Returns the data the key represents in the automation context, otherwise null.
        /// </summary>
        /// <typeparam name="TValue">The type of data being fetched.</typeparam>
        /// <param name="path">The map to the parameter.</param>
        /// <returns>The value the key represents.</returns>
        public async Task<TValue?> GetValue<TValue>(string path, IProviderContext? providerContext, IEnumerable<string>? similarPaths = null)
        {
            object? value = await this.GetValue(path, providerContext, similarPaths);
            if (value == null)
            {
                return (TValue?)value;
            }

            if (value is TValue alreadyTypedValue)
            {
                return alreadyTypedValue;
            }

            if (typeof(TValue) == typeof(string))
            {
                // Every object in C# has a ToString() method, so we know we can always call it.
                // If a specific ToString was not implemented, it will return the type name.
                return (TValue)((dynamic)value).ToString();
            }

            return (TValue)value;
        }

        public void RecordAutomationAlias(string automationAlias)
        {
            this.Automation.Add("automation", automationAlias);
        }

        public void SetAutomationProperty(string key, object value)
        {
            this.Automation[key] = value;
        }

        /// <summary>
        /// Updates this data variables with the parameters set on a specific path of the object.
        /// </summary>
        /// <param name="value">The value of the variable to add.</param>
        /// <param name="jsonPointer">The json pointer of the variable to add.</param>
        /// <param name="propertyName">The property name of the variable to add.</param>
        public void AddOrUpdateVariableByPath(object? value, string? jsonPointer = null, string? propertyName = null)
        {
            using (MiniProfiler.Current.Step($"{nameof(AutomationData)}.{nameof(this.AddOrUpdateVariableByPath)} {jsonPointer} ==> {propertyName}"))
            {
                var jsonPath = PathHelper.IsJsonPointer(jsonPointer)
                    ? PathHelper.ToJsonPath(jsonPointer!)
                    : jsonPointer ?? string.Empty;
                var valueToken = value != null ? JToken.FromObject(value) : JValue.CreateNull();
                lock (this.variablesLock)
                {
                    this.Variables = this.Variables ?? new JObject();
                    var pathValueToken = this.Variables.SelectToken(jsonPath);

                    // root of variables.
                    if (pathValueToken != null && string.IsNullOrEmpty(jsonPath) && !string.IsNullOrEmpty(propertyName))
                    {
                        pathValueToken[propertyName] = valueToken;
                        return;
                    }

                    // if path is null, create the path.
                    if (pathValueToken == null || pathValueToken.Type == JTokenType.Null)
                    {
                        this.Variables.PatchProperty(
                            new JsonPath(jsonPath + (!string.IsNullOrEmpty(propertyName) ? "." + propertyName : string.Empty)),
                            valueToken);
                        return;
                    }

                    // replace the value of the propertyName.
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        var propertyToken = pathValueToken.SelectToken(propertyName);

                        if (propertyToken == null)
                        {
                            // create the property if not found IF path is an object.
                            if (pathValueToken.Type == JTokenType.Object)
                            {
                                ((JObject)pathValueToken).PatchProperty(new JsonPath(propertyName), valueToken);
                                return;
                            }

                            throw new ArgumentException($"Could not set the value of property '{propertyName}' at the specified path '{jsonPointer}' within the variable context because the identified location is a primitive type. To set the value of a property, make sure that the parent property is an object.");
                        }

                        propertyToken.Replace(valueToken);
                        return;
                    }

                    // merge the two variables if both are objects.
                    if (pathValueToken.Type == JTokenType.Object && valueToken.Type == JTokenType.Object)
                    {
                        ((JObject)pathValueToken).Merge(valueToken);
                    }
                }
            }
        }

        /// <summary>
        /// Updates this databag with the alias of the automation that matched the details of the request.
        /// </summary>
        /// <param name="matchingTriggerAlias">The alias of the trigger that matched within the configured automation.</param>
        public void UpdateTriggerData(string matchingTriggerAlias)
        {
            if (this.Trigger == null)
            {
                throw new InvalidOperationException("An attempt was made to update the trigger data of an automation "
                    + $"with the matching trigger alias \"${matchingTriggerAlias}\", however the automation data does"
                    + "not have a trigger set yet.");
            }

            this.Trigger.TriggerAlias = matchingTriggerAlias;
        }

        /// <summary>
        /// Adds a new action data to this automation data.
        /// </summary>
        /// <param name="actionData">The action data to be added.</param>
        public void AddActionData(ActionData actionData)
        {
            if (!this.Actions.ContainsKey(actionData.Alias))
            {
                this.Actions.Add(actionData.Alias, actionData);
            }
        }

        /// <summary>
        /// Updates the automation data with the details of the unhandled error
        /// during the execution of the actions within this automation.
        /// </summary>
        /// <param name="error">The error to be appended.</param>
        public void SetError(Domain.Error? error)
        {
            this.Error = error;
        }

        /// <summary>
        /// Injects the dependencies required by the automation data and its properties.
        /// </summary>
        /// <param name="serviceProvider">The dependency service provider.</param>
        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
            if (this.System == null)
            {
                throw new InvalidOperationException("An attempt was made to update the system data of an automation "
                    + $"with the service provider, however the automation data does not have a system object set yet.");
            }
            this.System.SetProvider(serviceProvider);
            this.Actions.ForEach(data => data.Value.SetProviders(serviceProvider));
        }

        public ProductContext GetProductContextFromContext()
        {
            if (this.System == null)
            {
                throw new InvalidOperationException("An attempt was made to access the system data of an automation "
                    + $"with the service provider, however the automation data does not have a system object set yet.");
            }
            ICachingResolver? cachingResolver = this.ServiceProvider?.GetService<ICachingResolver>();
            if (cachingResolver == null)
            {
                var errorData = GenericErrorDataHelper.GetGeneralErrorDetails(
                this.ContextManager.Tenant.Id,
                this.ContextManager.Product.Id,
                this.System.Environment);
                throw new ErrorException(Errors.Automation.ServiceProviderNotFound(errorData));
            }

            return new ProductContext(
                this.ContextManager.Tenant.Id,
                this.ContextManager.Product.Id,
                this.System.Environment);
        }
    }
}
