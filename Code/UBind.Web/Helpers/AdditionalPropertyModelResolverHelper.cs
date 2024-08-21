// <copyright file="AdditionalPropertyModelResolverHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Helpers
{
    using System;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.ReadModel;
    using UBind.Web.ResourceModels;

    /// <summary>
    /// An additional property is defined with context and a parent context. Currently the contexts are
    /// the tenant, organisation and product. If a context is an organisation or a product it can
    /// have a parent context of a tenant, and if the context is tenant which is the top level context,
    /// its parent context is set null. Since the tenant and product ids are previously using alias as
    /// id and the organisation is using a Guid as id so the context id in an additional property
    /// was set to string to accept an Alias or Guid string values in the front end.
    /// Since the context id from the request model (query or body) is an Alias or Guid string,
    /// this class will help to generate a model that will be consumed by request handlers and for persistence,
    /// and the context id of the new generated model will use the new id (in Guid) if it was using an Alias
    /// and use the parsed Guid if it was already a Guid.
    /// </summary>
    public class AdditionalPropertyModelResolverHelper : IAdditionalPropertyModelResolverHelper
    {
        private readonly IAdditionalPropertyContextResolver contextResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyModelResolverHelper"/> class.
        /// </summary>
        /// <param name="contextResolver"><see cref="IAdditionalPropertyContextResolver"/>.</param>
        public AdditionalPropertyModelResolverHelper(
            IAdditionalPropertyContextResolver contextResolver)
        {
            this.contextResolver = contextResolver;
        }

        /// <inheritdoc/>
        public async Task<AdditionalPropertyDefinitionModel> ResolveToDefinitionModel(
            AdditionalPropertyDefinitionCreateOrUpdateModel model)
        {
            var definitionModel = new AdditionalPropertyDefinitionModel(model);

            var contextIdIsGuid = Guid.TryParse(model.ContextId, out Guid contextIsGuidResult);
            if (!contextIdIsGuid)
            {
                definitionModel.ContextId = await this.contextResolver.GetNewIdOfAnAdditionalPropertyContextAlias(
                    model.ContextId,
                    model.ContextType,
                    model.ParentContextId);
            }
            else
            {
                definitionModel.ContextId = contextIsGuidResult;
            }

            var parentContextIdIsGuid = Guid.TryParse(model.ParentContextId, out Guid parentContextIsGuidResult);
            if (!parentContextIdIsGuid)
            {
                definitionModel.ParentContextId = await this.contextResolver.GetNewIdOfAnAdditonalPropertyParentContextAlias(
                    model.ParentContextId,
                    model.ContextType);
            }
            else
            {
                definitionModel.ParentContextId = parentContextIsGuidResult;
            }

            return definitionModel;
        }

        /// <inheritdoc/>
        public async Task<AdditionalPropertyDefinitionReadModelFilters> ResolveToDomainReadModelFilter(
            AdditionalPropertyDefinitionQueryModel model)
        {
            var modelFilter = model.ToFilters();

            var contextIdIsGuid = Guid.TryParse(model.ContextId, out Guid contextIsGuidResult);
            if (!contextIdIsGuid)
            {
                modelFilter.ContextId = await this.contextResolver.GetNewIdOfAnAdditionalPropertyContextAlias(
                    model.ContextId,
                    model.ContextType.Value,
                    model.ParentContextId);
            }
            else
            {
                modelFilter.ContextId = contextIsGuidResult;
            }

            var parentContextIdIsGuid = Guid.TryParse(model.ParentContextId, out Guid parentContextIsGuidResult);
            if (!parentContextIdIsGuid)
            {
                modelFilter.ParentContextId = await this.contextResolver.GetNewIdOfAnAdditonalPropertyParentContextAlias(
                     model.ParentContextId,
                     model.ContextType.Value);
            }
            else
            {
                modelFilter.ParentContextId = parentContextIsGuidResult;
            }

            return modelFilter;
        }
    }
}
