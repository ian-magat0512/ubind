// <copyright file="SignEmailWithDkimCommandHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Commands.DkimSettings
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using MimeKit;
    using MimeKit.Cryptography;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;

    /// <summary>
    /// Handler for <see cref="SignEmailWithDkimCommandHandler"/> command.
    /// </summary>
    public class SignEmailWithDkimCommandHandler
        : ICommandHandler<SignEmailWithDkimCommand, Unit>
    {
        private readonly ILogger<SignEmailWithDkimCommand> logger;
        private readonly ICachingResolver cachingResolver;
        private readonly IOrganisationReadModelRepository organisationReadModelRepository;
        private readonly IDkimSettingRepository dkimSettingRepository;
        private readonly IDkimHeaderVerifier dkimVerifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="SignEmailWithDkimCommandHandler"/> class.
        /// </summary>
        public SignEmailWithDkimCommandHandler(
            IDkimSettingRepository dkimSettingRepository,
            IDkimHeaderVerifier dkimVerifier,
            ICachingResolver cachingResolver,
            IOrganisationReadModelRepository organisationReadModelRepository,
            ILogger<SignEmailWithDkimCommand> logger)
        {
            this.dkimSettingRepository = dkimSettingRepository;
            this.cachingResolver = cachingResolver;
            this.organisationReadModelRepository = organisationReadModelRepository;
            this.dkimVerifier = dkimVerifier;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<Unit> Handle(
            SignEmailWithDkimCommand request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var tenant = await this.cachingResolver.GetTenantOrThrow(new GuidOrAlias(request.TenantId));
            var organisationId = tenant.Details.DefaultOrganisationId;
            var dkimSetting = this.dkimSettingRepository.GetDkimSettingsbyOrganisationIdAndDomainName(request.TenantId, organisationId, request.MimeMessage?.From?.Mailboxes?.First()?.Domain).FirstOrDefault();
            var organisation = this.organisationReadModelRepository.Get(request.TenantId, organisationId);
            if (organisation == null)
            {
                throw new ErrorException(Errors.General.NotFound("Organisation", organisationId));
            }

            var isDomainMatchesWithConfiguredApplicableDomain = DomainNameHelper.DoesDomainMatchPartially(request.MimeMessage?.From?.Mailboxes?.First()?.Domain, dkimSetting?.ApplicableDomainNameList);
            if (!isDomainMatchesWithConfiguredApplicableDomain)
            {
                return await Task.FromResult(Unit.Value);
            }

            try
            {
                var headers = new HeaderId[] { HeaderId.From, HeaderId.Subject, HeaderId.Date };
                byte[] byteArray = Encoding.ASCII.GetBytes(dkimSetting.PrivateKey);
                MemoryStream privateKeyMemorySteam = new MemoryStream(byteArray);
                var agentOrIdentifier = !string.IsNullOrEmpty(dkimSetting.AgentOrUserIdentifier) ? dkimSetting.AgentOrUserIdentifier.Trim() : string.Empty;
                var signer = new DkimSigner(privateKeyMemorySteam, dkimSetting.DomainName, dkimSetting.DnsSelector, DkimSignatureAlgorithm.RsaSha256)
                {
                    HeaderCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple,
                    BodyCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Simple,
                    AgentOrUserIdentifier = agentOrIdentifier,
                    QueryMethod = "dns/txt",
                };
                request.MimeMessage.Prepare(EncodingConstraint.EightBit);
                signer.Sign(request.MimeMessage, headers);
                var isValidDkimSignature = this.dkimVerifier.Verify(request.MimeMessage);
                if (!isValidDkimSignature)
                {
                    this.dkimVerifier.RemoveDkimSignature(request.MimeMessage);
                }
            }
            catch (FormatException ex)
            {
                var logCannotSignAnEmailWithDKIM = LoggerMessage.Define<Guid, string>(
                    LogLevel.Error,
                    new EventId(1, "CannotSignAnEmailWithDKIM"),
                    "Cannot sign an email with DKIM for organisation Id {organisationId}. DKIM settings is invalid. error details: {errorMessage}");
                logCannotSignAnEmailWithDKIM(this.logger, organisationId, ex.Message, ex);
                this.dkimVerifier.RemoveDkimSignature(request.MimeMessage);
                if (request.EmailSource == EmailSource.Automation || request.EmailSource == EmailSource.TestEmail)
                {
                    if (ex.Message.Contains("Private key not found."))
                    {
                        throw new ErrorException(
                        Errors.DkimSetting.InvalidPrivateKey(
                            tenant.Id,
                            tenant.Details.Name,
                            organisation.Id,
                            organisation.Name,
                            ex.Message));
                    }
                    else if (ex.Message.Contains("Public key parameters not found in DNS TXT record."))
                    {
                        throw new ErrorException(
                        Errors.DkimSetting.InvalidDnsSelector(
                            tenant.Id,
                            tenant.Details.Name,
                            organisation.Id,
                            organisation.Name,
                            ex.Message));
                    }
                    else if (ex.Message.Contains("the domain in the AUID does not match the domain parameter"))
                    {
                        throw new ErrorException(
                        Errors.DkimSetting.InvalidDomainName(
                            tenant.Id,
                            tenant.Details.Name,
                            organisation.Id,
                            organisation.Name,
                            ex.Message));
                    }
                    else
                    {
                        throw new ErrorException(
                        Errors.DkimSetting.InvalidDKIMSettings(
                            tenant.Id,
                            tenant.Details.Name,
                            organisation.Id,
                            organisation.Name,
                            ex.Message));
                    }
                }
            }

            return await Task.FromResult(Unit.Value);
        }
    }
}
