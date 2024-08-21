// <copyright file="CachedJwtKeyService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using UBind.Domain.Helpers;
    using UBind.Domain.Models;
    using UBind.Domain.Repositories;

    public class CachedJwtKeyService : ICachedJwtKeyService
    {
        private static int cacheInHours = 24;
        private readonly DateTimeOffset cacheDuration = DateTimeOffset.Now.AddHours(cacheInHours);
        private readonly IJwtKeyRepository jwtKeyRepository;
        private readonly string key = "CachedJwtKeyService:activeKeys";

        public CachedJwtKeyService(IJwtKeyRepository jwtKeyRepository)
        {
            this.jwtKeyRepository = jwtKeyRepository;
        }

        public List<JwtKey> GetActiveKeys()
        {
            List<JwtKey> keys = MemoryCachingHelper.AddOrGet(
                this.key,
                () => this.jwtKeyRepository.GetActiveKeys(),
                this.cacheDuration);
            return keys;
        }

        public JwtKey GetLatestKey()
        {
            return this.GetActiveKeys().OrderByDescending(x => x.CreatedTimestamp).First();
        }
    }
}
