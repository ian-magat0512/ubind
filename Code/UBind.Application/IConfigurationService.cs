// <copyright file="IConfigurationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System.Threading.Tasks;
    using UBind.Application.Releases;
    using UBind.Domain;
    using UBind.Domain.Dto;
    using UBind.Domain.Product;

    /// <summary>
    /// An interface for obtaining product configuration.
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Get the configuration for a given product and environment.
        /// </summary>
        /// <param name="releaseContext">The product release context.</param>
        /// <param name="webFormAppType">The type of form the configuration is for.</param>
        /// <returns>A string containing the configuration JSON, and the release Id used to create it.</returns>
        Task<ReleaseProductConfiguration> GetConfigurationAsync(
            ReleaseContext productContext,
            WebFormAppType webFormAppType = WebFormAppType.Quote);

        /// <summary>
        /// Get the configuration for a given product and environment.
        /// </summary>
        /// <param name="releaseContext">The product release context.</param>
        /// <param name="webFormAppType">The type of form the configuration is for.</param>
        /// <returns>A instance of ProductComponentConfiguration.</returns>
        Task<IProductComponentConfiguration> GetProductComponentConfiguration(
            ReleaseContext productContext,
            WebFormAppType webFormAppType);

        /// <summary>
        /// Get the Displayable Fields configuration for a given product and environment.
        /// </summary>
        /// <param name="releaseContext">The product release context.</param>
        /// <param name="formType">The form that determines wether to retrieve a claim or quote configuration.</param>
        /// <returns>A DTO structure containing information about displayable fields.</returns>
        Task<DisplayableFieldDto> GetDisplayableFieldsAsync(
            ReleaseContext releaseContext,
            WebFormAppType formType = WebFormAppType.Quote);

        /// <summary>
        /// Returns a boolean value that determines whether a app component configuration exists for the given release.
        /// </summary>
        /// <param name="releaseContext">The release context.</param>
        /// <param name="webFormAppType">The type of the configuration to check. </param>
        /// <returns>True if a configuration exists, false otherwise.</returns>
        bool DoesConfigurationExist(ReleaseContext releaseContext, WebFormAppType webFormAppType);

        /// <summary>
        /// Returns a boolean value that determines whether a product configuration exists in the database or not.
        /// </summary>
        /// <param name="productContext">The product context.</param>
        /// <param name="webFormAppType">The type of the configuration to check. </param>
        /// <returns>True if a configuration exists, false otherwise.</returns>
        bool DoesConfigurationExist(ProductContext productContext, WebFormAppType webFormAppType);
    }
}
