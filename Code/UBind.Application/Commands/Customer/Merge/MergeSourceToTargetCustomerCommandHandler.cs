// <copyright file="MergeSourceToTargetCustomerCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Customer.Merge
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.Repositories;

    /// <summary>
    /// The merge logic for merging customer records from source to target customer.
    /// </summary>
    public class MergeSourceToTargetCustomerCommandHandler :
        MergeCustomerBaseCommandHandler<MergeSourceToTargetCustomerCommand>,
        ICommandHandler<MergeSourceToTargetCustomerCommand, Unit>
    {
        private readonly ICachingResolver cachingResolver;
        private readonly ICustomerReadModelRepository customerReadModelRepository;
        private readonly IUserReadModelRepository userReadModelRepository;

        public MergeSourceToTargetCustomerCommandHandler(
            IClaimAggregateRepository claimAggregateRepository,
            IClaimReadModelRepository claimReadModelRepository,
            ICustomerAggregateRepository customerAggregateRepository,
            ICustomerReadModelRepository customerReadModelRepository,
            IEmailRepository emailRepository,
            IPersonAggregateRepository personAggregateRepository,
            IPersonReadModelRepository personReadModelRepository,
            IQuoteAggregateRepository quoteAggregateRepository,
            IQuoteReadModelRepository quoteReadModelRepository,
            ISmsRepository smsRepository,
            IUserReadModelRepository userReadModelRepository,
            IHttpContextPropertiesResolver propertiesResolver,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator,
            ILogger<MergeSourceToTargetCustomerCommandHandler> logger,
            IClock clock)
            : base(
                claimAggregateRepository,
                claimReadModelRepository,
                emailRepository,
                personReadModelRepository,
                quoteAggregateRepository,
                quoteReadModelRepository,
                smsRepository,
                customerAggregateRepository,
                personAggregateRepository,
                propertiesResolver,
                clock,
                mediator,
                logger)
        {
            this.userReadModelRepository = userReadModelRepository;
            this.cachingResolver = cachingResolver;
            this.customerReadModelRepository = customerReadModelRepository;
        }

        public override async Task<Unit> Handle(MergeSourceToTargetCustomerCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sourceCustomer = this.customerReadModelRepository.GetCustomerById(request.TenantId, request.SourceCustomerId, true);
            var destinationCustomer = this.customerReadModelRepository.GetCustomerById(request.TenantId, request.TargetCustomerId, true);
            this.ThrowIfPerformingUserIsNotAuthorised(request.TenantId, request.PerformingUserId, sourceCustomer, destinationCustomer);
            EntityHelper.ThrowIfNotFound(sourceCustomer, request.SourceCustomerId, "customer");
            EntityHelper.ThrowIfNotFound(destinationCustomer, request.TargetCustomerId, "customer");
            this.VerifyCustomers(request.TenantId, sourceCustomer, destinationCustomer);
            var destinationPersonAggregate = this.PersonAggregateRepository.GetById(request.TenantId, destinationCustomer.PrimaryPersonId);
            EntityHelper.ThrowIfNotFound(destinationPersonAggregate, destinationCustomer.PrimaryPersonId, "person");
            DestinationPersonMergingModel dstParams =
                new DestinationPersonMergingModel(destinationPersonAggregate)
                    .SetNewOwnerFromCustomer(destinationCustomer);
            await this.MergeRelatedRecords(request.TenantId, request.SourceCustomerId, dstParams);
            await this.AssignAllPersonsFromSourceToDestinationCustomer(request.TenantId, sourceCustomer.Id, dstParams);
            await this.DeleteCustomer(request.TenantId, sourceCustomer.Id);
            request.SourceCustomerDisplayName = this.GetDisplayName(sourceCustomer);
            request.TargetCustomerDisplayName = destinationPersonAggregate.DisplayName;
            return Unit.Value;
        }

        private void ThrowIfPerformingUserIsNotAuthorised(Guid tenantId, Guid? performingUserId, CustomerReadModelDetail sourceCustomer, CustomerReadModelDetail destinationCustomer)
        {
            if (performingUserId.HasValue)
            {
                var user = this.userReadModelRepository.GetUserWithRoles(tenantId, performingUserId.Value);
                if (user.HasPermission(Domain.Permissions.Permission.ManageAllCustomersForAllOrganisations))
                {
                    return;
                }

                if (user.HasPermission(Domain.Permissions.Permission.ManageAllCustomers))
                {
                    if (sourceCustomer.OrganisationId != user.OrganisationId
                        || destinationCustomer.OrganisationId != user.OrganisationId)
                    {
                        throw new ErrorException(Errors.General.NotAuthorized("merge customers outside your organisation"));
                    }
                    else
                    {
                        return;
                    }
                }

                if (user.HasPermission(Domain.Permissions.Permission.ManageCustomers))
                {
                    if (sourceCustomer.OwnerUserId != user.Id
                        || destinationCustomer.OwnerUserId != user.Id)
                    {
                        throw new ErrorException(Errors.General.NotAuthorized("merge customers you do not own"));
                    }
                }
            }
        }

        /// <summary>
        /// Generate a display name for the customer, if it doenst have one.
        /// </summary>
        /// <returns>the display name.</returns>
        private string GetDisplayName(CustomerReadModelDetail sourceCustomer)
        {
            return sourceCustomer.DisplayName != null
                ? sourceCustomer.DisplayName
                : PersonPropertyHelper.GetDisplayName(
                    sourceCustomer.FirstName,
                    sourceCustomer.LastName,
                    sourceCustomer.PreferredName,
                    sourceCustomer.FullName,
                    sourceCustomer.Email,
                    sourceCustomer.MobilePhoneNumber);
        }

        /// <summary>
        /// Verify if the customers are fit to use for merging.
        /// </summary>
        private void VerifyCustomers(Guid executingUserTenantId, CustomerReadModelDetail sourceCustomer, CustomerReadModelDetail destinationCustomer)
        {
            var requestingTenantAlias = this.cachingResolver.GetTenantAliasOrThrow(executingUserTenantId);
            var srcTenantAlias = this.cachingResolver.GetTenantAliasOrThrow(sourceCustomer.TenantId);
            var dstTenantAlias = this.cachingResolver.GetTenantAliasOrThrow(destinationCustomer.TenantId);
            if (executingUserTenantId != sourceCustomer.TenantId || executingUserTenantId != destinationCustomer.TenantId)
            {
                var errorMessage = $"merge source customer from tenant \"{srcTenantAlias}\" because the executing user is from tenant \"{requestingTenantAlias}\". " +
                    $"Please make sure the user is from the same tenant as with both of the customers";
                throw new ErrorException(Errors.General.NotAuthorized(errorMessage));
            }

            if (sourceCustomer.TenantId != destinationCustomer.TenantId)
            {
                var errorMessage = $"merge source customer from tenant \"{srcTenantAlias}\" because the destination customer is from tenant \"{dstTenantAlias}\". " +
                   $"Please make sure both of the customers are from the same tenant";
                throw new ErrorException(Errors.General.NotAuthorized(errorMessage));
            }
        }
    }
}
