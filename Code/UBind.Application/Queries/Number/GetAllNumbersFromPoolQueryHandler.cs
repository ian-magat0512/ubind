// <copyright file="GetAllNumbersFromPoolQueryHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Number;

using UBind.Application.Authorisation;
using UBind.Domain;
using UBind.Domain.Helpers;
using UBind.Domain.Patterns.Cqrs;
using UBind.Domain.ReadModel;
using UBind.Domain.Services;

public class GetAllNumbersFromPoolQueryHandler : IQueryHandler<GetAllNumbersFromPoolQuery, NumberPoolSummaryModel>
{
    private readonly INumberPoolService numberPoolService;
    private readonly ICachingResolver cacheResolver;
    private readonly IAuthorisationService authorisationService;

    public GetAllNumbersFromPoolQueryHandler(
        INumberPoolService numberPoolService, ICachingResolver cacheResolver, IAuthorisationService authorisationService)
    {
        this.numberPoolService = numberPoolService;
        this.cacheResolver = cacheResolver;
        this.authorisationService = authorisationService;
    }

    public async Task<NumberPoolSummaryModel> Handle(GetAllNumbersFromPoolQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var productModel = await this.cacheResolver.GetProductOrThrow(new GuidOrAlias(request.Tenant), new GuidOrAlias(request.Product));
        await this.authorisationService.ThrowIfUserCannotAccessTenant(productModel.TenantId, request.User, "get", request.NumberPoolName);
        IEnumerable<string> numbers = this.numberPoolService.GetAll(
                productModel.TenantId,
                productModel.Id,
                request.NumberPoolName,
                request.Environment)
                .OrderBy(n => n);
        var model = new NumberPoolSummaryModel(productModel.Id, request.Environment, numbers);
        return model;
    }
}
