// <copyright file="DelimiterSeparatedValuesServiceTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Services.DelimiterSeparatedValues
{
    using System;
    using System.Data;
    using System.IO;
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.Services.DelimiterSeparatedValues;
    using UBind.Application.Tests.ThirdPartyDataSets;
    using Xunit;

    public class DelimiterSeparatedValuesServiceTest : IClassFixture<ThirdPartyDataSetsTestFixture>
    {
        private readonly ServiceCollection serviceCollection;

        public DelimiterSeparatedValuesServiceTest(ThirdPartyDataSetsTestFixture thirdPartyDataSetsTestFixture)
        {
            var services = new ServiceCollection();

            services.AddSingleton<IDelimiterSeparatedValuesService, DelimiterSeparatedValuesService>();
            services.AddSingleton<IDelimiterSeparatedValuesFileProvider, DelimiterSeparatedValuesFileProvider>();
            this.serviceCollection = services;
        }

        [Fact]
        public void DelimiterSeparatedValuesService_ReturnsDataTable_WhenConvertingDelimiterSeparatedValuesToDataTable()
        {
            //// Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IDelimiterSeparatedValuesService>();
            var dsvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Services\\DelimiterSeparatedValues\TestSample_psv.psv");

            var dataTableDefinition = new DataTable();
            dataTableDefinition.Columns.Add(new DataColumn("CODE"));
            dataTableDefinition.Columns.Add(new DataColumn("NAME"));
            dataTableDefinition.Columns.Add(new DataColumn("DESCRIPTION"));

            //// Act
            var result = sut.ConvertDelimiterSeparatedValuesToDataTable(
                dsvFilePath,
                "|",
                dataTableDefinition);

            //// Assert
            result.Should().NotBeNull();
            result.Rows.Count.Should().Be(8);
            result.Rows[0]["CODE"].Should().Be("FNNFS");
        }
    }
}
