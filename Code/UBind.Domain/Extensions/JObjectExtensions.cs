// <copyright file="JObjectExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using CSharpFunctionalExtensions;
    using Microsoft.Json.Pointer;
    using MoreLinq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Dto;
    using UBind.Domain.Helpers;
    using UBind.Domain.Json;
    using UBind.Domain.JsonConverters;

    /// <summary>
    /// Extension methods for JObject.
    /// </summary>
    public static class JObjectExtensions
    {
        /// <summary>
        /// Replaces property names of an object.
        /// </summary>
        /// <param name="obj">The object we are trying to modify.</param>
        /// <param name="oldToNewPropertyNameMap">The property map where the key is the property name now
        /// and the value is where the property name will be.
        /// </param>
        public static JObject RenamePropertyNamesUsingMap(
            this JObject obj,
            Dictionary<string, string> oldToNewPropertyNameMap)
        {
            JObject newObj = new JObject();
            foreach (JProperty property in obj.Properties())
            {
                string propertyName = property.Name;
                JToken value = property.Value;
                string newPropertyName;
                if (oldToNewPropertyNameMap.TryGetValue(propertyName, out newPropertyName))
                {
                    newObj.Add(newPropertyName, value);
                }
                else
                {
                    newObj.Add(property);
                }
            }

            return newObj;
        }

        /// <summary>
        /// Find the value or a property matching a given pattern.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="jObject">The instancen of <see cref="JObject"/> search.</param>
        /// <param name="keyPattern">A regular expression pattern to match the property key.</param>
        /// <returns>The value of the first matching property with non-default value.</returns>
        public static Result<TValue> FindValue<TValue>(this JObject jObject, string keyPattern)
        {
            return jObject.FindValue<TValue, TValue>(keyPattern, v => Result.Success(v));
        }

        /// <summary>
        /// Find the value or a property matching a given pattern.
        /// </summary>
        /// <typeparam name="TValue">The type of the value in the json.</typeparam>
        /// <typeparam name="TOutput">The desired output type.</typeparam>
        /// <param name="jObject">The instancen of <see cref="JObject"/> search.</param>
        /// <param name="keyPattern">A regular expression pattern to match the property key.</param>
        /// <param name="transformer">A function for transforming matching values into the desired output type.</param>
        /// <returns>The value of the first matching property with non-default transformed value.</returns>
        public static Result<TOutput> FindValue<TValue, TOutput>(
            this JObject jObject,
            string keyPattern,
            Func<TValue, Result<TOutput>> transformer)
        {
            return jObject
                .Descendants()
                .OfType<JProperty>()
                .Where(p => p.Value is JValue)
                .Where(p => Regex.Match(p.Name, keyPattern).Success)
                .Select(p => p.Value.Value<TValue>())
                .Select(value => transformer(value))
                .Where(result => result.IsSuccess)
                .FallbackIfEmpty(Result.Failure<TOutput>("Could not find matching value."))
                .First();
        }

        /// <summary>
        /// Flatten a JObject into a list of path value pairs.
        /// </summary>
        /// <param name="jToken">The object to flatten.</param>
        /// <returns>A list of path value pairs from the object.</returns>
        public static IList<KeyValuePair<string, string>> Flatten(this JObject jToken)
        {
            return jToken
                .Descendants()
                .Where(p => !p.Any())
                .Aggregate(new List<KeyValuePair<string, string>>(), (properties, leaf) =>
                {
                    properties.Add(new KeyValuePair<string, string>(leaf.Path, leaf.ToString()));
                    return properties;
                });
        }

        /// <summary>
        /// Flatten a JObject into a list of path value pairs.
        /// </summary>
        /// <param name="jToken">The object to flatten.</param>
        /// <returns>A list of path value pairs from the object.</returns>
        public static Dictionary<string, string> FlattenToDictionary(this JObject jToken)
        {
            return jToken
                .Descendants()
                .Where(p => !p.Any())
                .Aggregate(new Dictionary<string, string>(), (properties, leaf) =>
                {
                    properties.Add(leaf.Path, leaf.ToString());
                    return properties;
                });
        }

        /// <summary>
        /// Capitalizes the property names of the jToken.
        /// </summary>
        /// <param name="jToken">The jToken.</param>
        /// <returns>The Jtoken.</returns>
        public static JToken CapitalizePropertyNames(this JToken jToken)
        {
            return JsonHelper.CapitalizePropertyNames(jToken);
        }

        /// <summary>
        /// Make JToken into an IDictionary of string and object.
        /// </summary>
        /// <param name="jToken">The jToken.</param>
        /// <returns>An IDictionary.</returns>
        public static IDictionary<string, object> ToDictionary(this JToken jToken)
        {
            return JsonConvert.DeserializeObject<IDictionary<string, object>>(jToken.ToString(), new DictionaryConverter());
        }

        /// <summary>
        /// Convert json to URL parameter format.
        /// </summary>
        /// <param name="jObject">The json data to convert.</param>
        /// <param name="displayableFieldsDto">Field keys to exclude.</param>
        /// <returns>A string containing the URL parameter formatted data.</returns>
        public static JObject FilterDisplayableFieldKeys(this JObject jObject, DisplayableFieldDto displayableFieldsDto)
        {
            var displayableFields = displayableFieldsDto.DisplayableFields;
            var repeatingDisplayableFields = displayableFieldsDto.RepeatingDisplayableFields;

            JObject result = new JObject();
            foreach (var item in jObject)
            {
                if (displayableFields.Contains(item.Key) && (item.Value.Type == JTokenType.String || item.Value.Type == JTokenType.Boolean))
                {
                    result.Add(item.Key, item.Value);
                }

                if (displayableFields.Contains(item.Key) && item.Value.Type == JTokenType.Object)
                {
                    var objValue = item.Value.ToObject<JObject>();
                    JObject fitleredObjValue = new JObject();
                    foreach (var objValueItem in objValue)
                    {
                        if (displayableFields.Contains(objValueItem.Key))
                        {
                            fitleredObjValue.Add(objValueItem.Key, objValueItem.Value);
                        }
                    }

                    result.Add(item.Key, fitleredObjValue);
                }

                if (repeatingDisplayableFields.Contains(item.Key) && item.Value.Type == JTokenType.Array)
                {
                    var objValueArray = item.Value.ToObject<JObject[]>();
                    JArray filteredObjValueArray = new JArray();

                    foreach (var objValueArrayItem in objValueArray)
                    {
                        if (objValueArrayItem != null)
                        {
                            JObject fitleredObjValue = new JObject();
                            foreach (var objValueItem in objValueArrayItem)
                            {
                                if (repeatingDisplayableFields.Contains(objValueItem.Key))
                                {
                                    fitleredObjValue.Add(objValueItem.Key, objValueItem.Value);
                                }
                            }

                            filteredObjValueArray.Add(fitleredObjValue);
                        }
                    }

                    result.Add(item.Key, filteredObjValueArray);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the JToken with the specified json path or pointer .
        /// </summary>
        /// <param name="jObject">The json data object.</param>
        /// <param name="path">The json path or pointer.</param>
        /// <returns>The json token.</returns>
        public static JToken GetToken(this JObject jObject, string path)
        {
            JToken token = null;
            try
            {
                if (PathHelper.IsJsonPointer(path))
                {
                    var jPointer = new JsonPointer(path);
                    token = jPointer.Evaluate(jObject);  // This throws an exception when JSON pointer is not found
                }
                else
                {
                    token = jObject.GetValue(path, StringComparison.OrdinalIgnoreCase) ?? jObject.SelectToken(path);
                }
            }
            catch (Exception)
            {
                // JSON pointer throws an exception when not found. This will allow our method to return null.
            }

            return token;
        }

        /// <summary>
        /// Patch a property with a new value.
        /// </summary>
        /// <param name="jObject">The json data to patch.</param>
        /// <param name="path">A path specifying the property to patch.</param>
        /// <param name="newToken">The new value.</param>
        /// <exception cref="InvalidOperationException">Thrown when a property to patch cannot be identified using the path.</exception>
        public static void PatchProperty(this JObject jObject, JsonPath path, JToken newToken)
        {
            if (jObject == null)
            {
                return;
            }

            var tokens = jObject.SelectTokens(path.Value);
            if (tokens.Count() > 1)
            {
                throw new InvalidOperationException($"JSON path must identify unique property to patch. Path: {path.Value}. Json: {jObject}");
            }

            var propertyToken = tokens.SingleOrDefault();
            if (propertyToken != null)
            {
                propertyToken.Replace(newToken);
                return;
            }

            var parentProperty = CreateAncestors(jObject, path);
            parentProperty.Add(path.FinalChild, newToken);
        }

        /// <summary>
        /// Determine whether a given property can be patched.
        /// </summary>
        /// <param name="jObject">The json containing the property.</param>
        /// <param name="path">A path specifying the property.</param>
        /// <param name="rules">Additional rules governing to use for determining whether the property can be patched.</param>
        /// <returns>A result indicating success if the property can be patched, or the error if it cannot.</returns>
        public static Result CanPatchProperty(this JObject jObject, JsonPath path, PatchRules rules)
        {
            var tokens = jObject.SelectTokens(path.Value);
            if (tokens.Count() > 1)
            {
                return Result.Failure("Multiple matching properties exist.");
            }

            var token = tokens.SingleOrDefault();
            if (token != null)
            {
                if (rules.HasFlag(PatchRules.PropertyIsMissingOrNullOrEmpty))
                {
                    return token.IsNullOrEmpty()
                        ? Result.Success()
                        : Result.Failure("Property is not empty, but rules require it to be missing, null, or empty.");
                }

                if (rules.HasFlag(PatchRules.PropertyDoesNotExist))
                {
                    return Result.Failure("Property exists, but rules require that it does not exist.");
                }

                return Result.Success();
            }

            if (rules.HasFlag(PatchRules.PropertyExists))
            {
                return Result.Failure("Property does not exist, but rules require it to exist.");
            }

            var canFindParentResult = CanFindParentObject(jObject, path);
            if (canFindParentResult.IsSuccess)
            {
                return Result.Success();
            }

            if (rules.HasFlag(PatchRules.ParentMustExist))
            {
                return canFindParentResult;
            }

            return CanFindAncestorObject(jObject, path);
        }

        private static Result CanFindParentObject(JObject jObject, JsonPath path)
        {
            if (!path.CanIdentifyAncestors)
            {
                return Result.Failure($"Cannot identify parent objects to create when path is not simple dot separated list of property names. Path: {path.Value}. Json {jObject}");
            }

            var parentTokens = jObject.SelectTokens(path.ParentPath);
            if (parentTokens.Count() > 1)
            {
                return Result.Failure($"Cannot identify unique parent object. Path: {path.Value}. Json {jObject}");
            }

            var uniqueParentToken = parentTokens.SingleOrDefault() as JObject;
            return uniqueParentToken != null
                ? Result.Success()
                : Result.Failure("Cound not identify unique parent object. Path: {path.Value}. Json {jObject}");
        }

        private static Result CanFindAncestorObject(JObject jObject, JsonPath path)
        {
            if (!path.CanIdentifyAncestors)
            {
                return Result.Failure($"Cannot identify parent objects to create when path is not simple dot separated list of property names. Path: {path.Value}. Json {jObject}");
            }

            var currentObject = jObject;
            foreach (var ancestor in path.Ancestors)
            {
                var nextProperties = currentObject.SelectTokens(ancestor);
                if (!nextProperties.Any())
                {
                    return Result.Success();
                }

                if (nextProperties.Count() > 1)
                {
                    return Result.Failure($"Cannot identify parent objects to create when multiple ancestors with same name ({ancestor}) exist. Path: {path.Value}. Json {jObject}");
                }

                var nextObject = nextProperties.Single() as JObject;
                if (nextObject == null)
                {
                    return Result.Failure($"Cannot identify parent objects to create when existing ancestor ({ancestor}) is not an object. Path: {path.Value}. Json {jObject}");
                }

                currentObject = nextObject;
            }

            return Result.Success();
        }

        /// <summary>
        /// Create all necessary ancestor objects in path.
        /// </summary>
        /// <param name="jObject">The JSON object to create the objects in.</param>
        /// <param name="path">A path specifying which objects to create.</param>
        /// <returns>The parent object of the property specified in the path.</returns>
        private static JObject CreateAncestors(JObject jObject, JsonPath path)
        {
            if (!path.CanIdentifyAncestors)
            {
                throw new InvalidOperationException($"Cannot create ancestor for property when path is not simple dot separated format. Path: {path.Value}. Json {jObject}");
            }

            var currentObject = jObject;
            foreach (var ancestor in path.Ancestors)
            {
                var existingProperties = currentObject.SelectTokens(ancestor);
                if (!existingProperties.Any())
                {
                    var newObject = new JObject();
                    currentObject.Add(ancestor, newObject);
                    currentObject = newObject;
                    continue;
                }

                if (existingProperties.Count() == 1)
                {
                    var existingProperty = existingProperties.Single() as JObject;
                    if (existingProperty == null)
                    {
                        throw new InvalidOperationException("Cannot add property when ancestor is not an object. Path: {path.Value}. Json {jObject}");
                    }

                    currentObject = existingProperty;
                    continue;
                }

                throw new InvalidOperationException("Cannot insert property when ancestor cannot be uniquely identified. Path: {path.Value}. Json {jObject}");
            }

            return currentObject;
        }
    }
}
