// <copyright file="DkimPublicKeyLocator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DnsClient;
    using Heijden.DNS;
    using Heijden.Dns.Portable;
    using MimeKit.Cryptography;
    using Org.BouncyCastle.Crypto;

    /// <summary>
    /// This class is used as an implementation of DkimPublicKeyLocatorBase that locates and caches DKIM public keys.
    /// This class uses the DnsClient and Resolver libraries to query DNS TXT records for DKIM public keys.
    /// </summary>
    public class DkimPublicKeyLocator : DkimPublicKeyLocatorBase
    {
        private readonly Dictionary<string, AsymmetricKeyParameter> cache;
        private readonly Resolver resolver;
        private readonly LookupClient lookupClient;

        public DkimPublicKeyLocator()
        {
            this.lookupClient = new LookupClient();
            this.cache = new Dictionary<string, AsymmetricKeyParameter>();
            this.resolver = new Resolver()
            {
                TransportType = TransportType.Udp,
                UseCache = true,
                Retries = 3,
            };
        }

        public override AsymmetricKeyParameter LocatePublicKey(string methods, string domain, string selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var query = $"{selector}._domainkey.{domain}";
            if (this.cache.TryGetValue(query, out var pubkey))
            {
                return pubkey;
            }

            var response = this.lookupClient.Query(query, QueryType.TXT);

            List<string> textList = response.Answers.TxtRecords()
            .SelectMany(record => record.Text)
            .ToList();

            return this.GetPublicKey(textList, selector, domain);
        }

        public override async Task<AsymmetricKeyParameter> LocatePublicKeyAsync(string methods, string domain, string selector, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var query = selector + "._domainkey." + domain;
            if (this.cache.TryGetValue(query, out var pubkey))
            {
                return pubkey;
            }

            var response = await this.resolver.Query(query, QType.TXT);

            List<string> textList = response.RecordsTXT
                .SelectMany(record => record.TXT)
                .ToList();

            return this.GetPublicKey(textList, selector, domain);
        }

        private AsymmetricKeyParameter GetPublicKey(List<string> txtRecords, string selector, string domain)
        {
            var query = selector + "._domainkey." + domain;
            if (this.cache.TryGetValue(query, out var pubkey))
            {
                return pubkey;
            }

            var builder = new StringBuilder();
            foreach (var text in txtRecords)
            {
                builder.Append(text);
            }

            var txt = builder.ToString();
            pubkey = GetPublicKey(txt);
            this.cache.Add(query, pubkey);
            return pubkey;
        }
    }
}
