// <copyright file="JwtKeyRotator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services
{
    using System.Security.Cryptography;
    using System.Transactions;
    using Hangfire;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain.Extensions;
    using UBind.Domain.Models;
    using UBind.Domain.Repositories;

    public class JwtKeyRotator : IJwtKeyRotator
    {
        private readonly IJwtKeyRepository jwtKeyRepository;
        private readonly IClock clock;
        private readonly IUBindDbContext dbContext;
        private readonly IRecurringJobManager recurringJobManager;
        private readonly ILogger<JwtKeyRotator> logger;
        private readonly Duration rotateKeyAfterDuration = Duration.FromDays(90);
        private readonly Duration expireKeyAfterDuration = Duration.FromDays(180);
        private readonly int keySizeBits = 256;

        public JwtKeyRotator(
            IJwtKeyRepository jwtKeyRepository,
            IClock clock,
            IUBindDbContext dbContext,
            IRecurringJobManager recurringJobManager,
            ILogger<JwtKeyRotator> logger)
        {
            this.jwtKeyRepository = jwtKeyRepository;
            this.clock = clock;
            this.dbContext = dbContext;
            this.recurringJobManager = recurringJobManager;
            this.logger = logger;
        }

        /// <summary>
        /// This should be called from Startup.cs.
        /// </summary>
        public void CreateKeyRotationJob()
        {
            this.recurringJobManager.AddOrUpdate<JwtKeyRotator>(
                        "rotate-jwt-keys",
                        (x) => x.RotateKeys(),
                        "0 4 * * *"); // 4am daily
        }

        /// <summary>
        /// This algorithm ensures that after rotation there are always 2 keys active.
        /// </summary>
        public void RotateKeys()
        {
            var now = this.clock.Now();

            using (var transaction = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                this.dbContext.TransactionStack.Push(transaction);
                try
                {
                    var activeKeys = this.jwtKeyRepository.GetActiveKeys();

                    // rotate keys which need to be rotated
                    foreach (var activeKey in activeKeys)
                    {
                        if (!activeKey.IsRotated && activeKey.CreatedTimestamp + this.rotateKeyAfterDuration < now)
                        {
                            int ageDays = (int)(now - activeKey.CreatedTimestamp).TotalDays;
                            this.logger.LogInformation(
                                "Rotating JwtKey {KeyId} since it has aged {ageDays}",
                                activeKey.Id,
                                ageDays);
                            activeKey.IsRotated = true;
                        }
                    }

                    if (activeKeys.Count > 1)
                    {
                        // expire keys which need to be expired
                        var oldestActiveKey = activeKeys.OrderBy(x => x.CreatedTimestamp).First();
                        if (oldestActiveKey.CreatedTimestamp + this.expireKeyAfterDuration < now)
                        {
                            int ageDays = (int)(now - oldestActiveKey.CreatedTimestamp).TotalDays;
                            this.logger.LogInformation(
                                "Expiring JwtKey {KeyId} since it has aged {ageDays}",
                                oldestActiveKey.Id,
                                ageDays);
                            oldestActiveKey.IsExpired = true;
                            activeKeys.Remove(oldestActiveKey);
                        }
                    }

                    // generate new key if needed
                    if (activeKeys.Count == 0)
                    {
                        this.logger.LogInformation("Generating a JwtKey since there are none.");
                        activeKeys.Add(this.GenerateAndStoreNewKey());
                    }
                    else if (activeKeys.Count == 1)
                    {
                        var activeKey = activeKeys.Single();
                        if (activeKey.IsRotated)
                        {
                            this.logger.LogInformation("Adding a new JwtKey since the primary one was rotated.");
                            activeKeys.Add(this.GenerateAndStoreNewKey());
                        }
                    }
                    else if (activeKeys.Count > 2)
                    {
                        this.logger.LogWarning("There were more than 2 active JwtKeys found. This is suspicious. Please investigate.");
                    }

                    this.dbContext.SaveChanges();
                    transaction.Complete();
                }
                finally
                {
                    this.dbContext.TransactionStack.Pop();
                }
            }
        }

        private JwtKey GenerateAndStoreNewKey()
        {
            var newKey = this.GenerateNewKey();
            this.jwtKeyRepository.AddKey(newKey);
            return newKey;
        }

        private JwtKey GenerateNewKey()
        {
            string key;
            var randomNumber = new byte[this.keySizeBits / 8];
            RandomNumberGenerator.Fill(randomNumber);
            key = Convert.ToBase64String(randomNumber);
            return new JwtKey(Guid.NewGuid(), key, this.clock.Now());
        }
    }
}
