// <copyright file="WorkbookParserFixture.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Product.Component.Configuration.Parsers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Microsoft.Extensions.Logging;
    using Moq;
    using NodaTime;
    using UBind.Application.FlexCel;
    using UBind.Domain.Attributes;
    using UBind.Domain.Product.Component;
    using UBind.Domain.Product.Component.Form;

    public class WorkbookParserFixture : IDisposable
    {
        private List<OptionSet> optionSets;

        public WorkbookParserFixture()
        {
            this.Clock = SystemClock.Instance;
            this.Logger = new Mock<ILogger>().Object;
            var codeBaseUrl = new Uri(Assembly.GetExecutingAssembly().Location);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);
            var filePath = Path.Combine(dirPath, "Product\\Component\\Configuration\\Parsers\\carl-dev-Workbook.xlsx");
            byte[] workbookBytes = File.ReadAllBytes(filePath);
            this.Workbook = new FlexCelWorkbook(
                workbookBytes,
                Domain.WebFormAppType.Quote,
                this.Clock);
            this.ColumnToPropertyMapRegistry =
                new AttributeObjectPropertyMapRegistry<WorkbookTableColumnNameAttribute>();
            this.FieldFactory = new WorkbookFieldFactory();
        }

        public IClock Clock { get; }

        public ILogger Logger { get; }

        public FlexCelWorkbook Workbook { get; }

        public IAttributeObjectPropertyMapRegistry<WorkbookTableColumnNameAttribute> ColumnToPropertyMapRegistry { get; }

        public IWorkbookFieldFactory FieldFactory { get; }

        public IReadOnlyList<OptionSet> OptionSets
        {
            get
            {
                if (this.optionSets == null)
                {
                    this.optionSets = new List<OptionSet>
                    {
                        new OptionSet { Name = "Occupations" },
                        new OptionSet { Name = "Liability Limit" },
                        new OptionSet { Name = "Tools Limit" },
                        new OptionSet { Name = "Turnover" },
                        new OptionSet { Name = "Employees" },
                        new OptionSet { Name = "State" },
                        new OptionSet { Name = "Cancellation Reason" },
                        new OptionSet { Name = "Cancellation Competitor Reason" },
                        new OptionSet { Name = "Yes/No" },
                        new OptionSet { Name = "Payment Methods" },
                        new OptionSet { Name = "Debit Options" },
                        new OptionSet { Name = "Credit Cards" },
                        new OptionSet { Name = "Payment Options" },
                        new OptionSet { Name = "Refund Methods" },
                        new OptionSet { Name = "Contact Method" },
                        new OptionSet { Name = "Contact Time Option" },
                    };
                }

                return this.optionSets;
            }
        }

        public void Dispose()
        {
            // cleanup would go here if needed.
        }
    }
}
