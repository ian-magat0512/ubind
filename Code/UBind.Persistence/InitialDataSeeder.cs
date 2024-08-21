// <copyright file="InitialDataSeeder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Authentication;
    using UBind.Domain.Entities;
    using UBind.Domain.Extensions;
    using UBind.Domain.Permissions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Seed database with initial/default values required for the UBind application.
    /// This includes creation of UBind master tenant, users, roles, default organisations, etc.
    /// </summary>
    public class InitialDataSeeder : IInitialDataSeeder
    {
        private const string UbindAdminEmail = "test.admin@ubind.com.au";
        private readonly IUBindDbContext dbContext;
        private readonly IClock clock = SystemClock.Instance;
        private readonly IOrganisationAggregateRepository organisationAggregateRepository;
        private readonly IPersonAggregateRepository personRepository;
        private readonly IPasswordHashingService passwordHashingService;
        private readonly IUserLoginEmailRepository userLoginEmailRepository;
        private readonly IUserAggregateRepository userAggregateRepository;

        public InitialDataSeeder(
            IUBindDbContext dbContext,
            IOrganisationAggregateRepository organisationAggregateRepository,
            IPersonAggregateRepository personAggregateRepository,
            IPasswordHashingService passwordHashingService,
            IUserAggregateRepository userAggregateRepository,
            IUserLoginEmailRepository userLoginEmailRepository)
        {
            this.dbContext = dbContext;
            this.organisationAggregateRepository = organisationAggregateRepository;
            this.personRepository = personAggregateRepository;
            this.passwordHashingService = passwordHashingService;
            this.userAggregateRepository = userAggregateRepository;
            this.userLoginEmailRepository = userLoginEmailRepository;
        }

        public async Task Seed()
        {
            this.CreateMasterTenantIfRequired();
            await this.CreateDefaultOrganisationForTenants();
            this.CreateMasterRolesIfRequired();
            this.CreateClientAdminAndCustomerRolesIfRequired();
            await this.CreateMasterAdminIfRequired();
            await this.CreateClientAdminsIfRequired();
            await this.AssignMasterAdminRoleIfRequired();
            await this.AssignClientAdminRolesIfRequired();
        }

        private void CreateMasterTenantIfRequired()
        {
            var tenant = this.dbContext.Tenants.FirstOrDefault(t => t.Id == Tenant.MasterTenantId);
            if (tenant == null)
            {
                tenant = new Tenant(
                    Tenant.MasterTenantId,
                    Tenant.MasterTenantName,
                    Tenant.MasterTenantAlias,
                    null,
                    default,
                    default,
                    this.clock.Now());
                this.dbContext.Tenants.Add(tenant);
                this.dbContext.SaveChanges();
            }
        }

        private async Task CreateDefaultOrganisationForTenants()
        {
            var tenants = this.dbContext.Tenants
                .IncludeAllProperties()
                .Where(tenant => tenant.DetailsCollection.Any())
                .ToList();
            foreach (var tenant in tenants.Where(t => t.Details.DefaultOrganisationId == default))
            {
                var organisationAggregate = Organisation.CreateNewOrganisation(
                    tenant.Id, tenant.Details.Alias, tenant.Details.Name, null, default, this.clock.GetCurrentInstant());
                organisationAggregate.SetDefault(true, null, this.clock.GetCurrentInstant());
                await this.organisationAggregateRepository.Save(organisationAggregate);

                tenant.SetDefaultOrganisation(organisationAggregate.Id, this.clock.GetCurrentInstant());
            }

            this.dbContext.SaveChanges();
        }

        private void CreateMasterRolesIfRequired()
        {
            // during migrations, this won't have already been set.
            Role.SetDefaultRolePermissionsRegistry(new DefaultRolePermissionsRegistry());
            Role.SetDefaultRoleNameRegistry(new DefaultRoleNameRegistry());

            var organisationId = this.dbContext.OrganisationReadModel
                .FirstOrDefault(o => o.Alias == Tenant.MasterTenantAlias)
                .Id;
            var masterRoles = new List<Role>();

            var masterAdminRole = new Role(
                Tenant.MasterTenantId, organisationId, DefaultRole.MasterAdmin, this.clock.Now());
            masterRoles.Add(masterAdminRole);

            var masterProductDeveloper = new Role(
                Tenant.MasterTenantId, organisationId, DefaultRole.MasterProductDeveloper, this.clock.Now());
            masterRoles.Add(masterProductDeveloper);

            var masterSupportAgent = new Role(
                Tenant.MasterTenantId, organisationId, DefaultRole.MasterSupportAgent, this.clock.Now());
            masterRoles.Add(masterSupportAgent);

            foreach (var role in masterRoles)
            {
                if (!this.dbContext.Roles
                    .Any(r => r.TenantId == Tenant.MasterTenantId && r.Type == role.Type && r.Name == role.Name))
                {
                    this.dbContext.Roles.Add(role);
                }
            }

            this.dbContext.SaveChanges();
        }

        private void CreateClientAdminAndCustomerRolesIfRequired()
        {
            var tenants = this.dbContext.Tenants
                             .Where(tenant => tenant.Id != Tenant.MasterTenantId)
                             .ToList();

            foreach (var tenant in tenants)
            {
                var org = this.dbContext.OrganisationReadModel
                    .FirstOrDefault(o => o.TenantId == tenant.Id);
                if (org == null)
                {
                    // The tenant was probably deleted
                    continue;
                }

                var organisationId = org.Id;

                var roles = new List<Role>
                {
                    new Role(tenant.Id, organisationId, DefaultRole.TenantAdmin, this.clock.Now()),
                    new Role(tenant.Id, organisationId, DefaultRole.Customer, this.clock.Now()),
                };

                foreach (var role in roles)
                {
                    if (!this.dbContext.Roles.Where(r => r.TenantId == tenant.Id && r.Type == role.Type).Any())
                    {
                        this.dbContext.Roles.Add(role);
                    }
                }
            }

            this.dbContext.SaveChanges();
        }

        private async Task CreateMasterAdminIfRequired()
        {
            var hasMasterAdmin = this.dbContext.Users
                  .Where(u => u.TenantId == Tenant.MasterTenantId)
                  .Where(u => u.Email == UbindAdminEmail)
                  .Any();

            var tenant = this.dbContext.Tenants.First(x => x.Id == Tenant.MasterTenantId);
            if (!hasMasterAdmin)
            {
                var masterTenant = this.dbContext.Tenants.FirstOrDefault(t => t.Id == Tenant.MasterTenantId);
                await this.SeedAdminUser(
                    masterTenant.Id,
                    masterTenant.Details.DefaultOrganisationId,
                    DeploymentEnvironment.Development,
                    UserType.Master,
                    "uBind Admin",
                    "uBind",
                    "04 1122 3344",
                    UbindAdminEmail);
            }
        }

        /// <summary>
        /// Creates admin user with Tenant Admin or Ubind Admin role.
        /// </summary>
        private async Task SeedAdminUser(
            Guid tenantId,
            Guid organisationId,
            DeploymentEnvironment environment,
            UserType userType,
            string fullName,
            string preferredName,
            string mobilePhone,
            string email)
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            // Create person
            var personCommonProperties = new PersonCommonProperties
            {
                FullName = fullName,
                PreferredName = preferredName,
                MobilePhoneNumber = mobilePhone,
                Email = email,
                TenantId = tenantId,
                OrganisationId = organisationId,
            };
            var person = PersonAggregate.CreatePersonFromPersonalDetails(
                tenantId, organisationId, new PersonalDetails(tenantId, personCommonProperties), default, now);

            await this.personRepository.Save(person);

            // Create user
            var user = UserAggregate.CreateUser(person.TenantId, Guid.NewGuid(), userType, person, default, null, now);
            var roleInformation = userType == UserType.Client ?
                DefaultRole.TenantAdmin.GetAttributeOfType<RoleInformationAttribute>() :
                DefaultRole.MasterAdmin.GetAttributeOfType<RoleInformationAttribute>();
            var adminRole = this.dbContext.Roles
                .Where(r => r.TenantId == tenantId)
                .Where(r => r.OrganisationId == organisationId)
                .Where(r => r.Type == roleInformation.RoleType)
                .Where(r => r.IsAdmin)
                .Where(r => r.Name == roleInformation.Name)
                .Single();
            user.AssignRole(adminRole, default, now);

            var invitationId = user.CreateActivationInvitation(default, now);
            var saltedAndHashedPassword = this.passwordHashingService.SaltAndHash("ubindTest123*");
            user.Activate(invitationId, saltedAndHashedPassword, default, now);
            await this.userAggregateRepository.Save(user);
        }

        private async Task CreateClientAdminsIfRequired()
        {
            var tenants = this.dbContext.Tenants
                .IncludeAllProperties()
                .Where(t => t.Id != Tenant.MasterTenantId)
                .ToList()
                .Select(t => new { t.Id, t.Details.Alias, t.Details.DefaultOrganisationId });

            foreach (var tenant in tenants)
            {
                var tenantId = tenant.Id;
                var tenantAlias = tenant.Alias;
                var adminEmail = this.GetAdminEmail(tenantAlias);
                var hasAdmin = this.dbContext.Users
                    .Where(u => u.TenantId == tenant.Id)
                    .Any(user => user.Email == adminEmail);
                if (!hasAdmin)
                {
                    await this.SeedAdminUser(
                        tenantId,
                        tenant.DefaultOrganisationId,
                        DeploymentEnvironment.Development,
                        UserType.Client,
                        $"Default {tenantId} client admin",
                        "Client admin",
                        string.Empty,
                        adminEmail);
                }
            }
        }

        private string GetAdminEmail(string tenantAlias) => $"{tenantAlias}.client.admin@ubind.com.au";

        private async Task AssignMasterAdminRoleIfRequired()
        {
            var tenant = this.dbContext.Tenants.FirstOrDefault(t => t.Id == Tenant.MasterTenantId);

            var userLogin = this.userLoginEmailRepository.GetUserLoginByEmail(
                tenant.Id, tenant.Details.DefaultOrganisationId, UbindAdminEmail);
            var ubindAdmin = this.userAggregateRepository.GetById(tenant.Id, userLogin.Id);
            if (ubindAdmin != null && !ubindAdmin.RoleIds.Any())
            {
                var roleInformation = DefaultRole.MasterAdmin.GetAttributeOfType<RoleInformationAttribute>();
                var ubindAdminRole = this.dbContext.Roles
                   .Where(r => r.Type == roleInformation.RoleType)
                   .Where(r => r.TenantId == Tenant.MasterTenantId)
                   .Where(r => r.IsAdmin)
                   .Where(r => r.Name == roleInformation.Name)
                   .Single();
                ubindAdmin.AssignRole(ubindAdminRole, default, this.clock.Now());
                await this.userAggregateRepository.Save(ubindAdmin);
            }
        }

        private async Task AssignClientAdminRolesIfRequired()
        {
            var tenants = this.dbContext.Tenants
                .IncludeAllProperties()
                .Where(t => t.Id != Tenant.MasterTenantId)
                .ToList()
                .Select(t => new { t.Id, t.Details.Alias, t.Details.DefaultOrganisationId });

            foreach (var tenant in tenants)
            {
                var userLogin = this.userLoginEmailRepository.GetUserLoginByEmail(
                    tenant.Id, tenant.DefaultOrganisationId, this.GetAdminEmail(tenant.Alias));
                var clientAdmin = this.userAggregateRepository.GetById(tenant.Id, userLogin.Id);
                if (clientAdmin != null && !clientAdmin.RoleIds.Any())
                {
                    var clientAdminRole = this.dbContext.Roles
                        .Where(r => r.OrganisationId == tenant.DefaultOrganisationId)
                        .Where(r => r.IsAdmin)
                        .Single();
                    clientAdmin.AssignRole(clientAdminRole, default, this.clock.Now());
                    await this.userAggregateRepository.Save(clientAdmin);
                }
            }
        }
    }
}
