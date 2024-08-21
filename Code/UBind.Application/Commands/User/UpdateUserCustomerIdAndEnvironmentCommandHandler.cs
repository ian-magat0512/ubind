// <copyright file="UpdateUserCustomerIdAndEnvironmentCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Command handler for updating user aggregate customer Id and environment.
    /// </summary>
    public class UpdateUserCustomerIdAndEnvironmentCommandHandler : ICommandHandler<UpdateUserCustomerIdAndEnvironmentCommand, Unit>
    {
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly IPersonReadModelRepository personReadModelRepository;
        private readonly IUBindDbContext dbContext;
        private readonly IUserReadModelRepository userReadModelRepository;

        public UpdateUserCustomerIdAndEnvironmentCommandHandler(
            IUserAggregateRepository userAggregateRepository,
            ICustomerReadModelRepository customerReadModelRepository,
            IPersonReadModelRepository personReadModelRepository,
            IUBindDbContext dbContext,
            IUserReadModelRepository userReadModelRepository)
        {
            this.userAggregateRepository = userAggregateRepository;
            this.customerReadModelRepository = customerReadModelRepository;
            this.personReadModelRepository = personReadModelRepository;
            this.dbContext = dbContext;
            this.userReadModelRepository = userReadModelRepository;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(UpdateUserCustomerIdAndEnvironmentCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sqlQueryBuilder = new StringBuilder();
            sqlQueryBuilder.AppendLine("Select TenantId, UserId, CustomerId From PersonReadModels WHERE(CustomerId IS NOT NULL AND CustomerId != '00000000-0000-0000-0000-000000000000') ");
            sqlQueryBuilder.AppendLine("AND(UserId IS NOT NULL AND UserId != '00000000-0000-0000-0000-000000000000') ");
            sqlQueryBuilder.AppendLine("AND Id IN(SELECT PersonId FROM UserReadModels WHERE CustomerId IS NULL) ");
            var sqlQuery = sqlQueryBuilder.ToString();
            var personWithNullUserCustomerIds = this.dbContext.ExecuteSqlQuery<PersonWithNullUserIdAndCustomerId>(sqlQuery, timeout: 350);

            foreach (var personWithNullUserCustomerId in personWithNullUserCustomerIds)
            {
                var userAggregate = this.userAggregateRepository.GetById(personWithNullUserCustomerId.TenantId, personWithNullUserCustomerId.UserId);
                if (userAggregate == null)
                {
                    continue;
                }

                var person = this.personReadModelRepository.GetPersonById(userAggregate.TenantId, userAggregate.PersonId);
                if (person == null)
                {
                    continue;
                }

                var customer = this.customerReadModelRepository.GetCustomerById(person.TenantId, (Guid)person.CustomerId);
                if (customer == null)
                {
                    continue;
                }

                userAggregate.SetCustomerIdAndEnvironment(userAggregate.TenantId, personWithNullUserCustomerId.UserId, personWithNullUserCustomerId.CustomerId, customer.Environment);
                this.userReadModelRepository.SaveChanges();
                await this.userAggregateRepository.Save(userAggregate);
            }

            return Unit.Value;
        }
    }
}
