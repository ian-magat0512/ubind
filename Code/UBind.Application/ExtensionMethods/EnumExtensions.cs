// <copyright file="EnumExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.ExtensionMethods
{
    using System;

    /// <summary>
    /// Extension methods for enums.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Throw an ArgumentException if an enum parameter is null or empty.
        /// </summary>
        /// <typeparam name="T">The type of T to check null from.</typeparam>
        /// <param name="parameter">The parameter to test.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        public static void ThrowIfArgumentNullOrEmpty<T>(this T? parameter, string parameterName)
            where T : struct
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        /// <summary>
        /// Format the enum to camel case string.
        /// </summary>
        /// <typeparam name="T">The type of T to convert.</typeparam>
        /// <param name="value">The string.</param>
        /// <returns>The string in camel case format.</returns>
        public static string ToCamelCaseString<T>(this T value)
            where T : struct
        {
            var strValue = value.ToString();
            if (!string.IsNullOrEmpty(strValue) && strValue.Length > 1)
            {
                return char.ToLowerInvariant(strValue[0]) + strValue.Substring(1);
            }

            return strValue;
        }

        /// <summary>
        /// Convert an enum value to an enum value of a different type with a matching name.
        /// </summary>
        /// <typeparam name="TIn">The type of enum to convert from.</typeparam>
        /// <typeparam name="TOut">The type of enum to convert to.</typeparam>
        /// <param name="value">The enum value to convert.</param>
        /// <returns>An enum value of type TOut.</returns>
        public static TOut ConvertByName<TIn, TOut>(this TIn value)
            where TIn : struct, IConvertible ////, Enum (Can be uncommented when we switch to C# 7.3)
            where TOut : struct, IConvertible ////, Enum (Can be uncommented when we switch to C# 7.3)
        {
            return (TOut)Enum.Parse(typeof(TOut), value.ToString(), true);
        }

        /// <summary>
        /// Convert an enum value to an enum value of a different type with a matching value.
        /// </summary>
        /// <typeparam name="TIn">The type of enum to convert from.</typeparam>
        /// <typeparam name="TOut">The type of enum to convert to.</typeparam>
        /// <param name="value">The enum value to convert.</param>
        /// <returns>An enum value of type TOut.</returns>
        public static TOut ConvertByValue<TIn, TOut>(this TIn value)
            where TIn : struct, IConvertible ////, Enum (Can be uncommented when we switch to C# 7.3)
            where TOut : struct, IConvertible ////, Enum (Can be uncommented when we switch to C# 7.3)
        {
            return (TOut)Enum.ToObject(typeof(TOut), value);
        }
    }
}
