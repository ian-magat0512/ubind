// <copyright file="SequenceObfuscator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.NumberGenerators
{
    using System;

    /// <summary>
    /// Obfuscates number sequences using multiplicative inverses.
    /// </summary>
    /// <remarks>
    /// See https://ericlippert.com/2013/11/14/a-practical-use-of-multiplicative-inverses/
    /// .</remarks>
    public class SequenceObfuscator
    {
        private readonly long maxNumber;
        private readonly long coprime;
        private readonly long halfwayOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceObfuscator"/> class.
        /// </summary>
        /// <param name="maxNumber">The maximum number in the sequence to be obfuscated.</param>
        /// <param name="coprime">The coprime used to cycle through sequence out of order.</param>
        public SequenceObfuscator(long maxNumber, long coprime)
        {
            this.maxNumber = maxNumber;
            this.coprime = coprime;
            this.halfwayOffset = (maxNumber + 1) / 2;
        }

        /// <summary>
        /// Gets a sequence obfuscator for obfuscating a sequence of numbers up to 999,999.
        /// This maximum number is derived from 10^6 - 1 = 999999 (largest 6 digit number).
        /// </summary>
        public static SequenceObfuscator SixDigitSequenceObfuscator =>
            new SequenceObfuscator(
                999999,
                333339); // Coprime with 10^6 (sequence length)

        /// <summary>
        /// Gets a sequence obfuscator for obfuscating a sequence of numbers up to 308,915,775.
        /// This maximum number is derived from 26^6 - 1 = 308915775, equivalent to ZZZZZZ in base 26 using letters A-Z.
        /// </summary>
        public static SequenceObfuscator SixLetterSequenceObfuscator =>
            new SequenceObfuscator(
                308915775,
                40000003); // Coprime with 26^6

        /// <summary>
        /// Gets a sequence obfuscator for obfuscating a sequence of numbers up to 2,821,109,907,455.
        /// This maximum number is derived from 36^8 - 1 = 2821109907455, equivalent to ZZZZZZZZ in base 36 using numbers 0-9, and letters A-Z.
        /// </summary>
        public static SequenceObfuscator EightAlphaNumericSequenceObfuscator =>
            new SequenceObfuscator(
                2821109907455,
                40000003); // Coprime number

        /// <summary>
        /// Gets a sequence obfuscator for obfuscating a sequence of numbers up to 9,999,999,999.
        /// This maximum number is derived from 10^10 - 1 = 9999999999 (largest 10 digit number).
        /// </summary>
        public static SequenceObfuscator TenDigitSequenceObfuscator =>
            new SequenceObfuscator(
                9999999999,
                3333333339); // Coprime with (10^10, sequence length)

        /// <summary>
        /// Gets the size of the sequence from zero to maxNumber.
        /// </summary>
        private long SetSize => this.maxNumber + 1;

        /// <summary>
        /// Obfuscate a number from the sequence zero to N, by mapping each number in the sequence to a cycle through all numbers out of order.
        /// </summary>
        /// <param name="input">The sequence number.</param>
        /// <returns>A different number from the sequence guaranteed to be only mapped to by the given input within the range.</returns>
        public long Obfuscate(long input, long customOffset = 0)
        {
            if (input < 0)
            {
                throw new InvalidOperationException($"The sequence number ({input}) must be greater than zero.");
            }

            if (input > this.maxNumber)
            {
                // we allow the input to be larger than the max number,
                // but we will cycle the input to fit within the set size.
                input = input % this.SetSize;
            }

            var result = (this.halfwayOffset + (input * this.coprime)) % this.SetSize;
            result = result + customOffset;

            // check if adding the custom offset will exceed the max number, if so subtract the custom offset.
            if (result > this.maxNumber)
            {
                return result - customOffset;
            }

            return result;
        }
    }
}
