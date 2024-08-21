// <copyright file="PathLookupListObjectProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Providers.Object
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text.RegularExpressions;
    using MorseCode.ITask;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Object;
    using UBind.Application.Automation.PathLookup;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Resolves and creates an object out of an array of object-path values containing the property name
    /// and the path to be used to resolve the value. An optional default value is used for each property if the value
    /// cannot be found.
    /// </summary>
    /// <remarks>Schema key: objectPathLookupListObject.</remarks>
    public class PathLookupListObjectProvider : IObjectProvider
    {
        /// <summary>
        /// Regex pattern used for validating JSON property keys.
        /// </summary>
        /// <remarks>Key should start with lowercase letter, does not contain any whitespace
        /// and special characters.</remarks>
        private const string PropertyKeyPattern = "^[a-z]+([a-z]|[A-Z]|[0-9])+[a-zA-Z0-9]*$";

        private readonly IEnumerable<ObjectPathProperty> properties;
        private readonly IObjectProvider dataObjectProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PathLookupListObjectProvider"/> class.
        /// </summary>
        /// <param name="properties">The list of properties whose value needs to be resolved.</param>
        /// <param name="dataObjectProvider">An optional data object to read from. If omitted, the current automation data is used.</param>
        public PathLookupListObjectProvider(
            IEnumerable<ObjectPathProperty> properties,
            IObjectProvider dataObjectProvider)
        {
            this.properties = properties;
            this.dataObjectProvider = dataObjectProvider;
        }

        public string SchemaReferenceKey => "objectPathLookupListObject";

        /// <inheritdoc/>
        public async ITask<IProviderResult<Data<object>>> Resolve(IProviderContext providerContext)
        {
            using (MiniProfiler.Current.Step(nameof(PathLookupListObjectProvider) + "." + nameof(this.Resolve)))
            {
                Dictionary<string, object> outputDictionary = new Dictionary<string, object>();
                var customDataObject = (await this.dataObjectProvider.ResolveValueIfNotNull(providerContext))?.DataValue;
                foreach (var propertyPath in this.properties)
                {
                    var path = (await propertyPath.PathProvider.Resolve(providerContext)).GetValueOrThrowIfFailed();
                    IData? value = default;
                    var errorData = await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey);
                    if (!PathHelper.IsValidJsonPointer(path.DataValue))
                    {
                        errorData.Add("path", path.DataValue);
                        throw new ErrorException(
                            Errors.Automation.PathSyntaxError(
                                "Path uses invalid pointer syntax", this.SchemaReferenceKey, errorData));
                    }

                    var propertyName = (await propertyPath.PropertyNameProvider.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
                    if (!this.IsPropertyNameValid(propertyName))
                    {
                        errorData.Add("invalidPropertyName", propertyName);
                        throw new ErrorException(
                            Errors.Automation.Provider.PropertyKeyInvalid(
                                this.SchemaReferenceKey, propertyName, errorData));
                    }

                    if (customDataObject == null)
                    {
                        // Since no custom data object has been specified, we'll use the entire
                        // automation data, however we should ensure that context entities have been loaded:
                        await providerContext.AutomationData.ContextManager.LoadEntityAtPath(providerContext, path);
                    }

                    ;
                    var result = PocoPathLookupResolver.Resolve(
                        customDataObject ?? providerContext.AutomationData,
                        path,
                        this.SchemaReferenceKey,
                        await providerContext.GetDebugContextForProviders(this.SchemaReferenceKey));

                    if (result.IsFailure)
                    {
                        value = await PathLookupResolverHelper.ResolveValueOrThrowIfNotFound(
                           propertyPath.RaiseErrorIfNotFoundProvider,
                           propertyPath.ValueIfNotFoundProvider,
                           propertyPath.DefaultValueProvider,
                           providerContext,
                           this.SchemaReferenceKey,
                           result);
                    }
                    else
                    {
                        value = result.Value;
                    }

                    if (value == null)
                    {
                        value = await PathLookupResolverHelper.ResolveValueOrThrowIfNull(
                               propertyPath.RaiseErrorIfNullProvider,
                               propertyPath.ValueIfNullProvider,
                               propertyPath.DefaultValueProvider,
                               providerContext,
                               this.SchemaReferenceKey,
                               value);
                    }

                    // It should be removed once the migration of quotes to new release is done.
                    // will be remove on UB-12013
                    if ((value == null || string.IsNullOrEmpty(propertyName)) &&
                        providerContext.AutomationData.System.Environment == DeploymentEnvironment.Production)
                    {
                        continue;
                    }

                    if (value == null && string.IsNullOrEmpty(propertyName))
                    {
                        continue;
                    }

                    outputDictionary.Add(propertyName, value?.GetValueFromGeneric());
                }

                return ProviderResult<Data<object>>.Success(new Data<object>(
                    new ReadOnlyDictionary<string, object>(outputDictionary)));
            }
        }

        /// <summary>
        /// Verifies if the key used for the property is valid.
        /// </summary>
        /// <param name="propertyName">The key to be validated.</param>
        /// <returns>A flag specifying if the string is valid.</returns>
        private bool IsPropertyNameValid(string propertyName)
        {
            var match = Regex.Match(propertyName, PropertyKeyPattern, RegexOptions.None);
            return match.Success;
        }
    }
}
