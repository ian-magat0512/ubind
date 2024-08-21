// <copyright file="CreateClaimCalculationCommand.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Commands.Claim
{
    using System;
    using CSharpFunctionalExtensions;
    using UBind.Domain;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Product;

    /// <summary>
    /// Represents the command for creating new quote version.
    /// </summary>
    [RetryOnDbException(5)]
    public class CreateClaimCalculationCommand : ICommand<Result<Guid, Error>>
    {
        public CreateClaimCalculationCommand(ProductContext productContext, Guid claimId, string formDataJson)
        {
            this.ProductContext = productContext;
            this.ClaimId = claimId;
            this.FormDataJson = formDataJson;
        }

        public string FormDataJson { get; private set; }

        public Guid ClaimId { get; private set; }

        public ProductContext ProductContext { get; private set; }
    }
}
