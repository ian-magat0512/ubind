// <copyright file="DeletePolicyCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Policy;

using MediatR;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Security.Claims;
using UBind.Application.Authorisation;
using UBind.Application.ExtensionMethods;
using UBind.Application.Services;
using UBind.Domain;
using UBind.Domain.Aggregates.Claim;
using UBind.Domain.Aggregates.Customer;
using UBind.Domain.Aggregates.Person;
using UBind.Domain.Aggregates.Quote;
using UBind.Domain.Enums;
using UBind.Domain.Exceptions;
using UBind.Domain.Extensions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.ReadModel.Claim;
using UBind.Domain.ReferenceNumbers;
using UBind.Domain.Repositories;
using UBind.Domain.Search;
using UBind.Domain.Services;
using UBind.Domain.ValueTypes;

/// <summary>
/// Deletes a policy as well as its associcated claims, quotes, customers, etc.
/// </summary>
public class DeletePolicyCommandHandler : ICommandHandler<DeletePolicyCommand>
{
    private readonly ILogger<DeletePolicyCommandHandler> logger;
    private readonly IUBindDbContext dbContext;
    private readonly IClock clock;
    private readonly ICachingResolver cachingResolver;
    private readonly IAuthorisationService authorisationService;
    private readonly IQuoteService quoteService;
    private readonly IPolicyService policyService;
    private readonly IClaimService claimService;
    private readonly IQuoteAggregateRepository quoteAggregateRepository;
    private readonly IClaimAggregateRepository claimAggregateRepository;
    private readonly ICustomerAggregateRepository customerAggregateRepository;
    private readonly IPersonAggregateRepository personAggregateRepository;
    private readonly ILuceneRepository<IQuoteSearchIndexWriteModel, IQuoteSearchResultItemReadModel, QuoteReadModelFilters> luceneQuoteRepository;
    private readonly ILuceneRepository<IPolicySearchIndexWriteModel, IPolicySearchResultItemReadModel, PolicyReadModelFilters> lucenePolicyRepository;
    private readonly IPolicyNumberRepository policyNumberRepository;
    private readonly IClaimNumberRepository claimNumberRepository;
    private readonly ICustomerReadModelRepository customerReadModelRepository;

    public DeletePolicyCommandHandler(
        ILogger<DeletePolicyCommandHandler> logger,
        IUBindDbContext dbContext,
        IClock clock,
        ICachingResolver cachingResolver,
        IAuthorisationService authorisationService,
        IQuoteService quoteService,
        IPolicyService policyService,
        IClaimService claimService,
        IQuoteAggregateRepository quoteAggregateRepository,
        IClaimAggregateRepository claimAggregateRepository,
        ICustomerAggregateRepository customerAggregateRepository,
        IPersonAggregateRepository personAggregateRepository,
        ILuceneRepository<IQuoteSearchIndexWriteModel, IQuoteSearchResultItemReadModel, QuoteReadModelFilters> luceneQuoteRepository,
        ILuceneRepository<IPolicySearchIndexWriteModel, IPolicySearchResultItemReadModel, PolicyReadModelFilters> lucenePolicyRepository,
        IPolicyNumberRepository policyNumberRepository,
        IClaimNumberRepository claimNumberRepository,
        ICustomerReadModelRepository customerReadModelRepository)
    {
        this.logger = logger;
        this.dbContext = dbContext;
        this.clock = clock;
        this.cachingResolver = cachingResolver;
        this.authorisationService = authorisationService;
        this.quoteService = quoteService;
        this.policyService = policyService;
        this.claimService = claimService;
        this.quoteAggregateRepository = quoteAggregateRepository;
        this.claimAggregateRepository = claimAggregateRepository;
        this.customerAggregateRepository = customerAggregateRepository;
        this.personAggregateRepository = personAggregateRepository;
        this.luceneQuoteRepository = luceneQuoteRepository;
        this.lucenePolicyRepository = lucenePolicyRepository;
        this.policyNumberRepository = policyNumberRepository;
        this.claimNumberRepository = claimNumberRepository;
        this.customerReadModelRepository = customerReadModelRepository;
    }

