// <copyright file="ClaimReadModelRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.ReadModels.Claim
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using Dapper;
    using LinqKit;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.Repositories;
    using UBind.Persistence;
    using UBind.Persistence.Extensions;

    /// <inheritdoc />
    public class ClaimReadModelRepository : IClaimReadModelRepository
    {
        private readonly IUBindDbContext dbContext;
        private readonly IClock clock;
        private readonly IConnectionConfiguration connection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimReadModelRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="clock">The current clock instance.</param>
        public ClaimReadModelRepository(
            IUBindDbContext dbContext,
            ICachingResolver cachingResolver,
            IClock clock,
            IConnectionConfiguration connection)
        {
            this.dbContext = dbContext;
            this.clock = clock;
            this.connection = connection;
        }

        /// <summary>
        /// Gets an expression for instantiating summaries from QuoteReadModels for use in EF projections.
        /// </summary>
        public static Expression<Func<ClaimReadModel, Product, IClaimReadModelSummary>> SummarySelector =>
            (c, p) => new ClaimReadModelSummary
            {
                OwnerUserId = c.OwnerUserId,
                PolicyId = c.PolicyId,
                TenantId = c.TenantId,
                OrganisationId = c.OrganisationId,
                ProductId = c.ProductId,
                CustomerId = c.CustomerId,
                CustomerFullName = c.CustomerFullName,
                IsTestData = c.IsTestData,
                CustomerPreferredName = c.CustomerPreferredName,
                PolicyNumber = c.PolicyNumber ?? string.Empty,
                CreatedTicksSinceEpoch = c.CreatedTicksSinceEpoch,
                Environment = c.Environment,
                Id = c.Id,
                PersonId = c.PersonId,
                ClaimNumber = c.ClaimNumber,
                ClaimReference = c.ClaimReference,
                Amount = c.Amount,
                Description = c.Description,
                IncidentTicksSinceEpoch = c.IncidentTicksSinceEpoch,
                LatestFormData = c.LatestFormData,
                SerializedLatestCalculationResult = c.SerializedLatestCalcuationResult,
                Status = c.Status,
                WorkflowStep = c.WorkflowStep,
                ProductName = p.DetailsCollection
                .OrderByDescending(d => d.CreatedTicksSinceEpoch)
                .FirstOrDefault() != null
                ? p.DetailsCollection
                .OrderByDescending(d => d.CreatedTicksSinceEpoch)
                .FirstOrDefault()!.Name
                : string.Empty,
                LastModifiedTicksSinceEpoch = c.LastModifiedTicksSinceEpoch,
                TimeZoneId = c.TimeZoneId,
            };

        /// <summary>
        /// Gets an expression for instantiating details from QuoteReadModels for use in EF projections.
        /// </summary>
        private Expression<Func<ClaimAndOrganisationAndProduct, IClaimReadModelDetails>> DetailSelector =>
            q => new ClaimReadModelDetails
            {
                Id = q.Claim.Id,
                OrganisationName = q.Organisation.Name,
                ProductName = q.Product.DetailsCollection
                .OrderByDescending(d => d.CreatedTicksSinceEpoch)
                .FirstOrDefault() != null
                ? q.Product.DetailsCollection
                .OrderByDescending(d => d.CreatedTicksSinceEpoch)
                .FirstOrDefault()!.Name
                : string.Empty,
                TenantId = q.Claim.TenantId,
                OrganisationId = q.Organisation.Id,
                ProductId = q.Product.Id,
                Environment = q.Claim.Environment,
                PolicyId = q.Claim.PolicyId,
                PolicyNumber = q.Claim.PolicyNumber ?? string.Empty,
                CustomerId = q.Claim.CustomerId,
                PersonId = q.Claim.PersonId,
                CustomerFullName = q.Claim.CustomerFullName,
                IsTestData = q.Claim.IsTestData,
                OwnerUserId = q.Claim.OwnerUserId,
                CustomerPreferredName = q.Claim.CustomerPreferredName,
                ClaimReference = q.Claim.ClaimReference,
                ClaimNumber = q.Claim.ClaimNumber,
                Amount = q.Claim.Amount,
                Description = q.Claim.Description,
                IncidentTicksSinceEpoch = q.Claim.IncidentTicksSinceEpoch,
                CreatedTicksSinceEpoch = q.Claim.CreatedTicksSinceEpoch,
                Status = q.Claim.Status,
                WorkflowStep = q.Claim.WorkflowStep,
                LastModifiedTicksSinceEpoch = q.Claim.LastModifiedTicksSinceEpoch,
                LatestCalculationResultId = q.Claim.LatestCalculationResultId,
                SerializedLatestCalculationResult = q.Claim.SerializedLatestCalcuationResult,
                LatestFormData = q.Claim.LatestFormData,
                TimeZoneId = q.Claim.TimeZoneId,
            };

        /// <inheritdoc />
        public ClaimReadModel? GetById(Guid tenantId, Guid id)
        {
            return this.dbContext.ClaimReadModels.FirstOrDefault(c => c.TenantId == tenantId && c.Id == id);
        }

        /// <inheritdoc />
        public IClaimReadModelSummary? GetSummaryById(Guid tenantId, Guid id)
        {
            var query = this.dbContext.ClaimReadModels.Where(c => c.TenantId == tenantId);
            var claimReadModel = this.JoinWithProducts(query).SingleOrDefault(x => x.Id == id);
            if (claimReadModel != null)
            {
                claimReadModel.Documents = this.dbContext.ClaimAttachment.Where(c => c.ClaimId == claimReadModel.Id).ToList();
            }

            return claimReadModel;
        }

        /// <inheritdoc />
        public bool IsClaimNumberInUseByOtherClaim(Guid tenantId, Guid productId, string claimNumber)
        {
            var claim = this.dbContext.ClaimReadModels.Where(w =>
                w.TenantId == tenantId &&
                w.ClaimNumber == claimNumber &&
                w.ProductId == productId).FirstOrDefault();
            return claim != null;
        }

        /// <inheritdoc />
        public bool HasClaimsForCustomer(EntityListFilters filters, IEnumerable<Guid> excludedClaimIds)
        {
            return this.dbContext.ClaimReadModels.Any(c => c.TenantId == filters.TenantId
                && (filters.OrganisationIds != null && filters.OrganisationIds.Any() ? filters.OrganisationIds.Contains(c.OrganisationId) : true)
                && c.Environment == filters.Environment
                && c.CustomerId == filters.CustomerId
                && !excludedClaimIds.Contains(c.Id));
        }

        /// <inheritdoc />
        public IEnumerable<IClaimReadModelSummary> ListClaimsByQuoteId(Guid tenantId, Guid quoteId, EntityListFilters filters)
        {
            var query = this.dbContext.ClaimReadModels.Where(x => x.TenantId == tenantId && x.PolicyId == quoteId)
               .OrderByDescending(c => c.CreatedTicksSinceEpoch)
               .Paginate(filters);

            return this.JoinWithProducts(query);
        }

        /// <inheritdoc />
        public decimal GetTotalClaimsAmountByPolicyNumberInPastFiveYears(Guid tenantId, Guid productId, string policyNumber) =>
            this.GetClaimsByPolicyNumberInPastFiveYears(tenantId, productId, policyNumber)
                .Select(c => c.Amount)
                .DefaultIfEmpty(0m)
                .Sum()
                .GetValueOrDefault(0m);

        /// <inheritdoc />
        public IEnumerable<IClaimReadModel> GetClaimsByPolicyNumberInPastFiveYears(Guid tenantId, Guid productId, string policyNumber)
        {
            var fiveYears = Duration.FromDays(365.25 * 5.0);
            var fiveYearsAgoTimestamp = this.clock.Now().Minus(fiveYears);
            var fiveYearsAgoTicks = fiveYearsAgoTimestamp.ToUnixTimeTicks();
            return this.dbContext.ClaimReadModels.Where(
                x => x.TenantId == tenantId &&
                x.ProductId == productId &&
                x.PolicyNumber == policyNumber &&
                x.IncidentTicksSinceEpoch >= fiveYearsAgoTicks);
        }

        /// <inheritdoc />
        public IEnumerable<IClaimReadModelSummary> ListClaims(
            Guid tenantId, EntityListFilters filters)
        {
            var query = this.QueryAllClaims(tenantId, filters);

            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                query = query.Order(filters.SortBy, filters.SortOrder);
            }

            return this.JoinWithProducts(query.Paginate(filters));
        }

        public IQueryable<ClaimReadModel> GetClaimsForCustomerId(Guid tenantId, Guid organisationId, Guid customerId)
        {
            return this.GetClaimsForTenantIdAndOrganisationId(tenantId, organisationId)
                .Where(c => c.CustomerId == customerId);
        }

        public IQueryable<ClaimReadModel> GetClaimsForTenantIdAndOrganisationId(Guid tenantId, Guid organisationId)
        {
            return this.dbContext.ClaimReadModels
                .Where(c => c.TenantId == tenantId)
                .Where(c => c.OrganisationId == organisationId);
        }

        /// <inheritdoc />
        public IEnumerable<IClaimReadModel> ListClaimsWithoutJoiningProducts(
            Guid tenantId, EntityListFilters filters)
        {
            var query = this.QueryAllClaims(tenantId, filters);

            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                query = query.Order(filters.SortBy, filters.SortOrder);
            }

            return query.Paginate(filters);
        }

        /// <inheritdoc />
        public IEnumerable<ClaimReadModel> ListClaimsWithLodgeSettledDeclinedDatesNotSet(Guid tenantId, EntityListFilters filters)
        {
            var query = this.QueryAllClaims(tenantId, filters);
            query = query.Where(p =>
                p.LodgedTicksSinceEpoch == null
                && p.DeclinedTicksSinceEpoch == null
                && p.SettledTicksSinceEpoch == null);

            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                query = query.Order(filters.SortBy, filters.SortOrder);
            }

            return query.Paginate(filters);
        }

        /// <inheritdoc/>
        public IEnumerable<IClaimReadModelSummary> ListAllClaimsByCustomer(Guid tenantId, Guid customerId, EntityListFilters filters)
        {
            var query = this.dbContext.ClaimReadModels
                .Where(c => c.TenantId == tenantId)
                .Where(c => c.CustomerId == customerId)
                .Where(c => !ClaimState.Nascent.Equals(c.Status));

            if (filters.PolicyId != null)
            {
                query = query.Where(c => c.PolicyId == filters.PolicyId);
            }

            query = query.OrderByDescending(c => c.CreatedTicksSinceEpoch).Paginate(filters);
            return this.JoinWithProducts(query);
        }

        /// <inheritdoc/>
        public IEnumerable<IClaimReadModelSummary> ListClaimsByPolicy(Guid tenantId, Guid policyId, EntityListFilters filters)
        {
            var query = this.dbContext.ClaimReadModels
                .Where(c => c.TenantId == tenantId && c.PolicyId == policyId)
                .Where(c => !ClaimState.Nascent.Equals(c.Status))
                .OrderByDescending(c => c.CreatedTicksSinceEpoch)
                .Paginate(filters);

            return this.JoinWithProducts(query);
        }

        /// <inheritdoc/>
        public IEnumerable<ClaimReadModel> ListClaimsByPolicyWithoutJoiningProducts(Guid tenantId, Guid policyId)
        {
            return this.dbContext.ClaimReadModels
                .Where(c => c.TenantId == tenantId && c.PolicyId == policyId).ToList();
        }

        /// <inheritdoc />
        public bool HasClaimsByPolicyId(Guid tenantId, Guid policyId)
        {
            return this.dbContext.ClaimReadModels.Any(c => c.TenantId == tenantId
                && c.PolicyId == policyId);
        }

        /// <inheritdoc/>
        public IClaimReadModel? GetByClaimNumber(Guid tenantId, Guid productId, DeploymentEnvironment environment, string referenceNumber) =>
           this.dbContext.ClaimReadModels.FirstOrDefault((Expression<Func<ClaimReadModel, bool>>)(c =>
                c.ClaimReference == referenceNumber
                && c.ProductId == productId
                && c.TenantId == tenantId
                && c.Environment == environment));

        /// <inheritdoc />
        public IClaimReadModelWithRelatedEntities? GetClaimWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, Guid claimId, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForClaimDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
            return query.FirstOrDefault(c => c.Claim.Id == claimId);
        }

        /// <inheritdoc />
        public IClaimReadModelWithRelatedEntities? GetClaimWithRelatedEntitiesByReference(
            Guid tenantId, string claimReference, DeploymentEnvironment? environment, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForClaimDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
            return query.FirstOrDefault(c => c.Claim.ClaimReference == claimReference);
        }

        /// <inheritdoc />
        public IClaimReadModelWithRelatedEntities? GetClaimWithRelatedEntitiesByNumber(
            Guid tenantId, string claimNumber, DeploymentEnvironment? environment, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForClaimDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
            return query.FirstOrDefault(c => c.Claim.ClaimNumber == claimNumber);
        }

        /// <inheritdoc/>
        public IFileContentReadModel? GetDocumentContent(Guid tenantId, Guid documentId, Guid claimOrClaimVersionId)
        {
            var entities = from doc in this.dbContext.ClaimAttachment
                           join cont in this.dbContext.FileContents on doc.FileContentId equals cont.Id
                           where doc.TenantId == tenantId && doc.Id == documentId && doc.ClaimOrClaimVersionId == claimOrClaimVersionId
                           select new FileContentReadModel
                           {
                               FileContent = cont.Content,
                               ContentType = doc.Type,
                           };

            return entities.SingleOrDefault();
        }

        /// <inheritdoc/>
        public List<Guid> GetAllClaimIdsByEntityFilters(EntityFilters entityFilters)
        {
            var query = this.dbContext.ClaimReadModels.Where(
                crm => crm.TenantId == entityFilters.TenantId && crm.Status != ClaimState.Nascent);

            if (entityFilters.ProductId.HasValue)
            {
                query = query.Where(crm => crm.ProductId == entityFilters.ProductId.Value);
            }

            if (entityFilters.OrganisationId.HasValue)
            {
                query = query.Where(crm => crm.OrganisationId == entityFilters.OrganisationId.Value);
            }

            if (entityFilters.Skip.HasValue && entityFilters.PageSize.HasValue)
            {
                query = query.OrderByDescending(cl => cl.CreatedTicksSinceEpoch).Skip(
                    entityFilters.Skip.Value).Take(entityFilters.PageSize.Value);
            }

            return query.Select(cq => cq.Id).ToList();
        }

        /// <inheritdoc/>
        public IQueryable<ClaimReadModelWithRelatedEntities> CreateQueryForClaimDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, IEnumerable<string> relatedEntities)
        {
            return this.CreateQueryForClaimDetailsWithRelatedEntities(
                tenantId, environment, this.dbContext.ClaimReadModels, relatedEntities);
        }

        /// <inheritdoc/>
        public IQueryable<IClaimReadModel> GetAllClaimsAsQueryable()
        {
            return this.dbContext.ClaimReadModels.AsQueryable();
        }

        /// <summary>
        /// Gets the details of a claim.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="claimId">The claim ID.</param>
        /// <returns>The claim details.</returns>
        public IClaimReadModelDetails? GetClaimDetails(Guid tenantId, Guid claimId)
        {
            var query = this.dbContext.ClaimReadModels
                .Where(c => c.TenantId == tenantId && c.Id == claimId)
                .Join(
                    this.dbContext.OrganisationReadModel,
                    claim => claim.OrganisationId,
                    organisation => organisation.Id,
                    (c, o) => new ClaimAndOrganisation
                    {
                        Claim = c,
                        Organisation = o,
                    });

            var claimOrgProductQuery = query.Join(
                    this.dbContext.Products.Include(p => p.DetailsCollection),
                    co => co.Claim.ProductId,
                    prod => prod.Id,
                    (co, prod) => new ClaimAndOrganisationAndProduct
                    {
                        Claim = co.Claim,
                        Organisation = co.Organisation,
                        Product = prod,
                    });

            // TODO: create a migration for claim attachment.
            var claimReadModelDetails = claimOrgProductQuery
                .Select(this.DetailSelector)
                .FirstOrDefault();

            if (claimReadModelDetails != null)
            {
                claimReadModelDetails.Documents =
                    this.dbContext.ClaimAttachment.Where(
                        c => c.TenantId == tenantId && c.ClaimOrClaimVersionId == claimReadModelDetails.Id).ToList();
            }

            return claimReadModelDetails;
        }

        public IQueryable<ClaimReadModel> QueryAllClaims(Guid tenantId, EntityListFilters filters)
        {
            var query = this.dbContext.ClaimReadModels
                .Where(c => c.TenantId == tenantId);

            filters.OrganisationIds = filters.OrganisationIds ?? new List<Guid>();

            if (filters.Environment != null)
            {
                query = query.Where(c => c.Environment == filters.Environment);
            }

            if (filters.IsRideProtectOrganisation)
            {
                query = this.AddProductAndOrganisationExpressionsWithRideProtectCondition(query, filters);
            }
            else
            {
                if (filters.ProductIds.Any())
                {
                    ExpressionStarter<ClaimReadModel> productIdsPredicate =
                        PredicateBuilder.New<ClaimReadModel>(false);
                    foreach (var productId in filters.ProductIds)
                    {
                        productIdsPredicate = productIdsPredicate.Or(rm => rm.ProductId.Equals(productId));
                    }

                    query = query.Where(productIdsPredicate);
                }

                if (filters.OrganisationIds.Any())
                {
                    query = query.Where(c => filters.OrganisationIds.Contains(c.OrganisationId));
                }
            }

            if (filters.OwnerUserId != null)
            {
                query = query.Where(c => c.OwnerUserId == filters.OwnerUserId);
            }

            if (filters.PolicyId != null)
            {
                query = query.Where(c => c.PolicyId == filters.PolicyId);
            }

            if (filters.CustomerId.HasValue && filters.CustomerId.GetValueOrDefault() != default)
            {
                query = query.Where(c => c.CustomerId == filters.CustomerId);
            }

            if (!string.IsNullOrEmpty(filters.PolicyNumber))
            {
                query = query.Where(c => c.PolicyNumber == filters.PolicyNumber);
            }

            var searchPredicate = PredicateBuilder.New<ClaimReadModel>(false);

            if (filters.SearchTerms.Any())
            {
                foreach (var searchTerm in filters.SearchTerms)
                {
                    searchPredicate = searchPredicate.Or(c => c.CustomerFullName.IndexOf(searchTerm) > -1 ||
                    c.CustomerPreferredName.IndexOf(searchTerm) > -1);
                    searchPredicate = searchPredicate.Or(c => c.ClaimReference.IndexOf(searchTerm) > -1);
                    searchPredicate = searchPredicate.Or(c => c.ClaimNumber.IndexOf(searchTerm) > -1);
                    searchPredicate = searchPredicate.Or(c => c.Description.IndexOf(searchTerm) > -1);
                    searchPredicate = searchPredicate.Or(c => (c.PolicyNumber ?? string.Empty).IndexOf(searchTerm) > -1);
                }

                query = query.Where(searchPredicate);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.GreaterThanExpression<ClaimReadModel>(filters.DateFilteringPropertyName, filters.DateIsAfterTicks));
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.LessThanExpression<ClaimReadModel>(filters.DateFilteringPropertyName, filters.DateIsBeforeTicks));
            }

            if (filters.Statuses.Any())
            {
                var statusPredicate = PredicateBuilder.New<ClaimReadModel>(false);
                foreach (var status in filters.Statuses)
                {
                    // filter status can be composite status (e.g. "Active" so check must be whether filter
                    // status has flag of claim, and not other way round).
                    statusPredicate = statusPredicate.Or(c => status == c.Status);
                }

                query = query.Where(statusPredicate);
            }

            if (!filters.IncludeTestData)
            {
                query = query.Where(c => c.IsTestData == false);
            }
            return query;
        }

        public IEnumerable<IClaimReadModelWithRelatedEntities> GetClaimsWithRelatedEntities(
            Guid tenantId,
            DeploymentEnvironment environment,
            EntityListFilters filters,
            IEnumerable<string> relatedEntities)
        {
            var claims = this.QueryAllClaims(tenantId, filters);
            return this.CreateQueryForClaimDetailsWithRelatedEntities(
                tenantId, environment, claims, relatedEntities).ToList();
        }

        public async Task<IEnumerable<ClaimDashboardSummaryModel>> ListClaimsForPeriodicSummary(Guid tenantId, EntityListFilters filters, CancellationToken cancellationToken)
        {
            filters.TenantId = tenantId;
            return await this.QueryClaimsForPeriodicSummary(filters, cancellationToken);
        }

        public IEnumerable<IClaimReportItem> GetFilteredClaims(
            Guid tenantId,
            Guid organisationId,
            IEnumerable<Guid> productIds,
            DeploymentEnvironment environment,
            Instant fromTimestamp,
            Instant toTimestamp,
            bool includeTestData)
        {

            var commonTableExpressions = $@"
                        ;WITH CTE_ProductsLatestDetails
						AS
						(
							SELECT
								[ProductId]=P.Product_Id,
								P.[Name],
								P.Alias
							FROM    (
								SELECT
                                    Id = MAX(ID)
								FROM
                                    dbo.ProductDetails P (NOLOCK)
								<SELECTED_PRODUCT_CONDITIONS>
								GROUP BY
                                    P.Product_Id
                            )   P_Latest
							INNER JOIN
								dbo.ProductDetails P (NOLOCK)
								ON P.Id = P_Latest.Id
							GROUP BY
								P.Product_Id, P.[Name], P.Alias
						)
            ";

            var mainQuery = $@"
                        SELECT
                            [EnvironmentDescription]=(
                                CASE
                                    WHEN Claim.Environment = 1 THEN 'Development'
                                    WHEN Claim.Environment = 2 THEN 'Staging'
                                    WHEN Claim.Environment = 3 THEN 'Production'
                                    ELSE 'None'
                                END
                            ),
                            Claim.CreatedTicksSinceEpoch,
                            Claim.LastModifiedTicksSinceEpoch,
                            Claim.IncidentTicksSinceEpoch,
                            Claim.Amount,
                            Claim.ClaimNumber,
                            Claim.ClaimReference,
                            Claim.PolicyId,
                            Claim.PolicyNumber,
                            Claim.WorkflowStep,
                            Claim.[Status],
                            Claim.[Description],
                            CustomerEmail = Persons.Email,
				            CustomerAlternativeEmail = Persons.AlternativeEmail,
				            CustomerMobilePhone = Persons.MobilePhoneNumber,
				            CustomerHomePhone = Persons.HomePhoneNumber,
				            CustomerWorkPhone = Persons.WorkPhoneNumber,
				            CreditNoteNumber = Claim.ClaimNumber,
                            [OrganisationName]=Organisations.[Name],
                            [OrganisationAlias]=Organisations.Alias,
                            [AgentName]=Users.FullName,
                            [CustomerFullName]=Persons.FullName,
                            [CustomerPrefferedName]=Persons.PreferredName,
                            [ProductName]=Products.[Name],
                            [ProductAlias]=Products.Alias,
                            [LatestFormData]=Claim.LatestFormData,
                            [TestData]=(
                                CASE
                                    WHEN Claim.IsTestData = 1 THEN 'Yes'
                                    ELSE 'No'
                                END
                            ),
                            Claim.TimeZoneId
                        FROM
                            dbo.ClaimReadModels Claim (NOLOCK)
                        INNER JOIN
                            dbo.OrganisationReadModels Organisations (NOLOCK)
                            ON Claim.OrganisationId = Organisations.Id
                        INNER JOIN
                            dbo.UserReadModels Users (NOLOCK)
                            ON Claim.OwnerUserId = Users.Id
                        INNER JOIN
                            dbo.PersonReadModels Persons (NOLOCK)
                            ON Claim.PersonId = Persons.Id
                        INNER JOIN
                            CTE_ProductsLatestDetails Products (NOLOCK)
                            ON Claim.ProductId = Products.ProductId
                        WHERE
                            Claim.TenantId = @TenantId
                            AND Claim.OrganisationId = ISNULL(@OrganisationId,Claim.OrganisationId)
                            AND Claim.CreatedTicksSinceEpoch BETWEEN @FromTimeStamp AND @ToTimeStamp
                            AND Claim.Environment = @Environment
                            AND (
                                (@IncludeTestDate = 1 AND Claim.IsTestData IN (0,1)) -- both actual and test data
                                OR (@IncludeTestDate = 0 AND Claim.IsTestData = 0)  -- actual data only
                            )
                        ORDER BY
                            Claim.CreatedTicksSinceEpoch DESC
            ";

            var selectedProductsCondition =
                    productIds.Any() ? @"WHERE P.Product_TenantId = @TenantId AND P.Product_Id IN @ProductIds "
                    : "WHERE P.Product_TenantId = @TenantId ";
            commonTableExpressions = commonTableExpressions.Replace("<SELECTED_PRODUCT_CONDITIONS>", selectedProductsCondition);
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine(commonTableExpressions);
            queryBuilder.AppendLine(mainQuery);
            var sql = queryBuilder.ToString();
            var parameters = new DynamicParameters();
            parameters.Add("TenantId", tenantId);
            parameters.Add("OrganisationId", organisationId == Guid.Empty ? null! : organisationId);
            parameters.Add("ProductIds", productIds);
            parameters.Add("Environment", environment);
            parameters.Add("FromTimeStamp", fromTimestamp.ToUnixTimeTicks());
            parameters.Add("ToTimeStamp", toTimestamp.ToUnixTimeTicks());
            parameters.Add("IncludeTestDate", includeTestData);
            using (var connection = new SqlConnection(this.connection.UBind))
            {
                connection.Open();
                var claims = connection.Query<ClaimReportItem>(sql, parameters, null, true, 60);
                return claims;
            }
        }

        public IEnumerable<IClaimReportItem> GetClaimsDataForReports(
               Guid tenantId,
               Guid organisationId,
               IEnumerable<Guid> productIds,
               DeploymentEnvironment environment,
               Instant fromTimestamp,
               Instant toTimestamp,
               bool includeTestData)
        {
            var filteredClaims = this.GetFilteredClaims(
                tenantId,
                organisationId,
                productIds,
                environment,
                fromTimestamp,
                toTimestamp,
                includeTestData);

            if (filteredClaims == null)
            {
                return new List<IClaimReportItem>();
            }

            return filteredClaims;
        }

        /// <summary>
        /// Additional condition for Ride Protect organisation (to be removed after UB-8372).
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="filters">The entity filters.</param>
        /// <returns>The query with conditions for ride-protect organisation.</returns>
        private IQueryable<ClaimReadModel> AddProductAndOrganisationExpressionsWithRideProtectCondition(IQueryable<ClaimReadModel> query, EntityListFilters filters)
        {
            ExpressionStarter<ClaimReadModel> productAndOrganisationPredicate =
                   PredicateBuilder.New<ClaimReadModel>(true);

            filters.OrganisationIds = filters.OrganisationIds ?? new List<Guid>();
            if (filters.OrganisationIds.Any())
            {
                productAndOrganisationPredicate.And(rm => filters.OrganisationIds.Contains(rm.OrganisationId));
            }

            // hard code for ride protect organisation (to be removed after UB-8372)
            bool queryHasRideProtect = filters.IsRideProtectOrganisation;
            bool isQueryProductIdsSpecified = filters.ProductIds.Any();
            if (queryHasRideProtect)
            {
                queryHasRideProtect = (isQueryProductIdsSpecified
                                        && filters.ProductIds
                                        .Contains(filters.RideProtectProductId ?? Guid.Empty))
                                        || !isQueryProductIdsSpecified;
            }

            if (queryHasRideProtect)
            {
                ExpressionStarter<ClaimReadModel> productAndOtherOrganisationPredicateRideProtect =
                    PredicateBuilder.New<ClaimReadModel>(rm => rm.ProductId.Equals(filters.RideProtectProductId));
                if (filters.OrganisationIds.Any())
                {
                    productAndOrganisationPredicate.And(rm => filters.OrganisationIds.Contains(rm.OrganisationId));
                }

                if (isQueryProductIdsSpecified)
                {
                    productAndOrganisationPredicate.And(rm => filters.ProductIds.Contains(rm.ProductId));
                }

                productAndOrganisationPredicate.Or(productAndOtherOrganisationPredicateRideProtect);
            }
            else
            {
                if (isQueryProductIdsSpecified)
                {
                    productAndOrganisationPredicate.And(rm => filters.ProductIds.Contains(rm.ProductId));
                }
            }

            query = query.Where(productAndOrganisationPredicate);
            return query;
        }

        /// <summary>
        /// This is a temporary hardcode method specific for ride-protect sub-organisation,
        /// to be removed after the implementation of UB-8372.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns>A query for claims.</returns>
        private IQueryable<ClaimReadModel> QueryAllClaimsForRideProtectProductFromOtherOrganisation(Guid tenantId, EntityListFilters filters)
        {
            var query = this.dbContext.ClaimReadModels
                .Where(c => c.TenantId == tenantId);

            filters.OrganisationIds = filters.OrganisationIds ?? new List<Guid>();

            if (filters.Environment != null)
            {
                query = query.Where(c => c.Environment == filters.Environment);
            }

            if (filters.ProductIds.Any())
            {
                ExpressionStarter<ClaimReadModel> productIdsPredicate =
                    PredicateBuilder.New<ClaimReadModel>(false);
                foreach (var productId in filters.ProductIds)
                {
                    productIdsPredicate = productIdsPredicate.Or(rm => rm.ProductId.Equals(productId));
                }

                query = query.Where(productIdsPredicate);
            }

            if (filters.OrganisationIds.Any())
            {
                query = query.Where(c => filters.OrganisationIds.Contains(c.OrganisationId));
            }

            if (filters.OwnerUserId != null)
            {
                query = query.Where(c => c.OwnerUserId == filters.OwnerUserId);
            }

            if (filters.PolicyId != null)
            {
                query = query.Where(c => c.PolicyId == filters.PolicyId);
            }

            if (filters.CustomerId.HasValue && filters.CustomerId.GetValueOrDefault() != default)
            {
                query = query.Where(c => c.CustomerId == filters.CustomerId);
            }

            if (!string.IsNullOrEmpty(filters.PolicyNumber))
            {
                query = query.Where(c => c.PolicyNumber == filters.PolicyNumber);
            }

            var searchPredicate = PredicateBuilder.New<ClaimReadModel>(false);

            if (filters.SearchTerms.Any())
            {
                foreach (var searchTerm in filters.SearchTerms)
                {
                    searchPredicate = searchPredicate.Or(c => c.CustomerFullName.IndexOf(searchTerm) > -1 ||
                    c.CustomerPreferredName.IndexOf(searchTerm) > -1);
                    searchPredicate = searchPredicate.Or(c => c.ClaimReference.IndexOf(searchTerm) > -1);
                    searchPredicate = searchPredicate.Or(c => c.ClaimNumber.IndexOf(searchTerm) > -1);
                    searchPredicate = searchPredicate.Or(c => c.Description.IndexOf(searchTerm) > -1);
                    searchPredicate = searchPredicate.Or(c => (c.PolicyNumber ?? string.Empty).IndexOf(searchTerm) > -1);
                }

                query = query.Where(searchPredicate);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.GreaterThanExpression<ClaimReadModel>(filters.DateFilteringPropertyName, filters.DateIsAfterTicks));
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                query = query.Where(ExpressionHelper.LessThanExpression<ClaimReadModel>(filters.DateFilteringPropertyName, filters.DateIsBeforeTicks));
            }

            if (filters.Statuses.Any())
            {
                var statusPredicate = PredicateBuilder.New<ClaimReadModel>(false);
                foreach (var status in filters.Statuses)
                {
                    // filter status can be composite status (e.g. "Active" so check must be whether filter
                    // status has flag of claim, and not other way round).
                    statusPredicate = statusPredicate.Or(c => status == c.Status);
                }

                query = query.Where(statusPredicate);
            }

            if (!filters.IncludeTestData)
            {
                query = query.Where(c => c.IsTestData == false);
            }

            return query;
        }

        private IQueryable<ClaimReadModelWithRelatedEntities> CreateQueryForClaimDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment? environment, IQueryable<ClaimReadModel> dataSource, IEnumerable<string> relatedEntities)
        {
            var query = dataSource.Where(c => c.TenantId == tenantId);

            if (environment.HasValue)
            {
                query = query.Where(c => c.Environment == environment);
            }

            var withRelatedEntitiesQuery = query.Where(c => c.TenantId == tenantId).Select(c => new ClaimReadModelWithRelatedEntities
            {
                Claim = c,
                Customer = default!,
                Tenant = default!,
                TenantDetails = new TenantDetails[] { },
                Product = default!,
                ProductDetails = new ProductDetails[] { },
                Owner = default!,
                Documents = new ClaimAttachmentReadModel[] { },
                ClaimVersions = new ClaimVersionReadModel[] { },
                Emails = new Domain.ReadWriteModel.Email.Email[] { },
                Organisation = default!,
                Policy = default!,
                PolicyTransactions = new Domain.ReadModel.Policy.PolicyTransaction[] { },
                Sms = new Sms[] { },
                ToRelationships = new Relationship[] { },
                FromRelationships = new Relationship[] { },
                TextAdditionalPropertiesValues = new TextAdditionalPropertyValueReadModel[] { },
                StructuredDataAdditionalPropertyValues = new StructuredDataAdditionalPropertyValueReadModel[] { },
            });

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Claim.Tenant))))
            {
                withRelatedEntitiesQuery = withRelatedEntitiesQuery.Join(this.dbContext.Tenants, c => c.Claim.TenantId, t => t.Id, (c, tenant) => new ClaimReadModelWithRelatedEntities
                {
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = tenant,
                    TenantDetails = tenant.DetailsCollection,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    ClaimVersions = c.ClaimVersions,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Claim.Product))))
            {
                withRelatedEntitiesQuery = withRelatedEntitiesQuery.Join(this.dbContext.Products.IncludeAllProperties(), c => new { tenantId = c.Claim.TenantId, productId = c.Claim.ProductId }, t => new { tenantId = t.TenantId, productId = t.Id }, (c, product) => new ClaimReadModelWithRelatedEntities
                {
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = product,
                    ProductDetails = product.DetailsCollection,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    ClaimVersions = c.ClaimVersions,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Claim.Customer))))
            {
                withRelatedEntitiesQuery = withRelatedEntitiesQuery.Join(this.dbContext.CustomerReadModels, c => c.Claim.CustomerId, c => c.Id, (c, customer) => new ClaimReadModelWithRelatedEntities
                {
                    Claim = c.Claim,
                    Customer = customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    ClaimVersions = c.ClaimVersions,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Claim.Owner))))
            {
                withRelatedEntitiesQuery = withRelatedEntitiesQuery.GroupJoin(this.dbContext.Users, c => c.Claim.OwnerUserId, u => u.Id, (c, users) => new { c, users })
                    .SelectMany(
                        x => x.users.DefaultIfEmpty(),
                        (x, user) => new ClaimReadModelWithRelatedEntities
                        {
                            Claim = x.c.Claim,
                            Customer = x.c.Customer,
                            Tenant = x.c.Tenant,
                            TenantDetails = x.c.TenantDetails,
                            Product = x.c.Product,
                            ProductDetails = x.c.ProductDetails,
                            Owner = x.c.Claim.OwnerUserId != null ? user : null,
                            Documents = x.c.Documents,
                            ClaimVersions = x.c.ClaimVersions,
                            Emails = x.c.Emails,
                            Organisation = x.c.Organisation,
                            Policy = x.c.Policy,
                            PolicyTransactions = x.c.PolicyTransactions,
                            Sms = x.c.Sms,
                            ToRelationships = x.c.ToRelationships,
                            FromRelationships = x.c.FromRelationships,
                            TextAdditionalPropertiesValues = x.c.TextAdditionalPropertiesValues,
                            StructuredDataAdditionalPropertyValues = x.c.StructuredDataAdditionalPropertyValues,
                        });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Claim.Policy))))
            {
                withRelatedEntitiesQuery = withRelatedEntitiesQuery.Join(this.dbContext.Policies, c => c.Claim.PolicyId, p => p.Id, (c, policy) => new ClaimReadModelWithRelatedEntities
                {
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    ClaimVersions = c.ClaimVersions,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });

                withRelatedEntitiesQuery = withRelatedEntitiesQuery.GroupJoin(this.dbContext.PolicyTransactions, c => c.Policy.Id, pt => pt.PolicyId, (c, pt) => new ClaimReadModelWithRelatedEntities
                {
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    ClaimVersions = c.ClaimVersions,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = pt,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Claim.Organisation))))
            {
                withRelatedEntitiesQuery = withRelatedEntitiesQuery.Join(this.dbContext.OrganisationReadModel, c => c.Claim.OrganisationId, t => t.Id, (c, organisation) => new ClaimReadModelWithRelatedEntities
                {
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    ClaimVersions = c.ClaimVersions,
                    Emails = c.Emails,
                    Organisation = organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Claim.ClaimVersions))))
            {
                withRelatedEntitiesQuery = withRelatedEntitiesQuery.GroupJoin(this.dbContext.ClaimVersions, c => c.Claim.Id, c => c.ClaimId, (c, claimVersions) => new ClaimReadModelWithRelatedEntities
                {
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    ClaimVersions = claimVersions,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Claim.Documents))))
            {
                withRelatedEntitiesQuery = withRelatedEntitiesQuery.GroupJoin(this.dbContext.ClaimAttachment, c => c.Claim.Id, c => c.ClaimOrClaimVersionId, (c, documents) => new ClaimReadModelWithRelatedEntities
                {
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = documents,
                    ClaimVersions = c.ClaimVersions,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Claim.Messages))))
            {
                var emailQuery = from email in this.dbContext.Emails
                                 join relationship in this.dbContext.Relationships on new { EmailId = email.Id, Type = RelationshipType.ClaimMessage, FromEntityType = EntityType.Claim } equals new { EmailId = relationship.ToEntityId, relationship.Type, relationship.FromEntityType }
                                 select new
                                 {
                                     Email = email,
                                     RelationShip = relationship,
                                 };

                withRelatedEntitiesQuery = withRelatedEntitiesQuery.GroupJoin(emailQuery, c => c.Claim.Id, c => c.RelationShip.FromEntityId, (c, emails) => new ClaimReadModelWithRelatedEntities
                {
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    ClaimVersions = c.ClaimVersions,
                    Emails = emails.Select(a => a.Email),
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });

                var smsQuery = from sms in this.dbContext.Sms
                               join relationship in this.dbContext.Relationships on new { SmsId = sms.Id, Type = RelationshipType.ClaimMessage, FromEntityType = EntityType.Claim } equals new { SmsId = relationship.ToEntityId, Type = relationship.Type, FromEntityType = relationship.FromEntityType }
                               select new
                               {
                                   Sms = sms,
                                   RelationShip = relationship,
                               };

                withRelatedEntitiesQuery = withRelatedEntitiesQuery.GroupJoin(smsQuery, c => c.Claim.Id, c => c.RelationShip.FromEntityId, (c, sms) => new ClaimReadModelWithRelatedEntities
                {
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    ClaimVersions = c.ClaimVersions,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = sms.Select(s => s.Sms),
                    ToRelationships = c.ToRelationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Claim.Relationships))))
            {
                withRelatedEntitiesQuery = withRelatedEntitiesQuery.GroupJoin(this.dbContext.Relationships, c => c.Claim.Id, r => r.FromEntityId, (c, relationships) => new ClaimReadModelWithRelatedEntities
                {
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    ClaimVersions = c.ClaimVersions,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = c.ToRelationships,
                    FromRelationships = relationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });

                withRelatedEntitiesQuery = withRelatedEntitiesQuery.GroupJoin(this.dbContext.Relationships, c => c.Claim.Id, r => r.ToEntityId, (c, relationships) => new ClaimReadModelWithRelatedEntities
                {
                    Claim = c.Claim,
                    Customer = c.Customer,
                    Tenant = c.Tenant,
                    TenantDetails = c.TenantDetails,
                    Product = c.Product,
                    ProductDetails = c.ProductDetails,
                    Owner = c.Owner,
                    Documents = c.Documents,
                    ClaimVersions = c.ClaimVersions,
                    Emails = c.Emails,
                    Organisation = c.Organisation,
                    Policy = c.Policy,
                    PolicyTransactions = c.PolicyTransactions,
                    Sms = c.Sms,
                    ToRelationships = relationships,
                    FromRelationships = c.FromRelationships,
                    TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                    StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.Claim.AdditionalProperties))))
            {
                withRelatedEntitiesQuery = withRelatedEntitiesQuery.GroupJoin(
                    this.dbContext.TextAdditionalPropertValues.IncludeAllProperties(),
                    c => c.Claim.Id,
                    c => c.EntityId,
                    (c, apv) => new ClaimReadModelWithRelatedEntities
                    {
                        Claim = c.Claim,
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Product = c.Product,
                        ProductDetails = c.ProductDetails,
                        Owner = c.Owner,
                        Documents = c.Documents,
                        ClaimVersions = c.ClaimVersions,
                        Emails = c.Emails,
                        Organisation = c.Organisation,
                        Policy = c.Policy,
                        PolicyTransactions = c.PolicyTransactions,
                        Sms = c.Sms,
                        ToRelationships = c.ToRelationships,
                        FromRelationships = c.FromRelationships,
                        TextAdditionalPropertiesValues = (IEnumerable<TextAdditionalPropertyValueReadModel>)apv
                            .Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                        StructuredDataAdditionalPropertyValues = c.StructuredDataAdditionalPropertyValues,
                    })
                    .GroupJoin(
                    this.dbContext.StructuredDataAdditionalPropertyValues.IncludeAllProperties(),
                    c => c.Claim.Id,
                    c => c.EntityId,
                    (c, apv) => new ClaimReadModelWithRelatedEntities
                    {
                        Claim = c.Claim,
                        Customer = c.Customer,
                        Tenant = c.Tenant,
                        TenantDetails = c.TenantDetails,
                        Product = c.Product,
                        ProductDetails = c.ProductDetails,
                        Owner = c.Owner,
                        Documents = c.Documents,
                        ClaimVersions = c.ClaimVersions,
                        Emails = c.Emails,
                        Organisation = c.Organisation,
                        Policy = c.Policy,
                        PolicyTransactions = c.PolicyTransactions,
                        Sms = c.Sms,
                        ToRelationships = c.ToRelationships,
                        FromRelationships = c.FromRelationships,
                        TextAdditionalPropertiesValues = c.TextAdditionalPropertiesValues,
                        StructuredDataAdditionalPropertyValues = (IEnumerable<StructuredDataAdditionalPropertyValueReadModel>)apv.Where(df => !df.AdditionalPropertyDefinition.IsDeleted),
                    });
            }

            return withRelatedEntitiesQuery;
        }

        /// <summary>
        /// Join with products.
        /// </summary>
        /// <param name="query">test.</param>
        /// <returns>Queryable of claim read model summary.</returns>
        private IQueryable<IClaimReadModelSummary> JoinWithProducts(IQueryable<ClaimReadModel> query) =>
            query.Join(
                this.dbContext.Products.Include(p => p.DetailsCollection), // the source table of the inner join
                claim => new { productId = claim.ProductId, tenantId = claim.TenantId },        // Select the primary key (the first part of the "on" clause in an sql "join" statement)
                product => new { productId = product.Id, tenantId = product.TenantId },   // Select the foreign key (the second part of the "on" clause)
                SummarySelector); // selection

        private async Task<IEnumerable<ClaimDashboardSummaryModel>> QueryClaimsForPeriodicSummary(EntityListFilters filters, CancellationToken cancellationToken)
        {
            StringBuilder queryBuilder = new StringBuilder();
            var parameters = new DynamicParameters();
            filters.OrganisationIds ??= new List<Guid>();
            parameters.Add("@tenantId", filters.TenantId);
            parameters.Add("@environment", filters.Environment ?? DeploymentEnvironment.Production);
            parameters.Add("@statuses", filters.Statuses);
            queryBuilder.Append(@"
                SELECT 
                c.Id AS Id, 
                c.Status AS ClaimState, 
                c.ProductId AS ProductId, 
                c.CreatedTicksSinceEpoch AS CreatedTicksSinceEpoch, 
                c.LastModifiedTicksSinceEpoch AS LastModifiedTimeInTicksSinceEpoch,
                c.LodgedTicksSinceEpoch AS LodgedTicksSinceEpoch,
                c.SettledTicksSinceEpoch AS SettledTicksSinceEpoch,
                c.DeclinedTicksSinceEpoch AS DeclinedTicksSinceEpoch,
                c.Amount AS Amount
                FROM dbo.ClaimReadModels AS c
                WHERE (c.IsTestData <> 1)
                AND (c.TenantId = @tenantId) 
                AND (c.Environment = @environment)  
                AND (c.Status in @statuses)"
            );

            if (filters.DateIsAfterTicks.HasValue || filters.DateIsBeforeTicks.HasValue)
            {
                if (filters.DateIsAfterTicks.HasValue && filters.DateIsBeforeTicks.HasValue)
                {
                    queryBuilder.Append(@" AND (((c.DeclinedTicksSinceEpoch > @dateIsAfterTicks) AND (c.DeclinedTicksSinceEpoch < @dateIsBeforeTicks))
                    OR ((c.SettledTicksSinceEpoch > @dateIsAfterTicks) AND (c.SettledTicksSinceEpoch < @dateIsBeforeTicks)))");
                    parameters.Add("@dateIsAfterTicks", filters.DateIsAfterTicks);
                    parameters.Add("@dateIsBeforeTicks", filters.DateIsBeforeTicks);
                }
                else if (filters.DateIsAfterTicks.HasValue && !filters.DateIsBeforeTicks.HasValue)
                {
                    queryBuilder.Append(@" AND ((c.DeclinedTicksSinceEpoch > @dateIsAfterTicks)
                    OR (c.SettledTicksSinceEpoch > @dateIsAfterTicks))");
                    parameters.Add("@dateIsAfterTicks", filters.DateIsAfterTicks);
                }
                else if (filters.DateIsBeforeTicks.HasValue && !filters.DateIsAfterTicks.HasValue)
                {
                    queryBuilder.Append(@" AND ((c.DeclinedTicksSinceEpoch < @dateIsBeforeTicks)
                    OR (c.SettledTicksSinceEpoch < @dateIsBeforeTicks))");
                    parameters.Add("@dateIsBeforeTicks", filters.DateIsBeforeTicks);
                }
            }

            var organisationCondition = string.Empty;
            var organisationConditionForRideProtect = string.Empty;
            if (filters.OrganisationIds.Any())
            {
                parameters.Add("@OrganisationIds", filters.OrganisationIds);
                organisationCondition = " AND (c.OrganisationId IN @OrganisationIds)";
                organisationConditionForRideProtect = " AND (c.OrganisationId NOT IN @OrganisationIds)";
            }

            bool queryHasRideProtect = filters.IsRideProtectOrganisation;
            bool isQueryProductIdsSpecified = filters.ProductIds.Any();
            if (queryHasRideProtect)
            {
                queryHasRideProtect = (isQueryProductIdsSpecified && filters.ProductIds.Contains(filters.RideProtectProductId ?? Guid.Empty)) || !isQueryProductIdsSpecified;
            }

            if (queryHasRideProtect)
            {
                parameters.Add("@productIdRideProtect", filters.RideProtectProductId ?? Guid.Empty);
                var productAndOrganisationCondition = $"(c.ProductId = @productIdRideProtect {organisationConditionForRideProtect})";
                if (isQueryProductIdsSpecified)
                {
                    // quotes from ride-protect organisation of specified productIDs and ride-protect quote from any organisation
                    parameters.Add("@productIds", filters.ProductIds);
                    productAndOrganisationCondition = $"AND ({productAndOrganisationCondition} OR (c.ProductId in @productIds {organisationCondition}))";
                }
                else
                {
                    // quotes from ride-protect organisation and ride-protect quote from any organisation
                    productAndOrganisationCondition = $"AND ({productAndOrganisationCondition} {organisationCondition.Replace("AND", "OR")})";
                }

                queryBuilder.Append(productAndOrganisationCondition);
            }
            else
            {
                queryBuilder.Append(organisationCondition);
                if (isQueryProductIdsSpecified)
                {
                    parameters.Add("@productIds", filters.ProductIds);
                    queryBuilder.Append(" AND (c.ProductId in @productIds)");
                }
            }

            // filters for user permission to view quotes
            if (filters.OwnerUserId != null)
            {
                parameters.Add("@OwnerUserId", filters.OwnerUserId);
                queryBuilder.Append(" AND (c.OwnerUserId = @OwnerUserId)");
            }

            if (filters.CustomerId != null)
            {
                parameters.Add("@CustomerId", filters.CustomerId);
                queryBuilder.Append(" AND (c.CustomerId = @CustomerId)");
            }

            var sql = queryBuilder.ToString();
            sql = $"SELECT * from ({sql}) as qResult ORDER BY qResult.CreatedTicksSinceEpoch ASC";

            using (var connection = new SqlConnection(this.connection.UBind))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
                {
                    sql = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; " + sql;
                    var command = new CommandDefinition(
                        sql, parameters, transaction, 180, System.Data.CommandType.Text, CommandFlags.Buffered, cancellationToken);
                    var result = await connection.QueryAsync<ClaimDashboardSummaryModel>(command);
                    transaction.Commit();
                    return result;
                }
            }
        }

        /// <summary>
        /// Used for joins to capture multiple entity data.
        /// </summary>
        internal class ClaimAndOrganisation
        {
            /// <summary>
            /// Gets or sets the claim.
            /// </summary>
            public ClaimReadModel Claim { get; set; } = null!;

            /// <summary>
            /// Gets or sets the organisation.
            /// </summary>
            public OrganisationReadModel Organisation { get; set; } = null!;
        }

        /// <summary>
        /// Used for joins to capture multiple entity data.
        /// </summary>
        internal class ClaimAndOrganisationAndProduct
        {
            /// <summary>
            /// Gets or sets the product.
            /// </summary>
            public Product Product { get; set; } = null!;

            /// <summary>
            /// Gets or sets the claim.
            /// </summary>
            public ClaimReadModel Claim { get; set; } = null!;

            /// <summary>
            /// Gets or sets the organisation.
            /// </summary>
            public OrganisationReadModel Organisation { get; set; } = null!;
        }
    }
}
