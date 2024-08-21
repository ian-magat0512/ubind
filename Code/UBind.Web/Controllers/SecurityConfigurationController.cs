// <copyright file="SecurityConfigurationController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers
{
    using System.Text.RegularExpressions;
    using Microsoft.AspNetCore.Mvc;
    using UBind.Application.Authorisation;
    using UBind.Application.Services.Encryption;
    using UBind.Domain.Enums;
    using UBind.Web.Filters;

    /// <summary>
    /// Handles request related to system security configuration.
    /// </summary>
    [Route("api/v1/security")]
    public class SecurityConfigurationController : Controller
    {
        private readonly IAsymmetricEncryptionService encryptionService;
        private readonly IEncryptionConfiguration encryptionConfiguration;
        private readonly IAuthorisationService authorisationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityConfigurationController"/> class.
        /// </summary>
        /// <param name="encryptionService">The encryption service.</param>
        /// <param name="encryptionConfig">The encryption configuration.</param>
        /// <param name="authorisationService">The authorisation service.</param>
        public SecurityConfigurationController(
            IAsymmetricEncryptionService encryptionService,
            IEncryptionConfiguration encryptionConfig,
            IAuthorisationService authorisationService)
        {
            this.authorisationService = authorisationService;
            this.encryptionConfiguration = encryptionConfig;
            this.encryptionService = encryptionService;
        }

        /// <summary>
        /// Generates the public and private key pair for uBind system encryption.
        /// Once generated, the pair can then be copied and used as values for Octupus variables.
        /// </summary>
        /// <returns>
        /// The RSA key pair within a JSON object.
        /// Public - is the public key in PEM format
        /// Private - is the private key in XML format.
        /// </returns>
        /// <remarks>
        /// This utility endpoint is only used when setting-up a uBind instance. Keys need to be instance-specific, are not shared, and are used to encrypt sensitive information only.
        /// Also note that this does not store the generated key pair locally in this instance of uBind. That should be handled by Octupus.
        /// To use - just copy the keys from the response and paste it into the Octupus variable for the uBind instance.
        /// </remarks>
        [HttpPost]
        [Route("rsa-key-pair")]
        [RequestRateLimit(Period = 10, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [MustBeLoggedIn]
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public IActionResult GenerateRsaKeyPair()
        {
            this.authorisationService.ThrowIfUserIsNotFromMasterTenant(this.User, "generate public/private key pair");
            var keyPair = this.encryptionService.GenerateRSAKeys();
            return this.Ok(keyPair);
        }

        /// <summary>
        /// Retrieves the public key to be used for data-encryption.
        /// </summary>
        /// <returns>The public key.</returns>
        [HttpGet]
        [Route("rsa-key-pair/public")]
        [RequestRateLimit(Period = 60, Type = RateLimitPeriodType.Seconds, Limit = 60)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult GetPublicKey()
        {
            var pemKey = Regex.Unescape(this.encryptionConfiguration.RsaPublicKey);
            return this.Ok(pemKey);
        }
    }
}
