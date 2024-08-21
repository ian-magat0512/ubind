// <copyright file="SaveSmsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Sms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;

    public class SaveSmsCommandHandler : ICommandHandler<SaveSmsCommand>
    {
        private readonly ISmsRepository smsRepository;
        private readonly IClock clock;
        private readonly ICachingResolver resolver;

        public SaveSmsCommandHandler(
            ISmsRepository smsRepository,
            IClock clock,
            ICachingResolver resolver)
        {
            this.smsRepository = smsRepository;
            this.clock = clock;
            this.resolver = resolver;
        }

        public async Task<Unit> Handle(SaveSmsCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var timestamp = this.clock.Now();

            foreach (var to in request.To)
            {
                var sms = new Sms(
                    request.TenantId,
                    request.OrganisationId,
                    request.ProductId,
                    Guid.NewGuid(),
                    to.ToString(),
                    request.From.ToString(),
                    request.Message,
                    timestamp);
                this.smsRepository.Insert(sms);

                if (request.Relationships != null && request.Relationships.Any())
                {
                    foreach (var relationship in request.Relationships)
                    {
                        var targetEntity = relationship.TargetEntity;
                        var sourceEntity = relationship.SourceEntity;
                        var relationshipType = relationship.RelationshipType;

                        if (targetEntity != null)
                        {
                            var entityType = (EntityType)Enum.Parse(typeof(EntityType), targetEntity.GetType().Name);
                            var entityId = await this.ResolveEntityId(sms, targetEntity, entityType);

                            this.CreateRelationshipFromSmsToEntity(sms, relationshipType, entityType, entityId);
                        }

                        if (sourceEntity != null)
                        {
                            var entityType = (EntityType)Enum.Parse(typeof(EntityType), sourceEntity.GetType().Name);
                            var entityId = await this.ResolveEntityId(sms, sourceEntity, entityType);

                            this.CreateRelationshipFromEntityToSms(sms, relationshipType, entityType, entityId);
                        }
                    }
                }

                this.CreateUserDefinedTags(sms, request.Tags);
                this.CreatTagFromEnvironment(sms, request.Environment);
            }

            return Unit.Value;
        }

        private void CreateRelationshipFromSmsToEntity(Sms sms, RelationshipType relationshipType, EntityType entityType, Guid entityId)
        {
            var relationship = new Relationship(sms.TenantId, EntityType.Message, sms.Id, relationshipType, entityType, entityId, this.clock.Now());
            this.smsRepository.InsertSmsRelationship(relationship);
        }

        private void CreateRelationshipFromEntityToSms(Sms sms, RelationshipType relationshipType, EntityType entityType, Guid entityId)
        {
            var relationship = new Relationship(sms.TenantId, entityType, entityId, relationshipType, EntityType.Message, sms.Id, this.clock.Now());
            this.smsRepository.InsertSmsRelationship(relationship);
        }

        private void CreateUserDefinedTags(Sms sms, IEnumerable<string> tags)
        {
            if (tags != null && tags.Any())
            {
                foreach (var tag in tags)
                {
                    if (tag.IsNotNullOrEmpty())
                    {
                        var tagEntity = new Tag(EntityType.Message, sms.Id, TagType.UserDefined, tag, this.clock.Now());
                        this.smsRepository.InsertSmsTag(tagEntity);
                    }
                }
            }
        }

        private void CreatTagFromEnvironment(Sms sms, DeploymentEnvironment? environment)
        {
            if (environment.HasValue)
            {
                var tag = new Tag(EntityType.Message, sms.Id, TagType.Environment, environment.Value.ToString(), this.clock.Now());
                this.smsRepository.InsertSmsTag(tag);
            }
        }

        private async Task<Guid> ResolveEntityId(Sms sms, Domain.SerialisedEntitySchemaObject.IEntity sourceEntity, EntityType entityType)
        {
            if (entityType == EntityType.Product)
            {
                var product = await this.resolver.GetProductOrThrow(sms.TenantId, sourceEntity.Id);
                return product.Id;
            }
            else if (entityType == EntityType.Tenant)
            {
                var tenant = await this.resolver.GetTenantOrThrow(sourceEntity.Id);
                return tenant.Id;
            }
            else
            {
                return sourceEntity.Id;
            }
        }
    }
}
