// <copyright file="FakeSystemEventRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Fakes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain;
    using UBind.Domain.Automation;
    using UBind.Domain.Events;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// This is a mock event repository implementation.
    /// which saves to an array.
    /// </summary>
    public class FakeSystemEventRepository : ISystemEventRepository
    {
        private List<SystemEvent> events = new List<SystemEvent>();

        public IQueryable<Event> CreateQueryForAutomations(Guid tenantId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<SystemEvent> GetAll()
        {
            return this.events;
        }

        public List<SystemEvent> GetUnemitted(Guid tenantId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public SystemEvent? GetById(Guid id)
        {
            return this.events.FirstOrDefault(o => o.Id == id);
        }

        public IEnumerable<Relationship> GetRelationships(Guid id, EntityType? entityType = null)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Event> GetTopForCreatedTimeEpochCopy(int count)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Event> GetTopWithNonUpdatedCreatedTicksEpoch(int v)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Event> GetTopWithNonUpdatedExpiryTicksEpoch(int topCount)
        {
            throw new NotImplementedException();
        }

        public SystemEvent Insert(SystemEvent systemEvent, IEnumerable<string> tags)
        {
            this.events.Add(systemEvent);

            return systemEvent;
        }

        /// <inheritdoc/>
        public void Insert(SystemEvent systemEvent)
        {
            this.events.Add(systemEvent);
        }

        public void SaveChanges()
        {
            // do nothing
        }

        public void AddRange(IEnumerable<SystemEvent> systemEvents)
        {
            this.events.AddRange(systemEvents);
        }

        public int UpdateTopWithoutCreatedTicksEpochValue(int batch)
        {
            throw new NotImplementedException();
        }

        public int UpdateTopWithoutExpiredTicksEpochValue(int batch)
        {
            throw new NotImplementedException();
        }

        public void MarkEmitted(Guid systemEventId)
        {
            throw new NotImplementedException();
        }

        public ISystemEventWithRelatedEntities GetSystemEventWithRelatedEntities(Guid tenantId, Guid eventId, IEnumerable<string> relatedEntities)
        {
            throw new NotImplementedException();
        }

        public IQueryable<ISystemEventWithRelatedEntities> CreateQueryForSystemEventDetailsWithRelatedEntities(Guid tenantId, IEnumerable<string> relatedEntities)
        {
            throw new NotImplementedException();
        }

        public bool Exists(Guid eventId)
        {
            return this.events.Any(o => o.Id == eventId);
        }

        public bool Exists(Guid tenantId, Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
