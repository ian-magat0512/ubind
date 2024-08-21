// <copyright file="IAdditionalPropertyModelResolverHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Helpers
{
    using System.Threading.Tasks;
    using UBind.Domain.ReadModel;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// An additional property is defined with context and a parent context. Currently the the contexts are
    /// the tenant, organisation and product. If a context is an organisation or a product it can
    /// have a parent context of a tenant, and if the context is tenant which is the top level context,
    /// its parent context is set null. Since the tenant and product ids are previously using alias as
    /// id and the organisation is using a Guid as id so the context id in an additional property
    /// was set to string to accept an Alias or Guid string values in the front end.
    /// Since the context id from the request model (query or body) is an Alias or Guid string,
    /// this is a contract for a helper class is needed to generate a model that will be consumed by request handlers
    /// and for persistence, and the context id of the new generated model will use the new id (in Guid)
    /// if it was using an Alias and use the parsed Guid if it was already a Guid.
    /// </summary>
    public interface IAdditionalPropertyModelResolverHelper
    {
        /// <summary>
        /// Generate a additional property definition model where the context id
        /// and parent context id is a resolved new id if it was using an Alias and
        /// it will be parsed as Guid object if it was using a Guid string.
        /// </summary>
        /// <param name="model"><see cref="AdditionalPropertyDefinitionModel"/>.</param>
        /// <returns>The additional property definition model.</returns>
        Task<AdditionalPropertyDefinitionModel> ResolveToDefinitionModel(
            AdditionalPropertyDefinitionCreateOrUpdateModel model);

        /// <summary>
        /// Generate a additional property read model filter where the context id
        /// and parent context id is a resolved new id if it was using an Alias and
        /// it will be parsed as Guid object if it was using a Guid string.
        /// </summary>
        /// <param name="model"><see cref="AdditionalPropertyDefinitionQueryModel"/>.</param>
        /// <returns><see cref="AdditionalPropertyDefinitionReadModelFilters"/>.</returns>
        Task<AdditionalPropertyDefinitionReadModelFilters> ResolveToDomainReadModelFilter(
            AdditionalPropertyDefinitionQueryModel model);
    }
}
