// <copyright file="SystemEmailTemplateRepositoryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1600

namespace UBind.Persistence.Tests.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Persistence.Entities;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class SystemEmailTemplateRepositoryTests
    {
        private readonly IClock clock = SystemClock.Instance;
        private Guid tenantId;
        private Guid tenantId2;
        private Guid productId;
        private Guid productId2;
        private Guid portalId;
        private Guid portalId2;

        public SystemEmailTemplateRepositoryTests()
        {
            this.tenantId = Guid.NewGuid();
            this.tenantId2 = Guid.NewGuid();
            this.productId = Guid.NewGuid();
            this.productId2 = Guid.NewGuid();
            this.portalId = Guid.NewGuid();
            this.portalId2 = Guid.NewGuid();
        }

        [Fact]
        public void GetApplicableTemplates_ReturnsNoTemplates_WhenNoMatchesExist()
        {
            // Arrange
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new SystemEmailTemplateRepository(dbContext);

            // Act
            var templates = sut
                .GetApplicableTemplates(
                Guid.NewGuid(),
                SystemEmailType.AccountActivationInvitation,
                Guid.NewGuid(),
                null);

            // Assert
            Assert.False(templates.Any());
        }

        [Fact(Skip = "Needs rewriting as part of UB-2063")]
        public void GetApplicableTemplates_ReturnsMasterTemplate_WhenOnlyMasterTemplateExists()
        {
            // Arrange
            this.SetupMasterTemplate();
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId2,
                this.productId2); // Irrelevant templates
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new SystemEmailTemplateRepository(dbContext);

            // Act
            var templates = sut.GetApplicableTemplates(
                Guid.NewGuid(),
                SystemEmailType.AccountActivationInvitation,
                Guid.NewGuid(),
                Guid.NewGuid());

            // Assert
            var masterTemplate = templates.Single();
            Assert.True(masterTemplate.TenantId == Tenant.MasterTenantId);
        }

        [Fact(Skip = "Needs rewriting as part of UB-2063")]
        public void GetApplicableTemplates_ReturnsTenantTemplate_WhenOnlyTenantTemplateExists()
        {
            // Arrange
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId2,
                this.productId2); // Irrelevant templates
            this.SetupTenantLevelTemplate(
                this.tenantId); // Relevant template
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new SystemEmailTemplateRepository(dbContext);

            // Act
            var templates = sut.GetApplicableTemplates(
                this.tenantId,
                SystemEmailType.AccountActivationInvitation,
                this.productId,
                null);

            // Assert
            var template = templates.Single();
            Assert.True(template.TenantId == this.tenantId);
            Assert.Null(template.ProductId);
        }

        [Fact(Skip = "Needs rewriting as part of UB-2063")]
        public void GetApplicableTemplates_ReturnsProductTemplate_WhenOnlyProductTemplateExists()
        {
            // Arrange
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId2,
                this.productId2); // Irrelevant templates
            this.SetupProductLevelTemplate(
                this.tenantId,
                this.productId); // Relevant template
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new SystemEmailTemplateRepository(dbContext);

            // Act
            var templates = sut.GetApplicableTemplates(
                this.tenantId,
                SystemEmailType.AccountActivationInvitation,
                this.productId,
                null);

            // Assert
            var template = templates.Single();
            Assert.Equal(this.tenantId, template.TenantId);
            Assert.Equal(this.productId, template.ProductId);
        }

        [Fact]
        public void GetApplicableTemplates_ReturnstenantAndProductTemplates_WhenBothExists()
        {
            // Arrange
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId2,
                this.productId2); // Irrelevant templates
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId,
                this.productId); // Relevant template
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new SystemEmailTemplateRepository(dbContext);

            // Act
            var templates = sut.GetApplicableTemplates(
                this.tenantId,
                SystemEmailType.AccountActivationInvitation,
                this.productId,
                null);

            // Assert
            Assert.Equal(2, templates.Count());
            var tenantTemplate = templates.First();
            Assert.Equal(this.tenantId, tenantTemplate.TenantId);
            Assert.Null(tenantTemplate.ProductId);
            var productTemplate = templates.Last();
            Assert.Equal(this.tenantId, productTemplate.TenantId);
            Assert.Equal(this.productId, productTemplate.ProductId);
        }

        [Fact(Skip = "Temporarily skipping due to data change for release 9.0 - reenable this test afterwards")]
        public void GetApplicableTemplates_ReturnsAllTemplatesInTheRightOrder_WhenAllExists()
        {
            // Arrange
            this.SetupMasterTemplate();
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId2,
                this.productId2); // Irrelevant templates
            this.SetupPortalLevelTemplate(this.tenantId2, this.portalId2);
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId,
                this.productId); // Relevant template
            this.SetupPortalLevelTemplate(this.tenantId, this.portalId);
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new SystemEmailTemplateRepository(dbContext);

            // Act
            var templates = sut.GetApplicableTemplates(
                this.tenantId,
                SystemEmailType.AccountActivationInvitation,
                this.productId,
                this.portalId);

            // Assert
            // This should return 4 templates, in this order:
            // 1. Master defined template
            // 2. Tenant defined template
            // 3. Product defined template
            // 4. Portal defined template
            Assert.Equal(4, templates.Count());
            var masterTemplate = templates.First();
            Assert.Equal(Tenant.MasterTenantId, masterTemplate.TenantId);
            Assert.Null(masterTemplate.ProductId);
            Assert.Null(masterTemplate.PortalId);
            var tenantTemplate = templates.Skip(1).First();
            Assert.Equal(this.tenantId, tenantTemplate.TenantId);
            Assert.Equal(this.tenantId, tenantTemplate.TenantId);
            Assert.Null(tenantTemplate.ProductId);
            Assert.Null(tenantTemplate.PortalId);
            var productTemplate = templates.Skip(2).First();
            Assert.Equal(this.tenantId, productTemplate.TenantId);
            Assert.Equal(this.productId, productTemplate.ProductId);
            Assert.Equal(this.tenantId, productTemplate.TenantId);
            Assert.Equal(this.productId, productTemplate.ProductId);
            Assert.Null(tenantTemplate.PortalId);
            var portalTemplate = templates.Last();
            Assert.Equal(this.tenantId, portalTemplate.TenantId);
            Assert.Equal(this.tenantId, portalTemplate.TenantId);
            Assert.Equal(this.portalId, portalTemplate.PortalId);
            Assert.Null(portalTemplate.ProductId);
            Assert.Null(portalTemplate.ProductId);
        }

        [Fact]
        public void GetApplicableTemplates_ExcludesProductTemplates_WhenRequestIsForTenantLevelTemplates()
        {
            // Arrange
            this.SetupMasterTemplate();
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId2,
                this.productId2); // Irrelevant templates
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId,
                this.productId); // Relevant template
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new SystemEmailTemplateRepository(dbContext);

            // Act
            var templates = sut.GetApplicableTemplates(
                this.tenantId,
                SystemEmailType.AccountActivationInvitation,
                null,
                null);

            // Assert
            Assert.Equal(2, templates.Count());
            var masterTemplate = templates.First();
            Assert.Equal(Tenant.MasterTenantId, masterTemplate.TenantId);
            Assert.Null(masterTemplate.ProductId);
            var tenantTemplate = templates.Last();
            Assert.Equal(this.tenantId, tenantTemplate.TenantId);
            Assert.Null(tenantTemplate.ProductId);
        }

        [Fact]
        public void GetApplicableTemplates_ExcludesMasterTemplate_WhenDisabled()
        {
            // Arrange
            this.SetupMasterTemplate(enabled: false);
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId2,
                this.productId2); // Irrelevant templates
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId,
                this.productId); // Relevant template
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new SystemEmailTemplateRepository(dbContext);

            // Act
            IEnumerable<ISystemEmailTemplateSummary> templates = sut.GetApplicableTemplates(
                this.tenantId,
                SystemEmailType.AccountActivationInvitation,
                this.productId,
                null);

            // Assert
            templates.Where(t => t.Enabled == false && t.TenantId != Tenant.MasterTenantId).Should().HaveCount(0);
        }

        [Fact]
        public void GetApplicableTemplates_ExcludesProductTemplates_WhenDisabled()
        {
            // Arrange
            this.SetupMasterTemplate();
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId2,
                this.productId2); // Irrelevant templates
            this.SetupTenantLevelTemplate(
                this.tenantId); // Relevant template
            this.SetupProductLevelTemplate(
                this.tenantId,
                this.productId,
                enabled: false); // Relevant template
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new SystemEmailTemplateRepository(dbContext);

            // Act
            IEnumerable<ISystemEmailTemplateSummary> templates = sut.GetApplicableTemplates(
                this.tenantId,
                SystemEmailType.AccountActivationInvitation,
                this.productId,
                null);

            // Assert
            Assert.Single(templates.Where(t => t.TenantId != Tenant.MasterTenantId));
        }

        [Fact]
        public void GetApplicableTemplates_ExcludesTenantTemplates_WhenDisabled()
        {
            // Arrange
            this.SetupMasterTemplate();
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId2,
                this.productId2); // Irrelevant templates
            this.SetupTenantLevelTemplate(
                this.tenantId,
                enabled: false); // Relevant template
            this.SetupProductLevelTemplate(
                this.tenantId,
                this.productId); // Relevant template
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new SystemEmailTemplateRepository(dbContext);

            // Act
            IEnumerable<ISystemEmailTemplateSummary> templates = sut.GetApplicableTemplates(
                this.tenantId,
                SystemEmailType.AccountActivationInvitation,
                this.productId,
                null);

            // Assert
            Assert.Single(templates.Where(t => t.TenantId != Tenant.MasterTenantId));
        }

        [Fact]
        public void GetApplicableTemplates_ReturnsTheSameEmailType_ForRenewalInvitation()
        {
            // Arrange
            var emailType = SystemEmailType.RenewalInvitation;
            this.SetupMasterTemplate();
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId2,
                this.productId2); // Irrelevant templates
            this.SetupProductLevelTemplateByType(
                this.tenantId,
                this.productId,
                emailType); // Relevant template
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new SystemEmailTemplateRepository(dbContext);

            // Act
            var templates = sut.GetApplicableTemplates(
                this.tenantId,
                emailType,
                this.productId,
                null);

            // Assert
            var template = templates.First();
            Assert.Equal(template.Type, emailType);
        }

        [Fact]
        public void GetApplicableTemplates_ReturnsTheSameEmailType_ForActivationInvitation()
        {
            // Arrange
            var emailType = SystemEmailType.AccountActivationInvitation;
            this.SetupMasterTemplate();
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId2,
                this.productId2); // Irrelevant templates
            this.SetupProductLevelTemplateByType(
                this.tenantId,
                this.productId,
                emailType); // Relevant template
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new SystemEmailTemplateRepository(dbContext);

            // Act
            var templates = sut.GetApplicableTemplates(
                this.tenantId,
                emailType,
                this.productId,
                null);

            // Assert
            var template = templates.First();
            Assert.Equal(template.Type, emailType);
        }

        [Fact]
        public void GetApplicableTemplates_ReturnsTheSameEmailType_ForResetPasswordInvitation()
        {
            // Arrange
            var emailType = SystemEmailType.PasswordResetInvitation;
            this.SetupMasterTemplate();
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId2,
                this.productId2); // Irrelevant templates
            this.SetupProductLevelTemplateByType(
                this.tenantId,
                this.productId,
                emailType); // Relevant template
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new SystemEmailTemplateRepository(dbContext);

            // Act
            var templates = sut.GetApplicableTemplates(
                this.tenantId,
                emailType,
                this.productId,
                null);

            // Assert
            var template = templates.First();
            Assert.Equal(template.Type, emailType);
        }

        [Fact]
        public void GetApplicableTemplates_ExcludesAllTemplates_WhenDisabled()
        {
            // Arrange
            this.SetupMasterTemplate(enabled: false);
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId2,
                this.productId2); // Irrelevant templates
            this.SetupAllTemplatesForTenantAndProduct(
                this.tenantId,
                this.productId,
                enabled: false); // Relevant template
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new SystemEmailTemplateRepository(dbContext);

            // Act
            var templates = sut.GetApplicableTemplates(
                this.tenantId,
                SystemEmailType.AccountActivationInvitation,
                this.productId,
                null);

            // Assert
            Assert.False(templates.Any());
        }

        [Fact]
        public void GetApplicableTemplates_RecordHasNewIds_WhenCreatingTenantLevelTemplate()
        {
            // Arrange
            this.SetupTenantLevelTemplate(this.tenantId, true);
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new SystemEmailTemplateRepository(dbContext);

            // Act
            var templates = sut.GetApplicableTemplates(
                this.tenantId,
                SystemEmailType.AccountActivationInvitation,
                null,
                null);

            // Assert
            var template = templates.LastOrDefault();
            template.Should().NotBeNull();
            template.TenantId.Should().Be(this.tenantId);
            template.ProductId.Should().BeNull();
        }

        [Fact]
        public void GetApplicableTemplates_RecordHasNewIds_WhenCreatingProductLevelTemplate()
        {
            // Arrange
            this.SetupProductLevelTemplate(this.tenantId, this.productId, true);
            var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString);
            var sut = new SystemEmailTemplateRepository(dbContext);

            // Act
            var templates = sut.GetApplicableTemplates(
                this.tenantId,
                SystemEmailType.AccountActivationInvitation,
                this.productId,
                null);

            // Assert
            var template = templates.LastOrDefault();
            template.Should().NotBeNull();
            template.TenantId.Should().Be(this.tenantId);
            template.ProductId.Should().Be(this.productId);
        }

        private void SetupMasterTemplate(bool enabled = true)
        {
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var sut = new SystemEmailTemplateRepository(dbContext);
                var templates = sut.GetApplicableTemplates(
                    Tenant.MasterTenantId, SystemEmailType.AccountActivationInvitation, null, null);

                if (!templates.Any())
                {
                    var data = SystemEmailTemplateData.DefaultActivationData;
                    var masterTemplate = SystemEmailTemplate.CreateTenantEmailTemplateSetting(
                        Tenant.MasterTenantId,
                        SystemEmailType.AccountActivationInvitation,
                        data,
                        this.clock.Now());
                    if (enabled)
                    {
                        masterTemplate.Enable();
                    }

                    sut.Insert(masterTemplate);
                    sut.SaveChanges();
                }
            }
        }

        private void SetupAllTemplatesForTenantAndProduct(
            Guid tenantId,
            Guid productId,
            bool enabled = true)
        {
            this.SetupTenantLevelTemplate(tenantId, enabled);
            this.SetupProductLevelTemplate(tenantId, productId, enabled);
        }

        private void SetupTenantLevelTemplate(
            Guid tenantId,
            bool enabled = true)
        {
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var sut = new SystemEmailTemplateRepository(dbContext);
                var data = SystemEmailTemplateData.DefaultActivationData;
                var tenantTemplate = SystemEmailTemplate.CreateTenantEmailTemplateSetting(
                    tenantId, SystemEmailType.AccountActivationInvitation, data, this.clock.Now());
                if (enabled)
                {
                    tenantTemplate.Enable();
                }

                sut.Insert(tenantTemplate);
                sut.SaveChanges();
            }
        }

        private void SetupProductLevelTemplate(
            Guid tenantId,
            Guid productId,
            bool enabled = true)
        {
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var sut = new SystemEmailTemplateRepository(dbContext);
                var data = SystemEmailTemplateData.DefaultActivationData;
                var productTemplate = SystemEmailTemplate.CreateProductEmailTemplateSetting(
                    tenantId,
                    productId,
                    SystemEmailType.AccountActivationInvitation,
                    data,
                    this.clock.Now());
                if (enabled)
                {
                    productTemplate.Enable();
                }

                sut.Insert(productTemplate);
                sut.SaveChanges();
            }
        }

        private void SetupProductLevelTemplateByType(
            Guid tenantId,
            Guid productId,
            SystemEmailType systemEmailType,
            bool enabled = true)
        {
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var sut = new SystemEmailTemplateRepository(dbContext);
                var data = SystemEmailTemplateData.DefaultActivationData;
                var productTemplate = SystemEmailTemplate.CreateProductEmailTemplateSetting(
                    tenantId,
                    productId,
                    systemEmailType,
                    data,
                    this.clock.Now());
                if (enabled)
                {
                    productTemplate.Enable();
                }

                sut.Insert(productTemplate);
                sut.SaveChanges();
            }
        }

        private void SetupPortalLevelTemplate(
            Guid tenantId,
            Guid portalId,
            bool enabled = true)
        {
            using (var dbContext = new UBindDbContext(DatabaseFixture.TestConnectionString))
            {
                var sut = new SystemEmailTemplateRepository(dbContext);
                var data = SystemEmailTemplateData.DefaultActivationData;
                var template = SystemEmailTemplate.CreatePortalEmailTemplateSetting(
                    tenantId,
                    SystemEmailType.AccountActivationInvitation,
                    portalId,
                    data,
                    this.clock.Now());
                if (enabled)
                {
                    template.Enable();
                }

                sut.Insert(template);
                sut.SaveChanges();
            }
        }
    }
}