    public async Task<Unit> Handle(DeletePolicyCommand command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tenant = await this.cachingResolver.GetTenantOrThrow(
            new Domain.Helpers.GuidOrAlias(command.PerformingUser.GetTenantId()));
        var tenantId = tenant.Id;

        if (!this.policyService.HasPolicy(command.PolicyId))
        {
            throw new ErrorException(Errors.Policy.NotFound(command.PolicyId));
        }
        if (!this.policyService.HasPolicy(command.PolicyId, tenantId))
        {
            throw new ErrorException(Errors.Policy.NotFoundUnderTenant(command.PolicyId));
        }
        await this.authorisationService.ThrowIfUserCannotModifyPolicy(command.PerformingUser, command.PolicyId);
        var policyDetails = await this.policyService.GetPolicy(tenantId, command.PolicyId);

        if (command.DeleteOrphanedCustomers)
        {
            await this.authorisationService.ThrowIfUserDoesNotHaveImportCustomersPermission(command.PerformingUser);
        }

        if (this.claimService.HasClaimsFromPolicy(tenantId, command.PolicyId))
        {
            if (command.AssociatedClaimAction.Equals(DeletedPolicyClaimsActionType.Error))
            {
                throw new ErrorException(Errors.Policy.AssociationWithClaim.HasAssociation(command.PolicyId));
            }
            if (command.AssociatedClaimAction.Equals(DeletedPolicyClaimsActionType.Delete))
            {
                await this.ThrowIfDeletingClaimWithoutImportClaimsPermission(command.PerformingUser, command.PolicyId);
            }
            this.ThrowIfDisassociatingClaimThatMustBeCreatedAgainstPolicy(
                tenantId,
                command.AssociatedClaimAction,
                policyDetails.ProductId);
        }

        var quoteIds = this.quoteService.GetQuoteIdsFromPolicy(tenantId, command.PolicyId, policyDetails.Environment);
        var claims = this.claimService.GetClaimsFromPolicy(tenantId, command.PolicyId);

        await this.DeleteQuotesAndPolicy(
            tenant,
            command.PolicyId,
            command.PerformingUser.GetId(),
            policyDetails.ProductId,
            quoteIds,
            policyDetails.Environment,
            command.ReusePolicyNumber,
            policyDetails.PolicyNumber);

        await this.DoAssociatedClaimAction(
            tenantId,
            command.PolicyId,
            command.PerformingUser.GetId(),
            command.AssociatedClaimAction,
            command.ReuseClaimNumbers,
            policyDetails.ProductId,
            policyDetails.Environment,
            claims);

        if (command.DeleteOrphanedCustomers && policyDetails.CustomerId.HasValue)
        {
            await this.DeleteOrphanedCustomer(
                tenantId,
                command.PerformingUser.GetId(),
                command.PolicyId,
                policyDetails.CustomerId.Value,
                policyDetails.Environment,
                quoteIds,
                command.AssociatedClaimAction.Equals(DeletedPolicyClaimsActionType.Delete)
                    ? claims.Select(c => c.Id)
                    : Enumerable.Empty<Guid>());
        }

        await this.dbContext.SaveChangesAsync();
        this.logger.LogInformation($"Delete process completed for policy with ID {command.PolicyId}");
        return Unit.Value;
    }

    private async Task ThrowIfDeletingClaimWithoutImportClaimsPermission(ClaimsPrincipal performingUser, Guid policyId)
    {
        if (!await this.authorisationService.UserDoesHaveImportClaimsPermission(performingUser))
        {
            throw new ErrorException(Errors.Claim.NoPermissionToDeleteClaimsAssociatedWithPolicy(policyId));
        }
    }

