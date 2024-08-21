// <copyright file="EventRecordRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence
{
    using System.Data.SqlClient;
    using Dapper;
    using Newtonsoft.Json;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Repositories;

    /// <inheritdoc/>
    public class EventRecordRepository : IEventRecordRepository
    {
        private readonly IUBindDbContext dbContext;
        private readonly IConnectionConfiguration connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductOrganisationSettingRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public EventRecordRepository(
            IUBindDbContext dbContext,
            IConnectionConfiguration connection)
        {
            this.dbContext = dbContext;
            this.connection = connection;
        }

        /// <inheritdoc/>
        public void UpdateEventRecord<TEvent>(
            Guid tenantId,
            Guid aggregateId,
            AggregateType aggregateType,
            int sequenceNo,
            TEvent @event)
        {
            var eventRecord = this.dbContext.EventRecordsWithGuidIds.FirstOrDefault(
                a => a.TenantId == tenantId
                && a.AggregateId == aggregateId
                && a.Sequence == sequenceNo
                && a.AggregateType == aggregateType);
            if (eventRecord == null)
            {
                throw new ErrorException(Domain.Errors.General.NotFound("event record", aggregateId));
            }

            var eventJson = JsonConvert.SerializeObject(@event, CustomSerializerSetting.AggregateEventSerializerSettings);
            eventRecord.EventJson = eventJson;
        }

        public IEnumerable<TEventRecord> GetEventRecords<TEventRecord, TId>(
            Guid tenantId,
            TId aggregateId)
        {
            using (var connection = new SqlConnection(this.connection.UBind))
            {
                var tableName = typeof(TEventRecord).Name;
                var parameters = new DynamicParameters();
                parameters.Add("@AggregateId", aggregateId);
                parameters.Add("@TenantId", tenantId);

                string sqlQuery = $@"{this.GetEventRecordQuery(tableName)}  
                                    ORDER BY Sequence";

                return connection.Query<TEventRecord>(sqlQuery, parameters, null, true, 180, System.Data.CommandType.Text);
            }
        }

        public async Task<IEnumerable<TEventRecord>> GetEventRecordsAsync<TEventRecord, TId>(
            Guid tenantId,
            TId aggregateId)
        {
            using (var connection = new SqlConnection(this.connection.UBind))
            {
                var tableName = typeof(TEventRecord).Name;
                var parameters = new DynamicParameters();
                parameters.Add("@AggregateId", aggregateId);
                parameters.Add("@TenantId", tenantId);

                string sqlQuery = $@"{this.GetEventRecordQuery(tableName)}  
                                    ORDER BY Sequence";

                return await connection.QueryAsync<TEventRecord>(sqlQuery, parameters, null, 180, System.Data.CommandType.Text);
            }
        }

        public IEnumerable<TEventRecord> GetEventRecordsAfterSequence<TEventRecord, TId>(
            Guid tenantId,
            TId aggregateId,
            int sequenceNumber)
        {
            using (var connection = new SqlConnection(this.connection.UBind))
            {
                var tableName = typeof(TEventRecord).Name;
                var parameters = new DynamicParameters();
                parameters.Add("@AggregateId", aggregateId);
                parameters.Add("@TenantId", tenantId);
                parameters.Add("@Sequence", sequenceNumber);

                string sqlQuery = $@"{this.GetEventRecordQuery(tableName)}  
                                    AND Sequence > @Sequence     
                                    ORDER BY Sequence";

                return connection.Query<TEventRecord>(sqlQuery, parameters, null, true, 180, System.Data.CommandType.Text);
            }
        }

        public async Task<IEnumerable<TEventRecord>> GetEventRecordsAfterSequenceAsync<TEventRecord, TId>(
            Guid tenantId,
            TId aggregateId,
            int sequenceNumber)
        {
            using (var connection = new SqlConnection(this.connection.UBind))
            {
                var tableName = typeof(TEventRecord).Name;
                var parameters = new DynamicParameters();
                parameters.Add("@AggregateId", aggregateId);
                parameters.Add("@TenantId", tenantId);
                parameters.Add("@Sequence", sequenceNumber);

                string sqlQuery = $@"{this.GetEventRecordQuery(tableName)}  
                                    AND Sequence > @Sequence     
                                    ORDER BY Sequence";

                return await connection.QueryAsync<TEventRecord>(sqlQuery, parameters, null, 180, System.Data.CommandType.Text);
            }
        }

        public async Task<IEnumerable<TEventRecord>> GetEventRecordsAtSequence<TEventRecord, TId>(
            Guid tenantId,
            TId aggregateId,
            int sequenceNumber,
            int? version)
        {
            using (var connection = new SqlConnection(this.connection.UBind))
            {
                var tableName = typeof(TEventRecord).Name;
                var parameters = new DynamicParameters();
                parameters.Add("@AggregateId", aggregateId);
                parameters.Add("@TenantId", tenantId);
                parameters.Add("@Sequence", sequenceNumber);

                var additionalClause = string.Empty;
                if (version != null)
                {
                    additionalClause = "AND Sequence > @Version ";
                    parameters.Add("@Version", version);
                }

                string sqlQuery = $@"{this.GetEventRecordQuery(tableName)}  
                                    {additionalClause} AND Sequence <= @Sequence     
                                    ORDER BY Sequence";

                return await connection.QueryAsync<TEventRecord>(sqlQuery, parameters, null, 180, System.Data.CommandType.Text);
            }
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }

        private string GetEventRecordQuery(string tableName)
        {
            // Note: We can't parameterize the table name directly in dapper
            // because it would allow for SQL injection vulnerabilities if not handled correctly.
            // Therefore, we have to use string interpolation.
            // This is safe because the table name is not user input.
            return $@"SELECT     
                        AggregateId,    
                        TenantId,   
                        AggregateType,       
                        Sequence,   
                        EventJson,  
                        TicksSinceEpoch 
                    FROM {tableName}s  
                    WHERE AggregateId = @AggregateId AND TenantId = @TenantId  ";
        }
    }
}
