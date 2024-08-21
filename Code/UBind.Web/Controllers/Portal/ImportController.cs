// <copyright file="ImportController.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Controllers.Portal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Humanizer;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Queries.Organisation;
    using UBind.Application.Services.Imports;
    using UBind.Domain;
    using UBind.Domain.Enums;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;
    using UBind.Domain.Imports;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Permissions;
    using UBind.Domain.Product;
    using UBind.Domain.Services;
    using UBind.Web.Extensions;
    using UBind.Web.Filters;

    /// <summary>
    /// Controller for importing json file on the database.
    /// </summary>
    [Produces("application/json")]
    public class ImportController : Controller
    {
        private readonly ICachingResolver cachingResolver;
        private readonly IImportService importService;
        private readonly IOrganisationService organisationService;
        private readonly ICqrsMediator mediator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportController"/> class.
        /// </summary>
        /// <param name="importService">The service that handles the import.</param>
        /// <param name="cachingResolver">The tenant, product and portal resolver.</param>
        /// <param name="organisationService">The service that handles organisation-related queries and commands.</param>
        public ImportController(
            IImportService importService,
            ICachingResolver cachingResolver,
            IOrganisationService organisationService,
            ICqrsMediator mediator)
        {
            Contract.Requires(importService != null);
            Contract.Requires(organisationService != null);
            this.mediator = mediator;
            this.importService = importService;
            this.cachingResolver = cachingResolver;
            this.organisationService = organisationService;
        }

        /// <summary>
        /// A centralized controller API for imports that runs in the background thread that accepts CSV files for data importation.
        /// The API request handles both customer, policy and claim type.
        /// </summary>
        /// <param name="dataFile">The data file to be processed.</param>
        /// <param name="tenant">The ID or Alias of the tenant to be used, if available, otherwise the tenant of the user performing the import.</param>
        /// <param name="product">The ID or Alias of the product the data to be imported is for.</param>
        /// <param name="organisation">The ID or Alias of the organisation to be used, if available, otherwise the organisation of the user performing the import.</param>
        /// <param name="importType">Defines the type of data being imported (e.g., Claim, Customer, Policy, Quote).</param>
        /// <param name="environment">The environment the data to be imported is for (e.g., Development, Staging), otherwise defaults to Production.</param>
        /// <param name="formMappingJson">The file that contains the form data mapping to be used, if available.</param>
        /// <param name="calculationMappingJson">The file that contains the calculation result mapping to be used, if available.</param>
        /// <param name="updateEnabled">A flag depicting if existing data in system is to be updated if import data matches. The default value is false.</param>
        /// <returns>The content of the file uploaded in JSON format.</returns>
        [HttpPost]
        [Route("/api/v1/import/{product}/csv")]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [MustHavePermission(Permission.ImportData)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> CSVFileImport(
            IFormFile dataFile,
            [FromQuery] string tenant,
            [Required] string product,
            [FromQuery] string organisation,
            [Required] CsvImportType importType,
            [FromQuery] DeploymentEnvironment environment,
            IFormFile formMappingJson = null,
            IFormFile calculationMappingJson = null,
            bool updateEnabled = false)
        {
            string[] acceptedContentTypes = { "application/vnd.ms-excel", "text/csv" };
            if (dataFile.Length > 5242880)
            {
                throw new ErrorException(Errors.DataImports.FileSizeLimitExceeded());
            }

            // verify if valid csv first via content-type.
            if (!acceptedContentTypes.Contains(dataFile.ContentType))
            {
                throw new ErrorException(Errors.DataImports.FileTypeUnsupported(dataFile.FileName, importType.Humanize()));
            }

            // Environment is an optional parameter which defaults to production unless specified
            if (environment == DeploymentEnvironment.None)
            {
                environment = DeploymentEnvironment.Production;
            }

            // Tenant Id is an optional parameter which defaults to the user logged in tenant Id
            if (string.IsNullOrEmpty(tenant))
            {
                tenant = this.User.GetTenantId().ToString();
            }

            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));
            if (productModel == null)
            {
                return Errors.General.NotFound("product", product).ToProblemJsonResult();
            }

            // Organisation Id is an optional parameter which defaults to the tenant's default organisation Id
            if (string.IsNullOrEmpty(organisation))
            {
                var command = new GetDefaultOrganisationForTenantQuery(productModel.TenantId);
                var organisationSummary = await this.mediator.Send(command);
                organisation = organisationSummary.Id.ToString();
            }

            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(productModel.TenantId, new GuidOrAlias(organisation));

            // process csv to dictionary list
            var dataList = this.ConvertCsvToListOfDictionary(new StreamReader(dataFile.OpenReadStream()));

            // create Json for processing
            var importRequest = new JObject
            {
                { "data", JArray.FromObject(dataList) },
            };
            if (importType.Equals(CsvImportType.Policy) || importType.Equals(CsvImportType.Quote))
            {
                JToken customFormToken = null;
                JToken customCalcToken = null;
                if (formMappingJson != null)
                {
                    using (var formMappingFileStream = new StreamReader(formMappingJson.OpenReadStream()))
                    {
                        var jsonString = formMappingFileStream.ReadToEnd();
                        customFormToken = JToken.Parse(jsonString);
                    }
                }

                if (calculationMappingJson != null)
                {
                    using (var calculationResultMappingFileStream = new StreamReader(calculationMappingJson.OpenReadStream()))
                    {
                        var jsonString = calculationResultMappingFileStream.ReadToEnd();
                        customCalcToken = JToken.Parse(jsonString);
                    }
                }

                if (importType.Equals(CsvImportType.Policy))
                {
                    importRequest.Add("policyMapping", JObject.FromObject(PolicyMapping.Default(customFormToken, customCalcToken)));
                }
                else
                {
                    importRequest.Add("quoteMapping", JObject.FromObject(QuoteMapping.Default(customFormToken, customCalcToken)));
                }
            }

            if (importType.Equals(CsvImportType.Customer))
            {
                importRequest.Add("customerMapping", JObject.FromObject(CustomerMapping.Default));
            }

            if (importType.Equals(CsvImportType.Claim))
            {
                importRequest.Add("claimMapping", JObject.FromObject(ClaimMapping.Default));
            }

            var serializedObject = JsonConvert.SerializeObject(importRequest);
            var escapedString = Uri.UnescapeDataString(serializedObject);

            // Call the import service.
            var baseParam = new ImportBaseParam(
                productModel.TenantId,
                organisationModel.Id,
                productModel.Id,
                environment);
            this.importService.QueueBackgroundImport(baseParam, escapedString, updateEnabled);
            return this.Ok("Starting to do import in background thread. Please check hangfire for progress.");
        }

        /// <summary>
        /// A centralized controller API for imports that runs in the background thread.
        /// The API request handles both customer, policy and claim type.
        /// </summary>
        /// <param name="tenant">The destinated tenant ID or Alias of the json import.</param>
        /// <param name="product">The destinated product ID or Alias of the json import.</param>
        /// <param name="organisation">The destinated organisation ID or Alias of the json.</param>
        /// <param name="json">The json representation data in string.</param>
        /// <param name="environment">The deployment environment to use.</param>
        /// <param name="updateEnabled">The tag if updates on import is allowed. The default value is false.</param>
        /// <returns>Returns a contract that represents the result of an action method.</returns>
        [HttpPost]
        [Route("/api/v1/import/product/{product}")]
        [RequestRateLimit(Period = 20, Type = RateLimitPeriodType.Seconds, Limit = 10)]
        [ServiceFilter(typeof(ClientIPAddressFilterAttribute))]
        [MustHavePermission(Permission.ImportData)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> Import(
            [FromQuery] string tenant,
            [Required] string product,
            [FromQuery] string organisation,
            [Required][FromBody] JObject json,
            [FromQuery] DeploymentEnvironment environment,
            bool updateEnabled = false)
        {
            // Environment is an optional parameter which defaults to production unless specified
            if (environment == DeploymentEnvironment.None)
            {
                environment = DeploymentEnvironment.Production;
            }

            // Tenant Id is an optional parameter which defaults to the user logged in tenant Id
            if (string.IsNullOrEmpty(tenant))
            {
                tenant = this.User.GetTenantId().ToString();
            }

            Product productModel = await this.cachingResolver.GetProductOrThrow(new GuidOrAlias(tenant), new GuidOrAlias(product));

            if (productModel == null)
            {
                return Errors.General.NotFound("product", product).ToProblemJsonResult();
            }

            // Organisation Id is an optional parameter which defaults to the tenant's default organisation Id
            if (string.IsNullOrEmpty(organisation))
            {
                var command = new GetDefaultOrganisationForTenantQuery(productModel.TenantId);
                var organisationSummary = await this.mediator.Send(command);
                organisation = organisationSummary.Id.ToString();
            }

            var organisationModel = await this.cachingResolver.GetOrganisationOrThrow(productModel.TenantId, new GuidOrAlias(organisation));

            if (!json.HasValues)
            {
                return Errors.General.BadRequest("Cannot import empty json string").ToProblemJsonResult();
            }

            var serializedObject = JsonConvert.SerializeObject(json);
            var escapedString = Uri.UnescapeDataString(serializedObject);

            var baseParam = new ImportBaseParam(
                productModel.TenantId,
                organisationModel.Id,
                productModel.Id,
                environment);
            this.importService.QueueBackgroundImport(baseParam, escapedString, updateEnabled);
            return this.Ok("Starting to do import in background thread. Please check hangfire for progress.");
        }

        /// <summary>
        /// Converts the CSV content to a collection of dictionaries.
        /// </summary>
        /// <param name="csvStream">The content stream.</param>
        /// <returns>The data collection.</returns>
        public List<Dictionary<string, string>> ConvertCsvToListOfDictionary(StreamReader csvStream)
        {
            var contentString = new List<string[]>();
            List<string> properties = null;
            var dataList = new List<Dictionary<string, string>>();
            var csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            using (csvStream)
            {
                var propertiesArray = csvStream.ReadLine().Split(',');
                properties = new List<string>(propertiesArray);
                while (csvStream.Peek() >= 0)
                {
                    var record = csvStream.ReadLine();
                    contentString.Add(csvParser.Split(record));
                }
            }

            try
            {
                for (int i = 0; i < contentString.Count; i++)
                {
                    var propertyObject = new Dictionary<string, string>();
                    for (int j = 0; j < properties.Count; j++)
                    {
                        propertyObject.Add(properties[j], contentString[i][j]);
                    }

                    dataList.Add(propertyObject);
                }
            }
            catch (Exception ex) when (ex is IndexOutOfRangeException)
            {
                throw new ErrorException(Errors.DataImports.InvalidDataStructure());
            }

            return dataList;
        }
    }

    /// <summary>
    /// Defines the type of data in the csv import file.
    /// </summary>
#pragma warning disable SA1201 // Elements should appear in the correct order
    public enum CsvImportType
    {
        /// <summary>
        /// Claim data imports.
        /// </summary>
        Claim,

        /// <summary>
        /// Customer data imports.
        /// </summary>
        Customer,

        /// <summary>
        /// Policy data imports.
        /// </summary>
        Policy,

        /// <summary>
        /// Quote data imports.
        /// </summary>
        Quote,
    }
}
