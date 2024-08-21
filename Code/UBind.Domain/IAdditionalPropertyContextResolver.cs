// <copyright file="IAdditionalPropertyContextResolver.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using System.Threading.Tasks;
    using UBind.Domain.Enums;

    /// <summary>
    /// An additional property is defined with context and a parent context.
    /// Currently the contexts are the tenant, organisation and product. If a context is an organisation
    /// or a product it can have a parent context of tenant, and if the context is tenant,
    /// the parent context is set null. Since the tenant and product ids are previously using alias as id
    /// and the organisation is using a Guid as id so the context id in an additional property
    /// was set to string to accept an Alias or Guid string values.
    /// This is a contract for a resolver class that help to obtain the new id (in Guid) given the
    /// context or parent context Alias.
    /// </summary>
    public interface IAdditionalPropertyContextResolver
    {
        /// <summary>
        /// Gets the new id of an additional property context alias.
        /// </summary>
        /// <param name="contextAlias">The context id in string.</param>
        /// <param name="definitionContext">The definition context.</param>
        /// <param name="parentContextId">The parent context id in string.</param>
        /// <returns>The actual id.</returns>
        Task<Guid> GetNewIdOfAnAdditionalPropertyContextAlias(
            string contextAlias,
            AdditionalPropertyDefinitionContextType definitionContext,
            string parentContextId);

        /// <summary>
        /// Gets the new id of an additional property parent context alias.
        /// </summary>
        /// <param name="parentContextAlias">The additional property parent context id in string.</param>
        /// <param name="definitionContext">The definition context.</param>
        /// <returns>The actual id.</returns>
        Task<Guid?> GetNewIdOfAnAdditonalPropertyParentContextAlias(
            string parentContextAlias,
            AdditionalPropertyDefinitionContextType definitionContext);
    }
}
