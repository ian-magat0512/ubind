// <copyright file="AttachFilesToEntityActionConfigModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Newtonsoft.Json;
    using NodaTime;
    using UBind.Application.Automation.Attachment;
    using UBind.Application.Automation.DocumentAttacher;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.Entity;
    using UBind.Domain.Patterns.Cqrs;

    /// <summary>
    /// Model for building an instance of <see cref="AttachFilesToEntitiesAction"/>.
    /// </summary>
    /// <remarks>
    /// This is for backward compatibility till the previous implementation of said action has been made obsolete via 1.0.1 schema version.
    /// </remarks>
    public class AttachFilesToEntityActionConfigModel : BaseActionConfigurationModel, IBuilder<Action>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttachFilesToEntityActionConfigModel"/> class.
        /// </summary>
        /// <param name="name">The action name.</param>
        /// <param name="alias">The action alias.</param>
        /// <param name="description">The action description.</param>
        /// <param name="asynchronous">The value depicting if the action is to be run asynchronously or not.</param>
        /// <param name="runCondition">An optional condition.</param>
        /// <param name="beforeRunErrorConditions">The validation rules before the action.</param>
        /// <param name="afterRunErrorConditions">The validation rules after the action.</param>
        /// <param name="onErrorActions">The list of non successful actions.</param>
        /// <param name="entity">The entity to attach the file(s).</param>
        /// <param name="attachments">The file(s) to attach to the entity.</param>
        [JsonConstructor]
        public AttachFilesToEntityActionConfigModel(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IBuilder<IProvider<Data<bool>>> runCondition,
            IEnumerable<ErrorConditionConfigModel> beforeRunErrorConditions,
            IEnumerable<ErrorConditionConfigModel> afterRunErrorConditions,
            IEnumerable<IBuilder<Action>> onErrorActions,
            IBuilder<BaseEntityProvider> entity,
            IEnumerable<FileAttachmentProviderConfigModel> attachments)
            : base(
              name,
              alias,
              description,
              asynchronous,
              runCondition,
              beforeRunErrorConditions,
              afterRunErrorConditions,
              onErrorActions)
        {
            this.Entity = entity;
            this.Attachments = attachments ?? Enumerable.Empty<FileAttachmentProviderConfigModel>();
        }

        /// <summary>
        /// Gets the entity that the files should be attached to.
        /// </summary>
        public IBuilder<BaseEntityProvider> Entity { get; private set; }

        /// <summary>
        /// Gets the collection of file attachments to be attached to the entity.
        /// </summary>
        public IEnumerable<FileAttachmentProviderConfigModel> Attachments { get; private set; } = Enumerable.Empty<FileAttachmentProviderConfigModel>();

        /// <inheritdoc/>
        public override Action Build(IServiceProvider dependencyProvider)
        {
            var beforeRunConditions = this.BeforeRunErrorConditions?.Select(br => br.Build(dependencyProvider));
            var afterRunConditions = this.AfterRunErrorConditions?.Select(ar => ar.Build(dependencyProvider));
            var errorActions = this.OnErrorActions?.Select(oa => oa.Build(dependencyProvider));
            var attachments = this.Attachments?.Select(file => file.Build(dependencyProvider));
            var entityList = new List<BaseEntityProvider>();
            entityList.Add(this.Entity.Build(dependencyProvider));
            var documentAttachers = dependencyProvider.GetServices<IDocumentAttacher>();
            var clock = dependencyProvider.GetRequiredService<IClock>();
            var mediator = dependencyProvider.GetRequiredService<ICqrsMediator>();

            return new AttachFilesToEntitiesAction(
                this.Name,
                this.Alias,
                this.Description,
                this.Asynchronous,
                this.RunCondition?.Build(dependencyProvider),
                beforeRunConditions,
                afterRunConditions,
                errorActions,
                entityList,
                attachments,
                documentAttachers,
                clock,
                mediator);
        }
    }
}
