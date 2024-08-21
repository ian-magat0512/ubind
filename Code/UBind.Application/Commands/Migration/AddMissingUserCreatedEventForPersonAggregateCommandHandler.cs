// <copyright file="AddMissingUserCreatedEventForPersonAggregateCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration;

using Dapper;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using UBind.Domain;
using UBind.Domain.Aggregates.Person;
using UBind.Domain.Aggregates.User;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Persistence;

// <summary>
// This command handler is used to add UserCreatedEvents for PersonAggregates
// that doesnt have userId where user exists.
// </summary>
public class AddMissingUserCreatedEventForPersonAggregateCommandHandler
    : ICommandHandler<AddMissingUserCreatedEventForPersonAggregateCommand, Unit>
{
    private const string TemporaryTableName = "TempUsersProcessed";
    private const int BatchSize = 100;
    private readonly ILogger<AddMissingUserCreatedEventForPersonAggregateCommandHandler> logger;
    private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
    private readonly IPersonAggregateRepository personAggregateRepository;
    private readonly IUserAggregateRepository userAggregateRepository;
    private readonly IConnectionConfiguration connection;
    private readonly IClock clock;
    private Guid? performingUserId;

    public AddMissingUserCreatedEventForPersonAggregateCommandHandler(
        ILogger<AddMissingUserCreatedEventForPersonAggregateCommandHandler> logger,
        IHttpContextPropertiesResolver httpContextPropertiesResolver,
        IPersonAggregateRepository personAggregateRepository,
        IUserAggregateRepository userAggregateRepository,
        IConnectionConfiguration connection,
        IClock clock)
    {
        this.logger = logger;
        this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        this.personAggregateRepository = personAggregateRepository;
        this.userAggregateRepository = userAggregateRepository;
        this.clock = clock;
        this.connection = connection;
    }

    [JobDisplayName("Add Missing User Created Events to Person Aggregates")]
    public async Task<Unit> Handle(AddMissingUserCreatedEventForPersonAggregateCommand request, CancellationToken cancellationToken)
    {
        this.performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
        await this.CreateTemporaryTable(cancellationToken);
        await this.ProcessPersonsWithMissingUserId(cancellationToken);
        await this.DropTemporaryTable(cancellationToken);
        return Unit.Value;
    }

    private async Task ProcessPersonsWithMissingUserId(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var processedCount = BatchSize;
        var totalProcessed = 0;
        while (processedCount == BatchSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var usersToProcess = await this.GetUsersToProcess(cancellationToken);
            var updatedUsers = await this.UpdatePersonAggregates(usersToProcess, cancellationToken);
            await this.InsertRecords(updatedUsers, cancellationToken);
            processedCount = usersToProcess.Count;
            totalProcessed = updatedUsers.Count;
        }

        this.logger.LogInformation($"Updated {totalProcessed} records.");
    }

    private async Task<List<UserPersonModel>> GetUsersToProcess(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (var connection = new SqlConnection(this.connection.UBind))
        {
            connection.Open();
            string sql =
                @$"SELECT TOP (@BatchSize) u.TenantId[TenantId], u.Id[UserId], p.Id[PersonId]
                        FROM UserReadModels u
                        INNER JOIN PersonReadModels p ON p.Id = u.PersonId and p.TenantId = u.TenantId
                        LEFT JOIN {TemporaryTableName} AS tb ON tb.Id = u.Id
                        WHERE tb.Id IS NULL AND p.UserId IS NULL";

            var command = new CommandDefinition(
                sql,
                parameters: new { BatchSize = BatchSize },
                commandTimeout: 180,
                commandType: System.Data.CommandType.Text,
                cancellationToken: cancellationToken,
                flags: CommandFlags.Buffered); ;
            return (await connection.QueryAsync<UserPersonModel>(command)).ToList();
        }
    }

    private async Task CreateTemporaryTable(CancellationToken cancellationToken)
    {
        using (var connection = new SqlConnection(this.connection.UBind))
        {
            connection.Open();
            string checkTableQuery = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{TemporaryTableName}'";
            var command = new CommandDefinition(checkTableQuery, cancellationToken: cancellationToken);
            int tableCount = await connection.ExecuteScalarAsync<int>(command);
            if (tableCount == 0)
            {
                string createTableQuery = $"CREATE TABLE {TemporaryTableName} (Id UNIQUEIDENTIFIER PRIMARY KEY)";
                command = new CommandDefinition(createTableQuery, cancellationToken: cancellationToken);
                await connection.ExecuteAsync(command);
            }
        }
    }

    private async Task InsertRecords(List<Guid> ids, CancellationToken cancellationToken)
    {
        using (var connection = new SqlConnection(this.connection.UBind))
        {
            connection.Open();
            string insertRecordQuery = $"INSERT INTO {TemporaryTableName} (Id) VALUES (@RecordId)";
            var parameters = ids.Select(pId => new { RecordId = pId });
            var command = new CommandDefinition(insertRecordQuery, parameters: parameters, cancellationToken: cancellationToken);
            await connection.ExecuteAsync(command);
        }
    }

    private async Task DropTemporaryTable(CancellationToken cancellationToken)
    {
        using (var connection = new SqlConnection(this.connection.UBind))
        {
            connection.Open();
            string dropTableQuery = $"DROP TABLE {TemporaryTableName}";
            var command = new CommandDefinition(dropTableQuery, cancellationToken: cancellationToken);
            await connection.ExecuteAsync(command);
        }
    }

    private async Task<List<Guid>> UpdatePersonAggregates(List<UserPersonModel> users, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        List<Guid> userProcessed = new List<Guid>();
        foreach (var user in users)
        {
            async Task ProcessUser(CancellationToken cancellationToken)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                try
                {
                    bool save = false;
                    var personAggregate = this.personAggregateRepository.GetById(user.TenantId, user.PersonId);
                    if (personAggregate == null)
                    {
                        return;
                    }

                    if (!personAggregate.UserId.HasValue)
                    {
                        var userAggregate = this.userAggregateRepository.GetById(user.TenantId, user.UserId);
                        if (userAggregate == null)
                        {
                            // this should not happen. Log warning.
                            this.logger.LogWarning($"User {user.UserId} exist but aggregate does not exist. Please check.");
                            return;
                        }

                        this.logger.LogInformation($"Assigning UserId {user.UserId} to Person {personAggregate.Id}");
                        personAggregate.RecordUserAccountCreatedForPerson(
                            user.UserId, this.performingUserId, this.clock.GetCurrentInstant());
                        save = true;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    if (save)
                    {
                        await this.personAggregateRepository.Save(personAggregate);
                        userProcessed.Add(user.UserId);
                        await Task.Delay(1000, cancellationToken);
                    }
                }
                catch (Exception e)
                {
                    this.logger.LogInformation(
                        $"Error for Person: {user.PersonId} ErrorMessage: {e.Message}");
                    throw;
                }
            }
            await RetryPolicyHelper.ExecuteAsync<Exception>(() => ProcessUser(cancellationToken));
            await Task.Delay(1000, cancellationToken);
        }
        return userProcessed;
    }

    private class UserPersonModel
    {
        public Guid TenantId { get; set; }

        public Guid UserId { get; set; }

        public Guid PersonId { get; set; }
    }
}
