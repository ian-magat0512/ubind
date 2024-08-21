// <copyright file="DomainDefaultProductConfigurationProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1600

namespace UBind.Domain.Tests.Services
{
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Product;

    /// <summary>
    /// Defines the <see cref="DomainDefaultProductConfigurationProvider" />.
    /// </summary>
    public class DomainDefaultProductConfigurationProvider : IProductConfigurationProvider
    {
        public FormDataSchema GetFormDataSchema(ProductContext productContext, WebFormAppType webFormAppType)
        {
            return null;
        }

        public FormDataSchema GetFormDataSchema(ReleaseContext releaseContext, WebFormAppType webFormAppType)
        {
            return null;
        }

        /// <inheritdoc />
        public async Task<IProductConfiguration> GetProductConfiguration(ReleaseContext releaseContext, WebFormAppType webFormAppType)
        {
            return await Task.FromResult(new DefaultProductConfiguration());
        }
    }
}
