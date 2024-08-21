// <copyright file="TagRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;

    public class TagRepository : ITagRepository
    {
        private readonly IUBindDbContext dbContext;

        public TagRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public bool Exists(Guid tenantId, Guid tagId)
        {
            return this.dbContext.Tags.SingleOrDefault(r => /*r.TenantId == tenantId && */r.Id == tagId) != default;
        }

        public void Insert(Tag tag)
        {
            this.dbContext.Tags.Add(tag);
        }

        public void AddRange(IEnumerable<Tag> tags)
        {
            this.dbContext.Tags.AddRange(tags);
        }
    }
}