    private void ThrowIfDisassociatingClaimThatMustBeCreatedAgainstPolicy(
        Guid tenantId,
        DeletedPolicyClaimsActionType associatedClaimActionType,
        Guid productId)
    {
        if (!associatedClaimActionType.Equals(DeletedPolicyClaimsActionType.Disassociate))
        {
            return;
        }

        var productFeature = this.cachingResolver.GetProductSettingOrThrow(tenantId, productId);
        if (productFeature.MustCreateClaimAgainstPolicy)
        {
            throw new ErrorException(Errors.Claim.CannotDisassociateWithEnabledCreateClaimAgainstPolicy());
        }
    }

    private void ReuseClaimNumbers(
        Guid tenantId,
        Guid productId,
        DeploymentEnvironment environment,
        IEnumerable<ClaimReadModel> deletedClaims)
    {
        foreach (var claim in deletedClaims)
        {
            var claimNumber = claim.ClaimNumber;
            this.claimNumberRepository.ReuseOldClaimNumber(tenantId, productId, claimNumber, environment, false);
        }
        this.logger.LogInformation($"Reused claim numbers: {deletedClaims.Count()}");
    }

    private async Task DoAssociatedClaimAction(
        Guid tenantId,
        Guid policyId,
        Guid? performingUserId,
        DeletedPolicyClaimsActionType associatedClaimActionType,
        bool reuseClaimNumbers,
        Guid productId,
        DeploymentEnvironment environment,
        IEnumerable<ClaimReadModel> associatedClaims)
    {
        if (associatedClaimActionType.Equals(DeletedPolicyClaimsActionType.Delete))
        {
            foreach (var claim in associatedClaims)
            {
                var claimAggregate = this.claimAggregateRepository.GetById(tenantId, claim.Id);
                if (claimAggregate != null)
                {
                    claimAggregate.DeleteClaimRecords(claim.Id, performingUserId, this.clock.Now());
                    await this.claimAggregateRepository.ApplyChangesToDbContext(claimAggregate);
                    await this.claimAggregateRepository.DeleteById(tenantId, claimAggregate.Id);
                }
            }
            this.logger.LogInformation($"Deleted claims: {associatedClaims.Count()}");

            if (reuseClaimNumbers)
            {
                this.ReuseClaimNumbers(tenantId, productId, environment, associatedClaims);
            }
        }
        else if (associatedClaimActionType.Equals(DeletedPolicyClaimsActionType.Disassociate))
        {
            foreach (var claim in associatedClaims)
            {
                await this.claimService.DisassociateClaimWithPolicyAsync(tenantId, claim.Id, policyId);
            }
            this.logger.LogInformation($"Disassociated claims: {associatedClaims.Count()}");
        }
    }

    private async Task DeleteOrphanedCustomer(
        Guid tenantId,
        Guid? performingUserId,
        Guid policyId,
        Guid customerId,
        DeploymentEnvironment environment,
        IEnumerable<Guid> deletedQuoteIds,
        IEnumerable<Guid> deletedClaimIds)
    {
        var customerReadModel = this.customerReadModelRepository.GetCustomerById(tenantId, customerId);
        if (customerReadModel == null)
        {
            this.logger.LogWarning($"Can't retrieve read model for customer with ID {customerId}");
            return;
        }

        bool hasQuotes = this.CustomerHasQuote(
            customerReadModel.TenantId,
            customerReadModel.Id,
            customerReadModel.OrganisationId,
            environment,
            deletedQuoteIds);
        bool hasPolicies = this.CustomerHasPolicies(
            customerReadModel.TenantId,
            customerReadModel.Id,
            customerReadModel.OrganisationId,
            environment,
            policyId);
        bool hasClaims = this.CustomerHasClaims(
            customerReadModel.TenantId,
            customerReadModel.Id,
            customerReadModel.OrganisationId,
            environment,
            deletedClaimIds);
        if (hasQuotes || hasPolicies || hasClaims)
        {
            this.logger.LogInformation($"Customer with ID {customerId} has remaining associated items.");
            return;
        }

        await this.DeletePersonsFromCustomer(tenantId, performingUserId, customerReadModel.People.Select(p => p.Id));
        var customerAggregate = this.customerAggregateRepository.GetById(tenantId, customerId);
        if (customerAggregate != null)
        {
            customerAggregate.DeleteCustomerRecords(performingUserId, this.clock.Now());
            await this.customerAggregateRepository.ApplyChangesToDbContext(customerAggregate);
            await this.customerAggregateRepository.DeleteById(tenantId, customerAggregate.Id);
        }
        this.logger.LogInformation($"Orphaned customer with ID {customerId} deleted.");
    }

