// <copyright file="RollingNumber.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Helpers
{
    /// <summary>
    /// RollingNumber provides a flexible and reusable solution for managing rolling numeric sequences. When the current value
    /// reaches the maximum value, it wraps around to 1 (or 0 if isZeroBased is set to true). The returned data is always
    /// formatted to a string using the provided format string to give a neat way of adding prefix or suffix eg 01, 001.
    /// This was created to help manage the table suffixes in ThirdPartyDataSets, so that when we
    /// update a dataset, we can put it into a new table suffixed by a table number, which we increment each time.
    /// </summary>
    public class RollingNumber
    {
        private int maxValue;
        private string stringFormat;
        private int currentValue;
        private bool isZeroBased;

        /// <summary>
        /// Initializes a new instance of the <see cref="RollingNumber"/> class.
        /// </summary>
        /// <param name="maxValue"></param> The maximum value of the sequence.
        /// <param name="initialValue"></param> The initial value of the sequence.
        /// <param name="numberFormat"></param> The format string to use when formatting the returned data.
        /// <param name="isZeroBased"></param> Whether the sequence should be zero based.
        public RollingNumber(int maxValue, string initialValue = "0", string numberFormat = "D2", bool isZeroBased = false)
        {
            this.maxValue = maxValue;
            this.stringFormat = numberFormat;
            this.currentValue = this.ToInt(initialValue);
            this.isZeroBased = isZeroBased;
        }

        /// <summary>
        /// Sets or overrides the current value.
        /// </summary>
        /// <param name="value"></param>
        public void SetCurrent(string value)
        {
            this.currentValue = this.ToInt(value);
        }

        /// <summary>
        /// Dncrements the current value, ensuring it wraps around when reaching the maximum value, and returns the formatted result.
        /// </summary>
        /// <returns>Formatted value</returns>
        public string GetPrevious()
        {
            this.currentValue = (this.currentValue - 1) < 0 ? this.maxValue : (this.currentValue - 1) % this.maxValue;
            if (!this.isZeroBased && this.currentValue == 0)
            {
                this.currentValue = this.maxValue;
            }
            return this.FormatCurrentValue();
        }

        /// <summary>
        /// Increments the current value, ensuring it wraps around when reaching the maximum value, and returns the formatted result.
        /// </summary>
        /// <returns>Formatted value</returns>
        public string GetNext()
        {
            this.currentValue = (this.currentValue + 1) % (this.maxValue + 1);
            if (!this.isZeroBased && this.currentValue == 0)
            {
                this.currentValue = 1;
            }
            return this.FormatCurrentValue();
        }

        private string FormatCurrentValue()
        {
            return this.currentValue.ToString(this.stringFormat);
        }

        private int ToInt(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0;
            }
            if (int.TryParse(value, out int result))
            {
                return result;
            }

            throw new ArgumentException($"'{value}' is not a number.");
        }
    }
}
