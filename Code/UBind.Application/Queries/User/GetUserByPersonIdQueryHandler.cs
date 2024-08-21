// <copyright file="GetUserByPersonIdQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.User
{
    using System.Threading;
    using System.Threading.Tasks;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;

    public class GetUserByPersonIdQueryHandler : IQueryHandler<GetUserByPersonIdQuery, UserReadModel>
    {
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly IUserReadModelRepository userReadModelRepository;

        public GetUserByPersonIdQueryHandler(
            IPersonReadModelRepository personReadModelRepository,
            IUserReadModelRepository userReadModelRepository)
        {
            this.personReadModelRepository = personReadModelRepository;
            this.userReadModelRepository = userReadModelRepository;
        }

        public Task<UserReadModel> Handle(GetUserByPersonIdQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var person = this.personReadModelRepository.GetPersonById(request.TenantId, request.PersonId);
            if (person == null)
            {
                throw new ErrorException(Errors.Person.NotFound(request.PersonId));
            }
            else if (!person.UserId.HasValue)
            {
                throw new ErrorException(Errors.Person.CustomerNotFound(request.PersonId));
            }

            var userReadModel = this.userReadModelRepository.GetUser(request.TenantId, person.UserId.Value);
            return Task.FromResult(userReadModel);
        }
    }
}