    private bool CustomerHasQuote(
        Guid tenantId,
        Guid customerId,
        Guid organisationId,
        DeploymentEnvironment environment,
        IEnumerable<Guid> deletedQuoteIds)
    {
        var quoteFilters = new QuoteReadModelFilters
        {
            TenantId = tenantId,
            OrganisationIds = new List<Guid> { organisationId },
            Environment = environment,
            CustomerId = customerId,
            ExcludedStatuses = new List<string>() { StandardQuoteStates.Nascent },
        };
        return this.quoteService.HasQuotesForCustomer(quoteFilters, deletedQuoteIds);
    }

    private bool CustomerHasPolicies(
        Guid tenantId,
        Guid customerId,
        Guid organisationId,
        DeploymentEnvironment environment,
        Guid deletedPolicyId)
    {
        var policyFilters = new PolicyReadModelFilters
        {
            TenantId = tenantId,
            OrganisationIds = new List<Guid> { organisationId },
            Environment = environment,
            CustomerId = customerId,
        };
        return this.policyService.HasPoliciesForCustomer(policyFilters, new List<Guid> { deletedPolicyId });
    }

    private bool CustomerHasClaims(
        Guid tenantId,
        Guid customerId,
        Guid organisationId,
        DeploymentEnvironment environment,
        IEnumerable<Guid> deletedClaimsIds)
    {
        var claimFilters = new EntityListFilters
        {
            TenantId = tenantId,
            OrganisationIds = new List<Guid> { organisationId },
            Environment = environment,
            CustomerId = customerId,
        };
        return this.claimService.HasClaimsForCustomer(claimFilters, deletedClaimsIds);
    }

    private async Task DeletePersonsFromCustomer(Guid tenantId, Guid? performingUserId, IEnumerable<Guid> personIds)
    {
        foreach (var personId in personIds)
        {
            var personAggregate = this.personAggregateRepository.GetById(tenantId, personId);
            if (personAggregate != null)
            {
                personAggregate.DeletePersonRecords(performingUserId, this.clock.Now());
                await this.personAggregateRepository.ApplyChangesToDbContext(personAggregate);
                await this.personAggregateRepository.DeleteById(tenantId, personAggregate.Id);
            }
        }
    }

    private async Task DeleteQuotesAndPolicy(
        Tenant tenant,
        Guid policyId,
        Guid? performingUserId,
        Guid productId,
        IEnumerable<Guid> associatedQuoteIds,
        DeploymentEnvironment environment,
        bool reusePolicyNumber,
        string policyNumber)
    {
        var policyAggregate = this.quoteAggregateRepository.GetById(tenant.Id, policyId);
        if (policyAggregate != null)
        {
            policyAggregate.DeletePolicyRecords(performingUserId, this.clock.Now());
            await this.quoteAggregateRepository.ApplyChangesToDbContext(policyAggregate);
            await this.quoteAggregateRepository.DeleteById(tenant.Id, policyAggregate.Id);
        }

        this.luceneQuoteRepository.DeleteItemsFromIndex(tenant, environment, associatedQuoteIds);
        this.lucenePolicyRepository.DeleteItemsFromIndex(tenant, environment, new List<Guid> { policyId });
        this.logger.LogInformation($"Deleted records for policy with ID {policyId}");

        if (reusePolicyNumber)
        {
            this.policyNumberRepository.ReturnOldPolicyNumberToPool(
                tenant.Id,
                productId,
                policyNumber,
                environment);
            this.logger.LogInformation($"Reused policy number {policyNumber}");
        }
    }
}
