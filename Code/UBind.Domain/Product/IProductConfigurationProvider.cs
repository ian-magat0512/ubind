// <copyright file="IProductConfigurationProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Product
{
    using System.Threading.Tasks;
    using UBind.Domain.Configuration;

    /// <summary>
    /// For providing product server-side configuration for a given environment
    /// (as opposed to the client-side configuration that comes from the workbook).
    /// </summary>
    public interface IProductConfigurationProvider
    {
        /// <summary>
        /// Get the product configuration for a given environment.
        /// </summary>
        /// <param name="releaseContext">The context indicating which release/environment to use.</param>
        /// <param name="webFormAppType">The webFormAppType.</param>
        /// <returns>A task from which the product configuration for the given environment can be retrieved.</returns>
        Task<IProductConfiguration> GetProductConfiguration(
            ReleaseContext releaseContext,
            WebFormAppType webFormAppType);

        /// <summary>
        /// Get the Form data schema from product configuration for a given environment.
        /// </summary>
        /// <param name="releaseContext">The context indicating which release/environment to use.</param>
        /// <param name="webFormAppType">The webFormAppType.</param>
        /// <returns>A task from which the form data schema for the given environment can be retrieved.</returns>
        FormDataSchema GetFormDataSchema(
            ReleaseContext releaseContext,
            WebFormAppType webFormAppType);

        /// <summary>
        /// For backwards compatibility, get the product configuration for the default release in a given environment.
        /// </summary>
        /// <param name="productContext">The context indicating which product/environment to use.</param>
        /// <param name="webFormAppType">The webFormAppType.</param>
        /// <returns></returns>
        [Obsolete("Use the version that takes the ReleaseContext instead of the ProductContext.")]
        FormDataSchema GetFormDataSchema(ProductContext productContext, WebFormAppType webFormAppType);
    }
}
