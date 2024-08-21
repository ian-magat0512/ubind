// <copyright file="QuoteTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Tests.Aggregates.Quote.Quote
{
    using System.Linq;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Aggregates.Quote.Commands;
    using UBind.Domain.Json;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    /// <summary>
    /// Tests for quotes.
    /// </summary>
    public class QuoteTests
    {
        private readonly JsonPath validFormDataPath = new JsonPath("objectProperty.nestedProperty");
        private readonly JsonPath validCalculationResultPath = new JsonPath("questions.ratingPrimary.objectProperty.nestedProperty");

        /// <summary>
        /// Tests select and validate form data patch target function for global commands.
        /// </summary>
        [Fact]
        public void SelectAndValidateFormDataPatchTargets_ReturnsAllQuoteFormDataTargets_ForGlobalCommand()
        {
            // Arrange
            var quote = this.CreateQuoteForPatchTests();
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                this.validFormDataPath,
                this.validCalculationResultPath,
                "hello",
                PolicyDataPatchScope.CreateGlobalPatchScope(),
                PatchRules.None);

            // Act
            var result = quote.SelectAndValidateFormDataPatchTargets(patchCommand);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count());
            var quoteTarget = result.Value
                .Where(target => target.GetType() == typeof(QuotDataPatchTarget))
                .SingleOrDefault();
            Assert.NotNull(quoteTarget);
            var quoteVersionTarget = result.Value
                .OfType<QuoteVersionDataPatchTarget>()
                .SingleOrDefault();
            Assert.NotNull(quoteVersionTarget);
        }

        /// <summary>
        /// Tests select and validate form data patch target function for full quote scoped commands.
        /// </summary>
        [Fact]
        public void SelectAndValidateFormDataPatchTargets_ReturnsAllQuoteFormDataTargets_ForFullQuoteScopedCommand()
        {
            // Arrange
            var quote = this.CreateQuoteForPatchTests();
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                this.validFormDataPath,
                this.validCalculationResultPath,
                "hello",
                PolicyDataPatchScope.CreateFullQuotePatchScope(quote.Id),
                PatchRules.None);

            // Act
            var result = quote.SelectAndValidateFormDataPatchTargets(patchCommand);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count());
            var quoteTarget = result.Value
                .Where(target => target.GetType() == typeof(QuotDataPatchTarget))
                .SingleOrDefault();
            Assert.NotNull(quoteTarget);
            var quoteVersionTarget = result.Value
                .OfType<QuoteVersionDataPatchTarget>()
                .SingleOrDefault();
            Assert.NotNull(quoteVersionTarget);
        }

        /// <summary>
        /// Tests select and validate form data patch target function for latest quote scoped commands.
        /// </summary>
        [Fact]
        public void SelectAndValidateFormDataPatchTargets_ReturnsOnlyQuoteFormDataTarget_ForLatestsQuoteScopedCommand()
        {
            // Arrange
            var quote = this.CreateQuoteForPatchTests();
            var validFormDataPath = new JsonPath("objectProperty.nestedProperty");
            var validCalculationResultPath = new JsonPath("questions.ratingPrimary.objectProperty.nestedProperty");
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                validFormDataPath,
                validCalculationResultPath,
                "hello",
                PolicyDataPatchScope.CreateLatestQuotePatchScope(quote.Id),
                PatchRules.None);

            // Act
            var result = quote.SelectAndValidateFormDataPatchTargets(patchCommand);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            var quoteTarget = result.Value
                .OfType<QuotDataPatchTarget>()
                .SingleOrDefault();
            Assert.NotNull(quoteTarget);
        }

        /// <summary>
        /// Tests select and validate form data patch target function for quote version scoped commands.
        /// </summary>
        [Fact]
        public void SelectAndValidateFormDataPatchTargets_ReturnsOnlyQuoteVersionFormDataTarget_ForQuoteVersionScopedCommand()
        {
            // Arrange
            var quote = this.CreateQuoteForPatchTests();
            var validFormDataPath = new JsonPath("objectProperty.nestedProperty");
            var validCalculationResultPath = new JsonPath("questions.ratingPrimary.objectProperty.nestedProperty");
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                validFormDataPath,
                validCalculationResultPath,
                "hello",
                PolicyDataPatchScope.CreateQuoteVersionPatchScope(quote.Id, 1),
                PatchRules.None);

            // Act
            var result = quote.SelectAndValidateFormDataPatchTargets(patchCommand);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            var quoteTarget = result.Value
                .OfType<QuoteVersionDataPatchTarget>()
                .SingleOrDefault();
            Assert.NotNull(quoteTarget);
        }

        /// <summary>
        /// Tests select and validate form data patch target function for null or empty condition specified data.
        /// </summary>
        [Fact]
        public void SelectAndValidateFormDataPatchTargets_ReturnsError_WhenNullOrEmptyConditonSpecifiedButDataPresent()
        {
            // Arrange
            var quote = this.CreateQuoteForPatchTests();
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                this.validFormDataPath,
                this.validCalculationResultPath,
                "hello",
                PolicyDataPatchScope.CreateQuoteVersionPatchScope(quote.Id, 1),
                PatchRules.PropertyIsMissingOrNullOrEmpty);

            // Act
            var result = quote.SelectAndValidateFormDataPatchTargets(patchCommand);

            // Assert
            Assert.False(result.IsSuccess);
            var expectedError = $"Could not patch quote {quote.Id} form data property: objectProperty.nestedProperty (quote or quote version): Property is not empty, but rules require it to be missing, null, or empty.";
            Assert.Equal(expectedError, result.Error);
        }

        /// <summary>
        /// Tests select and validate form data patch target function for null or empty condition specified data.
        /// </summary>
        [Fact]
        public void SelectAndValidateFormDataPatchTargets_ReturnsSuccess_WhenNullOrEmptyConditonSpecifiedAndDataPresentOnlyInOldFormData()
        {
            // Arrange
            var quote = QuoteFactory.CreateNewBusinessQuote();
            quote.Aggregate
                .WithCalculationResult(
                    quote.Id,
                    FormDataJsonFactory.GetSampleFormDataJsonForPatching(),
                    CalculationResultJsonFactory.GetSampleCalculationResultForPatching())
                .WithCalculationResult(
                    quote.Id,
                    FormDataJsonFactory.GetSampleFormDataJsonForPatching(@""""""),
                    CalculationResultJsonFactory.GetSampleCalculationResultForPatching(@""""""))
                .WithCustomerDetails(quote.Id)
                .WithCustomer()
                .WithCustomerDetails(quote.Id)
                .WithQuoteVersion(quote.Id)
                .WithPolicy(quote.Id);

            var patchCommand = new GivenValuePolicyDataPatchCommand(
                this.validFormDataPath,
                this.validCalculationResultPath,
                "hello",
                PolicyDataPatchScope.CreateQuoteVersionPatchScope(quote.Id, 1),
                PatchRules.PropertyIsMissingOrNullOrEmpty);

            // Act
            var result = quote.SelectAndValidateFormDataPatchTargets(patchCommand);

            // Assert
            Assert.True(result.IsSuccess);
        }

        /// <summary>
        /// Tests select and validate calculation result patch target function for global commands.
        /// </summary>
        [Fact]
        public void SelectAndValidateCalculationResultPatchTargets_ReturnsAllQuoteCalculationResultTargets_ForGlobalCommand()
        {
            // Arrange
            var quote = this.CreateQuoteForPatchTests();
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                this.validFormDataPath,
                this.validCalculationResultPath,
                "hello",
                PolicyDataPatchScope.CreateGlobalPatchScope(),
                PatchRules.None);

            // Act
            var result = quote.SelectAndValidateCalculationResultPatchTargets(patchCommand);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count());
            var quoteTarget = result.Value
                .Where(target => target.GetType() == typeof(QuotDataPatchTarget))
                .SingleOrDefault();
            Assert.NotNull(quoteTarget);
            var quoteVersionTarget = result.Value
                .OfType<QuoteVersionDataPatchTarget>()
                .SingleOrDefault();
            Assert.NotNull(quoteVersionTarget);
        }

        /// <summary>
        /// Tests select and validate calculation result patch target function for full quote scoped commands.
        /// </summary>
        [Fact]
        public void SelectAndValidateCalculationResultPatchTargets_ReturnsAllQuoteCalculationResultTargets_ForFullQuoteScopedCommand()
        {
            // Arrange
            var quote = this.CreateQuoteForPatchTests();
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                this.validFormDataPath,
                this.validCalculationResultPath,
                "hello",
                PolicyDataPatchScope.CreateFullQuotePatchScope(quote.Id),
                PatchRules.None);

            // Act
            var result = quote.SelectAndValidateCalculationResultPatchTargets(patchCommand);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count());
            var quoteTarget = result.Value
                .Where(target => target.GetType() == typeof(QuotDataPatchTarget))
                .SingleOrDefault();
            Assert.NotNull(quoteTarget);
            var quoteVersionTarget = result.Value
                .OfType<QuoteVersionDataPatchTarget>()
                .SingleOrDefault();
            Assert.NotNull(quoteVersionTarget);
        }

        /// <summary>
        /// Tests select and validate calculation result patch target function for latest quote scoped commands.
        /// </summary>
        [Fact]
        public void SelectAndValidateCalculationResultPatchTargets_ReturnsOnlyQuoteCalculationResultTarget_ForLatestsQuoteScopedCommand()
        {
            // Arrange
            var quote = this.CreateQuoteForPatchTests();
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                this.validFormDataPath,
                this.validCalculationResultPath,
                "hello",
                PolicyDataPatchScope.CreateLatestQuotePatchScope(quote.Id),
                PatchRules.None);

            // Act
            var result = quote.SelectAndValidateCalculationResultPatchTargets(patchCommand);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            var quoteTarget = result.Value
                .OfType<QuotDataPatchTarget>()
                .SingleOrDefault();
            Assert.NotNull(quoteTarget);
        }

        /// <summary>
        /// Tests select and validate calculation result patch target function for quote version scoped commands.
        /// </summary>
        [Fact]
        public void SelectAndValidateCalculationResultPatchTargets_ReturnsOnlyQuoteVersionCalculationResultTarget_ForQuoteVersionScopedCommand()
        {
            // Arrange
            var quote = this.CreateQuoteForPatchTests();
            var patchCommand = new GivenValuePolicyDataPatchCommand(
                this.validFormDataPath,
                this.validCalculationResultPath,
                "hello",
                PolicyDataPatchScope.CreateQuoteVersionPatchScope(quote.Id, 1),
                PatchRules.None);

            // Act
            var result = quote.SelectAndValidateCalculationResultPatchTargets(patchCommand);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            var quoteTarget = result.Value
                .OfType<QuoteVersionDataPatchTarget>()
                .SingleOrDefault();
            Assert.NotNull(quoteTarget);
        }

        private Quote CreateQuoteForPatchTests(
            string formDataValueToBeOverwritten = @"""foo""",
            string calculationResultToBeOverwritten = @"""foo""")
        {
            var quote = QuoteFactory.CreateNewBusinessQuote();
            var aggregate = quote.Aggregate
                .WithCalculationResult(
                    quote.Id,
                    FormDataJsonFactory.GetSampleFormDataJsonForPatching(formDataValueToBeOverwritten),
                    CalculationResultJsonFactory.GetSampleCalculationResultForPatching(calculationResultToBeOverwritten))
                .WithCustomerDetails(quote.Id)
                .WithCustomer()
                .WithCustomerDetails(quote.Id)
                .WithQuoteVersion(quote.Id)
                .WithPolicy(quote.Id);
            return aggregate.GetQuoteOrThrow(quote.Id);
        }
    }
}
