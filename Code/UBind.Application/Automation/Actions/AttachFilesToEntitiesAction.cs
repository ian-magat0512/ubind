// <copyright file="AttachFilesToEntitiesAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CSharpFunctionalExtensions;
    using NodaTime;
    using StackExchange.Profiling;
    using UBind.Application.Automation.Attachment;
    using UBind.Application.Automation.DocumentAttacher;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Error;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Commands.Document;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.SerialisedEntitySchemaObject;
    using Void = UBind.Domain.Helpers.Void;

    /// <summary>
    /// This class is needed because we need a automation action to attach file(s) to entities.
    /// </summary>
    public class AttachFilesToEntitiesAction : Action
    {
        private readonly IEnumerable<IDocumentAttacher> documentAttachers;
        private readonly IClock clock;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachFilesToEntitiesAction"/> class.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        /// <param name="alias">The alias of the action.</param>
        /// <param name="description">The action description.</param>
        /// <param name="asynchronous">The value depicting if the action is to be run asynchronously or not.</param>
        /// <param name="runCondition">An optional condition.</param>
        /// <param name="beforeRunErrorConditions">The validation rules before the action.</param>
        /// <param name="afterRunErrorConditions">The validation rules after the action.</param>
        /// <param name="onErrorActions">The list of non successful actions.</param>
        /// <param name="entities">The entities to attach the file(s) to.</param>
        /// <param name="attachments">The list of files to attach to the entity.</param>
        /// <param name="documentAttachers">The list of supported document attachers.</param>
        /// <param name="clock">The clock for telling time.</param>
        public AttachFilesToEntitiesAction(
            string name,
            string alias,
            string description,
            bool asynchronous,
            IProvider<Data<bool>> runCondition,
            IEnumerable<ErrorCondition> beforeRunErrorConditions,
            IEnumerable<ErrorCondition> afterRunErrorConditions,
            IEnumerable<Action> onErrorActions,
            IEnumerable<IProvider<Data<IEntity>>> entities,
            IEnumerable<IProvider<Data<FileAttachmentInfo>>> attachments,
            IEnumerable<IDocumentAttacher> documentAttachers,
            IClock clock,
            ICqrsMediator mediator)
            : base(name, alias, description, asynchronous, runCondition, beforeRunErrorConditions, afterRunErrorConditions, onErrorActions)
        {
            this.Entities = entities;
            this.Attachments = attachments;
            this.documentAttachers = documentAttachers;
            this.clock = clock;
            this.mediator = mediator;
        }

        /// <summary>
        /// Gets the collection of entities where the file(s) are attached.
        /// </summary>
        public IEnumerable<IProvider<Data<IEntity>>> Entities { get; } = Enumerable.Empty<IProvider<Data<IEntity>>>();

        /// <summary>
        /// Gets or sets the collection of attachments for the entity.
        /// </summary>
        public IEnumerable<IProvider<Data<FileAttachmentInfo>>> Attachments { get; set; } = Enumerable.Empty<IProvider<Data<FileAttachmentInfo>>>();

        /// <inheritdoc/>
        public override ActionData CreateActionData() => new AttachFilesToEntitiesActionData(this.Name, this.Alias, this.clock);

        /// <summary>
        /// Attaches the given file(s) to the configured collection of entities.
        /// </summary>
        /// <param name="providerContext">The data and path to perform resolutions with.</param>
        /// <param name="actionData">The action data for this action.</param>
        /// <returns>An awaitable task.</returns>
        public override async Task<Result<Void, Domain.Error>> Execute(
            IProviderContext providerContext,
            ActionData actionData,
            bool isInternal)
        {
            using (MiniProfiler.Current.Step(nameof(AttachFilesToEntitiesAction) + "." + nameof(this.Execute)))
            {
                actionData.UpdateState(ActionState.Running);
                var resolveEntities = await this.Entities.SelectAsync(async entity => await entity.Resolve(providerContext));
                var entities = resolveEntities.Select(entity => entity.GetValueOrThrowIfFailed().DataValue).ToList();

                // check if file is resolved, as IncludeCondition is checked prior to file generation.
                var resolveAttachments = await this.Attachments.SelectAsync(async file => await file.Resolve(providerContext));
                var attachments = resolveAttachments.Select(file => file.GetValueOrThrowIfFailed()?.DataValue)
                    .Where(f => f != null && f.IsIncluded).ToList();
                var fileAttachments = attachments.Select(c => c.FileName.ToString()).ToList();
                var entityReferences = new Dictionary<string, string>();
                foreach (var entity in entities)
                {
                    var entityType = entity.GetType().Name;
                    entityReferences.Add(entity.Id.ToString(), entityType);

                    var documentAttacher = this.documentAttachers.FirstOrDefault(c => c.CanAttach(entity));
                    if (documentAttacher == null)
                    {
                        var errorData = await providerContext.GetDebugContext();
                        errorData.Add("entityId", entity.Id);
                        errorData.Add("attachments", string.Join(", ", fileAttachments));
                        throw new ErrorException(
                            Errors.Automation.Provider.Entity.AttachmentNotSupported(entityType, errorData));
                    }

                    // Set the entity to the document attacher
                    documentAttacher.SetEntity(entity);

                    try
                    {
                        await this.mediator.Send(new AttachFilesToEntityCommand(providerContext.AutomationData.ContextManager.Tenant.Id, documentAttacher, attachments));
                    }
                    catch (Exception ex) when (ex is NotFoundException || ex is ErrorException)
                    {
                        var errorDetails = ex is ErrorException exception ?
                            exception.Error :
                            Errors.Automation.ActionExecutionErrorEncountered(
                                this.Alias, data: await providerContext.GetDebugContext());
                        return Result.Failure<Void, Domain.Error>(errorDetails);
                    }
                }

                var typedActionData = actionData as AttachFilesToEntitiesActionData;
                typedActionData.EntityReferences = entityReferences;
                typedActionData.Attachments = fileAttachments;
                return Result.Success<Void, Domain.Error>(default);
            }
        }

        public override bool IsReadOnly() => false;
    }
}
