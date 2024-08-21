// <copyright file="GetOnlyContractResolver.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.JsonConverters
{
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Custom contract resolver that supports serialization, but not deserialization, of properties
    /// marked with <see cref="GetOnlyJsonPropertyAttribute"/>.
    /// </summary>
    public class GetOnlyContractResolver : DefaultContractResolver
    {
        /// <inheritdoc/>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (property != null && property.Writable)
            {
                var attributes = property.AttributeProvider.GetAttributes(typeof(GetOnlyJsonPropertyAttribute), true);
                if (attributes != null && attributes.Any())
                {
                    property.Writable = false;
                }
            }

            return property;
        }
    }
}
