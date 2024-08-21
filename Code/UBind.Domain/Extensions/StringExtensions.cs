// <copyright file="StringExtensions.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using CSharpFunctionalExtensions;
    using Humanizer;
    using MoreLinq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using NodaTime.Text;
    using UBind.Domain.Attributes;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Helpers;

    /// <summary>
    /// Extension methods for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Throw an ArgumentException if a string parameter is null or empty.
        /// </summary>
        /// <param name="parameter">The parameter to test.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        public static void ThrowIfArgumentNullOrEmpty(this string? parameter, string parameterName)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        /// <summary>
        /// Throw an ArgumentException if a string parameter is null or whitespace.
        /// </summary>
        /// <param name="parameter">The parameter to test.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        public static void ThrowIfArgumentNullOrWhitespace(this string? parameter, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        /// <summary>
        /// Convert a string to null if it is empty or whitespace.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <returns>The original string value, or null if it is empty or whitespace.</returns>
        public static string? ToNullIfWhitespace(this string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value;
        }

        /// <summary>
        /// Generate a hash from the string using the DBJ2 algorithm;.
        /// </summary>
        /// <param name="value">The string to hash.</param>
        /// <returns>An integer (unsigned long) hash value.</returns>
        public static uint Dbj2Hash(this string value)
        {
            uint hash = 5381;
            foreach (var ch in value)
            {
                hash = ((hash << 5) + hash) + (byte)ch;
            }

            return hash;
        }

        /// <summary>
        /// Converts an ISO8601 string representation of a date (yyyy-mm-dd) to a <see cref="LocalDate"/>.
        /// </summary>
        /// <param name="value">A string representing a date in ISO8601 formar (yyyy-mm-dd).</param>
        /// <returns>A new instance of <see cref="LocalDate"/> representing the date.</returns>
        public static LocalDate ToLocalDateFromIso8601(this string value)
        {
            var result = LocalDatePattern.Iso.Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            throw new ErrorException(Errors.General.Unexpected($"Date not in ISO format (yyyy-mm-dd): {value}."));
        }

        /// <summary>
        /// Converts an ISO8601 string representation of a date to a <see cref="LocalDateTime"/>.
        /// </summary>
        /// <param name="value">A string representing a date in ISO8601 format.</param>
        /// <returns>A new instance of <see cref="LocalDateTime"/> representing the date.</returns>
        public static LocalDateTime ToLocalDateTimeFromExtendedIso8601(this string value)
        {
            var result = LocalDateTimePattern.ExtendedIso.Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            throw new ErrorException(Errors.General.Unexpected($"Date not in Extended ISO format (uuuu-MM-ddTHH:mm:ss.fffffff): {value}."));
        }

        /// <summary>
        /// Converts the dateTime in Extended ISO8601 format to a <see cref="ZonedDateTime"/> in the given time zone.
        /// NOTE: This method is lenient and will not
        /// throw an exception if the resulting date time is ambiguous
        /// such as during daylight savings time.
        /// </summary>
        public static ZonedDateTime ToZonedDateTimeFromExtendedISO8601(this string dateTime, DateTimeZone timeZone)
        {
            return dateTime
                .ToLocalDateTimeFromExtendedIso8601()
                .InZoneLeniently(timeZone);
        }

        /// <summary>
        /// Converts the Extended ISO8601 datetime to zoned datetime in the given time zone
        /// and returns the ticks.
        /// NOTE: This method is lenient and will not
        /// throw an exception if the resulting date time is ambiguous
        /// such as during daylight savings time.
        /// </summary>
        public static long ToTicksFromExtendedISO8601InZone(this string dateTime, DateTimeZone timeZone)
        {
            return dateTime
                .ToLocalDateTimeFromExtendedIso8601()
                .InZoneLeniently(timeZone)
                .ToInstant()
                .ToUnixTimeTicks();
        }

        /// <summary>
        /// Converts a string representation of a date in format dd/mm/yyyy to a <see cref="LocalDate"/>.
        /// </summary>
        /// <param name="value">A string representation of a date in format dd/mm/yyyy.</param>
        /// <returns>A new instance of <see cref="LocalDate"/> representing the date.</returns>
        public static LocalDate ToLocalDateFromddMMyyyy(this string value)
        {
            var result = LocalDatePattern.CreateWithInvariantCulture("dd/MM/yyyy").Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            throw new ErrorException(Errors.General.Unexpected($"Date not in expected format (dd/MM/yyyy): {value}."));
        }

        /// <summary>
        /// Converts a string representation of a date in format M/d/yy to a <see cref="LocalDate"/>.
        /// </summary>
        /// <param name="value">A string representation of a date in format dd/mm/yyyy.</param>
        /// <returns>A new instance of <see cref="LocalDate"/> representing the date.</returns>
        public static LocalDate ToLocalDateFromMdyy(this string value)
        {
            var result = LocalDatePattern.CreateWithInvariantCulture("M/d/yy").Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            throw new ErrorException(Errors.General.Unexpected($"Date not in expected format (M/d/yy): {value}."));
        }

        /// <summary>
        /// Converts a string representation of a date in ISO8601 format or dd/mm/yyyy or dd/MM/yyy to a <see cref="LocalDate"/>.
        /// </summary>
        /// <param name="value">A string representation of a date in ISO8601 format or dd/mm/yyyy or dd/MM/yy.</param>
        /// <returns>A new instance of <see cref="LocalDate"/> representing the date.</returns>
        public static LocalDate ToLocalDateFromIso8601OrddMMyyyyOrddMMyy(
            this string value, string fieldName = "")
        {
            var result = LocalDatePattern.Iso.Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            result = LocalDatePattern.CreateWithInvariantCulture("dd/MM/yyyy").Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            result = LocalDatePattern.CreateWithInvariantCulture("dd/MM/yy").Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            var varName = fieldName.IsNotNullOrWhitespace() ? $" '{fieldName}'" : string.Empty;
            throw new ErrorException(Errors.General.Unexpected(
                $"Date{varName} not in any of expected formats (yyyy-mm-dd or dd/mm/yyyy or dd/MM/yy): {value}."));
        }

        /// <summary>
        /// Converts a string representation of a date in ISO8601 format or dd/mm/yyyy or dd/MM/yyy or dd-MM-yyyy or dd-MM-yy or d MMM yyyy to a <see cref="LocalDate"/>.
        /// </summary>
        /// <param name="value">A string representation of a date in ISO8601 format or dd/mm/yyyy or dd/MM/yy or dd-MM-yyyy or dd-MM-yy or d MMM yyyy.</param>
        /// <param name="cultureInfo">A locale for culture info.</param>
        /// <returns>A new instance of <see cref="LocalDate"/> representing the date.</returns>
        public static LocalDate ToLocalDateFromIso8601OrddMMyyyyOrddMMyyOrdMMMyyyyWithCulture(this string value, CultureInfo cultureInfo)
        {
            var result = LocalDatePattern.Iso.Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            result = LocalDatePattern.Create("dd/MM/yyyy", cultureInfo).Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            result = LocalDatePattern.Create("dd/MM/yy", cultureInfo).Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            result = LocalDatePattern.Create("dd-MM-yy", cultureInfo).Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            result = LocalDatePattern.Create("dd-MM-yyyy", cultureInfo).Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            result = LocalDatePattern.Create("d MMM yyyy", cultureInfo).Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            throw new ArgumentException($"Date not in either of expected formats (yyyy-mm-dd or dd/mm/yyyy or dd/MM/yy or dd-MM-yyyy or dd-MM-yy or d MMM yyyy): {value}.");
        }

        /// <summary>
        /// Converts a string representation of a date in ISO8601 format or datetime ISO8601 format to a <see cref="LocalDate"/>.
        /// </summary>
        /// <param name="value">A string representation of a date in ISO8601 format or datetime ISO8601 format.</param>
        /// <returns>A new instance of <see cref="LocalDate"/> representing the date.</returns>
        public static LocalDate ToLocalDateFromIso8601OrDateTimeIso8601(this string value)
        {
            var result = LocalDatePattern.Iso.Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            var dateTimeresult = OffsetDateTimePattern.ExtendedIso.Parse(value);
            if (dateTimeresult.Success)
            {
                return dateTimeresult.Value.ToInstant().ToLocalDateInAet();
            }

            throw new ArgumentException($"Date not in either of expected formats (date ISO8601 or dateTime ISO8601): {value}.");
        }

        /// <summary>
        /// Converts a string representation of a time in ISO8601 format or datetime ISO8601 format to a <see cref="LocalTime"/>.
        /// </summary>
        /// <param name="value">A string representation of a time in ISO8601 format or datetime ISO8601 format.</param>
        /// <returns>A new instance of <see cref="LocalTime"/> representing the time.</returns>
        public static LocalTime ToLocalTimeFromIso8601OrDateTimeIso8601(this string value)
        {
            var result = LocalTimePattern.ExtendedIso.Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            var dateTimeresult = OffsetDateTimePattern.ExtendedIso.Parse(value);
            if (dateTimeresult.Success)
            {
                return dateTimeresult.Value.ToInstant().ToLocalTimeInAet();
            }

            var cultureInfo = CultureInfo.GetCultureInfo(Locales.en_AU);
            throw new ArgumentException($"Time not in either of expected formats (Time ISO8601 or dateTime ISO8601): {value}.");
        }

        /// <summary>
        /// Converts a string representation of a time in ISO8601 format or h:mm tt or h:mm:ss tt or hh:mm tt or hh:mm:ss tt to a <see cref="LocalTime"/>.
        /// </summary>
        /// <param name="value">A string representation of a time in ISO8601 format or h:mm tt or h:mm:ss tt or hh:mm tt or hh:mm:ss tt or hh:mm:ss.FFFFFFF tt.</param>
        /// <param name="cultureInfo">A locale for culture info.</param>
        /// <returns>A new instance of <see cref="LocalTime"/> representing the time.</returns>
        public static LocalTime ToLocalTimeFromIso8601OrhmmttOrhhmmttOrhhmmssttWithCulture(this string value, CultureInfo cultureInfo)
        {
            var result = LocalTimePattern.ExtendedIso.Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            result = LocalTimePattern.Create("h:mm tt", cultureInfo).Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            result = LocalTimePattern.Create("h:mm:ss tt", cultureInfo).Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            result = LocalTimePattern.Create("hh:mm tt", cultureInfo).Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            result = LocalTimePattern.Create("hh:mm:ss tt", cultureInfo).Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            result = LocalTimePattern.Create("hh:mm:ss.FFFFFFF tt", cultureInfo).Parse(value);
            if (result.Success)
            {
                return result.Value;
            }

            throw new ArgumentException($"Time not in either of expected formats (h:mm tt or h:mm:ss tt or hh:mm tt or hh:mm:ss tt): {value}.");
        }

        /// <summary>
        /// Try and parse a string as a local date.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>The parsed date, or the none.</returns>
        public static Result<LocalDate> TryParseAsLocalDate(this string value)
        {
            var result = LocalDatePattern.Iso.Parse(value);
            if (result.Success)
            {
                return Result.Success(result.Value);
            }

            result = LocalDatePattern.CreateWithInvariantCulture("dd/MM/yyyy").Parse(value);
            if (result.Success)
            {
                return Result.Success(result.Value);
            }

            return Result.Failure<LocalDate>($"Could not parse '{value}' as LocalDate (supported formats are yyyy-mm-dd or dd/mm/yyyy).");
        }

        /// <summary>
        /// Converts a string representation of a datetime ISO8601 format to a <see cref="Instant"/>.
        /// </summary>
        /// <param name="value">A string representation of datetime ISO8601 format.</param>
        /// <returns>A new instance of <see cref="Instant"/> representing the instant.</returns>
        public static Instant ToInstantFromDateTimeIso8601(this string value)
        {
            var dateTimeresult = OffsetDateTimePattern.ExtendedIso.Parse(value);
            if (dateTimeresult.Success)
            {
                return dateTimeresult.Value.ToInstant();
            }

            throw new ArgumentException($"DateTime not expected formats dateTime ISO8601: {value}.");
        }

        /// <summary>
        /// Provides a more resilient way to parse a string to decimal, which works with currency values.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>The parsed data, or a failure.</returns>
        public static Result<decimal> TryParseAsDecimal(this string value)
        {
            Regex regex = new Regex(@"[^\d,\.]");
            string cleaned = regex.Replace(value, string.Empty);
            bool parsedSuccessfully = decimal.TryParse(cleaned, out decimal result);
            if (!parsedSuccessfully)
            {
                return Result.Failure<decimal>($"Could not parse '{value}' as decimal.");
            }

            return Result.Success(result);
        }

        /// <summary>
        /// Gets the number of ticks since the epoch at the start of the given date in a given timezone.
        /// </summary>
        /// <param name="localDateIso8601">A string representing the date in ISO8601 format.</param>
        /// <param name="timezone">The timezone to calculate the time in.</param>
        /// <returns>The number of ticks since the epoch for the specified moment in time.</returns>
        public static long GetTicksAtStartOfDayInZone(this string localDateIso8601, DateTimeZone timezone)
        {
            return localDateIso8601
                    .ToLocalDateFromIso8601()
                    .AtStartOfDayInZone(timezone)
                    .ToInstant()
                    .ToUnixTimeTicks();
        }

        /// <summary>
        /// Gets the number of ticks since the epoch at the end of the given date in a given timezone.
        /// </summary>
        /// <param name="localDateIso8601">A string representing the date in ISO8601 format.</param>
        /// <param name="timezone">The timezone to calculate the time in.</param>
        /// <returns>The number of ticks since the epoch for the specified moment in time.</returns>
        public static long GetTicksAtEndOfDayInZone(this string localDateIso8601, DateTimeZone timezone)
        {
            return localDateIso8601
                    .GetEndOfDayInZone(timezone)
                    .ToUnixTimeTicks();
        }

        /// <summary>
        /// Gets the number of ticks since the epoch at the end of the given date in a given timezone.
        /// </summary>
        /// <param name="localDateIso8601">A string representing the date in ISO8601 format.</param>
        /// <param name="timezone">The timezone to calculate the time in.</param>
        /// <returns>The the instant class for the specified moment in time.</returns>
        public static Instant GetEndOfDayInZone(this string localDateIso8601, DateTimeZone timezone)
        {
            return localDateIso8601
                    .ToLocalDateFromIso8601()
                    .PlusDays(1)
                    .AtStartOfDayInZone(timezone)
                    .ToInstant();
        }

        /// <summary>
        /// Tests whether a string contains another string using a given comparison method.
        /// </summary>
        /// <param name="source">The string to search withing.</param>
        /// <param name="value">The string to search for.</param>
        /// <param name="comparison">The comparison method.</param>
        /// <returns><c>true</c> if the value is found within the source, otherwise <c>false</c>.</returns>
        public static bool Contains(this string source, string value, StringComparison comparison)
        {
            return source != null && source.IndexOf(value, comparison) >= 0;
        }

        /// <summary>
        /// Convert a string value to an enum value.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The string value to convert.</param>
        /// <returns>The enum value.</returns>
        public static TEnum? ToEnumOrNull<TEnum>(this string? value)
            where TEnum : struct, Enum
        {
            if (value == null)
            {
                return null;
            }
            return (TEnum)(object)value.ToEnumOrNull(typeof(TEnum))!;
        }

        /// <summary>
        /// Convert a string value to an enum value.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The string value to convert.</param>
        /// <returns>The enum value.</returns>
        public static TEnum ToEnumOrThrow<TEnum>(this string? value)
            where TEnum : struct, Enum
        {
            if (value == null)
            {
                throw new ArgumentException($"Value given was null.");
            }
            return (TEnum)(object)value.ToEnumOrThrow(typeof(TEnum));
        }

        /// <summary>
        /// Convert a string value to an enum value dynamicly.
        /// </summary>
        /// <param name="value">The string value to convert.</param>
        /// <param name="enumType">The type of the enum.</param>
        /// <returns>The enum value.</returns>
        public static Enum? ToEnumOrNull(this string? value, Type enumType)
        {
            Type? underlyingType = Nullable.GetUnderlyingType(enumType);
            enumType = underlyingType ?? enumType;
            if (value == null && underlyingType == null)
            {
                return null;
            }

            Enum enumValue = value.DehumanizeTo(enumType, OnNoMatch.ReturnsNull);
            if (enumValue != null)
            {
                return enumValue;
            }

            // try converting to Sentence Case
            var adjustedValue1 = value.Humanize(LetterCasing.Sentence);
            enumValue = adjustedValue1.DehumanizeTo(enumType, OnNoMatch.ReturnsNull);
            if (enumValue != null)
            {
                return enumValue;
            }

            // Try title case with dashes removed
            var adjustedValue2 = adjustedValue1.Replace('-', ' ');
            enumValue = adjustedValue2.DehumanizeTo(enumType, OnNoMatch.ReturnsNull);
            if (enumValue != null)
            {
                return enumValue;
            }

            // Try removing whitespace
            var adjustedValue3 = adjustedValue2.RemoveWhitespace();
            enumValue = adjustedValue3.DehumanizeTo(enumType, OnNoMatch.ReturnsNull);
            if (enumValue != null)
            {
                return enumValue;
            }

            try
            {
                // Since Humanizer will try to match the description, we'll also try to the standard .net way
                // and match the enum name itself.
                if (Enum.TryParse(enumType, value, out var enumValueObj))
                {
                    if (enumValueObj != null)
                    {
                        return (Enum)enumValueObj;
                    }
                }
                else if (Enum.TryParse(enumType, adjustedValue3, out var enumValueObjAdjusted))
                {
                    if (enumValueObjAdjusted != null)
                    {
                        return (Enum)enumValueObjAdjusted;
                    }
                }
            }
            catch (OverflowException)
            {
            }
            catch (ArgumentException)
            {
            }

            // Try the MatchesStringValueAttribute
            FieldInfo? field = enumType.GetFields()
                .Where(f => f.GetCustomAttributes()
                    .Any(a => a is MatchesStringValueAttribute attr && attr.Value == value))
                .SingleOrDefault();
            if (field != null)
            {
                return (Enum)Enum.Parse(enumType, field.Name, true);
            }

            return null;
        }

        public static Enum ToEnumOrThrow(this string value, Type enumType)
        {
            Enum? val = ToEnumOrNull(value, enumType);
            if (val == null)
            {
                throw new ArgumentException($"\"{value}\" did not match any values in the Enum {enumType}.");
            }
            return val;
        }

        /// <summary>
        /// Removes whitespace from a string, including at the start/end or anywhere within the string.
        /// </summary>
        /// <param name="source">The source string.</param>
        /// <returns>A string with the whitespace removed.</returns>
        public static string RemoveWhitespace(this string source)
        {
            return new string(source.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
        }

        /// <summary>
        /// Gets a json property from a string containing json.
        /// </summary>
        /// <param name="value">The string containing json.</param>
        /// <param name="path">The path of the property to select.</param>
        /// <returns>A JToken representing the property, or null if no property was found.</returns>
        public static JToken? GetJsonProperty(this string value, string path)
        {
            var jtoken = JToken.Parse(value);
            return jtoken.SelectToken(path);
        }

        /// <summary>
        /// Gets a json property from a string containing json.
        /// </summary>
        /// <param name="value">The string containing json.</param>
        /// <param name="path">The path of the property to select.</param>
        /// <returns>A JToken representing the property, or null if no property was found.</returns>
        public static Maybe<JToken> TryGetJsonProperty(this string value, string path)
        {
            var rootToken = JToken.Parse(value);
            var propertyToken = rootToken.SelectToken(path);
            if (propertyToken == null)
            {
                return Maybe<JToken>.None;
            }
            return Maybe<JToken>.From(propertyToken);
        }

        public static T FromJson<T>(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default(T)!;
            }
            return JsonConvert.DeserializeObject<T>(value)!;
        }

        /// <summary>
        /// Converts a string to Title Case.
        /// </summary>
        /// <param name="value">The string to be converted to Title Case.</param>
        /// <returns>The string, in Title Case.</returns>
        public static string ToTitleCase(this string value)
        {
            TextInfo localeTextInfo = new CultureInfo("en-US", false).TextInfo;
            return localeTextInfo.ToTitleCase(value.ToLower());
        }

        /// <summary>
        /// Converts a word to title case if it contains only letters and at least a single vowel.
        /// For example, "multi" to "Multi", "MULTI" to "Multi", "GTS" to "GTS", "MY21" to "MY21".
        /// </summary>
        /// <param name="word">The word to be converted.</param>
        /// <returns>The result word in title case.</returns>
        public static string ToTitleCaseIfHasLettersOnlyWithVowel(this string word)
        {
            if (Regex.IsMatch(word, @"^[A-Z]+$", RegexOptions.IgnoreCase) && Regex.Matches(word, @"[AEIOU]", RegexOptions.IgnoreCase).Count > 0)
            {
                return word.ToLower().ToUpperFirstChar();
            }

            return word;
        }

        /// <summary>
        /// Converts the first character of the string to upper case.
        /// </summary>
        /// <param name="value">The string to be converted to uppercase first character.</param>
        /// <returns>The result string.</returns>
        public static string ToUpperFirstChar(this string value)
        {
            return string.Concat(value[0].ToString().ToUpper(), value.AsSpan(1));
        }

        /// <summary>
        /// Compares two strings and returns whether they are equal or not, ignoring case.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <param name="otherValue">The other string.</param>
        /// <returns>true if they are equal, ingoring case.</returns>
        public static bool EqualsIgnoreCase(this string value, string otherValue)
        {
            return value?.Equals(otherValue, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Expose string.IsNullOrEmpty() as extension method.
        /// </summary>
        /// <param name="value">The string (or null) to test.</param>
        /// <returns>true if the string is null or empty, otherwise false.</returns>
        /// <remarks>
        /// PLEASE DO NOT USE THIS. If you use this you will get null warnings.'
        /// Instead, use string.IsNullOrEmpty() directly.
        /// </remarks>
        [Obsolete("Please use string.IsNullOrEmpty() directly to avoid null dereference warnings.", false)]
        public static bool IsNullOrEmpty(this string? value)
        {
            return string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Expose !string.IsNullOrEmpty() as extension method.
        /// </summary>
        /// <param name="value">The string (or null) to test.</param>
        /// <returns>true if the string contains any characters, otherwise false.</returns>
        [Obsolete("Please use !string.IsNullOrEmpty() directly to avoid null dereference warnings.", false)]
        public static bool IsNotNullOrEmpty(this string? value)
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        /// Expose string.IsNullOrWhitespace() as extension method.
        /// </summary>
        /// <param name="value">The string (or null) to test.</param>
        /// <returns>true if the string is null, empty or contains only whitespace characters, otherwise false.</returns>
        [Obsolete("Please use string.IsNullOrWhitespace() directly to avoid null dereference warnings.", false)]
        public static bool IsNullOrWhitespace(this string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Expose !string.IsNullOrWhitespace() as extension method.
        /// </summary>
        /// <param name="value">The string (or null) to test.</param>
        /// <returns>true if the string contains any non-whitespace characters, otherwise false.</returns>
        [Obsolete("Please use !string.IsNullOrWhitespace() directly to avoid null dereference warnings.", false)]
        public static bool IsNotNullOrWhitespace(this string? value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Parses the string to a Guid, or if it fails to Parse then it throws a user (400 series) ErrorException.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>A Guid, if the string was a valid Guid, otherwise a user exception is thrown.</returns>
        public static Guid ParseGuidOrThrow(this string value)
        {
            if (!Guid.TryParse(value, out Guid result))
            {
                throw new ErrorException(Errors.General.InvalidGuid(value));
            }

            return result;
        }

        /// <summary>
        /// Truncates the string to the given length and adds ellipsis, only if the
        /// string exceeds the specified length.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <returns>The shortened string value.</returns>
        public static string LimitLengthWithEllipsis(this string value, int maxLength)
        {
            if (value?.Length > maxLength)
            {
                value = value.Substring(0, maxLength);
                value += "...";
            }

            return value ?? string.Empty;
        }

        /// <summary>
        /// Escape a character in a string by prepending it with a backslash.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <param name="characterToEscape">The character to escape.</param>
        /// <param name="escapeCharacter">The character that should be used as an escape character.</param>
        /// <returns>An escaped string.</returns>
        public static string EscapeCharacter(this string value, char characterToEscape, char escapeCharacter)
        {
            return value.Replace($"{characterToEscape}", $"{escapeCharacter}{characterToEscape}");
        }

        /// <summary>
        /// Escape double quotes in a string by prepending them with a backslash.
        /// </summary>
        /// <param name="value">The string to escape.</param>
        /// <returns>The escaped string.</returns>
        public static string EscapeDoubleQuotesByDoubling(this string value)
        {
            return value.EscapeCharacter('"', '"');
        }

        /// <summary>
        /// Wrap a string in double quotes, escaping any pre-existing double quotes and any backslashes in the string.
        /// </summary>
        /// <param name="value">The string to wrap.</param>
        /// <returns>A quoted string.</returns>
        public static string FormatForCsv(this string value)
        {
            return "\"" + value.EscapeDoubleQuotesByDoubling() + "\"";
        }

        /// <summary>
        /// Fixes malformed JSON string (calculation result).
        /// </summary>
        /// <param name="value">The string to escape.</param>
        /// <returns>The cleaned JSON string.</returns>
        public static string FixMalformedJsonString(this string value)
        {
            return value.Replace("},}", "}}")
                        .Replace(",}", "}")
                        .Replace(",]", "]")
                        .Replace("},]", "}]")
                        .Replace("],}", "]}")
                        .Replace("],]", "]]")
                        .Replace("\",}", "\"}");
        }

        /// <summary>
        /// Generate a hash for a given string.
        /// </summary>
        /// <param name="value">The string to hash.</param>
        /// <returns>The hash of the string.</returns>
        public static byte[] GetHash(this string value)
        {
            HashAlgorithm algorithm = SHA256.Create();
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// Generate a hashString for a given string.
        /// </summary>
        /// <param name="value">The string to hash.</param>
        /// <returns>The hashString of the string.</returns>
        public static string GetHashString(this string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(value))
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Replace the  double backslash to forward slash of the given string.
        /// </summary>
        /// <param name="value">The string to replace.</param>
        /// <returns>The replaced value.</returns>
        public static string ToWebPath(this string value)
        {
            return value.Replace("\\", "/");
        }

        /// <summary>
        /// Replace the forward slash to double backslash of the given string.
        /// </summary>
        /// <param name="value">The string to replace.</param>
        /// <returns>The replaced value.</returns>
        public static string ToLocalPath(this string value)
        {
            return value.Replace("/", "\\");
        }

        /// <summary>
        /// trims and replaces any other groups of 1 or more whitespace characters with a single space.
        /// </summary>
        /// <param name="value">The string (or null) to test.</param>
        /// <returns>the trimmed version.</returns>
        public static string NormalizeWhitespace(this string value)
        {
            value = Regex.Replace(value, @"\s+", " ").Trim(' ');

            return value;
        }

        /// <summary>
        /// Mask a name.
        /// </summary>
        /// <param name="value">The name to mask.</param>
        /// <returns>The masked name.</returns>
        public static string ToMaskedName(this string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var trimmedValue = value.Trim();

            if (Regex.IsMatch(trimmedValue, "\\s"))
            {
                return Regex.Replace(trimmedValue, "(\\s)(\\S)(\\S*)$", "$1$2****");
            }

            if (trimmedValue.Length > 0)
            {
                return trimmedValue[0] + "****";
            }

            return trimmedValue;
        }

        /// <summary>
        /// Format the string to camel case.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <returns>The string in camel case format.</returns>
        public static string ToCamelCase(this string value)
        {
            return value.Camelize();
        }

        /// <summary>
        /// Returns true if the string is "true" or "yes" (case insensitive), otherwise returns false.
        /// </summary>
        public static bool ToBoolean(this string value)
        {
            return value.EqualsIgnoreCase("true") || value.EqualsIgnoreCase("yes");
        }

        public static bool? ToNullableBoolean(this string value)
        {
            if (value == null)
            {
                return null;
            }

            return ToBoolean(value);
        }

        /// <summary>
        /// Appends a dot "." (period) if it doesn't already have it at the end of the string.
        /// </summary>
        /// <param name="value">The string to append a dot to.</param>
        /// <returns>The string with a dot at the end.</returns>
        public static string WithDot(this string value)
        {
            if (!value.EndsWith("."))
            {
                return value + ".";
            }

            return value;
        }

        /// <summary>
        /// Appends a space " " if it doesn't already have it at the end of the string.
        /// </summary>
        /// <param name="value">The string to append a space to.</param>
        /// <returns>The string with a space at the end.</returns>
        public static string WithSpace(this string value)
        {
            if (!value.EndsWith(" "))
            {
                return value + " ";
            }

            return value;
        }

        /// <summary>
        /// Joins a list of values with commas and an "or" for the last item.
        /// E.g.: "Red, Blue, Yellow, or Purple".
        /// </summary>
        /// <param name="values">The strings to join.</param>
        public static string JoinCommaSeparatedWithFinalOr(this IEnumerable<string> values)
        {
            var valueList = values.ToList();
            string result = string.Empty;
            for (int i = 0; i < valueList.Count; i++)
            {
                if (i == valueList.Count - 1)
                {
                    result += ", or ";
                }
                else if (i > 0)
                {
                    result += ", ";
                }

                result += valueList[i];
            }

            return result;
        }

        /// <summary>
        /// Checks if the string is a relative json pointer.
        /// </summary>
        public static bool IsRelativeJsonPointer(this string value)
        {
            return Regex.IsMatch(value, @"^\d+[/#]");
        }

        public static string HyphenToUnderscore(this string input)
        {
            return input.Replace("-", "_");
        }

        public static string MaskStringLeavingOnlyFirstAndLastCharacters(this string input, string token = "****")
        {
            if (input == null)
            {
                return string.Empty;
            }

            var trimmedInput = input.Trim();
            if (trimmedInput.Length > 1)
            {
                return trimmedInput[0] + token + trimmedInput[trimmedInput.Length - 1];
            }
            else if (trimmedInput.Length == 1)
            {
                return trimmedInput + token;
            }
            else
            {
                return string.Empty;
            }
        }

        public static string MaskStringLeavingOnlyFirstCharacter(this string input, string token = "****")
        {
            if (input == null)
            {
                return string.Empty;
            }

            var trimmedInput = input.Trim();
            if (trimmedInput.Length > 0)
            {
                return trimmedInput[0] + token;
            }

            return string.Empty;
        }
    }
}
