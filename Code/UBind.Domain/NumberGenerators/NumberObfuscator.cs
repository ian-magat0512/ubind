// <copyright file="NumberObfuscator.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.NumberGenerators
{
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;

    /// <summary>
    /// Obfuscates a number, e.g. by changing it into a string of alphanumeric digits of a particular length.
    /// </summary>
    public class NumberObfuscator
    {
        private readonly NumberObfuscationMethod method;
        private readonly long sequenceNumber;
        private readonly long offset;

        /// <summary>
        /// Constucts a NumberObfuscator which will use the given defined method for obfuscating sequence numbers.
        /// </summary>
        /// <param name="method">The obfuscation method.</param>
        /// <param name="sequenceNumber">The sequence number.</param>
        /// <param name="offset">An offset to use so that starting numbers aren't all the same.
        /// This is typically a number created from the tenant ID and product ID. With a different offset for every
        /// product, the numbers won't start off being the same, they'll all be slightly different across different
        /// products.</param>
        public NumberObfuscator(NumberObfuscationMethod method, long sequenceNumber, long offset = 0)
        {
            this.method = method;
            this.sequenceNumber = sequenceNumber;
            this.offset = offset;
        }

        public string ObfuscatedResult
        {
            get
            {
                long number = this.sequenceNumber;

                // If we are obfuscating the sequence then do that first
                var sequenceObfuscator = this.GetSequenceObfuscator();
                if (sequenceObfuscator != null)
                {
                    number = sequenceObfuscator.Obfuscate(this.sequenceNumber, this.offset);
                }

                // now obfuscate the appearance of the number
                switch (this.method)
                {
                    case NumberObfuscationMethod.None:
                        return number.ToString();
                    case NumberObfuscationMethod.SixDigitAlphabetic:
                        return this.ConvertToAlphabetic(number).PadLeft(6, 'A');
                    case NumberObfuscationMethod.SixDigitNonSequentialNumber:
                        return number.ToString("D6");
                    case NumberObfuscationMethod.TenDigitNonSequentialNumber:
                        return number.ToString("D10");
                    case NumberObfuscationMethod.EightDigitAlphaNumeric:
                        return this.ConvertToAlphaNumeric(number).PadLeft(8, 'A');
                    default:
                        throw new ErrorException(Errors.General.Unexpected(
                            "When trying to obfuscate a number, an unknown obfuscation method was used."));
                }
            }
        }

        private SequenceObfuscator GetSequenceObfuscator()
        {
            switch (this.method)
            {
                case NumberObfuscationMethod.None:
                    return null;
                case NumberObfuscationMethod.SixDigitAlphabetic:
                    return SequenceObfuscator.SixLetterSequenceObfuscator;
                case NumberObfuscationMethod.SixDigitNonSequentialNumber:
                    return SequenceObfuscator.SixDigitSequenceObfuscator;
                case NumberObfuscationMethod.TenDigitNonSequentialNumber:
                    return SequenceObfuscator.TenDigitSequenceObfuscator;
                case NumberObfuscationMethod.EightDigitAlphaNumeric:
                    return SequenceObfuscator.EightAlphaNumericSequenceObfuscator;
                default:
                    throw new ErrorException(Errors.General.Unexpected(
                        "When trying to obfuscate a number, an unknown obfuscation method was used."));
            }
        }

        private string ConvertToAlphabetic(long number)
        {
            return ((int)number).ToBase26();
        }

        private string ConvertToAlphaNumeric(long number)
        {
            return number.ToBase36();
        }
    }
}
