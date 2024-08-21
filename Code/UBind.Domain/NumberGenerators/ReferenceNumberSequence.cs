// <copyright file="ReferenceNumberSequence.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.NumberGenerators
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using UBind.Domain;

    /// <summary>
    /// A record for persisting sequences of numbers used for generating for unique numbers for a given tenant,
    /// product, environment, use case and method.
    /// </summary>
    public class ReferenceNumberSequence
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceNumberSequence"/> class.
        /// </summary>
        /// <param name="tenantId">The tenant id the quote number is for.</param>
        /// <param name="productId">The product id the quote number is for.</param>
        /// <param name="environment">The environment the quote number is for.</param>
        /// <param name="useCase">The use-case the number will be used in.</param>
        /// <param name="number">A unique number for the product and environment.</param>
        public ReferenceNumberSequence(
            Guid tenantId,
            Guid productId,
            DeploymentEnvironment environment,
            UniqueNumberUseCase useCase,
            int number)
        {
            this.TenantId = tenantId;
            this.ProductId = productId;
            this.Environment = environment;
            this.UseCase = useCase;
            this.Number = number;
        }

        /// <summary>
        /// Gets the tenant id.
        /// </summary>
        [Key]
        [Column(Order = 1)]
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the product id.
        /// </summary>
        [Key]
        [Column(Order = 2)]
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets the environment the quote number is for.
        /// </summary>
        [Key]
        [Column(Order = 3)]
        public DeploymentEnvironment Environment { get; private set; }

        /// <summary>
        /// Gets the quote generation method the sequence number is to be used in.
        /// </summary>
        [Key]
        [Column(Order = 4)]
        [Obsolete("Obfuscation method has nothing to do with a sequence number's storage so shouldn't be here. This will be deleted soon.")]
        public NumberObfuscationMethod Method { get; private set; }

        /// <summary>
        /// Gets a unique sequence number to use to generate a unique number for a given tenant, product, environment, use case and method.
        /// </summary>
        [Key]
        [Column(Order = 5)]
        public int Number { get; private set; }

        /// <summary>
        /// Gets the use case the sequence number is to be used in.
        /// </summary>
        [Key]
        [Column(Order = 6)]
        public UniqueNumberUseCase UseCase { get; private set; }

        /// <summary>
        /// Sets the tenant ID.
        /// </summary>
        /// <param name="tenantId">The tenant ID to place.</param>
        public void SetTenantId(Guid tenantId)
        {
            this.TenantId = tenantId;
        }

        /// <summary>
        /// Sets the product ID.
        /// </summary>
        /// <param name="productId">The product ID to place.</param>
        public void SetProductId(Guid productId)
        {
            this.ProductId = productId;
        }
    }
}
