// <copyright file="SmsConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.Sms
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UBind.Application.Automation.Extensions;
    using UBind.Application.Automation.Providers;
    using UBind.Domain.Extensions;

    public class SmsConfiguration
    {
        public SmsConfiguration(
            IEnumerable<IProvider<Data<string>>> to,
            IProvider<Data<string>>? from,
            IProvider<Data<string>> content)
        {
            this.To = to;
            this.From = from;
            this.Message = content;
        }

        public IEnumerable<IProvider<Data<string>>> To { get; } = Enumerable.Empty<IProvider<Data<string>>>();

        public IProvider<Data<string>>? From { get; }

        public IProvider<Data<string>> Message { get; }

        public async Task<Sms> ResolveSmsProperties(IProviderContext providerContext)
        {
            var resolveTo = await this.To.SelectAsync(async t => await t.Resolve(providerContext));
            var to = resolveTo.Select(t => t.GetValueOrThrowIfFailed().DataValue);
            string message = (await this.Message.Resolve(providerContext)).GetValueOrThrowIfFailed().DataValue;
            string? from = (await this.From.ResolveValueIfNotNull(providerContext))?.DataValue;
            return new Sms(to, from, message);
        }
    }
}
