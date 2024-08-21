// <copyright file="PolicyQuoteSearchIndexWriteModelHelper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Helpers;

using UBind.Domain.Search;
using UBind.Domain.Product;

public static class PolicyQuoteSearchIndexWriteModelHelper
{

    public static void PopulateProductNames(IEnumerable<IPolicySearchIndexWriteModel> policies, IEnumerable<Product> products)
    {
        foreach (var policy in policies)
        {
            var product = products.FirstOrDefault(p => p.Id == policy.ProductId);
            if (product != null)
            {
                policy.ProductName = product.Details?.Name;
            }
            else
            {
                policy.ProductName = "Disabled Product";
            }
        }
    }

    public static void PopulateProductNames(IEnumerable<IQuoteSearchIndexWriteModel> quotes, IEnumerable<Product> products)
    {
        foreach (var quote in quotes)
        {
            var product = products.FirstOrDefault(p => p.Id == quote.ProductId);
            if (product != null)
            {
                quote.ProductName = product.Details?.Name;
            }
            else
            {
                quote.ProductName = "Disabled Product";
            }
        }
    }
}
