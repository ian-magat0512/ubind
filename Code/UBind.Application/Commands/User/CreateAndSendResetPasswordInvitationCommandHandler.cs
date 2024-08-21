// <copyright file="CreateAndSendResetPasswordInvitationCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.User;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using UBind.Application.User;
using UBind.Domain;
using UBind.Domain.Authentication;
using UBind.Domain.Exceptions;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.Processing;

public class CreateAndSendResetPasswordInvitationCommandHandler
    : ICommandHandler<CreateAndSendResetPasswordInvitationCommand, Unit>
{
    private readonly IPasswordResetTrackingService passwordResetTrackingService;
    private readonly ICachingResolver cachingResolver;
    private readonly IUserPasswordResetInvitationService userPasswordResetInvitationService;
    private readonly IHttpContextPropertiesResolver httpContextPropertiesResolver;
    private readonly IJobClient jobClient;

    public CreateAndSendResetPasswordInvitationCommandHandler(
        IPasswordResetTrackingService passwordResetTrackingService,
        ICachingResolver cachingResolver,
        IUserPasswordResetInvitationService userPasswordResetInvitationService,
        IHttpContextPropertiesResolver httpContextPropertiesResolver,
        IJobClient jobClient)
    {
        this.passwordResetTrackingService = passwordResetTrackingService;
        this.cachingResolver = cachingResolver;
        this.userPasswordResetInvitationService = userPasswordResetInvitationService;
        this.httpContextPropertiesResolver = httpContextPropertiesResolver;
        this.jobClient = jobClient;
    }

    /// <inheritdoc/>
    public async Task<Unit> Handle(
        CreateAndSendResetPasswordInvitationCommand command, CancellationToken cancellationToken)
    {
        // rate limit attempts
        // Return early if the email is blocked as this does not reveal anything about account existance
        // and will reduce impact of spammed attempts.
        cancellationToken.ThrowIfCancellationRequested();
        var shouldBlock = this.passwordResetTrackingService.ShouldBlockRequest(
            command.TenantId,
            command.OrganisationId,
            command.Email,
            this.userPasswordResetInvitationService.PasswordResetRequestBlockingThreshold,
            this.userPasswordResetInvitationService.PasswordResetTrackingPeriodInMinutes);
        var tenant = await this.cachingResolver.GetTenantOrThrow(command.TenantId);
        if (shouldBlock)
        {
            throw new ErrorException(
                Errors.User.RequestResetPassword.TooManyAttempts(
                    command.Email,
                    tenant.Details.Alias,
                    this.userPasswordResetInvitationService.PasswordResetRequestBlockingThreshold,
                    this.userPasswordResetInvitationService.PasswordResetTrackingPeriodInMinutes));
        }

        var ipAddress = this.httpContextPropertiesResolver.ClientIpAddress.ToString();

        // Anything action beyond the initial disabled check is considered an attempt.
        this.passwordResetTrackingService.Record(tenant.Id, command.OrganisationId, command.Email, ipAddress);

        // Now lets do the rest as a hangfire job. This is important because we don't want to reveal if the account
        // exists or not. We'll never return an error, and we don't want the time it takes to process the request to
        // reveal that the account exists.
        this.jobClient.Enqueue<IUserPasswordResetInvitationService>(service => service.CreateAndSendPasswordResetInvitation(
            command.TenantId,
            command.OrganisationId,
            command.Email,
            command.Environment,
            command.IsPasswordExpired,
            null,
            command.ProductId));

        return Unit.Value;
    }
}
