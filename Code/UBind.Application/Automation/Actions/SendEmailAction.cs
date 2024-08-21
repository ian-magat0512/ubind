// <copyright file="SendEmailAction.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Actions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using MimeKit;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NodaTime;
using StackExchange.Profiling;
using UBind.Application.Automation.Data;
using UBind.Application.Automation.Email;
using UBind.Application.Automation.Enums;
using UBind.Application.Automation.Error;
using UBind.Application.Automation.Helper;
using UBind.Application.Automation.Extensions;
using UBind.Application.Automation.Providers;
using UBind.Application.Services;
using UBind.Domain;
using UBind.Domain.Extensions;
using Void = UBind.Domain.Helpers.Void;
using UBind.Domain.Exceptions;

/// <summary>
/// Represents an action of type SendEmailAction.
/// </summary>
public class SendEmailAction : Action
{
    private static readonly SemaphoreSlim SendEmailSemaphore = new SemaphoreSlim(1, 1);
    private readonly IMessagingService messagingService;
    private readonly IEmailService emailService;
    private readonly ILogger<SendEmailAction> logger;
    private readonly ICachingResolver cachingResolver;
    private readonly IClock clock;

    /// <summary>
    /// Initializes a new instance of the <see cref="SendEmailAction"/> class.
    /// </summary>
    /// <param name="messagingService">The messaging service.</param>
    /// <param name="name">The name of the action.</param>
    /// <param name="alias">The alias of the action.</param>
    /// <param name="description">The action description.</param>
    /// <param name="asynchronous">The asynchronous to be used.</param>
    /// <param name="runCondition">An optional condition.</param>
    /// <param name="beforeRunErrorConditions">The validation rules before the action.</param>
    /// <param name="afterRunErrorConditions">The validation rules after the action.</param>
    /// <param name="onErrorActions">The list of non successful actions.</param>
    /// <param name="outboundEmailServerAlias">The alias of the outbound mail server.</param>
    /// <param name="email">The email definition that the action will result in.</param>
    /// <param name="tags">The list of tags of this email.</param>
    /// <param name="relationships">The list of relationships of this email.</param>
    /// <param name="emailService">The email service.</param>
    /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
    public SendEmailAction(
        IMessagingService messagingService,
        string name,
        string alias,
        string description,
        bool asynchronous,
        IProvider<Data<bool>> runCondition,
        IEnumerable<ErrorCondition> beforeRunErrorConditions,
        IEnumerable<ErrorCondition> afterRunErrorConditions,
        IEnumerable<Action> onErrorActions,
        IProvider<Data<string>> outboundEmailServerAlias,
        EmailConfiguration email,
        IEnumerable<IProvider<Data<string>>> tags,
        IEnumerable<RelationshipConfiguration>? relationships,
        IEmailService emailService,
        ICachingResolver cachingResolver,
        ILogger<SendEmailAction> logger,
        IClock clock)
        : base(
              name,
              alias,
              description,
              asynchronous,
              runCondition,
              beforeRunErrorConditions,
              afterRunErrorConditions,
              onErrorActions)
    {
        this.logger = logger;
        this.cachingResolver = cachingResolver;
        this.OutboundEmailServerAlias = outboundEmailServerAlias;
        this.Email = email;
        this.messagingService = messagingService;
        this.Tags = tags;
        this.Relationships = relationships;
        this.emailService = emailService;
        this.clock = clock;
    }

    /// <summary>
    /// Gets the outbound server alias.
    /// </summary>
    public IProvider<Data<string>> OutboundEmailServerAlias { get; }

    /// <summary>
    /// Gets the email that the action will need to send.
    /// </summary>
    public EmailConfiguration Email { get; }

    /// <summary>
    /// Gets a collection representing the list of tags to be attached to email entity.
    /// </summary>
    public IEnumerable<IProvider<Data<string>>> Tags { get; private set; }

    /// <summary>
    /// Gets a collection representing the list of tags to be attached to email entity.
    /// </summary>
    public IEnumerable<RelationshipConfiguration>? Relationships { get; private set; }

    /// <inheritdoc/>
    public override ActionData CreateActionData() => new SendEmailActionData(this.Name, this.Alias, this.clock);

