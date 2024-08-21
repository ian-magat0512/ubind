// <copyright file="UrlTokenGenerator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.NumberGenerators;

using System;

/// <summary>
/// For generating URL tokens from a seed.
/// </summary>
public class UrlTokenGenerator : IUrlTokenGenerator
{

    public UrlTokenGenerator()
    {
    }

    /// <summary>
    /// Gets the unique number generation method.
    /// </summary>
    private NumberObfuscationMethod ObfuscationMethod => NumberObfuscationMethod.EightDigitAlphaNumeric;

    /// <inheritdoc/>
    public string Generate(long sequenceNumber)
    {
        if (this.ObfuscationMethod == NumberObfuscationMethod.None)
        {
            throw new ArgumentException($"Cannot automatically generate a unique token when method is {this.ObfuscationMethod.ToString()}");
        }

        return new NumberObfuscator(this.ObfuscationMethod, sequenceNumber).ObfuscatedResult.ToLower();
    }
}
