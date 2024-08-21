// <copyright file="DbUpdateExceptionExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Extensions
{
    using System.Data.Entity.Infrastructure;
    using System.Data.SqlClient;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Extension methods for DbUpdateException.
    /// </summary>
    public static class DbUpdateExceptionExtensions
    {
        private const int DuplicateKeyErrorNumber = 2627;
        private const int UniqueIndexViolationErrorNumber = 2601;

        /// <summary>
        /// Gets a value indicating whether the exception is caused by a duplicate key.
        /// </summary>
        /// <param name="ex">The instance of <see cref="DbUpdateException"/> to test.</param>
        /// <returns><c>true</c> if the exception is caused by a duplicate key, otherwise <c>false</c>.</returns>
        public static bool IsDuplicateKeyException(this DbUpdateException ex)
        {
            return ex.InnermostException<SqlException>()?.Number == DuplicateKeyErrorNumber;
        }

        /// <summary>
        /// Gets a value indicating whether the exception is caused by a duplicate key.
        /// </summary>
        /// <param name="ex">The instance of <see cref="DbUpdateException"/> to test.</param>
        /// <returns><c>true</c> if the exception is caused by a duplicate key, otherwise <c>false</c>.</returns>
        public static bool IsUniqueIndexViolation(this DbUpdateException ex)
        {
            return ex.InnermostException<SqlException>()?.Number == UniqueIndexViolationErrorNumber;
        }
    }
}
