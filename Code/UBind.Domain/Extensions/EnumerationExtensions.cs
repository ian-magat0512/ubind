// <copyright file="EnumerationExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Extensions
{
    using System;

    /// <summary>
    /// Extension class for enumeration objects.
    /// </summary>
    public static class EnumerationExtensions
    {
        /// <summary>
        /// Gets an attribute on an enum field value.
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve.</typeparam>
        /// <param name="enumVal">The enum value.</param>
        /// <returns>The attribute of type T that exists on the enum value.</returns>
        /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
        public static T GetAttributeOfType<T>(this Enum enumVal)
            where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }
    }
}