    public override bool IsReadOnly() => false;

    /// <summary>
    /// Generates a resolved email and sends it.
    /// </summary>
    /// <param name="providerContext">The data and path to perform resolutions with.</param>
    /// <param name="actionData">The action data for this action.</param>
    /// <returns>An awaitable task.</returns>
    public override async Task<Result<Void, Domain.Error>> Execute(
        IProviderContext providerContext,
        ActionData actionData,
        bool isInternal = false)
    {
        using (MiniProfiler.Current.Step(nameof(SendEmailAction) + "." + nameof(this.Execute)))
        {
            this.logger.LogInformation("Executing \"SendEmailAction\" " + actionData.Alias);
            var data = providerContext.AutomationData;
            actionData.UpdateState(ActionState.Running);
            var emailData = await this.Email.ResolveEmailProperties(providerContext);
            if (actionData is SendEmailActionData sendEmailActionDataForEmail)
            {
                sendEmailActionDataForEmail.Email = emailData;
            }

            using (var mailMessage = this.Email.ConvertToMailMessage(emailData))
            {
                try
                {
                    await SendEmailSemaphore.WaitAsync();
                    var tenantId = data.ContextManager.Tenant.Id;
                    var organisationId = data.ContextManager.Organisation.Id;
                    await this.messagingService.SendAsync(tenantId, organisationId, mailMessage);
                    var resolveTags = this.Tags != null ? await this.Tags.SelectAsync(async oa => (await oa.Resolve(providerContext)).GetValueOrThrowIfFailed()) : null;
                    var tags = resolveTags?.Where(c => !string.IsNullOrEmpty(c)).Select(c => c.DataValue).ToList();
                    var relationships = await RelationshipHelper.ResolveMessageRelationships(providerContext, this.Relationships, tenantId);

                    Guid.TryParse(data.Automation[AutomationData.ProductId]?.ToString(), out Guid productId);
                    this.emailService.SaveMailMessage(
                        tenantId,
                        mailMessage,
                        organisationId,
                        productId,
                        data.System.Environment,
                        tags,
                        relationships);
                    if (tags != null && tags.Any())
                    {
                        if (actionData is SendEmailActionData sendEmailActionDataForTags)
                        {
                            sendEmailActionDataForTags.Tags = tags;
                        }
                    }

                    if (relationships != null && relationships.Any())
                    {
                        if (actionData is SendEmailActionData sendEmailActionDataForRelationships)
                        {
                            sendEmailActionDataForRelationships.Relationships
                                = relationships.Select(r => new UBind.Domain.SerialisedEntitySchemaObject.Relationship(
                                r.RelationshipType,
                                r.SourceEntity,
                                r.TargetEntity)).ToList();
                        }
                    }

                    this.logger.LogInformation("\"SendEmailAction\" " + actionData.Alias + " - Success");
                    return Result.Success<Void, Domain.Error>(default);
                }
                catch (SmtpException ex)
                {
                    JObject errorData = await this.CreateErrorData(providerContext, mailMessage, ex);
                    var errorDetails = Errors.Automation.Action.SendEmailError(this.Alias, errorData);
                    return Result.Failure<Void, Domain.Error>(errorDetails);
                }
                catch (Exception ex)
                {
                    JObject errorData = await this.CreateErrorData(providerContext, mailMessage, ex);
                    var errorDetails = Errors.Automation.Action.SendEmailError(this.Alias, errorData);
                    throw new ErrorException(errorDetails, ex);
                }
                finally
                {
                    SendEmailSemaphore.Release();

                    // need to dispose all attachment to release memory stream
                    mailMessage.Dispose();
                }
            }
        }
    }

    private async Task<JObject> CreateErrorData(IProviderContext providerContext, MimeMessage mailMessage, Exception ex)
    {
        JObject errorData = await providerContext.GetDebugContext();
        errorData.Add(ErrorDataKey.ErrorMessage, ex.Message);
        errorData.Add("emailSubject", mailMessage.Subject);
        errorData.Add("emailMessage", mailMessage.HtmlBody);
        errorData.Add("emailTo", string.Join(", ", mailMessage.To.ToString()));
        return errorData;
    }
}
