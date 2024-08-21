// <copyright file="ReleaseValidator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application
{
    using System.IO;
    using Microsoft.AspNetCore.Hosting;
    using StackExchange.Profiling;
    using UBind.Application.Automation;
    using UBind.Domain;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Validates a release, for example by checking that json files validate against their schema.
    /// </summary>
    public class ReleaseValidator : IReleaseValidator
    {
        private readonly JsonSchemaValidator jsonSchemaValidator;
        private readonly IAutomationConfigurationValidator automationConfigurationValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseValidator"/> class.
        /// </summary>
        /// <param name="hostingEnvironment">The hosting environment, so we can get the web root path and find the json schema files.</param>
        public ReleaseValidator(IWebHostEnvironment hostingEnvironment, IAutomationConfigurationValidator automationConfigurationValidator)
        {
            this.jsonSchemaValidator = new JsonSchemaValidator(Path.Combine(hostingEnvironment.ContentRootPath, "schemas", "automations"));
            this.automationConfigurationValidator = automationConfigurationValidator;
        }

        /// <inheritdoc/>
        public void Validate(ReleaseDetails quoteDetails, ReleaseDetails claimDetails)
        {
            if (quoteDetails != null)
            {
                this.ValidateQuoteDetails(quoteDetails);
            }

            if (claimDetails != null)
            {
                this.ValidateClaimDetails(claimDetails);
            }
        }

        public void ValidateQuoteDetails(ReleaseDetails quoteDetails)
        {
            using (MiniProfiler.Current.Step($"{nameof(ReleaseValidator)}.{nameof(this.ValidateQuoteDetails)}"))
            {
                if (quoteDetails.AutomationsJson.IsNotNullOrEmpty())
                {
                    this.jsonSchemaValidator
                        .ValidateJsonAgainstSchema(
                        "quote-automations",
                        quoteDetails.AutomationsJson,
                        "automations");
                    this.ValidateAutomationConfiguration(quoteDetails.AutomationsJson);
                }
            }
        }

        public void ValidateClaimDetails(ReleaseDetails claimDetails)
        {
            using (MiniProfiler.Current.Step($"{nameof(ReleaseValidator)}.{nameof(this.ValidateClaimDetails)}"))
            {
                if (claimDetails.AutomationsJson.IsNotNullOrEmpty())
                {
                    this.jsonSchemaValidator
                        .ValidateJsonAgainstSchema(
                        "claim-automations",
                        claimDetails.AutomationsJson,
                        "automations");
                    this.ValidateAutomationConfiguration(claimDetails.AutomationsJson);
                }
            }
        }

        private void ValidateAutomationConfiguration(string automationsConfigJson)
        {
            var configModel = AutomationConfigurationParser.Parse(automationsConfigJson);
            this.automationConfigurationValidator.Validate(configModel);
        }
    }
}
