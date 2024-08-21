// <copyright file="IDocumentAttacher.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.DocumentAttacher
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UBind.Application.Automation.Attachment;
    using UBind.Domain;
    using UBind.Domain.SerialisedEntitySchemaObject;

    /// <summary>
    /// Interface for all entity document attachers. This interface is registered in startup.cs.
    /// </summary>
    public interface IDocumentAttacher
    {
        /// <summary>
        /// Determine if the attacher supports the entity.
        /// </summary>
        /// <param name="entity">The entity to attach the files.</param>
        /// <returns>True if entity is supported, otherwise false.</returns>
        bool CanAttach(IEntity entity);

        void SetEntity(IEntity entity);

        /// <summary>
        /// Gets the aggregate type or the aggregate where files are going be attached.
        /// </summary>
        AggregateType GetAggregateType();

        /// <summary>
        /// Gets the aggregate id or the aggregate where files are going be attached.
        /// </summary>
        /// <returns></returns>
        Guid GetAggregateId();

        /// <summary>
        /// Method for attaching files to an entity.
        /// </summary>
        /// <param name="tenantId">The Id of the tenant.</param>
        /// <param name="attachments">The files to attach.</param>
        /// <returns>Completed task.</returns>
        Task AttachFiles(Guid tenantId, List<FileAttachmentInfo> attachments);
    }
}
