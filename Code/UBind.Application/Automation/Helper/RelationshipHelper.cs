// <copyright file="RelationshipHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Automation.Helper;

using UBind.Application.Automation.Email;
using UBind.Application.Automation.Providers;
using UBind.Domain;
using UBind.Domain.Extensions;
using UBind.Domain.SerialisedEntitySchemaObject;

public class RelationshipHelper
{
    /// <summary>
    /// This method resolves the relationship with the provided context. If the relationship given is null,
    /// then it will iterate over the available context entities that matches the available message relationship type.
    /// </summary>
    /// <param name="providerContext"></param>
    /// <param name="relationships"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public static async Task<List<Entities.Relationship>?> ResolveMessageRelationships(
        IProviderContext providerContext,
        IEnumerable<RelationshipConfiguration>? relationships,
        Guid tenantId)
    {
        if (relationships == null)
        {
            return CreateDefaultMessageRelationships(providerContext, tenantId);
        }
        if (!relationships.Any())
        {
            return new List<Entities.Relationship>();
        }

        var resolveRelationships = await relationships
            .SelectAsync(async r => new
            {
                r.RelationshipType,
                SourceEntity = (await r.SourceEntity.ResolveValueIfNotNull(providerContext))?.DataValue,
                TargetEntity = (await r.TargetEntity.ResolveValueIfNotNull(providerContext))?.DataValue,
            });
        return resolveRelationships?.Select(r =>
            new Entities.Relationship(
                tenantId,
                r.RelationshipType,
                r.SourceEntity,
                r.TargetEntity)).ToList();
    }

    private static List<Entities.Relationship> CreateDefaultMessageRelationships(
         IProviderContext providerContext,
         Guid tenantId)
    {
        List<Entities.Relationship> messageRelationsips = new List<Entities.Relationship>();
        foreach (string contextEntityKey in providerContext.AutomationData.Context.Keys)
        {
            RelationshipType? relationshipType = GetMessageRelationshipType(contextEntityKey);
            if (relationshipType != null)
            {
                IEntity entity = (IEntity)providerContext.AutomationData.Context[contextEntityKey];
                var messageSourceAndTarget = GetMessageRelationshipSourceAndTarget((RelationshipType)relationshipType, entity);
                messageRelationsips.Add(new Entities.Relationship(
                    tenantId,
                    (RelationshipType)relationshipType,
                    messageSourceAndTarget.SourceEntity,
                    messageSourceAndTarget.TargetEntity));
            }
        }
        return messageRelationsips;
    }

    private static RelationshipType? GetMessageRelationshipType(string contextEntityKey)
    {
        if (contextEntityKey == "performingUser")
        {
            return RelationshipType.MessageSender;
        }
        EntityType? entityType = contextEntityKey.ToEnumOrNull<EntityType>();
        if (entityType != null)
        {
            switch (entityType)
            {
                case EntityType.Customer: return RelationshipType.CustomerMessage;
                case EntityType.Quote: return RelationshipType.QuoteMessage;
                case EntityType.QuoteVersion: return RelationshipType.QuoteVersionMessage;
                case EntityType.Organisation: return RelationshipType.OrganisationMessage;
                case EntityType.Policy: return RelationshipType.PolicyMessage;
                case EntityType.PolicyTransaction: return RelationshipType.PolicyTransactionMessage;
                case EntityType.Claim: return RelationshipType.ClaimMessage;
                case EntityType.ClaimVersion: return RelationshipType.ClaimVersionMessage;
                case EntityType.Report: return RelationshipType.ReportMessage;
                case EntityType.Product: return RelationshipType.ProductMessage;
                case EntityType.User: return RelationshipType.UserMessage;
            }
        }

        return null;
    }

    private static DefaultMessageSourceAndTarget GetMessageRelationshipSourceAndTarget(RelationshipType relationshipType, IEntity entity)
    {
        var targetRelationshipTypes = new RelationshipType[] {
                    RelationshipType.MessageSender,
                    RelationshipType.MessageRecipient,
                };
        if (targetRelationshipTypes.Any(x => x == relationshipType))
        {
            return new DefaultMessageSourceAndTarget(null, entity);
        }
        else
        {
            return new DefaultMessageSourceAndTarget(entity, null);
        }
    }

    public class DefaultMessageSourceAndTarget
    {
        public DefaultMessageSourceAndTarget(IEntity source, IEntity target)
        {
            this.SourceEntity = source;
            this.TargetEntity = target;
        }

        public IEntity SourceEntity { get; private set; }

        public IEntity TargetEntity { get; private set; }
    }
}
