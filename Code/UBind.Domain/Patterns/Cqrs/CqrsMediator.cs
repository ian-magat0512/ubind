// <copyright file="CqrsMediator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Patterns.Cqrs;

using System;
using System.ComponentModel;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Hangfire;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Contrib.WaitAndRetry;
using StackExchange.Profiling;
using StackExchange.Redis;
using UBind.Application.Services.Email;
using UBind.Domain.Attributes;
using UBind.Domain.Exceptions;
using UBind.Domain.Helpers;
using UBind.Domain.Repositories;

/// <inheritdoc/>
public class CqrsMediator : ICqrsMediator
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<CqrsMediator> logger;
    private readonly IErrorNotificationService errorNotificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CqrsMediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provide.</param>
    public CqrsMediator(IServiceProvider serviceProvider, ILogger<CqrsMediator> logger, IErrorNotificationService errorNotificationService)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
        this.errorNotificationService = errorNotificationService;
    }

    public async Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (MiniProfiler.Current.Step(nameof(CqrsMediator) + "." + nameof(this.Publish) + " " + notification.GetType().Name))
        {
            var commandContext = this.serviceProvider.GetRequiredService<ICqrsRequestContext>();
            if (!commandContext.RequestScopeCreated)
            {
                using (var serviceScope = this.serviceProvider.CreateScope())
                {
                    var provider = serviceScope.ServiceProvider;
                    commandContext = provider.GetRequiredService<ICqrsRequestContext>();
                    commandContext.RequestScopeCreated = true;
                    var mediatorService = provider.GetRequiredService<IMediator>();
                    await mediatorService.Publish(notification, cancellationToken);
                }
            }
            else
            {
                var mediatorService = this.serviceProvider.GetRequiredService<IMediator>();
                await mediatorService.Publish(notification, cancellationToken);
            }
        }
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        cancellationToken.ThrowIfCancellationRequested();
        await this.Publish((object)notification, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task Send(ICommand command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await this.Send<Unit>(command, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (MiniProfiler.Current.Step(nameof(CqrsMediator) + "." + nameof(this.Send) + " " + command.GetType().Name))
        {
            var customAttributes = Attribute.GetCustomAttributes(command.GetType());
            RetryOnDbExceptionAttribute dbRetryAttribute = customAttributes.FirstOrDefault(attribute
                => attribute.GetType().IsAssignableFrom(typeof(RetryOnDbExceptionAttribute))) as RetryOnDbExceptionAttribute;
            if (dbRetryAttribute == null)
            {
                dbRetryAttribute = new RetryOnDbExceptionAttribute(5);
            }

            var maxDelay = TimeSpan.FromMilliseconds(dbRetryAttribute.MaxDelayMilliseconds);
            var delay = Backoff.DecorrelatedJitterBackoffV2(
                    medianFirstRetryDelay: TimeSpan.FromSeconds(dbRetryAttribute.MedianFirstRetryDelayMilliseconds),
                    retryCount: dbRetryAttribute.MaxRetries)
                .Select(s => TimeSpan.FromTicks(Math.Min(s.Ticks, maxDelay.Ticks)));

            var retryPolicy = Policy
                .Handle<ConcurrencyException>()
                .Or<EntityException>()
                .Or<DbUpdateException>()
                .Or<SqlException>()
                .Or<EntityCommandExecutionException>()
                .Or<Win32Exception>()
                .Or<RedisConnectionException>()
                .Or<BackgroundJobClientException>()
                .WaitAndRetryAsync(delay, (exception, timeSpan, retryCount, context) =>
                {
                    if (retryCount == 1 && (exception is RedisConnectionException || exception is BackgroundJobClientException))
                    {
                        this.errorNotificationService.CaptureSentryException(exception, null);
                    }
                    this.logger.LogError($"Retrying command {command.GetType().Name} with retry count {retryCount} "
                        + $"due to {exception.GetType().Name}: {exception.GetAllMessages()}");
                });
            var createTransactionAndSavesChangesIfNoneExists = customAttributes.Any(attribute => attribute.GetType()
                .IsAssignableFrom(typeof(CreateTransactionThatSavesChangesIfNoneExistsAttribute)));
            var requestIntentAttribute = customAttributes.FirstOrDefault(attribute => attribute.GetType()
                .IsAssignableFrom(typeof(RequestIntentAttribute))) as RequestIntentAttribute;
            RequestIntent requestIntent = RequestIntent.ReadWrite;
            if (requestIntentAttribute != null)
            {
                requestIntent = requestIntentAttribute.RequestIntent;
            }

            var pollyResult = await retryPolicy.ExecuteAsync(async () =>
            {
                return await this.ExecuteCommand<TResponse>(
                    command,
                    cancellationToken,
                    createTransactionAndSavesChangesIfNoneExists,
                    requestIntent);
            });
            return pollyResult;
        }
    }

    public async Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using (MiniProfiler.Current.Step(nameof(CqrsMediator) + "." + nameof(this.Send) + " " + query.GetType().Name))
        {
            var customAttributes = Attribute.GetCustomAttributes(query.GetType());
            RetryOnDbExceptionAttribute dbRetryAttribute = customAttributes.FirstOrDefault(attribute
                => attribute.GetType().IsAssignableFrom(typeof(RetryOnDbExceptionAttribute))) as RetryOnDbExceptionAttribute;
            if (dbRetryAttribute == null)
            {
                dbRetryAttribute = new RetryOnDbExceptionAttribute(5);
            }

            var maxDelay = TimeSpan.FromMilliseconds(dbRetryAttribute.MaxDelayMilliseconds);
            var delay = Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromSeconds(dbRetryAttribute.MedianFirstRetryDelayMilliseconds),
            retryCount: dbRetryAttribute.MaxRetries)
            .Select(s => TimeSpan.FromTicks(Math.Min(s.Ticks, maxDelay.Ticks)));

            var retryPolicy = Policy
                .Handle<ConcurrencyException>()
                .Or<EntityException>()
                .Or<RedisConnectionException>()
                .Or<BackgroundJobClientException>()
                .WaitAndRetryAsync(delay, (exception, timeSpan, retryCount, context) =>
                {
                    if (retryCount == 1 && (exception is RedisConnectionException || exception is BackgroundJobClientException))
                    {
                        this.errorNotificationService.CaptureSentryException(exception, null);
                    }
                    this.logger.LogError($"Retrying query {query.GetType().Name} with retry count {retryCount} "
                        + $"due to {exception.GetType().Name}: {exception.GetAllMessages()}");
                });

            var pollyResult = await retryPolicy.ExecuteAsync(async () =>
                {
                    return await this.ExecuteQuery<TResponse>(query, cancellationToken);
                });
            return pollyResult;
        }
    }

    private async Task<TResponse> ExecuteCommand<TResponse>(
        ICommand<TResponse> command,
        CancellationToken token,
        bool createTransactionAndSaveChangesIfNoneExists,
        RequestIntent requestIntent = RequestIntent.ReadWrite)
    {
        token.ThrowIfCancellationRequested();
        var requestContext = this.serviceProvider.GetRequiredService<ICqrsRequestContext>();
        if (!requestContext.RequestScopeCreated)
        {
            using (var serviceScope = this.serviceProvider.CreateScope())
            {
                var provider = serviceScope.ServiceProvider;
                var innerRequestContext = provider.GetRequiredService<ICqrsRequestContext>();
                innerRequestContext.RequestScopeCreated = true;
                innerRequestContext.RequestIntent = requestIntent;
                var mediatorService = provider.GetRequiredService<IMediator>();
                var dbContext = provider.GetRequiredService<IUBindDbContext>();

                if (createTransactionAndSaveChangesIfNoneExists && !dbContext.HasTransaction())
                {
                    return await this.ExecuteCommandInTransactionAndSaveDbChanges(command, dbContext, mediatorService, token);
                }
                else
                {
                    return await mediatorService.Send(command, token);
                }
            }
        }
        else if (requestContext.RequestIntent == RequestIntent.ReadOnly && requestIntent == RequestIntent.ReadWrite)
        {
            throw new InvalidOperationException("An attempt was made to execute a command using an existing "
                + "request scope that was created with a RequestIntent of ReadOnly. That means it's likely that "
                + "someone has done something silly, like invoking a CQRS command from within a CQRS query. "
                + "A query should only be used to read data from the database, so you should not invoke a command "
                + "within a query unless that command is attributed with a read-only request intent.");
        }
        else
        {
            var mediatorService = this.serviceProvider.GetRequiredService<IMediator>();
            var dbContext = this.serviceProvider.GetRequiredService<IUBindDbContext>();
            if (createTransactionAndSaveChangesIfNoneExists && !dbContext.HasTransaction())
            {
                return await this.ExecuteCommandInTransactionAndSaveDbChanges(command, dbContext, mediatorService, token);
            }
            else
            {
                return await mediatorService.Send(command, token);
            }
        }
    }

    private async Task<TResponse> ExecuteCommandInTransactionAndSaveDbChanges<TResponse>(
        ICommand<TResponse> command,
        IUBindDbContext dbContext,
        IMediator mediatorService,
        CancellationToken token)
    {
        using (var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled))
        {
            dbContext.TransactionStack.Push(transaction);
            try
            {
                var response = await mediatorService.Send(command, token);
                dbContext.SaveChanges();
                transaction.Complete();
                return response;
            }
            finally
            {
                dbContext.TransactionStack.Pop();
            }
        }
    }

    private async Task<TResponse> ExecuteQuery<TResponse>(IQuery<TResponse> query, CancellationToken token)
    {
        var mediatorService = this.serviceProvider.GetRequiredService<IMediator>();
        return await mediatorService.Send(query, token);
    }
}
