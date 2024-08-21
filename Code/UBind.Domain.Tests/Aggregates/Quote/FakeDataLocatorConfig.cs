// <copyright file="FakeDataLocatorConfig.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Aggregates.Quote
{
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.DataLocator;

    public class FakeDataLocatorConfig : IDataLocatorConfig
    {
        private IQuoteDatumLocations quoteDataLocations;
        private DataLocators dataLocators;

        public FakeDataLocatorConfig(IQuoteDatumLocations quoteDataLocations, DataLocators dataLocators)
        {
            this.dataLocators = dataLocators;
            this.quoteDataLocations = quoteDataLocations;
        }

        public IQuoteDatumLocations QuoteDataLocations => this.quoteDataLocations;

        public DataLocators DataLocators => this.dataLocators;
    }
}
