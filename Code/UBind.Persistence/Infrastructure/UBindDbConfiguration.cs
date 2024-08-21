// <copyright file="UBindDbConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Infrastructure
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Data.Entity.SqlServer;

    /// <summary>
    /// Custom entity framework configuration.
    /// </summary>
    public class UBindDbConfiguration : DbConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UBindDbConfiguration"/> class with a command interceptor.
        /// </summary>
        /// <param name="interceptor">A DB command interceptor.</param>
        public UBindDbConfiguration(DbCommandInterceptor interceptor)
        {
            SqlProviderServices.TruncateDecimalsToScale = false;
            this.AddInterceptor(interceptor);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UBindDbConfiguration"/> class with no interceptors.
        /// </summary>
        public UBindDbConfiguration()
        {
        }
    }
}
