// <copyright file="CreateUserSessionCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Commands.User
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates;
    using UBind.Domain.Events;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Redis;

    public class CreateUserSessionCommandHandler : ICommandHandler<CreateUserSessionCommand, UserSessionModel>
    {
        private readonly Domain.Services.IUserSessionService userSessionService;
        private readonly IUserSystemEventEmitter userSystemEventEmitter;
        private readonly IUserService userService;
        private readonly ICachingResolver cachingResolver;

        public CreateUserSessionCommandHandler(
            Domain.Services.IUserSessionService userSessionService,
            IUserSystemEventEmitter userSystemEventEmitter,
            IUserService userService,
            ICachingResolver cachingResolver)
        {
            this.userSessionService = userSessionService;
            this.userSystemEventEmitter = userSystemEventEmitter;
            this.userService = userService;
            this.cachingResolver = cachingResolver;
        }

        public async Task<UserSessionModel> Handle(CreateUserSessionCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var organisation = await this.cachingResolver.GetOrganisationOrThrow(command.User.TenantId, command.User.OrganisationId);
            var effectivePermissions = await this.userService.GetEffectivePermissions(command.User, organisation);
            var session = await this.userSessionService.Create(command.User, effectivePermissions);
            await this.userSystemEventEmitter.CreateAndEmitSystemEvents(
                command.User.TenantId,
                command.User.Id,
                new List<SystemEventType> { SystemEventType.UserLoginAttemptSucceeded },
                command.User.Id);
            return session;
        }
    }
}
