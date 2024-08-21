// <copyright file="ObjectWrapper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.PathLookup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.List;

    /// <summary>
    /// For wrapping data as IData for returning from classes implementing <see cref="IProvider{IData}"/>.
    /// </summary>
    public static class ObjectWrapper
    {
        /// <summary>
        /// Wrap an object as an instance of <see cref="Data{T}"/>, where T is the actual type.
        /// </summary>
        /// <param name="obj">The object to wrap.</param>
        /// <returns>A new instance of <see cref="Data{T}"/>, returned as <see cref="IData"/>.</returns>
        public static IData? Wrap(object? obj)
        {
            if (obj is null)
            {
                return null;
            }

            // it's already wrapped
            if (obj is IData dataObject)
            {
                return dataObject;
            }

            return obj switch
            {
                string stringValue => new Data<string>(stringValue),
                Guid guidValue => new Data<string>(guidValue.ToString()),
                long longValue => new Data<long>(longValue),
                decimal decimalValue => new Data<decimal>(decimalValue),
                bool booleanValue => new Data<bool>(booleanValue),
                IReadOnlyDictionary<string, object> dictionary => new Data<IReadOnlyDictionary<string, object>>(dictionary),
                JObject jObject => new Data<JObject>(jObject),
                byte[] byteArray => new Data<byte[]>(byteArray),
                JArray jArray => Wrap(jArray),
                JValue jValue => Wrap(jValue),
                _ => new Data<object>(obj)
            };
        }

        /// <summary>
        /// Wrap an object as an instance of <see cref="Data{T}"/>, where T is the actual type.
        /// </summary>
        /// <param name="obj">The object to wrap.</param>
        /// <returns>A new instance of <see cref="Data{T}"/>, returned as <see cref="IData"/>.</returns>
        public static IData? Wrap(JToken? obj)
        {
            if (obj is null)
            {
                return null;
            }

            IData? result;
            switch (obj.Type)
            {
                case JTokenType.Object:
                    result = new Data<IReadOnlyDictionary<string, object>?>(
                        obj.ToObject<IReadOnlyDictionary<string, object>>());
                    break;
                case JTokenType.Array:
                    List<object?> list = obj.Select(token => Wrap(token)?.GetValueFromGeneric()).ToList<object?>();
                    result = new GenericDataList<object?>(list);
                    break;
                case JTokenType.Integer:
                    result = new Data<long>(obj.ToObject<long>());
                    break;
                case JTokenType.Float:
                    result = new Data<decimal>(obj.ToObject<decimal>());
                    break;
                case JTokenType.Date: // wrapping date value to string - allowing consumer to parse it as needed.
                case JTokenType.Guid:
                case JTokenType.String:
                    result = new Data<string>(obj.ToString());
                    break;
                case JTokenType.Boolean:
                    result = new Data<bool>(obj.ToObject<bool>());
                    break;
                case JTokenType.Bytes:
                    var byteArray = obj.ToObject<byte[]>();
                    result = byteArray != null ? new Data<byte[]>(byteArray) : null;
                    break;
                case JTokenType.Null:
                case JTokenType.Undefined:
                    result = null;
                    break;
                default:
                    throw new NotSupportedException($"Cannot wrap object of type {obj.GetType()}");
            }

            return result;
        }
    }
}
