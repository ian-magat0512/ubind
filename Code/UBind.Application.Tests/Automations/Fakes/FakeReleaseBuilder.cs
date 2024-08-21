// <copyright file="FakeReleaseBuilder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations.Fakes
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Tests.Fakes;

    /// <summary>
    /// Builder for creating fake releases for testing.
    /// </summary>
    public class FakeReleaseBuilder
    {
        private IClock clock = new SequentialClock();
        private bool isDevRelease;
        private Guid tenantId = TenantFactory.DefaultId;
        private Guid productId = ProductFactory.DefaultId;
        private string label = "Fake release";
        private int majorReleaseNumber = 1;
        private int minorReleaseNumber = 1;
        private string quoteFormConfigurationJson = "{}";
        private string quoteWorkflowJson = "{}";
        private string integrationJson = "{}";
        private string automationJson = "{}";
        private string quotePaymentJson = "{}";
        private string quotePaymentFormJson = "{}";
        private string quoteFundingJson = "{}";
        private string quoteProductConfigurationJson = "{}";
        private List<Asset> quoteFiles = new List<Asset>();
        private List<Asset> quoteAssets = new List<Asset>();
        private byte[] quoteWorkbookContent;

        private string claimFormConfigurationJson = "{}";
        private string claimWorkflowJson = "{}";
        private string claimPaymentFormJson = "{}";
        private string claimPaymentJson = "{}";
        private string claimFundingJson = "{}";
        private string claimProductConfigurationJson = "{}";
        private List<Asset> claimFiles = new List<Asset>();
        private List<Asset> claimAssets = new List<Asset>();
        private byte[] claimWorkbookContent;

        private FakeReleaseBuilder(Guid tenantId, Guid productId, bool isDevRelease)
        {
            this.isDevRelease = isDevRelease;
            this.tenantId = tenantId;
            this.productId = productId;
        }

        public static FakeReleaseBuilder CreateForProduct(
            Guid? tenantId = null,
            Guid? productId = null)
        {
            tenantId = tenantId ?? TenantFactory.DefaultId;
            productId = productId ?? ProductFactory.DefaultId;

            return new FakeReleaseBuilder(tenantId.Value, productId.Value, false);
        }

        public FakeReleaseBuilder WithQuoteFile(string fileName, string content)
        {
            this.quoteFiles.Add(this.CreateAsset(fileName, FileContent.CreateFromBytes(this.tenantId, Guid.NewGuid(), Encoding.UTF8.GetBytes(content))));
            return this;
        }

        public FakeReleaseBuilder WithClaimFile(string fileName, string content)
        {
            this.claimFiles.Add(this.CreateAsset(fileName, FileContent.CreateFromBytes(this.tenantId, Guid.NewGuid(), Encoding.UTF8.GetBytes(content))));
            return this;
        }

        public FakeReleaseBuilder WithQuoteAsset(string fileName, string content)
        {
            this.quoteAssets.Add(this.CreateAsset(fileName, FileContent.CreateFromBytes(this.tenantId, Guid.NewGuid(), Encoding.UTF8.GetBytes(content))));
            return this;
        }

        public FakeReleaseBuilder WithClaimAsset(string fileName, string content)
        {
            this.claimAssets.Add(this.CreateAsset(fileName, FileContent.CreateFromBytes(this.tenantId, Guid.NewGuid(), Encoding.UTF8.GetBytes(content))));
            return this;
        }

        public FakeReleaseBuilder WithQuoteFormConfiguration(string configurationJson)
        {
            this.quoteFormConfigurationJson = configurationJson;
            return this;
        }

        public FakeReleaseBuilder WithClaimFormConfiguration(string configurationJson)
        {
            this.claimFormConfigurationJson = configurationJson;
            return this;
        }

        public FakeReleaseBuilder WithQuoteProductConfiguration(string productConfigurationJson)
        {
            this.quoteProductConfigurationJson = productConfigurationJson;
            return this;
        }

        public FakeReleaseBuilder WithClaimProductConfiguration(string productConfigurationJson)
        {
            this.claimProductConfigurationJson = productConfigurationJson;
            return this;
        }

        public FakeReleaseBuilder WithQuoteWorkbookContent(byte[] workbookContext)
        {
            this.quoteWorkbookContent = workbookContext;
            return this;
        }

        public FakeReleaseBuilder WithClaimWorkbookContent(byte[] workbookContext)
        {
            this.claimWorkbookContent = workbookContext;
            return this;
        }

        public FakeReleaseBuilder WithAutomationsJson(string json)
        {
            this.automationJson = json;
            return this;
        }

        public Release BuildRelease(Instant time = default) => (Release)this.Build(time);

        public DevRelease BuildDevRelease(Instant time = default)
        {
            this.isDevRelease = true;
            return (DevRelease)this.Build(time);
        }

        private ReleaseBase Build(Instant time)
        {
            if (time == default)
            {
                time = SystemClock.Instance.Now();
            }

            var quoteDetails = new ReleaseDetails(
                WebFormAppType.Quote,
                this.quoteFormConfigurationJson,
                this.quoteWorkflowJson,
                this.integrationJson,
                this.automationJson,
                this.quotePaymentFormJson,
                this.quotePaymentJson,
                this.quoteFundingJson,
                this.quoteProductConfigurationJson,
                this.quoteFiles,
                this.quoteAssets,
                this.quoteWorkbookContent,
                time);
            var claimDetails = new ReleaseDetails(
                WebFormAppType.Claim,
                this.claimFormConfigurationJson,
                this.claimWorkflowJson,
                null,
                null,
                this.claimPaymentFormJson,
                this.claimPaymentJson,
                this.claimFundingJson,
                this.claimProductConfigurationJson,
                this.claimFiles,
                this.claimAssets,
                this.claimWorkbookContent,
                time);

            ReleaseBase release = this.isDevRelease
                ? (ReleaseBase)new DevRelease(this.tenantId, this.productId, time)
                : new Release(
                    this.tenantId,
                    this.tenantId,
                    this.majorReleaseNumber,
                    this.minorReleaseNumber,
                    this.label,
                    ReleaseType.Major,
                    time);
            release.QuoteDetails = quoteDetails;
            release.ClaimDetails = claimDetails;
            return release;
        }

        private Asset CreateAsset(string filename, FileContent content) => new Asset(Guid.NewGuid(), filename, this.clock.Now(), content, this.clock.Now());
    }
}
