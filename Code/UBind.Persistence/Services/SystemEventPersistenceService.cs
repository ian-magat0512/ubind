// <copyright file="SystemEventPersistenceService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Services
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using UBind.Domain.Events;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Persistence.Extensions;

    /// <summary>
    /// This is needed to persist missing relationships and tags which are created on a deferred basis
    /// during system event creation.
    /// </summary>
    public class SystemEventPersistenceService : ISystemEventPersistenceService
    {
        private readonly IUBindDbContext dbContext;
        private readonly IRelationshipRepository relationshipRepository;
        private readonly ITagRepository tagRepository;
        private readonly ISystemEventRepository systemEventRepository;

        public SystemEventPersistenceService(
            IUBindDbContext dbContext,
            IRelationshipRepository relationshipRepository,
            ITagRepository tagRepository,
            ISystemEventRepository systemEventRepository)
        {
            this.dbContext = dbContext;
            this.relationshipRepository = relationshipRepository;
            this.tagRepository = tagRepository;
            this.systemEventRepository = systemEventRepository;
        }

        public void Persist(IEnumerable<SystemEvent> systemEvents)
        {
            try
            {
                this.systemEventRepository.AddRange(systemEvents);
                this.dbContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                if (!ex.IsUniqueIndexViolation() && !ex.IsDuplicateKeyException())
                {
                    throw;
                }

                // Detach the entities that already exist so we can just the missing ones.
                var entries = this.dbContext.ChangeTracker.Entries();
                foreach (var entry in entries)
                {
                    if (entry.Entity is SystemEvent systemEvent)
                    {
                        if (this.systemEventRepository.Exists(systemEvent.Id))
                        {
                            // detach it so it won't be saved, since it already exists.
                            entry.State = EntityState.Detached;
                        }
                    }
                }

                // let's try saving again - this time it should not error due to duplicate keys.
                this.dbContext.SaveChanges();
            }
        }

        public void Persist(IEnumerable<Relationship> relationships, IEnumerable<Tag> tags)
        {
            try
            {
                bool hasRelationships = relationships.Any();
                bool hasTags = tags.Any();

                // insert relationships and tags
                if (hasRelationships)
                {
                    this.relationshipRepository.AddRange(relationships);
                }

                if (hasTags)
                {
                    this.tagRepository.AddRange(tags);
                }

                // Store the relationships and tags we just added
                if (hasRelationships || hasTags)
                {
                    this.dbContext.SaveChanges();
                }
            }
            catch (DbUpdateException)
            {
                // Detach the entities that already exist so we can just the missing ones.
                var entries = this.dbContext.ChangeTracker.Entries();
                foreach (var entry in entries)
                {
                    if (entry.Entity is Relationship relationship)
                    {
                        if (this.relationshipRepository.Exists(relationship.TenantId, relationship.Id))
                        {
                            // detach it so it won't be saved, since it already exists.
                            entry.State = EntityState.Detached;
                        }
                    }
                    else if (entry.Entity is Tag tag)
                    {
                        if (this.tagRepository.Exists(/*tag.TenantId*/default, tag.Id))
                        {
                            // detach it so it won't be saved, since it already exists.
                            entry.State = EntityState.Detached;
                        }
                    }
                    else if (entry.Entity is SystemEvent systemEvent && this.systemEventRepository.Exists(systemEvent.TenantId, systemEvent.Id))
                    {
                        // detach it so it won't be saved, since it already exists.
                        entry.State = EntityState.Detached;
                    }
                }

                // let's try saving again - this time it should not error due to duplicate keys.
                this.dbContext.SaveChanges();
            }
        }
    }
}
