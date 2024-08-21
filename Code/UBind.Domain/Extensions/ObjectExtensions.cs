// <copyright file="ObjectExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>
namespace UBind.Domain.Extensions
{
    /// <summary>
    /// Extension methods for objects.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Throw an ArgumentNullException if a parameter is null.
        /// </summary>
        /// <param name="parameter">The parameter to test.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        public static void ThrowIfArgumentNull(this object parameter, string parameterName)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
