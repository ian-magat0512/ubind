// <copyright file="GemBoxDataSourceHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.FileHandling.GemBoxServices.Helpers
{
    using Newtonsoft.Json.Linq;

    public class GemBoxDataSourceHelper
    {
        /// <summary>
        /// This needs to be done because GemBox only processes range merges
        /// with arrays
        /// </summary>
        /// <param name="token"></param>
        public static JToken Format(JToken token)
        {
            PlaceJObjectTypePropertiesInAnArray(token);
            if (token.Type == JTokenType.Array)
            {
                return token;
            }

            return new JArray(token);
        }

        /// <summary>
        /// This method iterates the entire object and its children
        /// to ensure that all properties or child properties
        /// that is of type JObject will be wrapped in an array.
        /// This will ignore properties that are of value type (string,
        /// integer, float, etc.)
        /// </summary>
        /// <param name="token"></param>
        private static void PlaceJObjectTypePropertiesInAnArray(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                var jObject = (JObject)token;
                foreach (var property in jObject.Properties())
                {
                    if (property.Value.Type == JTokenType.Object)
                    {
                        var nestedJObject = (JObject)property.Value;
                        var jArray = new JArray { nestedJObject };
                        jObject[property.Name] = jArray;
                    }

                    PlaceJObjectTypePropertiesInAnArray(property.Value);
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                var jArray = (JArray)token;
                foreach (var item in jArray)
                {
                    PlaceJObjectTypePropertiesInAnArray(item);
                }
            }
        }
    }
}
