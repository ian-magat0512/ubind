// <copyright file="ISystemEventPersistenceService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Repositories
{
    using System.Collections.Generic;
    using UBind.Domain.Events;
    using UBind.Domain.ReadWriteModel;

    /// <summary>
    /// This is needed to persist missing relationships and tags which are created on a deferred basis
    /// during system event creation.
    /// </summary>
    public interface ISystemEventPersistenceService
    {
        void Persist(IEnumerable<SystemEvent> systemEvents);

        void Persist(IEnumerable<Relationship> relationships, IEnumerable<Tag> tags);
    }
}
