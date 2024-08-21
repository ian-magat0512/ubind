// <copyright file="QueryFactoryTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.ThirdPartyDataSets.Query.Gnaf
{
    using FluentAssertions;
    using Microsoft.Extensions.DependencyInjection;
    using UBind.Application.ThirdPartyDataSets.Query.QueryFactories.Gnaf;
    using UBind.Domain.ThirdPartyDataSets.Gnaf;
    using Xunit;

    public class QueryFactoryTest
    {
        private readonly ServiceCollection serviceCollection;

        public QueryFactoryTest()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IQueryFactory, QueryFactory>();

            this.serviceCollection = services;
        }

        [Fact]
        public void GnafQueryFactory_ReturnSingleTermNumberWithNo_Asterisk_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var rawSearchString = "100";

            // Act
            var result = sut.CreateQueryTerms(rawSearchString);

            // Assert
            result.Should().Be("100");
        }

        [Fact]
        public void GnafQueryFactory_ReturnSingleTermStringWithPattern_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var rawSearchString = "singleWord";

            // Act
            var result = sut.CreateQueryTerms(rawSearchString);

            // Assert
            result.Should().Be("(singleword* OR s'ingleword*)");
        }

        [Fact]
        public void GnafQueryFactory_ReturnSingleTermStringWithPatternLowerCase_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var rawSearchString = "SINGLEWORD";

            // Act
            var result = sut.CreateQueryTerms(rawSearchString);

            // Assert
            result.Should().Be("(singleword* OR s'ingleword*)");
        }

        [Fact]
        public void GnafQueryFactory_ReturnDoubleTermNumberAndString_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var rawSearchString = "100 secondword";

            // Act
            var result = sut.CreateQueryTerms(rawSearchString);

            // Assert
            result.Should().Be("100 AND (secondword* OR s'econdword*)");
        }

        [Fact]
        public void GnafQueryFactory_ReturnDoubleTermString_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var rawSearchString = "firstword secondword";

            // Act
            var result = sut.CreateQueryTerms(rawSearchString);

            // Assert
            result.Should().Be("(firstword* OR f'irstword*) AND (secondword* OR s'econdword*)");
        }

        [Fact]
        public void GnafQueryFactory_ReturnDoubleTermStringLowerCase_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var rawSearchString = "FIRSTWORD SECONDWORD";

            // Act
            var result = sut.CreateQueryTerms(rawSearchString);

            // Assert
            result.Should().Be("(firstword* OR f'irstword*) AND (secondword* OR s'econdword*)");
        }

        [Fact]
        public void GnafQueryFactory_ReturnMoreTermStringAndNumbers_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var rawSearchString = "100 secondword thirdword";

            // Act
            var result = sut.CreateQueryTerms(rawSearchString);

            // Assert
            result.Should().Be("100 AND (secondword* OR s'econdword*) AND (thirdword* OR t'hirdword*)");
        }

        [Fact]
        public void GnafQueryFactory_ReturnMoreTermSingleLetter_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var rawSearchString = "o neil";

            // Act
            var result = sut.CreateQueryTerms(rawSearchString);

            // Assert
            result.Should().Be("(oneil* OR o'neil*)");
        }

        [Fact]
        public void GnafQueryFactory_ReturnSingleTermWithSingleQuote_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var rawSearchString = "o'neil";

            // Act
            var result = sut.CreateQueryTerms(rawSearchString);

            // Assert
            result.Should().Be("(o'neil* OR o'neil*)");
        }

        [Fact]
        public void GnafQueryFactory_ReturnDouleTermBothNumbers_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var rawSearchString = "1 2";

            // Act
            var result = sut.CreateQueryTerms(rawSearchString);

            // Assert
            result.Should().Be("1 AND 2*");
        }

        [Fact]
        public void GnafQueryFactory_ReturnMoreTermAllNumbers_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var rawSearchString = "1 2 3";

            // Act
            var result = sut.CreateQueryTerms(rawSearchString);

            // Assert
            result.Should().Be("1 AND 2* AND 3*");
        }

        [Fact]
        public void GnafQueryFactory_ReturnDoubleTermsFirstTextSecondNumber_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var rawSearchString = "firstword 2";

            // Act
            var result = sut.CreateQueryTerms(rawSearchString);

            // Assert
            result.Should().Be("(firstword* OR f'irstword*) AND 2*");
        }

        [Fact]
        public void CreateQueryFilterFields_ReturnSingleTermNumber_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var rawSearchString = "100";

            // Act
            var result = sut.CreateQueryFilterFields(sut.CreateQueryTerms(rawSearchString));

            // Assert
            result.Length.Should().Be(1);
            result[0].Should().Be(AddressConstants.NumberFirst);
        }

        [Fact]
        public void CreateQueryFilterFields_ReturnDoubleTermsNumberAndText_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var defaultFilterFields = this.GetDefaultFilterFields();

            var rawSearchString = "1 collins";

            // Act
            var result = sut.CreateQueryFilterFields(sut.CreateQueryTerms(rawSearchString));

            // Assert
            result.Length.Should().Be(defaultFilterFields.Length);
            foreach (var filter in result)
            {
                defaultFilterFields.Should().Contain(filter);
            }
        }

        [Fact]
        public void CreateQueryFilterFields_ReturnSingleTermText_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var defaultFilterFields = this.GetDefaultFilterFields();

            var rawSearchString = "singleword";

            // Act
            var result = sut.CreateQueryFilterFields(sut.CreateQueryTerms(rawSearchString));

            // Assert
            result.Length.Should().Be(defaultFilterFields.Length);
            foreach (var filter in result)
            {
                defaultFilterFields.Should().Contain(filter);
            }
        }

        [Fact]
        public void CreateQueryFilterFields_ReturnDefaultFilterFieldsAndFullAddres_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var defaultFilterFields = this.GetDefaultFilterFields();

            var rawSearchString = "100 secondword third fourth 1234";

            // Act
            var result = sut.CreateQueryFilterFields(sut.CreateQueryTerms(rawSearchString));

            // Assert
            result.Length.Should().Be(defaultFilterFields.Length);
            foreach (var filter in result)
            {
                defaultFilterFields.Should().Contain(filter);
            }
        }

        [Fact]
        public void CreateQueryFilterFields_ReturnDoubleTermsAllNumbers_DefaultFilterFieldsAndFullAddress_WhenCallingCreateQueryTerms()
        {
            // Arrange
            var service = this.serviceCollection.BuildServiceProvider();
            var sut = service.GetService<IQueryFactory>();
            var defaultFilterFields = this.GetDefaultFilterFields();

            var rawSearchString = "2 3";

            // Act
            var result = sut.CreateQueryFilterFields(sut.CreateQueryTerms(rawSearchString));

            // Assert
            result.Length.Should().Be(defaultFilterFields.Length);
            foreach (var filter in result)
            {
                defaultFilterFields.Should().Contain(filter);
            }
        }

        private string[] GetDefaultFilterFields()
        {
            return new[]
            {
                AddressConstants.FlatType,
                AddressConstants.FlatNumber,
                AddressConstants.LevelNumber,
                AddressConstants.LotNumber,
                AddressConstants.NumberFirst,
                AddressConstants.StreetName,
                AddressConstants.StreetTypeCode,
                AddressConstants.StreetTypeShortName,
                AddressConstants.LocalityName,
                AddressConstants.StateAbbreviation,
                AddressConstants.PostCode,
                AddressConstants.FullAddress,
                AddressConstants.Latitude,
                AddressConstants.Longitude,
            };
        }
    }
}
