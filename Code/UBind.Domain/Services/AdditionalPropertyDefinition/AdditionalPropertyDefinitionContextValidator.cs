// <copyright file="AdditionalPropertyDefinitionContextValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Services.AdditionalPropertyDefinition
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using Humanizer;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Base class of context validators.
    /// </summary>
    public abstract class AdditionalPropertyDefinitionContextValidator
    {
        private readonly IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionContextValidator"/> class.
        /// </summary>
        public AdditionalPropertyDefinitionContextValidator()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AdditionalPropertyDefinitionContextValidator"/> class.
        /// </summary>
        /// <param name="additionalPropertyDefinitionRepository">Additional property repository.</param>
        protected AdditionalPropertyDefinitionContextValidator(
            IAdditionalPropertyDefinitionRepository additionalPropertyDefinitionRepository)
        {
            this.additionalPropertyDefinitionRepository = additionalPropertyDefinitionRepository;
        }

        /// <summary>
        /// Validates inputs and throws exception if it fails the validations on create.
        /// </summary>
        /// <param name="name">Additional property name.</param>
        /// <param name="alias">Additional property alias.</param>
        /// <param name="contextId">Context ID.</param>
        /// <param name="entityType"><see cref="AdditionalPropertyEntityType"/>.</param>
        /// <param name="parentContextId">ID of the context which the immediate context falls under.</param>
        public abstract Task ThrowIfValidationFailsOnCreate(
          Guid tenantId,
          string name,
          string alias,
          Guid contextId,
          AdditionalPropertyEntityType entityType,
          Guid? parentContextId);

        /// <summary>
        /// Validates inputs and throws exception if it fails validations on update.
        /// </summary>
        /// <param name="id">Primary ID.</param>
        /// <param name="name">Additional property name.</param>
        /// <param name="alias">Additional property alias.</param>
        /// <param name="contextId">ID of the immediate context.</param>
        /// <param name="entityType">Enum item of <see cref="AdditionalPropertyEntityType"/>.</param>
        /// <param name="parentContextId">ID of the context which the immediate context falls under.</param>
        public abstract Task ThrowIfValidationFailsOnUpdate(
            Guid tenantId,
            Guid id,
            string name,
            string alias,
            Guid contextId,
            AdditionalPropertyEntityType entityType,
            Guid? parentContextId);

        /// <summary>
        /// Checks if the name or alias exists on create.
        /// </summary>
        /// <param name="name">Additional property definition name.</param>
        /// <param name="alias">Additional property definition alias.</param>
        /// <param name="entityType"><see cref="AdditionalPropertyEntityType"/>.</param>
        /// <param name="contextId">The ID of the main context.</param>
        /// <param name="parentContextId">The ID of the immediate context.</param>
        /// <returns>True if exists, otherwise false.</returns>
        /// <param name="contextType"><see cref="AdditionalPropertyDefinitionContextType"/>.</param>
        public bool IsNameOrAliasInUse(
            Guid tenantId,
            string name,
            string alias,
            AdditionalPropertyEntityType entityType,
            Guid contextId,
            Guid? parentContextId,
            AdditionalPropertyDefinitionContextType contextType)
        {
            var mainContextId = this.GetMainContextId(contextId, parentContextId);
            var queryable = this.additionalPropertyDefinitionRepository
                .GetByEntityTypeAndTopContextFromContextIdAndParentContextId(tenantId, mainContextId, entityType);

            if (!string.IsNullOrEmpty(name) && parentContextId.HasValue)
            {
                return queryable.Any(VerifyNameOnCreateWithinParentContext(
                    contextId, parentContextId.Value, contextType, name));
            }

            if (!string.IsNullOrEmpty(name) && !parentContextId.HasValue)
            {
                return queryable.Any(VerifyNameOnCreateWithinTheContext(name));
            }

            if (!string.IsNullOrEmpty(alias) && parentContextId.HasValue)
            {
                return queryable.Any(VerifyAliasOnCreateWithinParentContext(
                    contextId, parentContextId.Value, contextType, alias));
            }

            if (!string.IsNullOrEmpty(alias) && !parentContextId.HasValue)
            {
                return queryable.Any(VerifyAliasOnCreateWithinTheContext(alias));
            }

            return false;
        }

        /// <summary>
        /// Checks if the name or alias exists on update.
        /// </summary>
        /// <param name="id">Additional property definition ID.</param>
        /// <param name="name">Additional property definition name.</param>
        /// <param name="alias">Additional property definition alias.</param>
        /// <param name="entityType"><see cref="AdditionalPropertyEntityType"/>.</param>
        /// <param name="contextId">The ID of the main context.</param>
        /// <param name="parentContextId">The ID of the immediate context.</param>
        /// <returns>True if exists, otherwise false.</returns>
        /// <param name="contextType"><see cref="AdditionalPropertyDefinitionContextType"/>.</param>
        public bool IsNameOrAliasInUseOnUpdate(
            Guid tenantId,
            Guid id,
            string name,
            string alias,
            AdditionalPropertyEntityType entityType,
            Guid contextId,
            Guid? parentContextId,
            AdditionalPropertyDefinitionContextType contextType)
        {
            var mainContextId = this.GetMainContextId(contextId, parentContextId);
            var queryable = this.additionalPropertyDefinitionRepository
                .GetByEntityTypeAndTopContextFromContextIdAndParentContextId(tenantId, mainContextId, entityType);

            if (!string.IsNullOrEmpty(name) && parentContextId.HasValue)
            {
                return queryable.Any(
                    VerifyNameOnUpdateWithinTheParentContext(id, name, contextType, contextId, parentContextId.Value));
            }

            if (!string.IsNullOrEmpty(name) && !parentContextId.HasValue)
            {
                return queryable.Any(VerifyNameOnUpdateWithinTheContext(id, name, contextType));
            }

            if (!string.IsNullOrEmpty(alias) && parentContextId.HasValue)
            {
                return queryable.Any(
                    VerifyAliasOnUpdateWithinTheParentContext(
                        id,
                        alias,
                        contextType,
                        contextId,
                        parentContextId.Value));
            }

            if (!string.IsNullOrEmpty(alias) && !parentContextId.HasValue)
            {
                return queryable.Any(VerifyAliasOnUpdateWithinTheContext(id, alias, contextType));
            }

            return false;
        }

        /// <summary>
        /// Validates alias and property name for create and update operations, throws error exception if
        /// property name or alias is found.
        /// </summary>
        /// <param name="contextId">The primary ID of the context.</param>
        /// <param name="parentContextId">The primary ID of the parent context if the main context is a sub context.
        /// </param>
        /// <param name="entityType"><see cref="AdditionalPropertyEntityType"/>.</param>
        /// <param name="contextType"><see cref="AdditionalPropertyDefinitionContextType"/>.</param>
        /// <param name="name">Property name.</param>
        /// <exception cref="ErrorException"><see cref="ErrorException"/>.</exception>
        /// <param name="alias">Property alias.</param>
        protected void ThrowIfValidationFailsForPropertyAliasAndNameOnCreate(
            Guid tenantId,
            Guid contextId,
            Guid? parentContextId,
            AdditionalPropertyEntityType entityType,
            AdditionalPropertyDefinitionContextType contextType,
            string name,
            string alias)
        {
            if (!parentContextId.HasValue)
            {
                this.ThrowIfNameOrAliasAlreadyExists(
                    tenantId,
                    contextId,
                    parentContextId,
                    entityType,
                    contextType,
                    name,
                    alias,
                    VerifyNameOnCreateWithinTheContext(name),
                    VerifyAliasOnCreateWithinTheContext(alias));
            }
            else
            {
                this.ThrowIfNameOrAliasAlreadyExists(
                    tenantId,
                    contextId,
                    parentContextId,
                    entityType,
                    contextType,
                    name,
                    alias,
                    VerifyNameOnCreateWithinParentContext(contextId, parentContextId.Value, contextType, name),
                    VerifyAliasOnCreateWithinParentContext(contextId, parentContextId.Value, contextType, alias));
            }
        }

        /// <summary>
        /// Validates alias and property name for create and update operations, throws error exception if
        /// property name and alias is found. This only means that the additional property name or alias already
        /// belongs to something else within the context and entity.
        /// </summary>
        /// <param name="additionalPropertyId">Additional property definition ID.</param>
        /// <param name="name">Assigned property name.</param>
        /// <param name="alias">Assigned property alias.</param>
        /// <param name="contextType"><see cref="AdditionalPropertyDefinitionContextType"/>.</param>
        /// <param name="contextId">ID of the immediate context.</param>
        /// <param name="entityType">Enum item of <see cref="AdditionalPropertyEntityType"/>.</param>
        /// <exception cref="ErrorException"><see cref="ErrorException"/>.</exception>
        /// <param name="parentContextId">ID of the context which the immediate context falls under.</param>
        protected void ThrowIfValidationForNameAliasAndIdFailsOnUpdate(
            Guid tenantId,
            Guid additionalPropertyId,
            string name,
            string alias,
            AdditionalPropertyDefinitionContextType contextType,
            Guid contextId,
            AdditionalPropertyEntityType entityType,
            Guid? parentContextId)
        {
            if (!parentContextId.HasValue)
            {
                this.ThrowIfNameOrAliasAlreadyExists(
                    tenantId,
                    contextId,
                    parentContextId,
                    entityType,
                    contextType,
                    name,
                    alias,
                    VerifyNameOnUpdateWithinTheContext(additionalPropertyId, name, contextType),
                    VerifyAliasOnUpdateWithinTheContext(additionalPropertyId, alias, contextType));
            }
            else
            {
                this.ThrowIfNameOrAliasAlreadyExists(
                    tenantId,
                    contextId,
                    parentContextId,
                    entityType,
                    contextType,
                    name,
                    alias,
                    VerifyNameOnUpdateWithinTheParentContext(
                        additionalPropertyId,
                        name,
                        contextType,
                        contextId,
                        parentContextId.Value),
                    VerifyAliasOnUpdateWithinTheParentContext(
                        additionalPropertyId,
                        alias,
                        contextType,
                        contextId,
                        parentContextId.Value));
            }
        }

        /// <summary>
        /// Template method in retrieving and validating the uniqueness of the name and alias.
        /// </summary>
        /// <param name="contextId">The primary ID of the context.</param>
        /// <param name="parentContextId">The primary ID of the parent context if the main context is a sub context.
        /// </param>
        /// <param name="entityType"><see cref="AdditionalPropertyEntityType"/>.</param>
        /// <param name="contextType"><see cref="AdditionalPropertyDefinitionContextType"/>.</param>
        /// <param name="name">Property name.</param>
        /// <param name="alias">Property alias.</param>
        /// <param name="nameExists">Expression method in checking the uniqueness of the name.</param>
        /// <exception cref="ErrorException"><see cref="ErrorException"/>.</exception>
        /// <param name="aliasExists">Expression method in checking the uniqueness of the alias.</param>
        protected void ThrowIfNameOrAliasAlreadyExists(
            Guid tenantId,
            Guid contextId,
            Guid? parentContextId,
            AdditionalPropertyEntityType entityType,
            AdditionalPropertyDefinitionContextType contextType,
            string name,
            string alias,
            Expression<Func<AdditionalPropertyDefinitionReadModel, bool>> nameExists,
            Expression<Func<AdditionalPropertyDefinitionReadModel, bool>> aliasExists)
        {
            var entityDescription = entityType.Humanize();
            var contextDescription = contextType.Humanize();
            var topContextId = this.GetMainContextId(contextId, parentContextId);

            var queryable = this.additionalPropertyDefinitionRepository
                .GetByEntityTypeAndTopContextFromContextIdAndParentContextId(tenantId, topContextId, entityType);

            if (queryable.Any(nameExists))
            {
                throw new ErrorException(Errors.AdditionalProperties.NameInUse(
                   entityDescription, name, contextDescription));
            }

            if (queryable.Any(aliasExists))
            {
                throw new ErrorException(Errors.AdditionalProperties.AliasInUse(
                    alias, entityDescription, contextDescription));
            }
        }

        private static Expression<Func<AdditionalPropertyDefinitionReadModel, bool>>
            VerifyNameOnCreateWithinTheContext(string name)
        {
            return (apd) => apd.Name == name;
        }

        private static Expression<Func<AdditionalPropertyDefinitionReadModel, bool>>
            VerifyAliasOnCreateWithinTheContext(string alias)
        {
            return (apd) => apd.Alias == alias;
        }

        private static Expression<Func<AdditionalPropertyDefinitionReadModel, bool>>
            VerifyNameOnCreateWithinParentContext(
                Guid contextId,
                Guid parentContextId,
                AdditionalPropertyDefinitionContextType contextType,
                string name)
        {
            return (apd) => (apd.Name == name && apd.ContextId == parentContextId) ||
                    (apd.Name == name
                    && apd.ContextId == contextId
                    && apd.ContextType == contextType) ||
                    (apd.Name == name
                    && apd.ParentContextId == parentContextId
                    && apd.ContextType != contextType);
        }

        private static Expression<Func<AdditionalPropertyDefinitionReadModel, bool>>
            VerifyAliasOnUpdateWithinTheParentContext(
                Guid additionalPropertyId,
                string alias,
                AdditionalPropertyDefinitionContextType contextType,
                Guid contextId,
                Guid parentContextId)
        {
            return (apd) => (apd.Alias == alias
                    && apd.Id != additionalPropertyId
                    && apd.ContextType == contextType
                    && apd.ContextId == contextId)
                    ||
                    (apd.Alias == alias && apd.ContextType != contextType && apd.ContextId == parentContextId)
                    ||
                    (apd.Alias == alias
                    && apd.ContextType != contextType
                    && apd.ParentContextId == parentContextId);
        }

        private static Expression<Func<AdditionalPropertyDefinitionReadModel, bool>>
            VerifyNameOnUpdateWithinTheParentContext(
                Guid additionalPropertyId,
                string name,
                AdditionalPropertyDefinitionContextType contextType,
                Guid contextId,
                Guid parentContextId)
        {
            return (apd) => (apd.Name == name
                    && apd.Id != additionalPropertyId
                    && apd.ContextType == contextType
                    && apd.ContextId == contextId)
                        ||
                    (apd.Name == name && apd.ContextType != contextType && apd.ContextId == parentContextId)
                    ||
                    (apd.Name == name
                    && apd.ContextType != contextType
                    && apd.ParentContextId == parentContextId);
        }

        private static Expression<Func<AdditionalPropertyDefinitionReadModel, bool>>
            VerifyAliasOnUpdateWithinTheContext(
                Guid additionalPropertyId,
                string alias,
                AdditionalPropertyDefinitionContextType contextType)
        {
            return (apd) => (apd.Alias == alias
                    && apd.Id != additionalPropertyId
                    && apd.ContextType == contextType)
                    || (apd.Alias == alias && apd.ContextType != contextType);
        }

        private static Expression<Func<AdditionalPropertyDefinitionReadModel, bool>>
            VerifyNameOnUpdateWithinTheContext(
                Guid additionalPropertyId,
                string name,
                AdditionalPropertyDefinitionContextType contextType)
        {
            return (apd) => (apd.Name == name
                    && apd.Id != additionalPropertyId
                    && apd.ContextType == contextType) ||
                    (apd.Name == name && apd.ContextType != contextType);
        }

        private static Expression<Func<AdditionalPropertyDefinitionReadModel, bool>>
            VerifyAliasOnCreateWithinParentContext(
                Guid contextId,
                Guid parentContextId,
                AdditionalPropertyDefinitionContextType contextType,
                string alias)
        {
            return (apd) => (apd.Alias == alias && apd.ContextId == parentContextId) ||
                                    (apd.Alias == alias
                                    && apd.ContextId == contextId
                                    && apd.ContextType == contextType) ||
                                    (apd.Alias == alias
                                    && apd.ParentContextId == parentContextId
                                    && apd.ContextType != contextType);
        }

        private Guid GetMainContextId(Guid contextId, Guid? parentContextId)
        {
            return parentContextId.HasValue ? parentContextId.Value : contextId;
        }
    }
}
