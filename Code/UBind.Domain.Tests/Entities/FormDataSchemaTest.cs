// <copyright file="FormDataSchemaTest.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1600

namespace UBind.Domain.Tests.Entities
{
    using Newtonsoft.Json.Linq;
    using UBind.Domain.Json;
    using UBind.Domain.Tests.Fakes;
    using Xunit;

    public class FormDataSchemaTest
    {
        /// <summary>
        /// This evaluates Questions if can be change and changed after approval, asserting if invalidates approval.
        /// </summary>
        [Fact]
        public void EvaluateQuestionChanges_WithQuestionCanBeChangeChangedAfterApprove_InvalidatesApproval()
        {
            // Arrange
            var questionKey = "price";
            var formData = ConfigurationJsonFactory.GetSampleWithQuestionSet(questionKey, "true");
            var sut = new FormDataSchema(JObject.Parse(formData));
            var oldFormData = $@"{{
		        ""formModel"": {{
			        ""{questionKey}"": ""100""
		        }}
            }}";
            var newFormData = $@"{{
		        ""formModel"": {{
                    ""{questionKey}"": ""200""
		        }}
            }}";

            // Act
            var result = sut.EvaluateQuestionChanges(new CachingJObjectWrapper(oldFormData), new CachingJObjectWrapper(newFormData));

            // Assert
            Assert.False(result.ChangeInvalidatesApproval);
        }

        /// <summary>
        /// This evaluates Questions if cannot be change and changed after approval, asserting if invalidates approval.
        /// </summary>
        [Fact]
        public void EvaluateQuestionChanges_WithQuestionCannotBeChangeChangedAfterApprove_InvalidatesApproval()
        {
            // Arrange
            var questionKey = "price";
            var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSet(questionKey, "false");
            var sut = new FormDataSchema(JObject.Parse(baseConfiguration));
            var oldFormData = $@"{{
		        ""formModel"": {{
			        ""{questionKey}"": ""100""
		        }}
            }}";
            var newFormData = $@"{{
		        ""formModel"": {{
                    ""{questionKey}"": ""200""
		        }}
            }}";

            // Act
            var result = sut.EvaluateQuestionChanges(new CachingJObjectWrapper(oldFormData), new CachingJObjectWrapper(newFormData));

            // Assert
            Assert.True(result.ChangeInvalidatesApproval);
        }

        /// <summary>
        /// This evaluates Questions if cannot be change and has no changes , asserting if invalidates approval.
        /// </summary>
        [Fact]
        public void EvaluateQuestionChanges_WithQuestionCanBeChangeAndNoChanges_InvalidatesApproval()
        {
            // Arrange
            var questionKey = "price";
            var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSet(questionKey, "true");
            var sut = new FormDataSchema(JObject.Parse(baseConfiguration));
            var oldFormData = $@"{{
		        ""formModel"": {{
			        ""{questionKey}"": ""100""
		        }}
            }}";
            var newFormData = $@"{{
		        ""formModel"": {{
                    ""{questionKey}"": ""100""
		        }}
            }}";

            // Act
            var result = sut.EvaluateQuestionChanges(new CachingJObjectWrapper(oldFormData), new CachingJObjectWrapper(newFormData));

            // Assert
            Assert.False(result.ChangeInvalidatesApproval);
        }

        /// <summary>
        /// This evaluates Questions if cannot be change and has no changes , asserting if invalidates approval.
        /// </summary>
        [Fact]
        public void EvaluateQuestionChanges_WithQuestionCannotBeChangeAndNoChanges_InvalidatesApproval()
        {
            // Arrange
            var questionKey = "price";
            var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSet(questionKey, "false");
            var sut = new FormDataSchema(JObject.Parse(baseConfiguration));
            var oldFormData = $@"{{
		        ""formModel"": {{
			        ""{questionKey}"": ""100""
		        }}
            }}";
            var newFormData = $@"{{
		        ""formModel"": {{
                    ""{questionKey}"": ""100""
		        }}
            }}";

            // Act
            var result = sut.EvaluateQuestionChanges(new CachingJObjectWrapper(oldFormData), new CachingJObjectWrapper(newFormData));

            // Assert
            Assert.False(result.ChangeInvalidatesApproval);
        }

        /// <summary>
        /// This evaluates changes on Questions without meta data , asserting if does not invalidates approval.
        /// </summary>
        [Fact]
        public void EvaluateQuestionChanges_WithOutQUestionMetadata_DoesNotInvalidateApproval()
        {
            // Arrange
            var questionKey = "price";
            var baseConfiguration = $@"{{}}";
            var sut = new FormDataSchema(JObject.Parse(baseConfiguration));
            var oldFormData = $@"{{
		        ""formModel"": {{
			        ""{questionKey}"": ""100""
		        }}
            }}";
            var newFormData = $@"{{
		        ""formModel"": {{
                    ""{questionKey}"": ""200""
		        }}
            }}";

            // Act
            var result = sut.EvaluateQuestionChanges(new CachingJObjectWrapper(oldFormData), new CachingJObjectWrapper(newFormData));

            // Assert
            Assert.False(result.ChangeInvalidatesApproval);
        }

        /// <summary>
        /// This evaluates changes on Questions with missing property in the calculation result , asserting if does not invalidates approval.
        /// </summary>
        [Fact]
        public void EvaluateQuestionChanges_WithMissingPropertyInNewFormModelDoesNotInvalidateApproval()
        {
            // Arrange
            var questionKey = "price";
            var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSet(questionKey, "false");

            var oldFormData = $@"{{
		        ""formModel"": {{
			        ""price"": ""11""
		        }}
            }}";
            var newFormData = $@"{{
		        ""formModel"": {{
		        }}
            }}";
            var sut = new FormDataSchema(JObject.Parse(baseConfiguration));

            // Act
            var result = sut.EvaluateQuestionChanges(new CachingJObjectWrapper(oldFormData), new CachingJObjectWrapper(newFormData));

            // Assert
            Assert.False(result.ChangeInvalidatesApproval);
        }

        /// <summary>
        /// This evaluates changes on Questions with new property in the new calculation result , asserting if does not invalidates approval.
        /// </summary>
        [Fact]
        public void EvaluateQuestionChanges_WithNewPropertyInNewFormModelDoesNotInvalidateApproval()
        {
            // Arrange
            var questionKey = "price";
            var baseConfiguration = ConfigurationJsonFactory.GetSampleWithQuestionSet(questionKey, "false");

            var previousCalculationData = $@"{{
	            ""formModel"": {{
	            }}
            }}";

            var newCalculationData = $@"{{
	            ""formModel"": {{
			        ""price"": ""11""
	            }}
            }}";
            var sut = new FormDataSchema(JObject.Parse(baseConfiguration));

            // Act
            var result = sut.EvaluateQuestionChanges(new CachingJObjectWrapper(previousCalculationData), new CachingJObjectWrapper(newCalculationData));

            // Assert
            Assert.False(result.ChangeInvalidatesApproval);
        }
    }
}
