// <copyright file="UserSessionRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Redis.Repositories
{
    using System.Text.Json;
    using StackExchange.Redis;
    using UBind.Domain.Helpers;
    using UBind.Domain.Redis;
    using UBind.Domain.Repositories.Redis;
    using UBind.Persistence.Configuration;

    public class UserSessionRepository : RedisRepository, IUserSessionRepository
    {
        private readonly TimeSpan maxSessionDuration = TimeSpan.FromDays(90);

        public UserSessionRepository(
            IRedisConfiguration redisConfiguration,
            IConnectionMultiplexer connectionMultiplexer)
            : base(redisConfiguration, connectionMultiplexer)
        {
        }

        protected override string Prefix => "userSession:";

        public async Task Upsert(Guid tenantId, UserSessionModel userSessionModel)
        {
            var db = this.connectionMultiplexer.GetDatabase();
            var upsertTasks = new List<Task>();
            var sessionId = userSessionModel.Id.ToString();

            string json = JsonSerializer.Serialize(userSessionModel, SystemJsonHelper.GetSerializerOptions());
            string key = this.GetKey(tenantId, sessionId);
            upsertTasks.Add(db.StringSetAsync(key, json, this.maxSessionDuration));

            // also add the session ID by user ID, so if we need to lookup the session IDs for a given the user Id, we can do that
            string keyByUserId = this.GetSessionIdsByUserKey(tenantId, userSessionModel.UserId.ToString());
            upsertTasks.Add(db.SetAddAsync(keyByUserId, sessionId));

            // also add the session ID to a list of all session IDs, so if we need to delete all sessions for a given tenant, we can do that
            string keyAll = this.GetAllSessionIdsKey(tenantId);
            upsertTasks.Add(db.SetAddAsync(keyAll, sessionId));

            // and add the user ID to a list of all user IDs, so if we need to delete all session Ids per user for a given tenant, we can do that
            string keyAllUserIds = this.GetAllUserIdsKey(tenantId);
            upsertTasks.Add(db.SetAddAsync(keyAllUserIds, sessionId));

            if (userSessionModel.SamlSessionData != null)
            {
                // For SAML sessions, add the session ID by Issuer, NameID and SessionIndex, so we can terminate the session
                // when the user logs out of the IdP
                string keyBySamlSessionData = this.GetSessionIdsBySamlSessionDataKey(tenantId, userSessionModel.SamlSessionData);
                upsertTasks.Add(db.StringSetAsync(keyBySamlSessionData, sessionId, this.maxSessionDuration));
            }

            await Task.WhenAll(upsertTasks);
        }

        public async Task<UserSessionModel?> Get(Guid tenantId, Guid userSessionModelId)
        {
            string key = this.GetKey(tenantId, userSessionModelId.ToString());
            string? json = await this.connectionMultiplexer.GetDatabase().StringGetAsync(key);
            return string.IsNullOrEmpty(json)
                ? null
                : JsonSerializer.Deserialize<UserSessionModel>(json, SystemJsonHelper.GetSerializerOptions());
        }

        public async Task Delete(Guid tenantId, Guid userId, Guid userSessionId)
        {
            var db = this.connectionMultiplexer.GetDatabase();
            var deleteTasks = new List<Task>();

            string key = this.GetKey(tenantId, userSessionId.ToString());
            deleteTasks.Add(db.KeyDeleteAsync(key));

            // also remove the session ID from the user's list of session IDs
            string keyByUserId = this.GetSessionIdsByUserKey(tenantId, userId.ToString());
            deleteTasks.Add(db.SetRemoveAsync(keyByUserId, userSessionId.ToString()));

            await Task.WhenAll(deleteTasks);
        }

        public async Task<UserSessionModel?> DeleteSamlSession(Guid tenantId, SamlSessionData samlSessionData)
        {
            var db = this.connectionMultiplexer.GetDatabase();
            var deleteTasks = new List<Task>();

            string keyBySamlSessionData = this.GetSessionIdsBySamlSessionDataKey(tenantId, samlSessionData);
            string? sessionId = await db.StringGetAsync(keyBySamlSessionData);
            if (sessionId != null)
            {
                var userSessionModel = await this.Get(tenantId, Guid.Parse(sessionId));
                if (userSessionModel != null)
                {
                    deleteTasks.Add(this.Delete(tenantId, userSessionModel.UserId, userSessionModel.Id));
                }

                // also delete the key that maps the SAML session data to the session ID
                deleteTasks.Add(db.KeyDeleteAsync(keyBySamlSessionData));

                await Task.WhenAll(deleteTasks);
                return userSessionModel;
            }

            return null;
        }

        public async Task<bool> DeleteAllSessionsForUser(Guid tenantId, Guid userId)
        {
            var db = this.connectionMultiplexer.GetDatabase();
            var deleteTasks = new List<Task<bool>>();
            string keyByUserId = this.GetSessionIdsByUserKey(tenantId, userId.ToString());

            // get all sesssion IDs in the set
            RedisValue[] sessionIds = await db.SetMembersAsync(keyByUserId);
            foreach (var sessionId in sessionIds)
            {
                // delete each session
                string key = this.GetKey(tenantId, sessionId.ToString());
                deleteTasks.Add(db.KeyDeleteAsync(key));
            }

            // delete the set
            deleteTasks.Add(db.KeyDeleteAsync(keyByUserId));
            var results = await Task.WhenAll(deleteTasks);
            return results.Any(p => p);
        }

        public async Task<List<Guid>> DeleteAll(Guid tenantId)
        {
            var db = this.connectionMultiplexer.GetDatabase();
            var getUserSessionModelTasks = new List<Task<UserSessionModel?>>();
            var deleteTasks = new List<Task>();
            string allSessionIdsKey = this.GetAllSessionIdsKey(tenantId);
            RedisValue[] allSessionIds = await db.SetMembersAsync(allSessionIdsKey);
            deleteTasks.Add(db.KeyDeleteAsync(allSessionIdsKey));
            string allUserIdsKey = this.GetAllUserIdsKey(tenantId);
            RedisValue[] allUserIds = await db.SetMembersAsync(allUserIdsKey);
            deleteTasks.Add(db.KeyDeleteAsync(allUserIdsKey));

            foreach (var sessionId in allSessionIds)
            {
                getUserSessionModelTasks.Add(this.Get(tenantId, Guid.Parse(sessionId.ToString())));
                string key = this.GetKey(tenantId, sessionId.ToString());
                deleteTasks.Add(db.KeyDeleteAsync(key));
            }

            foreach (var userId in allUserIds)
            {
                string keyByUserId = this.GetSessionIdsByUserKey(tenantId, userId.ToString());
                deleteTasks.Add(db.KeyDeleteAsync(keyByUserId));
            }

            var deletedUserSessions = await Task.WhenAll(getUserSessionModelTasks);
            await Task.WhenAll(deleteTasks);

            return deletedUserSessions
                .Where(p => p != null)
                .Select(o => o.UserId)
                .Distinct()
                .ToList();
        }

        private string GetSessionIdsByUserKey(Guid tenantId, string userId)
        {
            return this.redisConfiguration.Prefix + "{" + tenantId.ToString() + "}:userSessionIdsByUser:" + userId;
        }

        private string GetSessionIdsBySamlSessionDataKey(Guid tenantId, SamlSessionData samlSessionData)
        {
            string key = this.redisConfiguration.Prefix + "{" + tenantId.ToString() + "}:userSessionIdsBySamlSessionData:"
                + samlSessionData.Issuer + ":" + samlSessionData.NameId;
            if (samlSessionData.SessionIndex != null)
            {
                key += ":" + samlSessionData.SessionIndex;
            }

            return key;
        }

        private string GetAllSessionIdsKey(Guid tenantId)
        {
            return this.redisConfiguration.Prefix + "{" + tenantId.ToString() + "}:userSessionIdsAll";
        }

        private string GetAllUserIdsKey(Guid tenantId)
        {
            return this.redisConfiguration.Prefix + "{" + tenantId.ToString() + "}:userSessionAllUserIds";
        }
    }
}
