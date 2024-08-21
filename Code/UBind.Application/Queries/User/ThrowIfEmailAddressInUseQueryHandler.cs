// <copyright file="ThrowIfEmailAddressInUseQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Queries.User
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Humanizer;
    using MediatR;
    using UBind.Domain;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Repositories;

    public class ThrowIfEmailAddressInUseQueryHandler : IQueryHandler<ThrowIfEmailAddressInUseQuery, Unit>
    {
        private readonly IUserLoginEmailRepository userLoginEmailRepository;
        private readonly IUserAggregateRepository userAggregateRepository;
        private readonly IUserReadModelRepository userReadModelRepository;

        public ThrowIfEmailAddressInUseQueryHandler(
            IUserLoginEmailRepository userLoginEmailRepository,
            IUserAggregateRepository userAggregateRepository,
            IUserReadModelRepository userReadModelRepository)
        {
            this.userLoginEmailRepository = userLoginEmailRepository;
            this.userAggregateRepository = userAggregateRepository;
            this.userReadModelRepository = userReadModelRepository;
        }

        public Task<Unit> Handle(ThrowIfEmailAddressInUseQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var userLogin = this.userLoginEmailRepository.GetUserLoginByEmail(
                request.TenantId, request.OrganisationId, request.Email);

            if (userLogin == null)
            {
                return Task.FromResult(Unit.Value);
            }

            var userAggregate = this.userAggregateRepository.GetById(request.TenantId, userLogin.Id);
            if (userAggregate == null)
            {
                return Task.FromResult(Unit.Value);
            }

            if (userAggregate.LoginEmail == null)
            {
                throw new ErrorException(
                    Errors.General.Unexpected("When checking if email address is already in use, " +
                    "the user did not have a login email. " +
                    "This is a platform error, which a platform developer needs to fix. " +
                    "To resolve this issue, please contact technical support."));
            }

            // If there is an existing user aggregate record by the given email address, evaluate the type and
            // ensure that it has a unique email address with its invitation/activation flag
            if (userAggregate.UserType == UserType.Customer.Humanize())
            {
                this.EnsureCustomerUserHasUniqueEmailAddress(
                    request.TenantId, userAggregate.OrganisationId, userAggregate.LoginEmail);
            }
            else
            {
                this.EnsureClientUserHasUniqueEmailAddress(request.TenantId, userAggregate.LoginEmail);
            }

            return Task.FromResult(Unit.Value);
        }

        private void EnsureClientUserHasUniqueEmailAddress(Guid tenantId, string email)
        {
            var userReadModel
                = this.userReadModelRepository.GetInvitedOrActivatedUserByEmailAndTenantId(tenantId, email);
            if (userReadModel != null && userReadModel.HasBeenActivated)
            {
                throw new ErrorException(Errors.User.UserEmailAddressInUseByAnotherUser(email));
            }
        }

        private void EnsureCustomerUserHasUniqueEmailAddress(Guid tenantId, Guid organisationId, string email)
        {
            var anyUserByEmailAndOrganisationId = this.userReadModelRepository
                .GetInvitedUserByEmailTenantIdAndOrganisationId(tenantId, email, organisationId);
            if (anyUserByEmailAndOrganisationId != null && anyUserByEmailAndOrganisationId.HasBeenActivated)
            {
                throw new ErrorException(Errors.User.CustomerEmailAddressInUseByAnotherUser(email));
            }

            var anyClientUserByEmailAndTenantId
                = this.userReadModelRepository.GetInvitedClientUserByEmailAndTenantId(tenantId, email);
            if (anyClientUserByEmailAndTenantId != null)
            {
                throw new ErrorException(Errors.User.CustomerEmailAddressInUseByAnotherUser(email));
            }
        }
    }
}
