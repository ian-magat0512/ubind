// <copyright file="DataObjectHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Helper
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Http;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    public class DataObjectHelper
    {
        private static BindingFlags propertyBindingFlags
            = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance;

        /// <summary>
        /// Attempts to get the value of a property on an object.
        /// Ignores properties with the <see cref="JsonIgnoreAttribute"/>.
        /// </summary>
        /// <param name="dataObject">The object to try.</param>
        /// <param name="propertyName">The property name.</param>
        /// <param name="value">The output value of the property.</param>
        /// <returns>True if the property was found, otherwise false.</returns>
        public static bool TryGetPropertyValue(object dataObject, string propertyName, out object value)
        {
            if (dataObject is IData iDataObject)
            {
                dataObject = iDataObject.GetValueFromGeneric();
            }

            value = null;
            if (dataObject is IDictionary)
            {
                var dataDictionary = dataObject as IDictionary;
                if (!dataDictionary.Contains(propertyName))
                {
                    return false;
                }

                value = dataDictionary[propertyName];
                return true;
            }

            if (dataObject is JObject)
            {
                var dataJObject = dataObject as JObject;
                if (!dataJObject.ContainsKey(propertyName))
                {
                    return false;
                }

                value = dataJObject[propertyName];
                return true;
            }

            // normal non-nested properties.
            PropertyInfo propertyInfo = dataObject.GetType().GetProperty(propertyName, propertyBindingFlags);

            if (propertyInfo == null

                // if it has a JsonIgnore then we'll pretend the property doesn't exist
                || propertyInfo.GetCustomAttribute<JsonIgnoreAttribute>() != null)
            {
                return false;
            }

            var jsonConverterType = propertyInfo.GetCustomAttribute<JsonConverterAttribute>();
            var tmpValue = propertyInfo.GetValue(dataObject);

            // excluded some types
            bool canJsonConvert =
                jsonConverterType != null
                && !(tmpValue is byte[])
                && !(tmpValue is List<ContentPart>)
                && !(tmpValue is string)
                && !(tmpValue is JValue)
                && tmpValue != null;

            // if cannot be json converted, return value.
            if (!canJsonConvert)
            {
                value = tmpValue;
                return true;
            }

            // process properties that can be json converted.
            if (tmpValue is JObject)
            {
                var instance = (JsonConverter)Activator.CreateInstance(jsonConverterType.ConverterType, jsonConverterType.ConverterParameters);
                var setting = new JsonSerializerSettings { Converters = new List<JsonConverter>() { instance } };
                value = JObject.FromObject((object)tmpValue, JsonSerializer.Create(setting)) ?? tmpValue;
            }
            else
            {
                var jObject = JObject.FromObject((object)dataObject);
                value = jObject[propertyName.ToCamelCase()] ?? tmpValue;
            }

            return true;
        }

        /// <summary>
        /// Get the value of a property on an object, returning null if the property doesn't exist.
        /// Fails on properties with the <see cref="JsonIgnoreAttribute"/>.
        /// </summary>
        /// <param name="dataObject">The object to try.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The property value, or null if it doesn't exist.</returns>
        public static object GetPropertyValue(object dataObject, string propertyName)
        {
            DataObjectHelper.TryGetPropertyValue(dataObject, propertyName, out object value);
            return value;
        }

        /// <summary>
        /// Get the value of a proepry on an object, throwing an exception if the property doesn't exist.
        /// Throws on properties with the <see cref="JsonIgnoreAttribute"/>.
        /// </summary>
        /// <param name="dataObject">The object to try.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The property value, or null if it doesn't exist.</returns>
        /// <remarks>Throws <see cref="ArgumentException"/> if the property doesn't exist.</remarks>
        public static object GetPropertyValueOrThrow(object dataObject, string propertyName)
        {
            if (!DataObjectHelper.TryGetPropertyValue(dataObject, propertyName, out object value))
            {
                throw new ArgumentException($"The property \"{propertyName}\" doesn't exist.");
            }

            return value;
        }

        public static bool ContainsProperty(object dataObject, string propertyName)
        {
            if (dataObject is IData iDataObject)
            {
                dataObject = iDataObject.GetValueFromGeneric();
            }

            if (dataObject is IDictionary)
            {
                var dataDictionary = dataObject as IDictionary;
                return dataDictionary.Contains(propertyName);
            }

            PropertyInfo propertyInfo = dataObject.GetType().GetProperty(propertyName, propertyBindingFlags);
            return propertyInfo != null;
        }

        public static object CountProperties(object dataObject)
        {
            if (dataObject is IData iDataObject)
            {
                dataObject = iDataObject.GetValueFromGeneric();
            }

            if (dataObject is IDictionary)
            {
                return (dataObject as IDictionary).Keys.Count;
            }

            return dataObject.GetType().GetProperties(propertyBindingFlags).Length;
        }

        /// <summary>
        /// Checks if passed value is actually an object and not a primitive.
        /// If it's not a proper object, it throws an exception.
        /// </summary>
        /// <param name="dataObject">The object.</param>
        public static void ThrowIfNotObjectOrArray(object dataObject, string providerName, JObject debugContext)
        {
            if (dataObject is IData iDataObject)
            {
                dataObject = iDataObject.GetValueFromGeneric();
            }

            if (dataObject.GetType().IsPrimitive || !IsStructuredObjectOrArray(dataObject))
            {
                debugContext.Add(ErrorDataKey.ErrorMessage, $"Was expecting an object or array but received the primitive type \"{dataObject.GetType()}\".");
                throw new ErrorException(Errors.Automation.InvalidValueTypeObtained(
                    "object",
                    providerName,
                    debugContext));
            }
        }

        /// <summary>
        /// Returns true if the passed value is an object (e.g. with sub properties), an array or list, or a dictionary.
        /// </summary>
        /// <param name="dataObject">The value to check.</param>
        /// <returns>True if it's not a primitive type or string.</returns>
        public static bool IsStructuredObjectOrArray(object dataObject)
        {
            Type valueType = dataObject is IData
                ? (dataObject as IData).GetInnerType()
                : dataObject.GetType();

            return !(valueType.IsPrimitive || valueType == typeof(string) || valueType == typeof(JValue));
        }

        public static bool IsArray(object dataObject)
        {
            if (dataObject == null)
            {
                return false;
            }

            if (dataObject is IData iDataObject)
            {
                dataObject = iDataObject.GetValueFromGeneric();
            }

            var isArray = dataObject is Array || dataObject is JArray || dataObject is StringValues;
            var isList = dataObject is IList<object> || dataObject is IQueryable || dataObject is IEnumerable<object>;
            var isAssignableToList = dataObject.GetType().IsGenericType &&
                dataObject.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
            return !(dataObject is JObject) && !(dataObject is IDictionary)
                && (isArray || isList || isAssignableToList);
        }

        public static void ThrowIfNotObject(object dataObject, string providerName, JObject debugContext)
        {
            if (dataObject is IData iDataObject)
            {
                dataObject = iDataObject.GetValueFromGeneric();
            }

            if (!IsObject(dataObject))
            {
                debugContext.Add(ErrorDataKey.ErrorMessage, $"Was expecting an object or array but received the primitive type \"{dataObject.GetType()}\".");
                throw new ErrorException(Errors.Automation.InvalidValueTypeObtained(
                    "object",
                    providerName,
                    debugContext));
            }
        }

        public static bool IsObject(object? dataObject)
        {
            if (dataObject == null)
            {
                return false;
            }

            if (dataObject is IData iDataObject)
            {
                dataObject = iDataObject.GetValueFromGeneric();
            }

            return !IsArray(dataObject) && !IsPrimitive(dataObject);
        }

        public static bool IsPrimitive(object dataObject)
        {
            Type valueType = dataObject is IData data ? data.GetInnerType() : dataObject.GetType();

            // Per MSDN documentation, the following types are considered primitive: boolean, byte, char, double, int, long, sbyte, short, uint, ulong, ushort.
            // https://docs.microsoft.com/en-us/dotnet/api/system.type.isprimitive?view=netframework-4.8
            return valueType.IsPrimitive
                || valueType == typeof(string)
                || valueType == typeof(JValue)
                || valueType == typeof(decimal)
                || valueType == typeof(float);
        }

        public static bool IsEqual(object obj, object anotherObj)
        {
            if (ReferenceEquals(obj, anotherObj))
            {
                return true;
            }

            var objType = obj.GetType();
            var anotherObjType = anotherObj.GetType();

            if (obj == null || anotherObj == null)
            {
                return false;
            }

            if (objType != anotherObjType &&
                !(objType.IsGenericType && anotherObjType.IsGenericType))
            {
                return false;
            }

            string objJson, anotherJson;
            ValidateObject(obj, anotherObj, out objJson, out anotherJson);
            return objJson == anotherJson;
        }

        public static string ConvertToString(object dataObject)
        {
            if (dataObject is IData iDataObject)
            {
                dataObject = iDataObject.GetValueFromGeneric();
            }

            if (dataObject == null)
            {
                return "null";
            }

            if (dataObject is bool boolValue)
            {
                return boolValue.ToString().ToLower();
            }

            return dataObject.ToString();
        }

        private static void ValidateObject(object obj, object anotherObj, out string objJson, out string anotherJson)
        {
            bool objIsDictionary = obj.GetType().IsGenericType
                && obj.GetType().GetGenericTypeDefinition() == typeof(ReadOnlyDictionary<,>);
            bool anotherObjIsDictionary = anotherObj.GetType().IsGenericType
                && anotherObj.GetType().GetGenericTypeDefinition() == typeof(ReadOnlyDictionary<,>);

            if (objIsDictionary)
            {
                var objDictionary = (Dictionary<string, object>)obj;
                obj = objDictionary.Select(p => new { propertyName = p.Key, value = p.Value }).ToList();
            }

            if (anotherObjIsDictionary)
            {
                var objAnotherDictionary = (ReadOnlyDictionary<string, object>)anotherObj;
                anotherObj = objAnotherDictionary.Select(p => new { propertyName = p.Key, value = p.Value }).ToList();
            }

            objJson = JsonConvert.SerializeObject(obj);
            anotherJson = JsonConvert.SerializeObject(anotherObj);
        }
    }
}
