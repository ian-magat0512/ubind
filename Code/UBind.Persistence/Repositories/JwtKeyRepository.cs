// <copyright file="JwtKeyRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Repositories
{
    using System.Collections.Generic;
    using UBind.Domain.Models;
    using UBind.Domain.Repositories;

    public class JwtKeyRepository : IJwtKeyRepository
    {
        private readonly IUBindDbContext dbContext;

        public JwtKeyRepository(IUBindDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void AddKey(JwtKey jwtKey)
        {
            this.dbContext.JwtKeys.Add(jwtKey);
        }

        public List<JwtKey> GetActiveKeys()
        {
            return this.dbContext.JwtKeys.Where(j => !j.IsExpired).ToList();
        }
    }
}
