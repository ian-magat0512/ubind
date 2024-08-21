// <copyright file="CalculationResultPaths.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable SA1600
#pragma warning disable CS1591

namespace UBind.Domain.Aggregates.Common
{
    public class CalculationResultPaths
    {
        public const string JsonKey = @"Json";
        public const string QuestionsKey = @"questions";
        public const string FormModelKey = @"formModel";
        public const string PolicyAdditionsKey = @"policyAdditions";
        public const string InsurersTermsKey = @"insurersTerms";
        public const string ExcessDeductibleKey = @"excessDeductible";
        public const string DocumentCommentsKey = @"documentComments";
        public const string ClausesKey = @"clauses";
        public const string ExcessCommentsKey = @"excessComments";

        public const string RatingPrimaryPath = @"questions.ratingPrimary";
        public const string PolicyAdditionsPath = @"questions.policyAdditions";
        public const string ClausesPath = @"questions.ratingPrimary.clauses";
        public const string ExcessCommentsPath = @"questions.ratingPrimary.excessComments";
        public const string DocumentCommentsPath = @"questions.ratingPrimary.documentComments";
    }
}
