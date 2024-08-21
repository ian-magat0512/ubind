// <copyright file="CreateDefaultPortalsCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Migration;

using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using MediatR;
using UBind.Application.Commands.Organisation;
using UBind.Application.Commands.Portal;
using UBind.Application.Queries.Organisation;
using UBind.Domain.Aggregates.Portal;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Product;
using UBind.Domain.ReadModel;
using UBind.Domain.Repositories;

public class CreateDefaultPortalsCommandHandler : ICommandHandler<CreateDefaultPortalsCommand, Unit>
{
    private readonly ITenantRepository tenantRepository;
    private readonly IOrganisationReadModelRepository organisationReadModelRepository;
    private readonly ICqrsMediator cqrsMediator;
    private readonly IUBindDbContext dbContext;
    private readonly IProductRepository productRepository;
    private readonly IProductPortalSettingRepository productPortalSettingRepository;

    public CreateDefaultPortalsCommandHandler(
        ITenantRepository tenantRepository,
        IOrganisationReadModelRepository organisationReadModelRepository,
        ICqrsMediator cqrsMediator,
        IUBindDbContext dbContext,
        IProductRepository productRepository,
        IProductPortalSettingRepository productPortalSettingRepository)
    {
        this.tenantRepository = tenantRepository;
        this.organisationReadModelRepository = organisationReadModelRepository;
        this.cqrsMediator = cqrsMediator;
        this.dbContext = dbContext;
        this.productRepository = productRepository;
        this.productPortalSettingRepository = productPortalSettingRepository;
    }

    public async Task<Unit> Handle(CreateDefaultPortalsCommand command, CancellationToken cancellationToken)
    {
        var tenants = this.tenantRepository.GetActiveTenants();
        foreach (var tenant in tenants)
        {
            if (tenant.Details.DefaultPortalId != default)
            {
                // it already has a default portal (just being idempotent)
                continue;
            }

            using (var transaction = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                this.dbContext.TransactionStack.Push(transaction);
                try
                {
                    await this.CreateAgentPortalForTenancy(tenant);

                    // the master tenancy doesn't need a customer portal.
                    if (tenant.Id != Domain.Tenant.MasterTenantId)
                    {
                        await this.CreateCustomerPortalForTenancy(tenant);
                    }

                    this.dbContext.SaveChanges();
                    transaction.Complete();
                }
                finally
                {
                    this.dbContext.TransactionStack.Pop();
                }
            }

            // Make sure we don't overwhelm the database
            await Task.Delay(500);
        }

        return Unit.Value;
    }

    private async Task CreateAgentPortalForTenancy(Domain.Tenant tenant)
    {
        // create an agent portal for the tenancy
        var agentPortal = await this.cqrsMediator.Send(new CreatePortalCommand(
        tenant.Id,
        PortalAggregate.DefaultAgentPortalName,
        PortalAggregate.DefaultAgentPortalAlias,
        tenant.Details.DefaultPortalTitle,
        Domain.PortalUserType.Agent,
        tenant.Details.DefaultOrganisationId));

        // set the stylesheet url if it has one
        if (!string.IsNullOrEmpty(tenant.Details.DefaultPortalStylesheetUrl))
        {
            await this.cqrsMediator.Send(new UpdatePortalStylesCommand(
                tenant.Id,
                agentPortal.Id,
                tenant.Details.DefaultPortalStylesheetUrl,
                null));
        }

        // make it a default portal
        await this.cqrsMediator.Send(new SetPortalAsDefaultCommand(
            tenant.Id,
            tenant.Details.DefaultOrganisationId,
            agentPortal.Id));

        // enable all of the current products in the tenancy for the agent portal
        var products = this.productRepository.GetAllActiveProductSummariesForTenant(tenant.Id);
        foreach (var product in products)
        {
            this.productPortalSettingRepository.AddOrUpdateProductSetting(
                tenant.Id, agentPortal.Id, product.Id, true);
        }
    }

    private async Task CreateCustomerPortalForTenancy(Domain.Tenant tenant)
    {
        // create a customer portal for the tenancy
        var customerPortal = await this.cqrsMediator.Send(new CreatePortalCommand(
            tenant.Id,
            PortalAggregate.DefaultCustomerPortalName,
            PortalAggregate.DefaultCustomerPortalAlias,
            tenant.Details.DefaultPortalTitle,
            Domain.PortalUserType.Customer,
            tenant.Details.DefaultOrganisationId));

        // set the stylesheet url if it has one
        if (!string.IsNullOrEmpty(tenant.Details.DefaultPortalStylesheetUrl))
        {
            await this.cqrsMediator.Send(new UpdatePortalStylesCommand(
                tenant.Id,
                customerPortal.Id,
                tenant.Details.DefaultPortalStylesheetUrl,
                null));
        }

        // make it a default portal
        await this.cqrsMediator.Send(new SetPortalAsDefaultCommand(
            tenant.Id,
            tenant.Details.DefaultOrganisationId,
            customerPortal.Id));

        // check if the org has customer self registration enabled and copy that to the portal
        var organisationEntitySettings = await this.cqrsMediator.Send(
            new GetOrganisationEntitySettingsQuery(tenant.Id, tenant.Details.DefaultOrganisationId));
#pragma warning disable CS0618 // Type or member is obsolete
        if (organisationEntitySettings.AllowCustomerSelfAccountCreation)
        {
            await this.cqrsMediator.Send(
                new UpdatePortalCustomerSelfAccountCreationSettingCommand(tenant.Id, customerPortal.Id, true));
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
