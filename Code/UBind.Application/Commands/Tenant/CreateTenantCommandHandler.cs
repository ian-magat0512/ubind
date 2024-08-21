// <copyright file="CreateTenantCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Tenant
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using NodaTime;
    using UBind.Application.Automation;
    using UBind.Application.Commands.AuthenticationMethod;
    using UBind.Application.Model.AuthenticationMethod;
    using UBind.Application.Services;
    using UBind.Application.User;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Portal;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Repositories;

    public class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, Guid>
    {
        private readonly ITenantRepository tenantRepository;
        private readonly IFilesystemFileRepository fileRepository;
        private readonly IFilesystemStoragePathService pathService;
        private readonly IClock clock;
        private readonly IOrganisationAggregateRepository organisationAggregateRepository;
        private readonly IUserService userService;
        private readonly IRoleService roleService;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IAutomationPeriodicTriggerScheduler periodicTriggerScheduler;
        private readonly IFeatureSettingRepository featureSettingRepository;
        private readonly ITenantService tenantService;
        private readonly IPortalAggregateRepository portalAggregateRepository;
        private readonly IUBindDbContext dbContext;
        private readonly ICqrsMediator mediator;
        private readonly ICachingResolver cachingResolver;

        public CreateTenantCommandHandler(
            ITenantRepository tenantRepository,
            IFilesystemFileRepository fileRepository,
            IFilesystemStoragePathService pathService,
            IUserService userService,
            IRoleService roleService,
            IOrganisationAggregateRepository organisationAggregateRepository,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IAutomationPeriodicTriggerScheduler periodicTriggerScheduler,
            IFeatureSettingRepository featureSettingRepository,
            IClock clock,
            ITenantService tenantService,
            IPortalAggregateRepository portalAggregateRepository,
            IUBindDbContext dbContext,
            ICachingResolver cachingResolver,
            ICqrsMediator mediator)
        {
            this.tenantRepository = tenantRepository;
            this.fileRepository = fileRepository;
            this.pathService = pathService;
            this.clock = clock;
            this.organisationAggregateRepository = organisationAggregateRepository;
            this.userService = userService;
            this.roleService = roleService;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.periodicTriggerScheduler = periodicTriggerScheduler;
            this.featureSettingRepository = featureSettingRepository;
            this.tenantService = tenantService;
            this.portalAggregateRepository = portalAggregateRepository;
            this.dbContext = dbContext;
            this.mediator = mediator;
            this.cachingResolver = cachingResolver;
        }

        public async Task<Guid> Handle(CreateTenantCommand command, CancellationToken cancellationToken)
        {
            this.tenantService.ThrowIfTenantAliasIsNull(command.Alias);
            this.tenantService.ThrowIfTenantAliasInUse(command.Alias);
            this.tenantService.ThrowIfTenantNameInUse(command.Name);
            this.tenantService.ThrowIfCustomDomainInUse(command.CustomDomain);
            var tenantId = Guid.NewGuid();
            var agentPortalId = Guid.NewGuid();
            var customerPortalId = Guid.NewGuid();
            var now = this.clock.GetCurrentInstant();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;

            using (var transaction = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                this.dbContext.TransactionStack.Push(transaction);
                try
                {
                    // create the default organisation
                    var organisationAggregate = Organisation.CreateNewOrganisation(
                        tenantId,
                        command.Alias,
                        command.Name,
                        null, performingUserId,
                        now);
                    organisationAggregate.SetDefault(true, performingUserId, now);
                    organisationAggregate.SetDefaultPortal(agentPortalId, performingUserId, now);
                    var organisationId = organisationAggregate.Id;

                    // Create the local account sign in method for the default organisation
                    var upsertModel = new LocalAccountAuthenticationMethodUpsertModel
                    {
                        Tenant = tenantId.ToString(),
                        Organisation = organisationId.ToString(),
                        Name = "Local Account",
                        TypeName = "Local",
                        IncludeSignInButtonOnPortalLoginPage = true,
                        Disabled = false,
                    };

                    // create the default portals
                    var agentPortalAggregate = new PortalAggregate(
                        tenantId,
                        agentPortalId,
                        PortalAggregate.DefaultAgentPortalName,
                        PortalAggregate.DefaultAgentPortalAlias,
                        command.Name,
                        PortalUserType.Agent,
                        organisationId,
                        performingUserId,
                        this.clock.Now());
                    agentPortalAggregate.SetDefault(true, performingUserId, now);
                    var customerPortalAggregate = new PortalAggregate(
                        tenantId,
                        customerPortalId,
                        PortalAggregate.DefaultCustomerPortalName,
                        PortalAggregate.DefaultCustomerPortalAlias,
                        command.Name,
                        PortalUserType.Customer,
                        organisationId,
                        performingUserId,
                        this.clock.Now());
                    customerPortalAggregate.SetDefault(true, performingUserId, now);

                    // create the tenant
                    var tenant = new Tenant(
                        tenantId,
                        command.Name,
                        command.Alias,
                        command.CustomDomain,
                        organisationId,
                        agentPortalId,
                        now);
                    this.tenantRepository.Insert(tenant);

                    // we cache the tenant so event observers dispatched when we apply the changes below
                    // can use cachingResolver to get the tenant even if it has not been saved to the db yet
                    this.cachingResolver.CacheTenant(tenantId, tenant);
                    await this.organisationAggregateRepository.ApplyChangesToDbContext(organisationAggregate);
                    await this.mediator.Send(new CreateAuthenticationMethodCommand(tenantId, organisationId, upsertModel));
                    await this.portalAggregateRepository.ApplyChangesToDbContext(agentPortalAggregate);
                    await this.portalAggregateRepository.ApplyChangesToDbContext(customerPortalAggregate);

                    await this.CreateFolders(command.Alias);
                    this.roleService.CreateDefaultRolesForTenant(tenant);
                    await this.userService.CreateDefaultUsersForOrganisationAsync(tenant, organisationAggregate);

                    // we have to save changes so that the tenant exists in the db first before we update the tenant settings
                    // this seems to be a restriction of EF6, relating to the structure of the settings entity, where it has a
                    // SettingDetails collection.
                    this.dbContext.SaveChanges();
                    transaction.Complete();
                }
                finally
                {
                    this.dbContext.TransactionStack.Pop();
                }
            }

            this.featureSettingRepository.SetInitialSettings(tenantId);
            this.dbContext.SaveChanges();
            await this.periodicTriggerScheduler.RegisterPeriodicTriggerJobs(tenantId);
            return tenantId;
        }

        private async Task CreateFolders(string tenantAlias)
        {
            var token = await this.fileRepository.GetAuthenticationToken();

            // Create tenant development folder
            await this.fileRepository.CreateFolder(this.pathService.DevelopmentFolderPath, tenantAlias, token);

            // Create tenant release folder
            await this.fileRepository.CreateFolder(this.pathService.ReleasesFolderPath, tenantAlias, token);
        }
    }
}
