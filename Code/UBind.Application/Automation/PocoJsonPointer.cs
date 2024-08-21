// <copyright file="PocoJsonPointer.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Microsoft.Extensions.Primitives;
    using Microsoft.Json.Pointer;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation.Enums;
    using UBind.Application.Automation.Helper;
    using UBind.Application.Automation.Providers;
    using UBind.Domain;
    using UBind.Domain.Exceptions;

    /// <summary>
    /// Represents a JSON Pointer as defined in RFC 6901, which can evaluate against
    /// plain on C# objects instead of needing it to be a JToken/JObject/JArray.
    /// </summary>
    public class PocoJsonPointer : IEquatable<PocoJsonPointer>
    {
        private const char TokenSeparator = '/';
        private const char UriFragmentDelimiter = '#';

        private static readonly Regex IndexPattern =
            new Regex(@"^(0|[1-9][0-9]*)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static readonly Regex PointerPattern = new Regex(
@"^
    (
        /?
        (?<referenceToken>
            (
                [^~/]
                | ~0
                | ~1
                )*
        )?
    )*
$",
RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace);

        private readonly string pointer;
        private readonly JsonPointerRepresentation representation;

        /// <summary>
        /// Initializes a new instance of the <see cref="PocoJsonPointer"/> class with the
        /// specified string.
        /// </summary>
        /// <param name="pointer">
        /// The string value of the JSON Pointer.
        /// </param>
        /// <param name="callingProviderName">The automation provider name, for error reporting purposes.</param>
        /// <param name="representation">
        /// A value that specifies the representation of the JON Pointer.
        /// </param>
        public PocoJsonPointer(
            string pointer,
            string callingProviderName = "Unspecified",
            JObject? debugContext = null,
            JsonPointerRepresentation representation = JsonPointerRepresentation.Normal)
        {
            this.pointer = pointer;
            this.CallingProviderName = callingProviderName;
            this.DebugContext = debugContext;
            this.representation = representation;

            this.ReferenceTokens = ImmutableArray.CreateRange(this.Parse(this.pointer));
            this.IsRelative = !this.pointer.StartsWith("/");
        }

        public bool IsRelative { get; }

        public bool IsAbsolute => !this.IsRelative;

        public ImmutableArray<string> ReferenceTokens { get; }

        private string CallingProviderName { get; }

        private JObject? DebugContext { get; set; }

        public static bool operator ==(PocoJsonPointer left, PocoJsonPointer right)
        {
            if (((object)left) == null || ((object)right) == null)
            {
                return Equals(left, right);
            }

            return left.Equals(right);
        }

        public static bool operator !=(PocoJsonPointer left, PocoJsonPointer right)
        {
            if (((object)left) == null || ((object)right) == null)
            {
                return !Equals(left, right);
            }

            return !left.Equals(right);
        }

        public bool Equals(PocoJsonPointer? other)
        {
            if (other is null)
            {
                return false;
            }

            if ((this.ReferenceTokens == null) != (other.ReferenceTokens == null))
            {
                return false;
            }

            if (this.ReferenceTokens == null)
            {
                return true;
            }

            if (this.ReferenceTokens.Length != other.ReferenceTokens.Length)
            {
                return false;
            }

            for (int i = 0; i < this.ReferenceTokens.Length; ++i)
            {
                if (!this.ReferenceTokens[i].Equals(other.ReferenceTokens[i], StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (!(obj is PocoJsonPointer otherPointer))
            {
                return false;
            }

            return this.Equals(otherPointer);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 13;
                foreach (string token in this.ReferenceTokens)
                {
                    hashCode = (hashCode * 397) ^ token.GetHashCode();
                }

                return hashCode;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (!this.IsRelative)
            {
                sb.Append(TokenSeparator);
            }

            sb.Append(string.Join(TokenSeparator, this.ReferenceTokens));
            return sb.ToString();
        }

        /// <summary>
        /// Converts the current relative JSON Pointer to an absolute JSON Pointer by using the
        /// contextJsonPointer.
        /// </summary>
        /// <param name="contextJsonPointer">An absolute JSON Pointer to the current position in the object structure.</param>
        /// <returns>An absoluate JSON Pointer.</returns>
        public PocoJsonPointer ToAbsolute(PocoJsonPointer contextJsonPointer)
        {
            if (!this.IsRelative)
            {
                throw new ArgumentException($"When trying to convert a relative JSON Pointer \"{this}\" to an "
                    + $"absolute JSON Pointer using the context JSON Pointer \"{contextJsonPointer}\", the relative "
                    + $"JSON Pointer was supoosed to be relative but was found to be absolute.");
            }

            if (contextJsonPointer.IsRelative)
            {
                throw new ArgumentException($"When trying to convert a relative JSON Pointer \"{this}\" to an "
                    + $"absolute JSON Pointer using the context JSON Pointer \"{contextJsonPointer}\", the context "
                    + $"JSON Pointer was supoosed to be absolute but was found to be relative.");
            }

            string firstRelativeToken = this.ReferenceTokens[0];
            if (firstRelativeToken.Contains(UriFragmentDelimiter))
            {
                throw new ArgumentException($"When trying to convert a relative JSON Pointer \"{this}\" to an "
                    + $"absolute JSON Pointer using the context JSON Pointer \"{contextJsonPointer}\", we found "
                    + $"the relative JSON Pointer uses the \"#\" character to evaluate the current array index "
                    + $"which is not supported");
            }

            List<string> remainingRelativeTokens = this.ReferenceTokens.ToList();
            remainingRelativeTokens.RemoveAt(0);
            int numSegmentsToRemove = int.Parse(firstRelativeToken);
            if (numSegmentsToRemove > contextJsonPointer.ReferenceTokens.Length)
            {
                throw new ArgumentException($"When trying to convert a relative JSON Pointer \"{this}\" to an "
                    + $"absolute JSON Pointer using the context JSON Pointer \"{contextJsonPointer}\", the relative "
                    + $"JSON Pointer removed more segments from the path than the number that exists.");
            }

            List<string> resultTokens = contextJsonPointer.ReferenceTokens.ToList();
            if (numSegmentsToRemove > 0)
            {
                resultTokens.RemoveRange(resultTokens.Count - numSegmentsToRemove, numSegmentsToRemove);
            }

            resultTokens.AddRange(remainingRelativeTokens);
            return new PocoJsonPointer(TokenSeparator + string.Join(TokenSeparator, resultTokens), this.CallingProviderName, this.DebugContext);
        }

        /// <summary>
        /// Evaluates the json pointer.
        /// </summary>
        /// <param name="pocObject">The object to evaluate against.</param>
        /// <param name="contextJsonPointer">The context, or starting point within the object. This is required
        /// when the JSONPointer is relative.</param>
        /// <returns>The resolved value within the object.</returns>
        public IProviderResult<object?> Evaluate(object pocObject, PocoJsonPointer? contextJsonPointer = null)
        {
            if (this.IsRelative)
            {
                return this.EvaluateRelative(pocObject, contextJsonPointer);
            }

            object? result = pocObject;
            StringBuilder pathBuilder = new StringBuilder();
            foreach (string referenceToken in this.ReferenceTokens)
            {
                string unescapedToken = referenceToken.UnescapeJsonPointer();
                var evaluationResult = this.Evaluate(unescapedToken, pathBuilder, result);

                if (evaluationResult.IsFailure)
                {
                    return evaluationResult;
                }

                result = evaluationResult.Value;
                pathBuilder.Append(TokenSeparator + referenceToken);
            }

            return ProviderResult<object>.Success(result);
        }

        private IProviderResult<object?> EvaluateRelative(object pocObject, PocoJsonPointer? contextJsonPointer)
        {
            if (contextJsonPointer is null)
            {
                throw new ArgumentException($"When trying to evaluate the JSON Pointer \"${this.pointer}\", it was "
                    + "found to be a relative JSON Pointer, and no context path was provided. We therefore cannot "
                    + "resolve this JSON Pointer because there is no defined starting point within the object "
                    + "stucture.");
            }

            if (contextJsonPointer.IsRelative)
            {
                throw new ArgumentException($"When trying to evaluate a relative JSON Pointer \"{this}\" "
                    + $"using the context JSON Pointer \"{contextJsonPointer}\", the context "
                    + $"JSON Pointer was supoosed to be absolute but was found to be relative.");
            }

            if (!this.ReferenceTokens.Any())
            {
                // we have no path, so just return the whole object.
                return ProviderResult<object>.Success(pocObject);
            }

            string firstToken = this.ReferenceTokens.First();
            if (this.IsRelative && firstToken.EndsWith("#"))
            {
                // Evaluate the relative json pointer for a property name or array index:
                string numberPart = firstToken.Substring(0, firstToken.Length - 1);
                int numSegmentsToRemove = int.Parse(numberPart);
                if (numSegmentsToRemove > contextJsonPointer.ReferenceTokens.Length)
                {
                    throw new ArgumentException($"When trying to evaluate a relative JSON Pointer \"{this}\" using a "
                        + $"context JSON Pointer \"{contextJsonPointer}\", the relative "
                        + $"JSON Pointer removed more segments from the path than the number that exists.");
                }

                List<string> resultTokens = contextJsonPointer.ReferenceTokens.ToList();
                resultTokens.RemoveRange(resultTokens.Count - numSegmentsToRemove, numSegmentsToRemove);
                string token = resultTokens.Last();
                if (IndexPattern.IsMatch(token))
                {
                    return ProviderResult<object>.Success(int.Parse(token));
                }
                else
                {
                    return ProviderResult<object>.Success(token);
                }
            }

            // Convert it to absolute and evaluate it:
            return this.ToAbsolute(contextJsonPointer).Evaluate(pocObject);
        }

        private IProviderResult<object?> Evaluate(string referenceToken, StringBuilder pathBuilder, object? current)
        {
            if ((current == null || (current is JToken currentToken && currentToken.Type == JTokenType.Null))
                && !referenceToken.Equals(string.Empty)
                && !referenceToken.Equals("/"))
            {
                this.AddOrUpdateDebugContextQueryPath();
                return ProviderResult<object>.Failure(Errors.Automation.PathNotFound(
                    this.CallingProviderName,
                    this.pointer,
                    pathBuilder.ToString(),
                    referenceToken,
                    this.GetDebugContext()));
            }

            if (current is not null && DataObjectHelper.IsStructuredObjectOrArray(current))
            {
                // check if it's an array
                if (DataObjectHelper.IsArray(current))
                {
                    if (current is StringValues currentStringValues)
                    {
                        return ProviderResult<object>.Success(this.EvaluateArrayReference(referenceToken, pathBuilder, currentStringValues.ToList<string>()));
                    }

                    return ProviderResult<object>.Success(this.EvaluateArrayReference(referenceToken, pathBuilder, (IList)current));
                }

                // it must be a POCO, Dictionary or JObject
                return this.EvaluateObjectReference(
                    referenceToken,
                    pathBuilder,
                    current);
            }

            throw new ErrorException(
                Errors.Automation.PathResolvesToPrimitiveWhenWhenObjectOrArrayExpected(
                    this.CallingProviderName,
                    this.pointer,
                    pathBuilder.ToString(),
                    referenceToken,
                    this.GetDebugContext()));
        }

        private IProviderResult<object?> EvaluateObjectReference(
            string referenceToken,
            StringBuilder pathBuilder,
            object current)
        {
            if (DataObjectHelper.TryGetPropertyValue(current, referenceToken, out object value))
            {
                return ProviderResult<object>.Success(value);
            }

            this.AddOrUpdateDebugContextQueryPath();
            return ProviderResult<object>.Failure(Errors.Automation.PathNotFound(
                this.CallingProviderName,
                this.pointer,
                pathBuilder.ToString(),
                referenceToken,
                this.GetDebugContext()));
        }

        private object? EvaluateArrayReference(string referenceToken, StringBuilder pathBuilder, IList array)
        {
            if (IndexPattern.IsMatch(referenceToken))
            {
                int index = int.Parse(referenceToken, NumberStyles.None, CultureInfo.InvariantCulture);
                if (index >= array.Count)
                {
                    throw new ErrorException(Errors.Automation.IndexOutOfRangeError(
                        this.CallingProviderName,
                        this.pointer,
                        pathBuilder.ToString(),
                        index,
                        array.Count,
                        this.GetDebugContext()));
                }

                return array[index];
            }

            throw new ErrorException(Errors.Automation.PathResolvesToArrayWhenObjectExpected(
                this.CallingProviderName,
                this.pointer,
                pathBuilder.ToString(),
                referenceToken,
                this.GetDebugContext()));
        }

        private IEnumerable<string> Parse(string value)
        {
            if (this.representation == JsonPointerRepresentation.JsonString)
            {
                // TODO: Handle control characters 0x00-0x1F.
                value = value.Replace(@"\\", @"\").Replace(@"\""", @"""");
            }
            else if (this.representation == JsonPointerRepresentation.UriFragment)
            {
                if (value[0] != UriFragmentDelimiter)
                {
                    throw new ArgumentException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            $"Invalid fragment start character when parsing \"{value}\" in the URI Fragment representation.",
                            value),
                        nameof(value));
                }

                value = Uri.UnescapeDataString(value.Substring(1));
            }

            Match match = PointerPattern.Match(value);
            if (match.Success)
            {
                CaptureCollection referenceTokenCaptures = match.Groups["referenceToken"].Captures;
                var captureArray = new Capture[referenceTokenCaptures.Count];
                referenceTokenCaptures.CopyTo(captureArray, 0);
                return captureArray.Select(c => c.Value).Where(v => v != string.Empty);
            }
            else
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        $"Invalid JSON Pointer when parsing \"{value}\".",
                        value),
                    nameof(value));
            }
        }

        private JObject GetDebugContext()
        {
            return this.DebugContext ?? new JObject();
        }

        private void AddOrUpdateDebugContextQueryPath()
        {
            this.DebugContext ??= new JObject();
            this.DebugContext[ErrorDataKey.QueryPath] = this.pointer;
        }
    }
}
