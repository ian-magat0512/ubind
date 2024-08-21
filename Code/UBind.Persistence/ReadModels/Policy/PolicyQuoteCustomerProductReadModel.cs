﻿// <copyright file="PolicyQuoteCustomerProductReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.Customer;

    /// <summary>
    /// For queries joining policy, quote, customers read models with their product.
    /// </summary>
    internal class PolicyQuoteCustomerProductReadModel : PolicyQuoteReadModel
    {
        public Product Product { get; set; }

        public CustomerReadModel Customer { get; set; }
    }
}
