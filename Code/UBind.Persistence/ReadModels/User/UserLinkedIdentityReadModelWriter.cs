// <copyright file="UserLinkedIdentityReadModelWriter.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels.User
{
    using UBind.Domain.Aggregates;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.Repositories;

    public class UserLinkedIdentityReadModelWriter : IUserLinkedIdentityReadModelWriter
    {
        private readonly IUBindDbContext dbContext;
        private readonly IAuthenticationMethodReadModelRepository authenticationMethodRepository;

        public UserLinkedIdentityReadModelWriter(
            IUBindDbContext dbContext,
            IAuthenticationMethodReadModelRepository authenticationMethodRepository)
        {
            this.dbContext = dbContext;
            this.authenticationMethodRepository = authenticationMethodRepository;
        }

        public void Dispatch(
            UserAggregate aggregate,
            IEvent<UserAggregate, Guid> @event,
            int sequenceNumber,
            IEnumerable<Type> observerTypes = null)
        {
            this.DispatchIfHandlerExists(aggregate, @event, sequenceNumber);
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserIdentityLinkedEvent @event, int sequenceNumber)
        {
            var authenticationMethod = this.authenticationMethodRepository.Get(@event.TenantId, @event.AuthenticationMethodId);
            var readModel = new UserLinkedIdentityReadModel
            {
                TenantId = @event.TenantId,
                UserId = @event.AggregateId,
                AuthenticationMethodId = @event.AuthenticationMethodId,
                AuthenticationMethodName = authenticationMethod.Name,
                AuthenticationMethodTypeName = authenticationMethod.TypeName,
                UniqueId = @event.UniqueId,
            };
            this.dbContext.UserLinkedIdentities.Add(readModel);
        }

        public void Handle(UserAggregate aggregate, UserAggregate.UserIdentityUnlinkedEvent @event, int sequenceNumber)
        {
            var readModel = this.dbContext.UserLinkedIdentities.Find(@event.AggregateId, @event.AuthenticationMethodId);
            if (readModel != null)
            {
                this.dbContext.UserLinkedIdentities.Remove(readModel);
            }
        }
    }
}
