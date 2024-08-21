// <copyright file="ApplicationViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ViewModels
{
    /// <summary>
    /// View model for applications.
    /// </summary>
    public class ApplicationViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationViewModel"/> class.
        /// </summary>
        /// <param name="tenant">The ID of the tenant the application is for.</param>
        /// <param name="portal">The portal which the application or customer will be associated with.</param>
        /// <param name="organisation">The alias of the organisation the application is for.</param>
        /// <param name="isDefaultOrganisation">Indicates whether the organisation is a default of a tenancy.</param>
        /// <param name="productId">The ID of the product the application is for.</param>
        /// <param name="productAlias">The alias of the product the application is for.</param>
        /// <param name="environment">The environment to use for the application.</param>
        /// <param name="quoteId">The ID of the quote, otherwise null.</param>
        /// <param name="policyId">The ID of the policy, otherwise null.</param>
        /// <param name="mode">the quoter mode.</param>
        /// <param name="testMode">The value indicating whether the form submission should be considered test data and not a real submission.</param>
        /// <param name="debug">The value indicating whether to render debugging information on the form.</param>
        /// <param name="debugLevel">The debug level. A number, the higher the nunber the more debugging details are shown.</param>
        /// <param name="isLoadedWithinPortal">True if the web form is loaded within the portal.</param>
        /// <param name="quoteType">The type of quote.</param>
        /// <param name="version">The quote version to load, otherwise null.</param>
        /// <param name="formType">The type of form to load.</param>
        /// <param name="claimId">The ID of the claim, otherwise null.</param>
        /// <param name="sidebarOffsetConfiguration">The sidebar offset configuration (e.g. "xs,100"), or null.</param>
        /// <param name="productReleaseNumber">The release number being set, or null.</param>
        public ApplicationViewModel(
            string tenant,
            string portal,
            string organisation,
            bool isDefaultOrganisation,
            string productId,
            string productAlias,
            string environment,
            string quoteId,
            string policyId,
            string mode,
            bool testMode,
            bool debug,
            int debugLevel,
            bool isLoadedWithinPortal,
            string quoteType,
            string version,
            string formType,
            string claimId,
            string sidebarOffsetConfiguration,
            string productReleaseNumber)
        {
            this.Tenant = tenant;
            this.Portal = portal;
            this.Organisation = organisation;
            this.IsDefaultOrganisation = isDefaultOrganisation.ToString();
            this.ProductId = productId;
            this.ProductAlias = productAlias;
            this.Environment = environment;
            this.QuoteId = quoteId;
            this.PolicyId = policyId;
            this.Mode = mode;
            this.IsTestData = testMode.ToString();
            this.Debug = debug.ToString();
            this.QuoteType = quoteType;
            this.Version = version;
            this.ClaimId = claimId;
            this.FormType = formType;
            this.DebugLevel = debugLevel.ToString();
            this.IsLoadedWithinPortal = isLoadedWithinPortal.ToString();
            this.SidebarOffsetConfiguration = sidebarOffsetConfiguration;
            this.ProductReleaseNumber = productReleaseNumber;
        }

        /// <summary>
        /// Gets the ID of the tenant.
        /// </summary>
        public string Tenant { get; }

        /// <summary>
        /// Gets the alias of the portal.
        /// </summary>
        public string Portal { get; }

        /// <summary>
        /// Gets the alias of the organisation.
        /// </summary>
        public string Organisation { get; }

        /// <summary>
        /// Gets a value indicating whether the organisation is a default of a tenancy in string.
        /// </summary>
        public string IsDefaultOrganisation { get; }

        /// <summary>
        /// Gets the alias of the product.
        /// </summary>
        public string ProductAlias { get; }

        /// <summary>
        /// Gets the ID of the product.
        /// </summary>
        public string ProductId { get; }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        public string Environment { get; }

        /// <summary>
        /// Gets the ID of the quote.
        /// </summary>
        public string QuoteId { get; }

        /// <summary>
        /// Gets the ID of the quote.
        /// </summary>
        public string PolicyId { get; }

        /// <summary>
        /// Gets the ID of the quoter mode.
        /// </summary>
        public string Mode { get; }

        /// <summary>
        /// Gets the ID of the quoter mode.
        /// </summary>
        public string QuoteType { get; }

        /// <summary>
        /// Gets a value indicating whether the form submission should be considered test data and not a real submission.
        /// This is used for testing in a live(production) environment, but it reduces payment amounts to 1c so that we can do
        /// a real and live test of the payment gateway.
        /// </summary>
        public string IsTestData { get; }

        /// <summary>
        /// Gets a value indicating whether to render debugging information on the form.
        /// </summary>
        public string Debug { get; }

        /// <summary>
        /// Gets a value representing the desired level of debugging output. The higher the number, the more debugging details are output.
        /// </summary>
        public string DebugLevel { get; }

        /// <summary>
        /// Gets a value indicating whether to the web form is being loaded from within the portal.
        /// </summary>
        public string IsLoadedWithinPortal { get; }

        /// <summary>
        /// Gets a value indicating the version number to load.
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// Gets the ID of the claim.
        /// </summary>
        public string ClaimId { get; }

        /// <summary>
        /// Gets the type of form.
        /// </summary>
        public string FormType { get; }

        /// <summary>
        /// Gets the sidebar offset configuration, e.g. "xs,100|md,200".
        /// </summary>
        public string SidebarOffsetConfiguration { get; }

        /// <summary>
        /// Gets the releaseNumber.
        /// </summary>
        public string ProductReleaseNumber { get; }
    }
}
