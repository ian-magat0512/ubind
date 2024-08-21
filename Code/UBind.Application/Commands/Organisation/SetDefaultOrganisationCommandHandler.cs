// <copyright file="SetDefaultOrganisationCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Organisation
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    /// <summary>
    /// Sets the default org for a tenant.
    /// </summary>
    public class SetDefaultOrganisationCommandHandler : ICommandHandler<SetDefaultOrganisationCommand>
    {
        private readonly IOrganisationAggregateRepository organisationAggregateRepository;
        private readonly ITenantRepository tenantRepository;
        private readonly IUserReadModelRepository userReadModelRepository;
        private readonly IRoleRepository roleRepository;
        private readonly Domain.Services.IUserService userService;
        private readonly IClock clock;
        private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;

        public SetDefaultOrganisationCommandHandler(
            IOrganisationAggregateRepository organisationAggregateRepository,
            ITenantRepository tenantRepository,
            IUserReadModelRepository userReadModelRepository,
            IRoleRepository roleRepository,
            Domain.Services.IUserService userService,
            IClock clock,
            IHttpContextPropertiesResolver httpContextPropertiesResolver,
            IOrganisationReadModelRepository organisationReadModelRepository)
        {
            this.organisationAggregateRepository = organisationAggregateRepository;
            this.tenantRepository = tenantRepository;
            this.userReadModelRepository = userReadModelRepository;
            this.roleRepository = roleRepository;
            this.userService = userService;
            this.clock = clock;
            this.httpContextPropertiesResolver = httpContextPropertiesResolver;
            this.organisationReadModelRepository = organisationReadModelRepository;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(SetDefaultOrganisationCommand command, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Instant now = this.clock.Now();
            var performingUserId = this.httpContextPropertiesResolver.PerformingUserId;
            var tenant = this.tenantRepository.GetTenantById(command.TenantId);
            Tenant.ThrowIfNotFound(command.TenantId, tenant);
            if (command.OrganisationId == tenant.Details.DefaultOrganisationId)
            {
                var organisationReadModel = this.organisationReadModelRepository.Get(tenant.Id, command.OrganisationId);
                throw new ErrorException(Errors.Organisation.AlreadyDefault(organisationReadModel.Name));
            }

            var incomingOrganisationAggregate
                = this.organisationAggregateRepository.GetById(command.TenantId, command.OrganisationId);
            Organisation.ThrowIfNotFound(incomingOrganisationAggregate, command.OrganisationId);

            // check that there is at least one Organisation Admin user because they will become
            // the Tenant Admin users
            var incomingOrganisationAdminUsers = this.userReadModelRepository.GetUsersWithRoles(
                command.TenantId,
                command.OrganisationId,
                new string[] { "Organisation Admin", "Tenant Admin" });
            if (incomingOrganisationAdminUsers.None())
            {
                throw new ErrorException(
                    Errors.Organisation.CannotSetOrganisationToDefaultWithoutAnyOrganisationAdminUser(
                        incomingOrganisationAggregate.Name));
            }

            var outgoingOrganisationAdminUsers = this.userReadModelRepository.GetUsersWithRoles(
                command.TenantId,
                tenant.Details.DefaultOrganisationId,
                new string[] { "Tenant Admin" });

            var tenantAdminRole = this.roleRepository.GetRoleByNameOrThrow(command.TenantId, "Tenant Admin");
            var organisationAdminRole = this.roleRepository.GetRoleByNameOrThrow(command.TenantId, "Organisation Admin");

            var outgoingOrganisationAggregate
                = this.organisationAggregateRepository.GetById(command.TenantId, tenant.Details.DefaultOrganisationId);
            Organisation.ThrowIfNotFound(outgoingOrganisationAggregate, command.OrganisationId);
            outgoingOrganisationAggregate.SetDefault(false, performingUserId, now);
            incomingOrganisationAggregate.SetDefault(true, performingUserId, now);
            await this.organisationAggregateRepository.ApplyChangesToDbContext(outgoingOrganisationAggregate);
            await this.organisationAggregateRepository.ApplyChangesToDbContext(incomingOrganisationAggregate);

            await this.userService.SwapRoleForUsers(
                command.TenantId,
                performingUserId,
                organisationAdminRole,
                tenantAdminRole,
                incomingOrganisationAdminUsers);

            await this.userService.SwapRoleForUsers(
                command.TenantId,
                performingUserId,
                tenantAdminRole,
                organisationAdminRole,
                outgoingOrganisationAdminUsers);

            if (!incomingOrganisationAggregate.IsActive)
            {
                incomingOrganisationAggregate.Activate(performingUserId, now);
            }

            tenant.SetDefaultOrganisation(command.OrganisationId, this.clock.GetCurrentInstant());
            return await Task.FromResult(Unit.Value);
        }
    }
}
