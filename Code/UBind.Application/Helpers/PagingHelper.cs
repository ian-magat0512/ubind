// <copyright file="PagingHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Helpers
{
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Helper for paging params.
    /// </summary>
    public static class PagingHelper
    {
        /// <summary>
        /// Throws an exception if the page number is not a valid value.
        /// Note: you can pass null here and it will not throw.
        /// </summary>
        /// <param name="page">The page number.</param>
        public static void ThrowIfPageNumberInvalid(int? page)
        {
            if (page.HasValue && page.Value < 1)
            {
                throw new ErrorException(Errors.General.BadRequest(
                    string.Format("Invalid page value: {0}", page.Value)));
            }
        }
    }
}
