// <copyright file="SystemEventRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using Dapper;
    using UBind.Domain;
    using UBind.Domain.Automation;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Repository for storing event for persistence.
    /// </summary>
    public class SystemEventRepository : ISystemEventRepository
    {
        private readonly IConnectionConfiguration connection;
        private readonly IUBindDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemEventRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public SystemEventRepository(
            IUBindDbContext dbContext,
            IConnectionConfiguration connection)
        {
            this.connection = connection;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public SystemEvent? GetById(Guid id)
        {
            return this.dbContext.SystemEvents.FirstOrDefault(o => o.Id == id);
        }

        /// <inheritdoc/>
        public bool Exists(Guid eventId)
        {
            return this.dbContext.SystemEvents.SingleOrDefault(r => r.Id == eventId) != default;
        }

        /// <inheritdoc/>
        public IEnumerable<SystemEvent> GetAll()
        {
            return this.dbContext.SystemEvents.AsQueryable();
        }

        /// <inheritdoc/>
        public void Insert(SystemEvent systemEvent)
        {
            this.dbContext.SystemEvents.Add(systemEvent);
        }

        /// <inheritdoc/>
        public void AddRange(IEnumerable<SystemEvent> systemEvents)
        {
            this.dbContext.SystemEvents.AddRange(systemEvents);
        }

        /// <inheritdoc/>
        public IEnumerable<Relationship> GetRelationships(Guid eventId, EntityType? entityType = null)
        {
            var relationshipQuery =
                this.dbContext.SystemEvents
                .GroupJoin(
                    this.dbContext.Relationships,
                    systemEvent => systemEvent.Id,
                    relationship => relationship.FromEntityId,
                    (@event, relationships) => new { Event = @event, Relationships = relationships })
                   .Where(x => x.Event.Id == eventId)
                .GroupJoin(
                    this.dbContext.Relationships,
                    model => model.Event.Id,
                    relationship => relationship.ToEntityId,
                    (model, relationships) => new EventTagRelationshipModel(model.Event, model.Relationships.Concat(relationships), Enumerable.Empty<Tag>()))
                   .Where(x => x.Event.Id == eventId);

            if (entityType != null)
            {
                relationshipQuery = relationshipQuery.Where(x => x.Relationships.Any(y => y.FromEntityType == entityType || y.ToEntityType == entityType));
            }

            return relationshipQuery.SelectMany(x => x.Relationships).ToList();
        }

        /// <inheritdoc/>
        public IQueryable<Event> CreateQueryForAutomations(Guid tenantId)
        {
            return
                this.dbContext.SystemEvents
                .Where(e => e.TenantId == tenantId)
                .GroupJoin(
                    this.dbContext.Tags,
                    e => e.Id,
                    t => t.EntityId,
                    (e, tags) => new Event
                    {
                        Id = e.Id,
                        CreatedTicksSinceEpoch = e.CreatedTicksSinceEpoch,
                        EventType = e.EventType,
                        CustomEventAlias = e.CustomEventAlias,
                        Tags = tags.Select(t => t.Value),
                    });
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public int UpdateTopWithoutExpiredTicksEpochValue(int batch)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@count", batch);

            string sql = @"UPDATE TOP (@count) [SystemEvents]
                            SET [ExpiryTicksSinceEpoch] = [ExpiryTimeAsTicksSinceEpoch]
                            WHERE [ExpiryTicksSinceEpoch] <> [ExpiryTimeAsTicksSinceEpoch]";

            using (var connection = new SqlConnection(this.connection.UBind))
            {
                var rowsAffected = connection.Execute(sql, parameters, null, 0, System.Data.CommandType.Text);
                return rowsAffected;
            }
        }

        /// <inheritdoc/>
        public int UpdateTopWithoutCreatedTicksEpochValue(int batch)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@count", batch);

            string sql = @"UPDATE TOP (@count) [SystemEvents]
                            SET [CreatedTicksSinceEpoch] = [CreationTimeInTicksSinceEpoch]
                            WHERE [CreatedTicksSinceEpoch] <> [CreationTimeInTicksSinceEpoch]";

            using (var connection = new SqlConnection(this.connection.UBind))
            {
                var rowsAffected = connection.Execute(sql, parameters, null, 0, System.Data.CommandType.Text);
                return rowsAffected;
            }
        }

        /// <inheritdoc/>
        public ISystemEventWithRelatedEntities? GetSystemEventWithRelatedEntities(Guid tenantId, Guid eventId, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForSystemEventDetailsWithRelatedEntities(tenantId, relatedEntities);
            return query.FirstOrDefault(q => q.SystemEvent != null && q.SystemEvent.Id == eventId && q.SystemEvent.TenantId == tenantId);
        }

        /// <inheritdoc/>
        public IQueryable<ISystemEventWithRelatedEntities> CreateQueryForSystemEventDetailsWithRelatedEntities(Guid tenantId, IEnumerable<string> relatedEntities)
        {
            return this.CreateQueryForSystemEventDetailsWithRelatedEntities(
                tenantId, this.dbContext.SystemEvents, relatedEntities);
        }

        public bool Exists(Guid tenantId, Guid id)
        {
            return this.dbContext.SystemEvents.AsNoTracking().Any(r => r.TenantId == tenantId && r.Id == id);
        }

        private IQueryable<SystemEventWithRelatedEntities> CreateQueryForSystemEventDetailsWithRelatedEntities(
            Guid tenantId, IQueryable<SystemEvent> dataSource, IEnumerable<string> relatedEntities)
        {
            var query = from systemEvent in dataSource
                        where systemEvent.TenantId == tenantId
                        select new SystemEventWithRelatedEntities
                        {
                            SystemEvent = systemEvent,
                            Tenant = default,
                            TenantDetails = new TenantDetails[] { },
                            Organisation = default,
                            Product = default,
                            ProductDetails = default,
                            FromRelationships = new Relationship[] { },
                            ToRelationships = new Relationship[] { },
                        };
            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Event.Tenant))))
            {
                query = query.Join(this.dbContext.Tenants, u => u.SystemEvent != null ? u.SystemEvent.TenantId : default, t => t.Id, (u, tenant) => new SystemEventWithRelatedEntities
                {
                    SystemEvent = u.SystemEvent,
                    Tenant = tenant,
                    TenantDetails = tenant.DetailsCollection,
                    Organisation = u.Organisation,
                    Product = u.Product,
                    ProductDetails = u.ProductDetails,
                    FromRelationships = u.FromRelationships,
                    ToRelationships = u.ToRelationships,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Event.Organisation))))
            {
                query = query.Join(this.dbContext.OrganisationReadModel, u => u.SystemEvent != null ? u.SystemEvent.OrganisationId : default, t => t.Id, (u, organisation) => new SystemEventWithRelatedEntities
                {
                    SystemEvent = u.SystemEvent,
                    Tenant = u.Tenant,
                    TenantDetails = u.TenantDetails,
                    Organisation = organisation,
                    Product = u.Product,
                    ProductDetails = u.ProductDetails,
                    FromRelationships = u.FromRelationships,
                    ToRelationships = u.ToRelationships,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Event.Product))))
            {
                query = query.Join(this.dbContext.Products, u => u.SystemEvent != null ? u.SystemEvent.ProductId : default, t => t.Id, (u, product) => new SystemEventWithRelatedEntities
                {
                    SystemEvent = u.SystemEvent,
                    Tenant = u.Tenant,
                    TenantDetails = u.TenantDetails,
                    Organisation = u.Organisation,
                    Product = product,
                    ProductDetails = product.Details,
                    FromRelationships = u.FromRelationships,
                    ToRelationships = u.ToRelationships,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Event.Relationships))))
            {
                query = query.GroupJoin(this.dbContext.Relationships, u => u.SystemEvent != null ? u.SystemEvent.Id : default, r => r.FromEntityId, (u, relationships) => new SystemEventWithRelatedEntities
                {
                    SystemEvent = u.SystemEvent,
                    Tenant = u.Tenant,
                    TenantDetails = u.TenantDetails,
                    Organisation = u.Organisation,
                    Product = u.Product,
                    ProductDetails = u.ProductDetails,
                    FromRelationships = relationships,
                    ToRelationships = u.ToRelationships,
                });

                query = query.GroupJoin(this.dbContext.Relationships, u => u.SystemEvent != null ? u.SystemEvent.Id : default, r => r.ToEntityId, (u, relationships) => new SystemEventWithRelatedEntities
                {
                    SystemEvent = u.SystemEvent,
                    Tenant = u.Tenant,
                    TenantDetails = u.TenantDetails,
                    Organisation = u.Organisation,
                    Product = u.Product,
                    ProductDetails = u.ProductDetails,
                    FromRelationships = u.FromRelationships,
                    ToRelationships = relationships,
                });
            }

            return query;
        }

        private class EventTagRelationshipModel
        {
            public EventTagRelationshipModel(SystemEvent @event, IEnumerable<Relationship> relationships, IEnumerable<Tag> tags)
            {
                this.Event = @event;
                this.Relationships = relationships;
                this.Tags = tags;
            }

            public SystemEvent Event { get; set; }

            public IEnumerable<Relationship> Relationships { get; set; }

            public IEnumerable<Tag> Tags { get; set; }
        }
    }
}
