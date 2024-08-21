// <copyright file="AssociateEmailRelationshipWithCustomerCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Messages
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    /// <summary>
    /// A command handler that is responsible for assigning new customer Id from existing emails (relationships).
    /// </summary>
    public class AssociateEmailRelationshipWithCustomerCommandHandler
        : ICommandHandler<AssociateEmailRelationshipWithCustomerCommand, Unit>
    {
        private readonly ICustomerAggregateRepository customerAggregateRepository;
        private readonly IEmailRepository emailRepository;

        public AssociateEmailRelationshipWithCustomerCommandHandler(
            ICustomerAggregateRepository customerAggregateRepository,
            IEmailRepository emailRepository)
        {
            this.customerAggregateRepository = customerAggregateRepository;
            this.emailRepository = emailRepository;
        }

        public async Task<Unit> Handle(AssociateEmailRelationshipWithCustomerCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var oldCustomerAggregate = this.customerAggregateRepository.GetById(request.TenantId, request.OldCustomerId);
            var newCustomerAggregate = this.customerAggregateRepository.GetById(request.TenantId, request.NewCustomerId);

            foreach (var emailId in request.EmailIds)
            {
                var relationships = this.emailRepository.GetRelationships(request.TenantId, emailId).ToList();
                foreach (var relationship in relationships)
                {
                    if (relationship.FromEntityId == oldCustomerAggregate.Id)
                    {
                        relationship.UpdateFromEntityId(newCustomerAggregate.Id);
                    }
                    else if (relationship.FromEntityId == oldCustomerAggregate.PrimaryPersonId)
                    {
                        relationship.UpdateFromEntityId(newCustomerAggregate.PrimaryPersonId);
                    }

                    if (relationship.ToEntityId == oldCustomerAggregate.Id)
                    {
                        relationship.UpdateToEntityId(newCustomerAggregate.Id);
                    }
                    else if (relationship.ToEntityId == oldCustomerAggregate.PrimaryPersonId)
                    {
                        relationship.UpdateToEntityId(newCustomerAggregate.PrimaryPersonId);
                    }
                }

                this.emailRepository.SaveChanges();

                // Have to force sleep while saving email changes to avoid table locking
                await Task.Delay(100, cancellationToken);
            }

            return Unit.Value;
        }
    }
}
