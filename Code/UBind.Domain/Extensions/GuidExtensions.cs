// <copyright file="GuidExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Extensions
{
    using System;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Extension methods for guids.
    /// </summary>
    public static class GuidExtensions
    {
        /// <summary>
        /// Checks if guid nullable is null or empty.
        /// </summary>
        /// <param name="guid">The guid.</param>
        /// <returns>true if null or empty, false if not.</returns>
        public static bool IsNullOrEmpty(this Guid? guid)
        {
            return guid == null || guid.Value == Guid.Empty;
        }

        public static Guid ThrowIfNullOrEmpty(this Guid? guid, string alias)
        {
            if (guid == null || guid.Value == Guid.Empty)
            {
                throw new NullReferenceException($"When trying to access GUID value for \"{alias}\", the value was null or empty ");
            }
            return (Guid)guid;
        }

        public static void ThrowIfEmpty(this Guid guid, string alias)
        {
            if (guid == Guid.Empty)
            {
                throw new ErrorException(Errors.General.BadRequest($"The {alias} had an empty value."));
            }
        }
    }
}
